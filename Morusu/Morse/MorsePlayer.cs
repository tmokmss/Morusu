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
        double memoryStartPosition = 1.0; // 0~1でメモリ開始位置を指定 1以上ならメモリ無効
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
            await Task.Run(() => be.EmitDit());

            morseCode.Dit();

            intervalTime = bufferDurationSeconds * (ditunit + spaceunit) * 1000;
            squeezeNext = Dah;
        }

        private async void BeepDah()
        {
            await Task.Run(() => be.EmitDah());

            morseCode.Dah();

            intervalTime = bufferDurationSeconds * (dahunit + spaceunit) * 1000;
            squeezeNext = Dit;
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
            while (isDitKeyDown || isDahKeyDown)
            {
                var letter = "";
                int justBeeped = 0;
                if (isDahFirstKeyDown && !isDitKeyDown) //それまで何も押されて無くて、初めてDahが押されたとき
                {
                    Console.WriteLine("Dah first pushed");
                    isDahFirstKeyDown = false;
                    if (intervalTime != bufferDurationSeconds * (ditunit + spaceunit) * 1000)
                        intervalTime = bufferDurationSeconds * (dahunit + spaceunit) * 1000;
                }

                else if (isDitFirstKeyDown && !isDahKeyDown)    //それまで何も押されて無くて、初めてDitが押されたとき
                {
                    Console.WriteLine("Dit first pushed");
                    isDitFirstKeyDown = false;
                    if (intervalTime != bufferDurationSeconds * (dahunit + spaceunit) * 1000)
                        intervalTime = bufferDurationSeconds * (ditunit + spaceunit) * 1000;
                }

                else if (sw.ElapsedMilliseconds > intervalTime) //音を出して良いタイミングに達している
                {
                    Console.WriteLine("Beep!");

                    if (isDahKeyDown && isDitKeyDown)   // 両キーが押されている→スクイズ
                    {
                        if (squeezeNext == Dit)  //スクイズでDitを出す
                        {
                            Console.WriteLine("DitKeyDownsqq");
                            if (morseCode.elapsedMilliseconds() > bufferDurationSeconds * 1000 * (morseCode.NetLengthFactor + letterspacefac * spaceunit))
                            {
                                morseCode.Reset();
                                OnSingleLetterFinished();
                            }
                            else
                            {
                                OnLetterCorrected();
                            }
                            BeepDit();
                            OnBeep(BeepType.SqueezeDit);
                            justBeeped = Dit;
                            letter = morseCode.CheckCode();

                            sw.Reset();
                            sw.Start();
                        }
                        else  //スクイズでDahを出す
                        {
                            Console.WriteLine("DahKeyDownsqq");
                            if (morseCode.elapsedMilliseconds() > bufferDurationSeconds * 1000 * (morseCode.NetLengthFactor + letterspacefac * spaceunit))
                            {
                                morseCode.Reset();
                                OnSingleLetterFinished();
                            }
                            else
                            {
                                OnLetterCorrected();
                            }
                            BeepDah();
                            OnBeep(BeepType.SqueezeDah);
                            justBeeped = Dah;
                            letter = morseCode.CheckCode();

                            sw.Reset();
                            sw.Start();
                        }
                    }

                    else if (isDahKeyDown)  //Dahのみ押されている
                    {
                        Console.WriteLine("DahKeyDown");
                        if (morseCode.elapsedMilliseconds() > bufferDurationSeconds * 1000 * (morseCode.NetLengthFactor + letterspacefac * spaceunit))
                        {
                            morseCode.Reset();
                            OnSingleLetterFinished();
                        }
                        else
                        {
                            OnLetterCorrected();
                        }
                        BeepDah();
                        OnBeep(BeepType.OnlyDah);
                        justBeeped = Dah;
                        letter = morseCode.CheckCode();

                        sw.Reset();
                        sw.Start();
                    }

                    else if (isDitKeyDown)  //Ditのみ押されている
                    {
                        Console.WriteLine("DitKeyDown");
                        if (morseCode.elapsedMilliseconds() > bufferDurationSeconds * 1000 * (morseCode.NetLengthFactor + letterspacefac * spaceunit))
                        {
                            morseCode.Reset();
                            OnSingleLetterFinished();
                        }
                        else
                        {
                            OnLetterCorrected();
                        }
                        BeepDit();
                        OnBeep(BeepType.OnlyDit);
                        justBeeped = Dit;
                        letter = morseCode.CheckCode();

                        sw.Reset();
                        sw.Start();
                    }
                    queue = 0;
                }

                else if (sw.ElapsedMilliseconds > intervalTime * memoryStartPosition)
                {
                    if (isDahKeyDown && isDitKeyDown)   // 両キーが押されている→スクイズ
                    {
                        queue = squeezeNext;
                    }
                    else if (isDahKeyDown)
                    {
                        queue = Dah;
                    }
                    else if (isDitKeyDown)
                    {
                        queue = Dit;
                    }
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
                    //Thread.Sleep(1);
                    if (sw.ElapsedMilliseconds > intervalTime)
                    {
                        Console.WriteLine("Memory worked");
                        if (temp == Dit)
                        {
                            Thread.Sleep(1);
                            OnKeyDown(Dit);
                            Thread.Sleep(1);
                            OnKeyUp(Dit);
                        }
                        else
                        {
                            Thread.Sleep(1);
                            OnKeyDown(Dah);
                            Thread.Sleep(1);
                            OnKeyUp(Dah);
                        }
                        break;
                    }
                }
            }
        }

    }
}
