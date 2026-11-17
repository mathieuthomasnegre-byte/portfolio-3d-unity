// Stockage local : tout reste dans le navigateur de l'appareil.

const KEY = 'libanais.v1';

const DEFAULTS = {
  settings: { azureKey: '', azureRegion: '', voice: 'ar-LB-LaylaNeural', slow: false },
  cards: {}, // id -> { box, due }
  days: {}, // 'AAAA-MM-JJ' -> { listen, speak, review }
};

function load() {
  try {
    const raw = JSON.parse(localStorage.getItem(KEY) || '{}');
    return {
      settings: { ...DEFAULTS.settings, ...(raw.settings || {}) },
      cards: raw.cards || {},
      days: raw.days || {},
    };
  } catch {
    return structuredClone(DEFAULTS);
  }
}

export const state = load();

export function save() {
  try {
    localStorage.setItem(KEY, JSON.stringify(state));
  } catch {
    // stockage indisponible (navigation privée) : l'appli continue sans mémoriser
  }
}

// --- Activité du jour et série de jours ---

export const GOALS = { listen: 5, speak: 3, review: 5 };

const dayKey = (d = new Date()) => d.toLocaleDateString('sv'); // AAAA-MM-JJ, heure locale

export function todayStats() {
  return { listen: 0, speak: 0, review: 0, ...(state.days[dayKey()] || {}) };
}

// kind : 'listen' | 'speak' | 'review'
export function track(kind) {
  const stats = todayStats();
  stats[kind]++;
  state.days[dayKey()] = stats;
  save();
}

// Nombre de jours consécutifs avec au moins une activité, jusqu'à aujourd'hui (ou hier).
export function streak() {
  const d = new Date();
  if (!state.days[dayKey(d)]) d.setDate(d.getDate() - 1);
  let n = 0;
  while (state.days[dayKey(d)]) {
    n++;
    d.setDate(d.getDate() - 1);
  }
  return n;
}

// --- Révision espacée (boîtes de Leitner) ---

const DAY = 24 * 60 * 60 * 1000;
const INTERVALS = [0, 1, 2, 4, 7, 15, 30, 60]; // jours, par boîte

export function addCards(ids) {
  let added = 0;
  for (const id of ids) {
    if (!state.cards[id]) {
      state.cards[id] = { box: 0, due: Date.now() };
      added++;
    }
  }
  save();
  return added;
}

export function dueCards(now = Date.now()) {
  return Object.entries(state.cards)
    .filter(([, c]) => c.due <= now)
    .sort((a, b) => a[1].due - b[1].due)
    .map(([id]) => id);
}

// grade : 'again' | 'good' | 'easy'
export function gradeCard(id, grade, now = Date.now()) {
  const card = state.cards[id];
  if (!card) return;
  if (grade === 'again') {
    card.box = 0;
    card.due = now + 10 * 60 * 1000;
  } else {
    card.box = Math.min(card.box + (grade === 'easy' ? 2 : 1), INTERVALS.length - 1);
    card.due = now + INTERVALS[card.box] * DAY;
  }
  save();
}
