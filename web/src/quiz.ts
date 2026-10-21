export type Question = { original: string; alphabet: string };
export type QuestionResult = { alphabet: string; typed: string; accuracy: number; wpm: number };
export type TotalResult = { name: string; count: number; accuracy: number; wpm: number; date: string };

// 1行に「表示用,打鍵用」のカンマが1つだけある行を問題にする
export function parseDictionary(text: string): Question[] {
  return text
    .split(/\r?\n/)
    .map((line) => line.split(","))
    .filter((cols) => cols.length === 2 && cols[1].trim() !== "")
    .map(([original, alphabet]) => ({ original: original.trim(), alphabet: alphabet.trim().toUpperCase() }));
}

// .dicはShift_JISで配られてきたので、UTF-8として読めなければShift_JISで読む
export function decodeDictionary(buf: ArrayBuffer): string {
  try {
    return new TextDecoder("utf-8", { fatal: true }).decode(buf);
  } catch {
    return new TextDecoder("shift_jis").decode(buf);
  }
}

export class Quiz {
  question: Question;
  typed = "";
  results: QuestionResult[] = [];

  constructor(private questions: Question[]) {
    this.question = this.pick();
  }

  // 問題を1つ打ち終えたらtrue
  type(letter: string, wpm: number): boolean {
    this.typed += letter;
    if (this.typed.length < this.question.alphabet.length) return false;
    const { alphabet } = this.question;
    const correct = [...alphabet].filter((c, i) => this.typed[i] === c).length;
    this.results.push({ alphabet, typed: this.typed, accuracy: (correct / alphabet.length) * 100, wpm });
    this.question = this.pick();
    this.typed = "";
    return true;
  }

  total(): Omit<TotalResult, "name" | "date"> {
    const n = this.results.length;
    const avg = (f: (r: QuestionResult) => number) => this.results.reduce((s, r) => s + f(r), 0) / n;
    return { count: n, accuracy: avg((r) => r.accuracy), wpm: avg((r) => r.wpm) };
  }

  private pick(): Question {
    return this.questions[Math.floor(Math.random() * this.questions.length)];
  }
}
