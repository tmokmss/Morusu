using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Morusu.Morse
{
    class MorsePlayer : IDisposable
    {
        private static readonly int Dit = 1;
        private static readonly int Dah = 2;

        //波形の作成に使うものたち
        public int Wpm { private set; get; }
        public double Frequency { private set; get; }
        public int Amplitude { set { be.Amplitude = value; } get { return be.Amplitude; } }
        public string WaveShape { set { be.WaveShape = value; } get { return be.WaveShape; } }

        public string LetterNow { get { return morseCode.CheckCode(); } }

        IBeepEmitter be;

        Thread loopThread;
        Stopwatch sw = new Stopwatch();
        
        //押下キー判別に使うものたち
        bool isDahFirstKeyDown = true;
        bool isDahKeyDown = false;
        bool isDitFirstKeyDown = true;
        bool isDitKeyDown = false;

        //モールス符号生成に使うものたち
        MorseCode morseCode = new MorseCode();
        double bufferDurationSeconds;    // 短点の長さ 基本単位
        readonly double ditunit = 1.0;
        double dahunit = 3.0;
        double spaceunit = 1.0;
        double letterspacefac = 1.2; // これだけ余計に空いたら別文字と認識 普通は2だが感覚的には短くとったほうが良い
        double memoryStartPosition = 2.0; // 0~1でメモリ開始位置を指定 1以上ならメモリ無効
        double intervalTime;
        int squeezeNext;

        public delegate void BeepEventHandler(object sender, BeepEventArgs e);
        public event BeepEventHandler Beeped;
        public event EventHandler SingleLetterFinished;
        public event EventHandler LetterCorrected;

        public MorsePlayer(IBeepEmitter be)
        {
            this.be = be;
            loopThread = new Thread(new ThreadStart(Loop)); // null参照回避
            sw.Start();
        }

        public void SetFrequencyAndWPM(int wpm, double frequency)
        {
            Wpm = wpm;
            bufferDurationSeconds = 1.2 / wpm;
            int freqFac = (int)(frequency / bufferDurationSeconds);
            Frequency = bufferDurationSeconds * freqFac;
            be.DitLengthSecond = bufferDurationSeconds;
            be.Frequency = Frequency;
        }

        void MakeNewLoopThread()
        {
            loopThread = new Thread(new ThreadStart(Loop));
            loopThread.Name = "key watching Thread";
            loopThread.Start();
        }

        private async void BeepDit()
        {
            morseCode.Dit();
            intervalTime = bufferDurationSeconds * (ditunit + spaceunit) * 1000;
            squeezeNext = Dah;

            await Task.Run(()=>be.EmitDit());
        }

        private async void BeepDah()
        {
            morseCode.Dah();
            intervalTime = bufferDurationSeconds * (dahunit + spaceunit) * 1000;
            squeezeNext = Dit;

            await Task.Run(()=>be.EmitDah());
        }

        void OnSingleLetterFinished()
        {
            if (SingleLetterFinished != null)
            {
                SingleLetterFinished(this, new EventArgs());
            }
        }

        void OnLetterCorrected()
        {
            if (LetterCorrected != null)
            {
                LetterCorrected(this, new EventArgs());
            }
        }

        void OnBeep(BeepType type)
        {
            if (Beeped != null)
            {
                var e = new BeepEventArgs();
                e.Type = type;
                Beeped(this, e);
            }
        }

        public void OnKeyDown(int key)
        {
            if (key == Dah)
            {
                if (isDahFirstKeyDown)
                {
                    isDahKeyDown = true;
                    if (!loopThread.IsAlive)
                    {
                        MakeNewLoopThread();
                    }
                }
            }
            else // =Dit
            {
                if (isDitFirstKeyDown)
                {
                    isDitKeyDown = true;
                    if (!loopThread.IsAlive)
                    {
                        MakeNewLoopThread();
                    }
                }
            }
        }

        public void OnKeyUp(int key)
        {
            if (key == Dah)
            {
                isDahFirstKeyDown = true;
                isDahKeyDown = false;
            }
            else
            {
                isDitFirstKeyDown = true;
                isDitKeyDown = false;
            }
        }

        public void Dispose()
        {
            be.Dispose();
            loopThread.Abort();
        }

        private void Loop()
        {
            var queue = 0;
            //sw.Reset();sw.Start();intervalTime = 1000;
            while ((isDitKeyDown || isDahKeyDown))// || sw.ElapsedMilliseconds < bufferDurationSeconds*1200)
            {
                if (isDahFirstKeyDown && !isDitKeyDown) //それまで何も押されて無くて、初めてDahが押されたとき
                {
                    //Console.WriteLine("Dah first pushed");
                    isDahFirstKeyDown = false;
                    if (intervalTime != bufferDurationSeconds * (ditunit + spaceunit) * 1000)
                        intervalTime = bufferDurationSeconds * (dahunit + spaceunit) * 1000;
                }

                else if (isDitFirstKeyDown && !isDahKeyDown)    //それまで何も押されて無くて、初めてDitが押されたとき
                {
                    //Console.WriteLine("Dit first pushed");
                    isDitFirstKeyDown = false;
                    if (intervalTime != bufferDurationSeconds * (dahunit + spaceunit) * 1000)
                        intervalTime = bufferDurationSeconds * (ditunit + spaceunit) * 1000;
                }

                else if (sw.ElapsedMilliseconds > intervalTime) //音を出して良いタイミングに達している
                {
                    #region nominal beep process
                    var next = 0;
                    if (isDahKeyDown && isDitKeyDown)
                    {
                        //Console.Write("Squeeze");
                        next = squeezeNext;   // 両キーが押されている→スクイズ
                    }
                    else if (isDahKeyDown || isDitKeyDown)
                    {
                        //Console.Write("Normal");
                        next = (isDahKeyDown) ? Dah : Dit;
                    }
                    else
                    {
                        continue;
                    }

                    if (morseCode.elapsedMilliseconds() > bufferDurationSeconds * 1000 * (morseCode.NetLengthFactor + letterspacefac * spaceunit))
                    {
                        //Console.WriteLine("A letter finished!");
                        OnSingleLetterFinished();
                        morseCode.Reset();
                    }
                    else
                    {
                        OnLetterCorrected();
                    }

                    if (next == Dit)
                    {
                        //Console.WriteLine("DitBeep!");
                        BeepDit();
                        OnBeep(BeepType.SqueezeDit);
                    }
                    else if (next == Dah)
                    {
                        //Console.WriteLine("DahBeep!");
                        BeepDah();
                        OnBeep(BeepType.SqueezeDah);
                    }
                    sw.Reset();
                    sw.Start();
                    #endregion
                }

                else if (sw.ElapsedMilliseconds > intervalTime * memoryStartPosition)
                {
                    if (isDahKeyDown && isDitKeyDown)   // 両キーが押されている→スクイズ
                    {
                    Console.WriteLine("memory updated");
                        queue = squeezeNext;
                    }
                    /*
                    else if (isDahKeyDown)
                    {
                        queue = Dah;
                    }
                    else if (isDitKeyDown)
                    {
                        queue = Dit;
                    }
                    */
                }
            }
            // 要実装
            // http://a1club.net/faq/faq-25.htm
            if (queue != 0)
            {
                var temp = queue;
                queue = 0;
                while (true)
                {
                    Console.WriteLine("Memory worked");
                    if (sw.ElapsedMilliseconds > intervalTime)
                    {
                        if (temp == Dit)
                        {
                            BeepDit();
                            OnBeep(BeepType.SqueezeDit);
                            OnSingleLetterFinished();
                            morseCode.Reset();
                        }
                        else
                        {
                            BeepDah();
                            OnBeep(BeepType.SqueezeDah);
                            OnSingleLetterFinished();
                            morseCode.Reset();
                        }
                    }
                    break;
                }
            }
        }

    }
}
