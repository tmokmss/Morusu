import { Beeper } from "./audio";
import { Keyer, Paddle } from "./keyer";
import { decode, encode, toMarks } from "./morse";
import { Quiz, TotalResult, decodeDictionary, parseDictionary } from "./quiz";

type Settings = { wpm: number; freq: number; volume: number; wave: OscillatorType; dahKey: string; invert: boolean; dict: string };
type Dictionary = { name: string; text: string };

const $ = <T extends HTMLElement>(id: string) => document.getElementById(id) as T;

function load<T>(key: string, fallback: T): T {
  try {
    const v = localStorage.getItem(key);
    return v ? { ...fallback, ...JSON.parse(v) } : fallback;
  } catch {
    return fallback;
  }
}

function save(key: string, value: unknown) {
  try {
    localStorage.setItem(key, JSON.stringify(value));
  } catch {}
}

const settings = load<Settings>("morusu.settings", {
  wpm: 25, freq: 500, volume: 30, wave: "sine", dahKey: "KeyA", invert: false, dict: "sample.dic",
});
const userDicts = load<{ list: Dictionary[] }>("morusu.dicts", { list: [] });
const history = load<{ list: TotalResult[] }>("morusu.history", { list: [] });

const beeper = new Beeper();
const keyer = new Keyer(beeper);
let quiz: Quiz;
let dicts: Dictionary[] = [];

const other = (p: Paddle): Paddle => (p === "dit" ? "dah" : "dit");
const keyLabel = (code: string) => code.replace(/^Key|^Digit/, "");

// ---- 描画

function renderQuestion() {
  const { original, alphabet } = quiz.question;
  $("original").textContent = original;
  $("alphabet").replaceChildren(
    ...[...alphabet].map((c, i) => {
      const span = document.createElement("span");
      span.textContent = c;
      if (i < quiz.typed.length) span.className = quiz.typed[i] === c ? "ok" : "ng";
      else if (i === quiz.typed.length) span.className = "current";
      return span;
    }),
  );
  $("teacher").replaceChildren(
    ...[...alphabet.slice(quiz.typed.length, quiz.typed.length + 2)].map((c) => {
      const div = document.createElement("div");
      div.textContent = `${c} : ${toMarks(encode(c) ?? "")}`;
      return div;
    }),
  );
  renderKeying();
}

function renderKeying() {
  const code = keyer.currentCode;
  $("typed").textContent = quiz.typed + (code ? decode(code) : "");
  $("marks").textContent = toMarks(code);
}

function renderResults() {
  $("results").replaceChildren(
    ...quiz.results.toReversed().map((r) => row([r.alphabet, r.typed, r.accuracy.toFixed(0), String(r.wpm)])),
  );
  $<HTMLButtonElement>("finish").disabled = quiz.results.length === 0;
}

function renderHistory() {
  $("history").replaceChildren(
    ...history.list.map((r) => row([r.name, r.accuracy.toFixed(0), r.wpm.toFixed(0), String(r.count), r.date])),
  );
  $("cwer-count").textContent = `${history.list.length} CWer`;
}

function row(cells: string[]) {
  const tr = document.createElement("tr");
  for (const c of cells) {
    const td = document.createElement("td");
    td.textContent = c;
    tr.append(td);
  }
  return tr;
}

// ---- 出題

function startQuiz() {
  const dict = dicts.find((d) => d.name === settings.dict) ?? dicts[0];
  quiz = new Quiz(parseDictionary(dict.text));
  keyer.reset();
  renderQuestion();
  renderResults();
}

keyer.onElement = () => {
  renderKeying();
  // Bluetoothなどで遅延が大きいと、キーヤーが次の符号を決める時点で前の音がまだ聞こえていない
  const ms = Math.round(beeper.latency * 1000);
  const warning = $("latency-warning");
  warning.hidden = ms <= 100;
  warning.textContent = `音声出力の遅延が${ms}msあります。符号の数がずれるので、内蔵スピーカーか有線イヤホンで遊んでください。`;
};
keyer.onLetter = (code) => {
  if (quiz.type(decode(code), settings.wpm)) renderResults();
  renderQuestion();
};

// ---- 入力

function press(paddle: Paddle) {
  pad.querySelector(`[data-paddle=${paddle}]`)!.classList.add("down");
  keyer.press(paddle);
}

function release(paddle: Paddle) {
  pad.querySelector(`[data-paddle=${paddle}]`)!.classList.remove("down");
  keyer.release(paddle);
}

const keyerKeys = new Map<string, Paddle>();
let capturingDahKey = false;

window.addEventListener("keydown", (e) => {
  if (capturingDahKey) {
    e.preventDefault();
    settings.dahKey = e.code;
    capturingDahKey = false;
    applySettings();
    return;
  }
  const t = e.target as HTMLElement;
  if (t.matches("input, select, textarea") || $<HTMLDialogElement>("result-dialog").open) return;
  if (e.ctrlKey || e.metaKey || e.altKey || /^(Tab|Escape|Shift|Control|Alt|Meta|F\d+)/.test(e.code + e.key)) return;
  e.preventDefault();
  if (e.repeat) return;
  const paddle: Paddle = (e.code === settings.dahKey) !== settings.invert ? "dah" : "dit";
  keyerKeys.set(e.code, paddle);
  press(paddle);
});

window.addEventListener("keyup", (e) => {
  const paddle = keyerKeys.get(e.code);
  if (!paddle) return;
  keyerKeys.delete(e.code);
  release(paddle);
});

// マウスは2つ目のボタンでpointerdownが来ないので、mousedownでボタンごとに扱う
const pad = $("pad");
const mouseHeld = new Map<number, Paddle>();
const touchHeld = new Map<number, Paddle>();
const padOf = (e: Event) => (e.target as HTMLElement).closest<HTMLElement>("[data-paddle]")?.dataset.paddle as Paddle | undefined;

pad.addEventListener("contextmenu", (e) => e.preventDefault());
pad.addEventListener("mousedown", (e) => {
  const half = padOf(e);
  if (!half || e.button > 2) return;
  e.preventDefault();
  const paddle = e.button === 2 ? other(half) : half;
  mouseHeld.set(e.button, paddle);
  press(paddle);
});
window.addEventListener("mouseup", (e) => {
  const paddle = mouseHeld.get(e.button);
  if (!paddle) return;
  mouseHeld.delete(e.button);
  release(paddle);
});

pad.addEventListener("pointerdown", (e) => {
  const half = padOf(e);
  if (e.pointerType === "mouse" || !half) return;
  e.preventDefault();
  touchHeld.set(e.pointerId, half);
  press(half);
});
const releaseTouch = (e: PointerEvent) => {
  const paddle = touchHeld.get(e.pointerId);
  if (!paddle) return;
  touchHeld.delete(e.pointerId);
  release(paddle);
};
window.addEventListener("pointerup", releaseTouch);
window.addEventListener("pointercancel", releaseTouch);

// ---- 設定

function applySettings() {
  keyer.wpm = settings.wpm;
  beeper.frequency = settings.freq;
  beeper.volume = settings.volume / 100;
  beeper.wave = settings.wave;
  $<HTMLInputElement>("wpm").value = String(settings.wpm);
  $("wpm-out").textContent = String(settings.wpm);
  $<HTMLInputElement>("freq").value = String(settings.freq);
  $("freq-out").textContent = String(settings.freq);
  $<HTMLInputElement>("volume").value = String(settings.volume);
  $<HTMLSelectElement>("wave").value = settings.wave;
  $<HTMLInputElement>("invert").checked = settings.invert;
  $("dah-key").textContent = capturingDahKey ? "キーを押してください…" : keyLabel(settings.dahKey);
  $("dah-key-hint").textContent = keyLabel(settings.dahKey);
  pad.classList.toggle("inverted", settings.invert);
  save("morusu.settings", settings);
}

const bindRange = (id: "wpm" | "freq" | "volume") =>
  $<HTMLInputElement>(id).addEventListener("input", (e) => {
    settings[id] = Number((e.target as HTMLInputElement).value);
    applySettings();
  });
bindRange("wpm");
bindRange("freq");
bindRange("volume");
$<HTMLSelectElement>("wave").addEventListener("change", (e) => {
  settings.wave = (e.target as HTMLSelectElement).value as OscillatorType;
  applySettings();
});
$<HTMLInputElement>("invert").addEventListener("change", (e) => {
  settings.invert = (e.target as HTMLInputElement).checked;
  applySettings();
});
$("dah-key").addEventListener("click", () => {
  capturingDahKey = true;
  applySettings();
});

function renderDictList() {
  $<HTMLSelectElement>("dict").replaceChildren(
    ...dicts.map((d) => new Option(d.name, d.name, false, d.name === settings.dict)),
  );
}

$<HTMLSelectElement>("dict").addEventListener("change", (e) => {
  settings.dict = (e.target as HTMLSelectElement).value;
  save("morusu.settings", settings);
  startQuiz();
});

$<HTMLInputElement>("dict-file").addEventListener("change", async (e) => {
  const input = e.target as HTMLInputElement;
  const file = input.files?.[0];
  input.value = "";
  if (!file) return;
  const text = decodeDictionary(await file.arrayBuffer());
  if (parseDictionary(text).length === 0) {
    alert("「表示用,打鍵用」の行が見つかりませんでした");
    return;
  }
  userDicts.list = [...userDicts.list.filter((d) => d.name !== file.name), { name: file.name, text }];
  save("morusu.dicts", userDicts);
  dicts = [dicts[0], ...userDicts.list];
  settings.dict = file.name;
  save("morusu.settings", settings);
  renderDictList();
  startQuiz();
});

// ---- 結果

const dialog = $<HTMLDialogElement>("result-dialog");
let total: ReturnType<Quiz["total"]>;

$("finish").addEventListener("click", () => {
  total = quiz.total();
  $("r-count").textContent = String(total.count);
  $("r-acc").textContent = total.accuracy.toFixed(2);
  $("r-wpm").textContent = total.wpm.toFixed(2);
  $("register").querySelector("button")!.disabled = false;
  renderHistory();
  dialog.showModal();
});

$("register").addEventListener("submit", (e) => {
  e.preventDefault();
  const name = $<HTMLInputElement>("name").value.trim();
  history.list.unshift({ ...total, name, date: new Date().toLocaleString("ja-JP") });
  save("morusu.history", history);
  renderHistory();
  $("register").querySelector("button")!.disabled = true;
});

dialog.addEventListener("close", startQuiz);

// ---- 起動

applySettings();
const sample = await fetch("./sample.dic").then((r) => r.text());
dicts = [{ name: "sample.dic", text: sample }, ...userDicts.list];
renderDictList();
startQuiz();
