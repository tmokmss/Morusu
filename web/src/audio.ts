export class Beeper {
  frequency = 500;
  volume = 0.3;
  wave: OscillatorType = "sine";
  private ctx?: AudioContext;

  // ブラウザはユーザー操作の中でしか音声を開始できないため、キー押下のたびに呼ぶ
  unlock() {
    this.ctx ??= new AudioContext();
    if (this.ctx.state === "suspended") void this.ctx.resume();
  }

  beep(seconds: number) {
    if (!this.ctx) return;
    const t = this.ctx.currentTime;
    // 立ち上がり・立ち下がりを数msかけてクリック音を防ぐ
    const ramp = Math.min(0.004, seconds / 4);
    const osc = this.ctx.createOscillator();
    osc.type = this.wave;
    osc.frequency.value = this.frequency;
    const gain = this.ctx.createGain();
    gain.gain.setValueAtTime(0, t);
    gain.gain.linearRampToValueAtTime(this.volume, t + ramp);
    gain.gain.setValueAtTime(this.volume, t + seconds - ramp);
    gain.gain.linearRampToValueAtTime(0, t + seconds);
    osc.connect(gain).connect(this.ctx.destination);
    osc.start(t);
    osc.stop(t + seconds + 0.01);
  }
}
