import { LESSONS, DIALOGUES, ALL_ITEMS, itemById } from './data.js';
import { state, save, addCards, dueCards, gradeCard, GOALS, todayStats, track, streak } from './store.js';
import { VOICES, voiceReady, speak, playUrl, startRecording, recognize, similarity } from './voice.js';

const root = document.getElementById('app');
const toastEl = document.getElementById('toast');

// Petit constructeur d'éléments : le texte passe toujours par textContent.
function h(tag, props = {}, ...children) {
  const el = document.createElement(tag);
  for (const [k, v] of Object.entries(props)) {
    if (k.startsWith('on')) el.addEventListener(k.slice(2), v);
    else if (k === 'class') el.className = v;
    else if (k === 'style') el.style.cssText = v; // via le DOM : un attribut style serait bloqué par la CSP
    else if (v !== false && v != null) el.setAttribute(k, v === true ? '' : v);
  }
  for (const c of children.flat()) {
    if (c != null && c !== false) el.append(c);
  }
  return el;
}

let toastTimer;
function toast(message) {
  toastEl.textContent = message;
  toastEl.hidden = false;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => (toastEl.hidden = true), 5000);
}

// Icône du jeu défini dans index.html (#i-nom).
function icon(name) {
  const NS = 'http://www.w3.org/2000/svg';
  const svg = document.createElementNS(NS, 'svg');
  svg.setAttribute('class', 'icon');
  svg.setAttribute('aria-hidden', 'true');
  const use = document.createElementNS(NS, 'use');
  use.setAttribute('href', `#i-${name}`);
  svg.append(use);
  return svg;
}

async function listen(text, slow) {
  try {
    await speak(text, { slow });
    track('listen');
  } catch (e) {
    toast(e.message);
  }
}

function listenButtons(text) {
  return h(
    'div',
    { class: 'actions' },
    h('button', { class: 'btn small', onclick: () => listen(text, false) }, icon('play'), 'Écouter'),
    h('button', { class: 'btn small ghost', onclick: () => listen(text, true) }, icon('slow'), 'Lent'),
  );
}

function phrase(item, { showFr = true } = {}) {
  return h(
    'div',
    { class: 'phrase' },
    h('p', { class: 'ar', lang: 'ar', dir: 'rtl' }, item.ar),
    h('p', { class: 'tr' }, item.tr),
    showFr && h('p', { class: 'fr' }, item.fr),
    item.note && h('p', { class: 'note' }, item.note),
  );
}

function page(title, ...content) {
  return [
    h('header', { class: 'page-head' }, h('a', { href: '#/', class: 'back' }, '← Accueil'), h('h1', {}, title)),
    ...content,
  ];
}

function voiceBanner() {
  if (voiceReady()) return null;
  return h(
    'p',
    { class: 'banner' },
    'La voix n’est pas encore configurée. ',
    h('a', { href: '#/reglages' }, 'Ajouter ma clé Azure'),
  );
}

// --- Accueil ---

function viewHome() {
  const dueIds = dueCards();
  const due = dueIds.length;
  const learned = Object.keys(state.cards).length;
  const days = streak();
  const stats = todayStats();

  // L'action du jour : réviser s'il y a des cartes, sinon poursuivre la première leçon non ajoutée.
  const nextLesson = LESSONS.find((l) => l.items.some((i) => !state.cards[i.id]));
  const cta = due
    ? { href: '#/revision', label: `Réviser mes ${due} carte${due > 1 ? 's' : ''}` }
    : nextLesson
      ? { href: `#/lecon/${nextLesson.id}`, label: `Commencer : ${nextLesson.title}` }
      : { href: '#/prononciation', label: 'M’entraîner à voix haute' };

  const goal = (label, done, total) => {
    const reached = done >= total;
    return h(
      'li',
      {},
      h('span', {}, label),
      h('span', { class: 'count' }, `${Math.min(done, total)} / ${total}`),
      h(
        'div',
        { class: 'bar', role: 'progressbar', 'aria-label': label, 'aria-valuenow': Math.min(done, total), 'aria-valuemin': 0, 'aria-valuemax': total },
        h('i', { class: reached ? 'done' : '', style: `width:${Math.min(100, (100 * done) / total)}%` }),
      ),
    );
  };

  return [
    h(
      'header',
      { class: 'top' },
      h('span', { class: 'brand' }, icon('logo'), h('span', { class: 'wordmark' }, 'ahla')),
      h('span', { class: `streak${days ? '' : ' off'}`, title: 'Jours d’affilée' }, icon('flame'), days ? `${days} jour${days > 1 ? 's' : ''}` : 'Jour 1'),
    ),
    h(
      'section',
      { class: 'hero' },
      h('h1', {}, 'Ahla ! On parle 5 minutes ?'),
      h('p', { class: 'note' }, due ? `${due} phrase${due > 1 ? 's t’attendent' : ' t’attend'} aujourd’hui.` : 'Écoute, répète à voix haute, puis révise.'),
    ),
    voiceBanner(),
    h('a', { class: 'btn primary block', href: cta.href }, icon('play'), cta.label),
    h('h2', {}, 'Objectifs du jour'),
    h(
      'ul',
      { class: 'goals' },
      goal(`Écouter ${GOALS.listen} phrases`, stats.listen, GOALS.listen),
      goal(`Répéter ${GOALS.speak} phrases à voix haute`, stats.speak, GOALS.speak),
      goal(`Réviser ${GOALS.review} cartes`, stats.review, GOALS.review),
    ),
    due > 0 && h('h2', {}, 'À réviser'),
    due > 0 &&
      h(
        'ul',
        { class: 'rev' },
        dueIds.slice(0, 3).map((id) => {
          const item = itemById(id);
          return (
            item &&
            h(
              'li',
              {},
              h('button', { class: 'ibtn audio', 'aria-label': `Écouter : ${item.tr}`, onclick: () => listen(item.ar, false) }, icon('play')),
              h('div', {}, h('b', {}, item.tr), h('span', { class: 'note' }, item.fr)),
            )
          );
        }),
      ),
    h(
      'section',
      { class: 'tiles' },
      h(
        'a',
        { class: 'tile', href: '#/revision' },
        h('strong', {}, 'Révisions'),
        h('span', {}, `${learned} expression${learned > 1 ? 's' : ''} apprise${learned > 1 ? 's' : ''}`),
      ),
      h('a', { class: 'tile', href: '#/prononciation' }, h('strong', {}, 'Prononciation'), h('span', {}, 'Écouter, répéter, comparer')),
    ),
    h('h2', {}, 'Leçons'),
    h(
      'ul',
      { class: 'list' },
      LESSONS.map((l) => {
        const inReview = l.items.filter((i) => state.cards[i.id]).length;
        return h(
          'li',
          {},
          h('a', { href: `#/lecon/${l.id}` }, h('strong', {}, l.title), h('span', {}, `${l.items.length} expressions · ${inReview} en révision`)),
        );
      }),
    ),
    h('h2', {}, 'Mini-dialogues'),
    h(
      'ul',
      { class: 'list' },
      DIALOGUES.map((d) => h('li', {}, h('a', { href: `#/dialogue/${d.id}` }, h('strong', {}, d.title), h('span', {}, `${d.lines.length} répliques`)))),
    ),
    h('nav', { class: 'foot' }, h('a', { href: '#/guide' }, 'Lire la translittération'), h('a', { href: '#/reglages' }, 'Réglages')),
  ];
}

// --- Leçon ---

function viewLesson(id) {
  const lesson = LESSONS.find((l) => l.id === id);
  if (!lesson) return viewHome();
  const addBtn = h(
    'button',
    {
      class: 'btn primary',
      onclick: () => {
        const n = addCards(lesson.items.map((i) => i.id));
        toast(n ? `${n} expression${n > 1 ? 's ajoutées' : ' ajoutée'} à vos révisions.` : 'Cette leçon est déjà dans vos révisions.');
      },
    },
    'Ajouter cette leçon à mes révisions',
  );
  return page(
    lesson.title,
    voiceBanner(),
    h('ul', { class: 'cards' }, lesson.items.map((item) => h('li', { class: 'card' }, phrase(item), listenButtons(item.ar)))),
    addBtn,
  );
}

// --- Révision espacée ---

function viewReview() {
  const queue = dueCards();
  const box = h('div', {});

  function show() {
    const id = queue.shift();
    const item = id && itemById(id);
    if (!item) {
      box.replaceChildren(
        h('p', { class: 'empty' }, Object.keys(state.cards).length ? 'Rien à réviser pour le moment. Revenez plus tard.' : 'Ajoutez d’abord une leçon à vos révisions.'),
        h('a', { class: 'btn', href: '#/' }, 'Retour à l’accueil'),
      );
      return;
    }
    const grade = (g) => () => {
      gradeCard(id, g);
      track('review');
      show();
    };
    const answer = h(
      'div',
      { hidden: true },
      phrase(item, { showFr: false }),
      listenButtons(item.ar),
      h(
        'div',
        { class: 'actions grades' },
        h('button', { class: 'btn', onclick: grade('again') }, 'À revoir'),
        h('button', { class: 'btn primary', onclick: grade('good') }, 'Correct'),
        h('button', { class: 'btn', onclick: grade('easy') }, 'Facile'),
      ),
    );
    const reveal = h(
      'button',
      {
        class: 'btn primary',
        onclick: () => {
          answer.hidden = false;
          reveal.hidden = true;
        },
      },
      'Voir la réponse',
    );
    box.replaceChildren(
      h('p', { class: 'count' }, `${queue.length + 1} carte${queue.length ? 's' : ''} restante${queue.length ? 's' : ''}`),
      h('div', { class: 'card big' }, h('p', { class: 'prompt' }, 'Comment dit-on en libanais ?'), h('p', { class: 'fr big' }, item.fr), reveal, answer),
    );
  }

  show();
  return page('Révision', voiceBanner(), box);
}

// --- Prononciation ---

function viewPronunciation() {
  let items = ALL_ITEMS;
  let index = 0;
  const box = h('div', {});

  const select = h(
    'select',
    {
      onchange: () => {
        const lesson = LESSONS.find((l) => l.id === select.value);
        items = lesson ? lesson.items : ALL_ITEMS;
        index = 0;
        show();
      },
    },
    h('option', { value: '' }, 'Toutes les leçons'),
    LESSONS.map((l) => h('option', { value: l.id }, l.title)),
  );

  function show() {
    const item = items[index];
    let recorder = null;
    let recordingUrl = null;
    const result = h('div', { class: 'result' });
    const replay = h('button', { class: 'btn ghost', hidden: true, onclick: () => playUrl(recordingUrl).catch((e) => toast(e.message)) }, 'Me réécouter');

    const setRec = (recording) => {
      rec.replaceChildren(icon(recording ? 'stop' : 'mic'));
      rec.classList.toggle('recording', recording);
      rec.setAttribute('aria-pressed', String(recording));
      rec.setAttribute('aria-label', recording ? 'Arrêter l’enregistrement' : 'Parler');
      recHint.textContent = recording ? 'Je t’écoute… appuie pour arrêter' : 'Appuie et dis la phrase';
    };
    const recHint = h('p', { class: 'note' }, 'Appuie et dis la phrase');

    const rec = h(
      'button',
      {
        class: 'ibtn mic',
        'aria-label': 'Parler',
        'aria-pressed': 'false',
        onclick: async () => {
          try {
            if (!recorder) {
              recorder = await startRecording();
              setRec(true);
              return;
            }
            const blob = await recorder.stop();
            recorder = null;
            setRec(false);
            track('speak');
            if (recordingUrl) URL.revokeObjectURL(recordingUrl);
            recordingUrl = URL.createObjectURL(blob);
            replay.hidden = false;
            if (!voiceReady()) {
              result.replaceChildren(h('p', { class: 'note' }, 'Sans clé Azure, seule la réécoute est disponible.'));
              return;
            }
            result.replaceChildren(h('p', { class: 'note' }, 'Analyse en cours…'));
            const heard = await recognize(blob);
            const score = similarity(item.ar, heard);
            result.replaceChildren(
              h('p', { class: 'label' }, 'Ce que le service a compris'),
              h('p', { class: 'ar small', lang: 'ar', dir: 'rtl' }, heard || '—'),
              h('p', { class: `score ${score >= 80 ? 'ok' : score >= 50 ? 'mid' : 'low'}` }, heard ? `Ressemblance : ${score} %` : 'Rien n’a été reconnu. Réessayez plus près du micro.'),
              h('p', { class: 'note' }, 'Indication approximative : comparez surtout votre enregistrement au modèle.'),
            );
          } catch (e) {
            recorder = null;
            setRec(false);
            result.replaceChildren();
            toast(e.name === 'NotAllowedError' ? 'Le micro est bloqué. Autorise-le dans le navigateur, puis réessaie.' : e.message);
          }
        },
      },
      icon('mic'),
    );

    const go = (delta) => () => {
      index = (index + delta + items.length) % items.length;
      show();
    };

    box.replaceChildren(
      h('p', { class: 'count' }, `${index + 1} / ${items.length}`),
      h('div', { class: 'card big' }, phrase(item), listenButtons(item.ar), h('div', { class: 'mic-zone' }, rec, recHint, replay), result),
      h('div', { class: 'actions spread' }, h('button', { class: 'btn', onclick: go(-1) }, '← Précédente'), h('button', { class: 'btn', onclick: go(1) }, 'Suivante →')),
    );
  }

  show();
  return page('Prononciation', voiceBanner(), h('label', { class: 'field' }, 'Leçon', select), box);
}

// --- Dialogue ---

function viewDialogue(id) {
  const dialogue = DIALOGUES.find((d) => d.id === id);
  if (!dialogue) return viewHome();
  let playing = false;
  const rows = dialogue.lines.map((line) =>
    h('li', { class: `card line who-${line.who}` }, h('span', { class: 'who' }, line.who), h('div', {}, phrase(line), listenButtons(line.ar))),
  );
  const playAll = h(
    'button',
    {
      class: 'btn primary',
      onclick: async () => {
        if (playing) {
          playing = false;
          return;
        }
        playing = true;
        playAll.textContent = 'Arrêter';
        try {
          for (let i = 0; i < dialogue.lines.length && playing; i++) {
            rows.forEach((r, j) => r.classList.toggle('active', i === j));
            rows[i].scrollIntoView({ block: 'nearest', behavior: 'smooth' });
            await speak(dialogue.lines[i].ar);
          }
        } catch (e) {
          toast(e.message);
        }
        playing = false;
        rows.forEach((r) => r.classList.remove('active'));
        playAll.textContent = 'Écouter tout le dialogue';
      },
    },
    'Écouter tout le dialogue',
  );
  return page(dialogue.title, voiceBanner(), playAll, h('ul', { class: 'cards' }, rows));
}

// --- Guide de translittération ---

function viewGuide() {
  const rows = [
    ['2', 'ء / ق', 'Coup de glotte : petit arrêt net, comme entre « oh » et « oh » dans « oh-oh ». En libanais, la lettre ق se prononce presque toujours ainsi.'],
    ['3', 'ع', 'Son de gorge voisé, sans équivalent en français. À écouter et imiter.'],
    ['7', 'ح', 'Un « h » fort et soufflé, venant du fond de la gorge.'],
    ['kh', 'خ', 'Comme la « jota » espagnole ou le « ch » allemand de « Bach ».'],
    ['gh', 'غ', 'Proche du « r » français de « Paris ».'],
    ['sh', 'ش', 'Comme « ch » dans « chat ».'],
    ['r', 'ر', '« r » roulé, comme en espagnol ou en italien.'],
    ['h', 'ه', '« h » aspiré, comme en anglais dans « hello ».'],
    ['ee / oo', 'ي / و', 'Voyelles longues « i » et « ou ».'],
  ];
  return page(
    'Lire la translittération',
    h('p', {}, 'Les chiffres remplacent des sons arabes qui n’existent pas dans l’alphabet latin. C’est la façon dont les Libanais écrivent eux-mêmes dans leurs messages.'),
    h(
      'table',
      { class: 'guide' },
      h('thead', {}, h('tr', {}, h('th', {}, 'Écrit'), h('th', {}, 'Lettre'), h('th', {}, 'Prononciation'))),
      h('tbody', {}, rows.map(([a, b, c]) => h('tr', {}, h('td', {}, h('strong', {}, a)), h('td', { lang: 'ar', dir: 'rtl' }, b), h('td', {}, c)))),
    ),
  );
}

// --- Réglages ---

function viewSettings() {
  const s = state.settings;
  const key = h('input', { type: 'password', value: s.azureKey, autocomplete: 'off', placeholder: 'Clé de la ressource Speech' });
  const region = h('input', { type: 'text', value: s.azureRegion, autocomplete: 'off', placeholder: 'ex. francecentral, westeurope' });
  const voice = h('select', {}, VOICES.map((v) => h('option', { value: v.id, selected: v.id === s.voice }, v.label)));
  const slow = h('input', { type: 'checkbox', checked: s.slow });

  const apply = () => {
    s.azureKey = key.value.trim();
    s.azureRegion = region.value.trim().toLowerCase();
    s.voice = voice.value;
    s.slow = slow.checked;
    save();
  };

  return page(
    'Réglages',
    h('h2', {}, 'Voix libanaise (Azure AI Speech)'),
    h(
      'p',
      { class: 'note' },
      'La clé reste dans ce navigateur. Elle n’est envoyée qu’à Microsoft Azure, avec le texte à prononcer et vos enregistrements à analyser.',
    ),
    h('label', { class: 'field' }, 'Clé Azure', key),
    h('label', { class: 'field' }, 'Région', region),
    h('label', { class: 'field' }, 'Voix', voice),
    h('label', { class: 'field check' }, slow, 'Parler lentement par défaut'),
    h(
      'div',
      { class: 'actions' },
      h(
        'button',
        {
          class: 'btn primary',
          onclick: async () => {
            apply();
            try {
              await speak('مرحبا، كيفك؟');
              toast('Réglages enregistrés. La voix fonctionne.');
            } catch (e) {
              toast(e.message);
            }
          },
        },
        'Enregistrer et tester',
      ),
      h(
        'button',
        {
          class: 'btn',
          onclick: () => {
            key.value = '';
            apply();
            toast('Clé effacée de ce navigateur.');
          },
        },
        'Effacer la clé',
      ),
    ),
  );
}

// --- Routage ---

function render() {
  const [, view, param] = location.hash.split('/');
  const routes = {
    lecon: () => viewLesson(param),
    revision: viewReview,
    prononciation: viewPronunciation,
    dialogue: () => viewDialogue(param),
    guide: viewGuide,
    reglages: viewSettings,
  };
  root.replaceChildren(...(routes[view] || viewHome)().filter(Boolean));
  window.scrollTo(0, 0);
}

window.addEventListener('hashchange', render);
render();
