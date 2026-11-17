// Voix : Azure AI Speech par requêtes HTTPS directes (aucun SDK).
// Seuls le texte à prononcer et l'audio enregistré partent vers Azure, avec la clé de l'utilisateur.

import { state } from './store.js';

export const VOICES = [
  { id: 'ar-LB-LaylaNeural', label: 'Layla (femme)' },
  { id: 'ar-LB-RamiNeural', label: 'Rami (homme)' },
];

const REGION_RE = /^[a-z0-9]{3,30}$/;

export function voiceReady() {
  const { azureKey, azureRegion } = state.settings;
  return Boolean(azureKey) && REGION_RE.test(azureRegion);
}

function requireConfig() {
  if (!voiceReady()) throw new Error('Voix non configurée : ajoutez votre clé Azure dans les Réglages.');
  return state.settings;
}

function explain(status) {
  if (status === 401 || status === 403) return 'Clé ou région Azure refusée. Vérifiez les Réglages.';
  if (status === 429) return 'Quota Azure atteint. Réessayez plus tard.';
  return `Erreur du service vocal (${status}).`;
}

const escapeXml = (s) =>
  s.replace(/[<>&'"]/g, (c) => ({ '<': '&lt;', '>': '&gt;', '&': '&amp;', "'": '&apos;', '"': '&quot;' }[c]));

// --- Synthèse vocale ---

const audioCache = new Map(); // "voix|vitesse|texte" -> URL d'objet
let current = null;

export async function speak(text, { slow = state.settings.slow } = {}) {
  const { azureKey, azureRegion, voice } = requireConfig();
  const cacheKey = `${voice}|${slow}|${text}`;
  let url = audioCache.get(cacheKey);
  if (!url) {
    const ssml =
      `<speak version='1.0' xml:lang='ar-LB'><voice name='${voice}'>` +
      `<prosody rate='${slow ? '-30%' : '0%'}'>${escapeXml(text)}</prosody></voice></speak>`;
    const res = await fetch(`https://${azureRegion}.tts.speech.microsoft.com/cognitiveservices/v1`, {
      method: 'POST',
      headers: {
        'Ocp-Apim-Subscription-Key': azureKey,
        'Content-Type': 'application/ssml+xml',
        'X-Microsoft-OutputFormat': 'audio-24khz-48kbitrate-mono-mp3',
      },
      body: ssml,
    });
    if (!res.ok) throw new Error(explain(res.status));
    url = URL.createObjectURL(await res.blob());
    audioCache.set(cacheKey, url);
  }
  return playUrl(url);
}

export function playUrl(url) {
  if (current) current.pause();
  current = new Audio(url);
  return new Promise((resolve, reject) => {
    current.onended = resolve;
    current.onerror = () => reject(new Error('Lecture audio impossible.'));
    current.play().catch(reject);
  });
}

// --- Enregistrement du micro ---

export async function startRecording() {
  const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
  const recorder = new MediaRecorder(stream);
  const chunks = [];
  recorder.ondataavailable = (e) => chunks.push(e.data);
  recorder.start();
  return {
    stop: () =>
      new Promise((resolve) => {
        recorder.onstop = () => {
          stream.getTracks().forEach((t) => t.stop());
          resolve(new Blob(chunks, { type: recorder.mimeType }));
        };
        recorder.stop();
      }),
  };
}

// Azure attend du WAV PCM 16 kHz mono : on convertit l'enregistrement du navigateur.
async function toWav16k(blob) {
  const ctx = new AudioContext();
  const decoded = await ctx.decodeAudioData(await blob.arrayBuffer());
  ctx.close();
  const rate = 16000;
  const offline = new OfflineAudioContext(1, Math.ceil(decoded.duration * rate), rate);
  const src = offline.createBufferSource();
  src.buffer = decoded;
  src.connect(offline.destination);
  src.start();
  const samples = (await offline.startRendering()).getChannelData(0);

  const view = new DataView(new ArrayBuffer(44 + samples.length * 2));
  const str = (o, s) => [...s].forEach((c, i) => view.setUint8(o + i, c.charCodeAt(0)));
  str(0, 'RIFF');
  view.setUint32(4, 36 + samples.length * 2, true);
  str(8, 'WAVEfmt ');
  view.setUint32(16, 16, true);
  view.setUint16(20, 1, true); // PCM
  view.setUint16(22, 1, true); // mono
  view.setUint32(24, rate, true);
  view.setUint32(28, rate * 2, true);
  view.setUint16(32, 2, true);
  view.setUint16(34, 16, true);
  str(36, 'data');
  view.setUint32(40, samples.length * 2, true);
  samples.forEach((s, i) => {
    const v = Math.max(-1, Math.min(1, s));
    view.setInt16(44 + i * 2, v < 0 ? v * 0x8000 : v * 0x7fff, true);
  });
  return new Blob([view], { type: 'audio/wav' });
}

// --- Reconnaissance vocale ---

export async function recognize(blob) {
  const { azureKey, azureRegion } = requireConfig();
  const wav = await toWav16k(blob);
  const res = await fetch(
    `https://${azureRegion}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language=ar-LB&format=simple`,
    {
      method: 'POST',
      headers: {
        'Ocp-Apim-Subscription-Key': azureKey,
        'Content-Type': 'audio/wav; codecs=audio/pcm; samplerate=16000',
        Accept: 'application/json',
      },
      body: wav,
    },
  );
  if (!res.ok) throw new Error(explain(res.status));
  const data = await res.json();
  return data.RecognitionStatus === 'Success' ? data.DisplayText || '' : '';
}

// --- Comparaison texte attendu / texte reconnu ---

function normalize(s) {
  return s
    .replace(/[ً-ْٰـ]/g, '') // voyelles brèves, tatweel
    .replace(/[أإآٱ]/g, 'ا')
    .replace(/ى/g, 'ي')
    .replace(/ة/g, 'ه')
    .replace(/[^ء-ي ]/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();
}

function levenshtein(a, b) {
  let prev = Array.from({ length: b.length + 1 }, (_, j) => j);
  for (let i = 1; i <= a.length; i++) {
    const row = [i];
    for (let j = 1; j <= b.length; j++) {
      row[j] = Math.min(prev[j] + 1, row[j - 1] + 1, prev[j - 1] + (a[i - 1] === b[j - 1] ? 0 : 1));
    }
    prev = row;
  }
  return prev[b.length];
}

// Pourcentage de ressemblance entre ce qui était attendu et ce que le service a compris.
export function similarity(expected, heard) {
  const a = normalize(expected);
  const b = normalize(heard);
  if (!a || !b) return 0;
  return Math.round((1 - levenshtein(a, b) / Math.max(a.length, b.length)) * 100);
}
