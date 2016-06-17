using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Microsoft.DirectX.DirectSound;
using System.Diagnostics;
using System.IO;

namespace Morusu
{
    class MorsePlayer : IDisposable
    {
        private static readonly int Dit = 1;
        private static readonly int Dah = 2;

        //波形の作成に使うものたち
        public int Wpm { set; get; }
        public double Frequency { set; get; }
        public int Amplitude { set; get; }
        public string WaveShape { set; get; }

        static Device device;
        Form parent;
        BufferDescription bufferDesc;
        WaveFormat waveFormat;
        SecondaryBuffer buffer;
        Thread loopThread;
        Stopwatch sw = new Stopwatch();
        
        //押下キー判別に使うものたち
        bool isDahFirstKeyDown = true;
        bool isDahKeyDown = false;
        bool isDitFirstKeyDown = true;
        bool isDitKeyDown = false;

        //モールス符号生成に使うものたち
        double bufferDurationSeconds;    // 短点の長さ 基本単位
        readonly double ditunit = 1.0;
        public static double dahunit = 3.0;
        public static double spaceunit = 1.0;
        double letterspacefac = 1.2; // これだけ余計に空いたら別文字と認識 普通は2だが感覚的には短くとったほうが良い
        double memoryStartPosition = 1.0; // 0~1でメモリ開始位置を指定 1以上ならメモリ無効
        MorseCode morseCode = new MorseCode();
        double intervalTime;    //1000はすぐに上書きされます
        int squeezeNext;

        public delegate void BeepEventHandler(object sender, BeepEventArgs e);
        public event BeepEventHandler Beeped;
        public event EventHandler SingleLetterFinished;
        public event EventHandler LetterCorrected;

        public MorsePlayer(Form parent)
        {
            this.parent = parent;
            if (device == null)
            {
                device = new Device();
                device.SetCooperativeLevel(parent, CooperativeLevel.Priority);
            }
            loopThread = new Thread(new ThreadStart(Loop)); // null参照回避
            sw.Start();
        }

        char[] GenerateOneUnitSound(double lengthFactor)
        {
            int numSamples = Convert.ToInt32(lengthFactor * bufferDurationSeconds *
                waveFormat.SamplesPerSecond * waveFormat.BlockAlign);
            char[] sampleData = new char[numSamples];
            double angle = (Math.PI * 2 * Frequency) /
                (waveFormat.SamplesPerSecond * waveFormat.Channels);

            switch (WaveShape)
            {
                default: //case "Square":
                    for (int i = 0; i < numSamples; i += 1)
                    {
                        if (Math.Sin(angle * i) >= 0)
                            sampleData[i] = (char)Amplitude;
                        else// if (Math.Sin(angle * i) < 0)
                            sampleData[i] = (char)(-Amplitude);
                    }
                    break;

                case "Sine":
                    for (int i = 0; i < numSamples; i += 1)
                    {
                        sampleData[i] = (char)(Amplitude * Math.Sin(angle * i));
                    }
                    break;

                case "Sawtooth":
                    {
                        int numOneWave = (int)(numSamples / (Frequency * lengthFactor * bufferDurationSeconds));
                        int cycle = 1;
                        int ii = 0;
                        while (ii < numSamples)
                        {
                            for (int jj = numOneWave * (cycle - 1); jj < numOneWave * cycle; jj++)
                            {
                                if (ii == numSamples) break;    //配列のインデックスが上限を超えてしまう場合、脱出
                                sampleData[ii] = (char)(Amplitude * (jj - numOneWave * (cycle - 1)) / numOneWave);
                                ii++;
                            }
                            cycle++;
                        }
                    }
                    break;

                case "Triangle":
                    {
                        int numOneWave = (int)(numSamples / (Frequency * lengthFactor * bufferDurationSeconds));
                        int cycle = 1;
                        int ii = 0;
                        bool isUp = true;   //傾きが正か負か
                        while (ii < numSamples)
                        {
                            for (int jj = numOneWave * (cycle - 1) / 2; jj < numOneWave * cycle / 2; jj++)
                            {
                                if (ii == numSamples) break;    //配列のインデックスが上限を超えてしまう場合、脱出
                                if (isUp)
                                    sampleData[ii] = (char)(Amplitude * (jj - numOneWave * (cycle - 1) / 2) / numOneWave / 2);
                                else
                                    sampleData[ii] = (char)(Amplitude * (1 - (jj - numOneWave * (cycle - 1) / 2) / numOneWave / 2));
                                ii++;
                            }
                            cycle++;
                            isUp = !isUp;
                        }
                    }
                    break;

            }

            return sampleData;
        }

        private void setBufferAndWave(double lengthFactor)
        {
            waveFormat = new Microsoft.DirectX.DirectSound.WaveFormat();
            waveFormat.SamplesPerSecond = 44140;
            waveFormat.Channels = 2;
            waveFormat.FormatTag = WaveFormatTag.Pcm;
            waveFormat.BitsPerSample = 16;
            waveFormat.BlockAlign = (short)(waveFormat.Channels * waveFormat.BitsPerSample / 8);
            waveFormat.AverageBytesPerSecond = waveFormat.BlockAlign * waveFormat.SamplesPerSecond;

            bufferDesc = new BufferDescription(waveFormat);
            bufferDesc.DeferLocation = true;
            bufferDesc.Control3D = false;
            bufferDesc.ControlEffects = false;
            bufferDesc.ControlFrequency = true;
            bufferDesc.ControlPan = true;
            bufferDesc.ControlVolume = true;
            bufferDesc.GlobalFocus = true;
            int dee = Convert.ToInt32(lengthFactor * bufferDurationSeconds *
                waveFormat.AverageBytesPerSecond);
            bufferDesc.BufferBytes = Convert.ToInt32(lengthFactor * bufferDurationSeconds *
                waveFormat.AverageBytesPerSecond);
        }

        void MakeNewLoopThread()
        {
            loopThread = new Thread(new ThreadStart(Loop));
            loopThread.Name = "key watching Thread";
            loopThread.Start();
        }

        private void BeepDit()
        {
            setBufferAndWave(ditunit);
            buffer = new SecondaryBuffer(bufferDesc, device);
            buffer.Write(0, GenerateOneUnitSound(ditunit), LockFlag.EntireBuffer);
            buffer.Volume = (int)Volume.Max;
            buffer.Play(0, BufferPlayFlags.Default);

            morseCode.Dit();

            intervalTime = bufferDurationSeconds * (ditunit + spaceunit) * 1000;
            squeezeNext = Dah;
        }

        private void BeepDah()
        {
            setBufferAndWave(dahunit);
            buffer = new SecondaryBuffer(bufferDesc, device);
            buffer.Write(0, GenerateOneUnitSound(dahunit), LockFlag.EntireBuffer);
            buffer.Volume = (int)Volume.Max;
            buffer.Play(0, BufferPlayFlags.Default);

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

        void OnSingleLetterFinished(EventArgs e)
        {
            if (SingleLetterFinished != null)
            {
                SingleLetterFinished(this, e);
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
            //存在すればセカンダリバッファ破棄
            if (buffer != null)
                buffer.Dispose();
            //デバイス破棄
            device.Dispose();
            //スレッド破棄
            loopThread.Abort();
        }

        private void Loop()
        {
            var queue = 0;
            while (isDitKeyDown || isDahKeyDown)
            {
                var letter = "";
                bool? isLastPosition = null;
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
                    if (buffer != null)
                    {
                        buffer.Dispose();   //メモリを開放する
                        buffer = null;
                    }

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
                /*
                if (isLastPosition == true)
                {
                    // 問題1個打ち終えた場合
                    SetNextQuestion();  // 後の処理のため、ここはこのスレッドでしっかり済ませる
                    textBox1.BeginInvoke(new noargDelegate(SetQuestionUI));
                    textBox1.BeginInvoke(new noargDelegate(RefreshResultList));
                }
                if (isLastPosition != null)
                {
                    // 1文字打ち終えた場合
                    textBox1.BeginInvoke(new noargDelegate(RefreshCurrentPosition));
                }
                if (letter != "")
                {
                    // 何らかのキーが押された場合
                    qmarker.IsTypedkeyCorrect(letter);
                }
                if (justBeeped == Dit)
                {
                    // ditが鳴らされた場合
                    ditDahBox.BeginInvoke(new noargDelegate(AppendDitToBox));
                }
                else if (justBeeped == Dah)
                {
                    // dahが鳴らされた場合
                    ditDahBox.BeginInvoke(new noargDelegate(AppendDahToBox));
                }
                */
            }
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
                            this.BeginInvoke((MethodInvoker)delegate()
                            {
                                Thread.Sleep(1);
                                OnKeyDown(Dit);
                                Thread.Sleep(1);
                                OnKeyUp(Dit);
                            });
                        }
                        else
                        {
                            this.BeginInvoke((MethodInvoker)delegate()
                            {
                                Thread.Sleep(1);
                                OnKeyDown(Dah);
                                Thread.Sleep(1);
                                OnKeyUp(Dah);
                            });
                        }
                        break;
                    }
                }
            }
        }

    }
}
