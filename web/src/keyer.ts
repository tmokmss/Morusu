import { Beeper } from "./audio";

export type Paddle = "dit" | "dah";

const units: Record<Paddle, number> = { dit: 1, dah: 3 };
// 符号間の1短点に加えてこれだけ空いたら文字の区切りとみなす（本来は2だが、短い方が打ちやすい）
const letterGap = 1.2;

// 両パドルを押すと短点・長点を交互に出すエレクトリックキーヤー（スクイズ）
// 符号の途中で反対側を押した場合は、離していても次に1つ出す（メモリ）
export class Keyer {
  wpm = 25;
  onElement: (paddle: Paddle) => void = () => {};
  onLetter: (code: string) => void = () => {};

  private down: Record<Paddle, boolean> = { dit: false, dah: false };
  private playing: Paddle | null = null;
  private last: Paddle | null = null;
  private memory: Paddle | null = null;
  private code = "";
  private gapTimer?: number;

  constructor(private beeper: Beeper) {}

  get ditMs() {
    return 1200 / this.wpm;
  }

  get currentCode() {
    return this.code;
  }

  press(paddle: Paddle) {
    this.beeper.unlock();
    if (this.down[paddle]) return;
    this.down[paddle] = true;
    if (this.playing === null) this.next();
    else if (this.playing !== paddle) this.memory = paddle;
  }

  release(paddle: Paddle) {
    this.down[paddle] = false;
  }

  reset() {
    clearTimeout(this.gapTimer);
    this.code = "";
  }

  private next() {
    let paddle: Paddle | null = this.memory;
    this.memory = null;
    if (!paddle) {
      if (this.down.dit && this.down.dah) paddle = this.last === "dit" ? "dah" : "dit";
      else if (this.down.dit) paddle = "dit";
      else if (this.down.dah) paddle = "dah";
    }
    this.playing = paddle;
    if (!paddle) {
      this.gapTimer = window.setTimeout(() => this.finishLetter(), this.ditMs * letterGap);
      return;
    }
    clearTimeout(this.gapTimer);
    this.last = paddle;
    this.code += paddle === "dah" ? "0" : "1";
    this.beeper.beep((this.ditMs * units[paddle]) / 1000);
    this.onElement(paddle);
    window.setTimeout(() => this.next(), this.ditMs * (units[paddle] + 1));
  }

  private finishLetter() {
    const code = this.code;
    this.code = "";
    if (code) this.onLetter(code);
  }
}
