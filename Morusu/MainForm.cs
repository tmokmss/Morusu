using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX.DirectSound;

namespace Morusu
{
    public partial class MainForm : Form
    {
        // 見栄え用定数
        private static readonly int Dit = 1;
        private static readonly int Dah = 2;

        //波形の作成に使うものたち
        static Device device;
        int Wpm { set; get; }
        double Frequency { set; get; }
        int Amplitude { set; get; }
        string WaveShape { set; get; }
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
        MouseButtons mouseDahButton = MouseButtons.Left;
        Keys dahKey = Keys.A;

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

        //音再生スレッド用からtextBoxにアクセスするデリゲート
        delegate void writingDelegate(string text);
        delegate void noargDelegate();

        // 問題出題用
        QuestionMaster qmaster;
        QuestionMarker qmarker;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (device == null)
            {
                device = new Device();
                device.SetCooperativeLevel(this, CooperativeLevel.Priority);
            }
            loopThread = new Thread(new ThreadStart(Loop)); // null参照回避
            sw.Start();

            SetFrequency();
            SetAmplitude();
            LoadDictList();

            waveShapeList.SelectedIndex = 0;
            
            InitializePlayMode();
        }

        void LoadDictList()
        {
            dictList.Items.Clear();
            string[] files = Directory.GetFiles(@"dict\", "*.dic");
            foreach (var file in files)
            {
                dictList.Items.Add(Path.GetFileName(file));
            }
            dictList.SelectedIndex = 0;
        }

        void InitializePlayMode() 
        {
            qmaster = null;
            qmarker = null;
            ReadQuestionFile();
            SetNextQuestion();
            SetQuestionUI();
            RefreshResultList();
        }

        void ReadQuestionFile()
        {
            if (qmaster == null)
            {
                qmaster = new QuestionMaster();
            }

            var dictpath = @"dict\" + dictList.SelectedItem.ToString() ;
            qmaster.SetQuestion(dictpath);
        }

        void SetQuestionUI()
        {
            var question = qmarker.QCurrent;
            questionText.Text = question.Original;
            questionTextBox.Text = question.Alphabet;

            var length = questionTextBox.Text.Length;
            questionTextBox.Select(0, length);
            questionTextBox.SelectionColor = Color.Black;
        }
        
        void SetNextQuestion()
        {
            var nextq = qmaster.GetNextQuestion();
            if (qmarker == null)
            {
                qmarker = new QuestionMarker();
            }
            qmarker.SetNextQuestion(nextq);
            qmarker.Wpm = Wpm;
        }

        void RefreshResultList()
        {
            var resultList = qmarker.ResultToList();
            resultListBox.Items.Clear();
            for (var i = 0; i < resultList.Length; i++)
            {
                resultListBox.Items.Add(resultList[i]);
            }
            resultListBox.SelectedIndex = resultList.Length - 1;

            if (refreshCheck.Checked)
                textBox1.Text = "";
            ditDahBox.Text = "";
        }
        　
        void RefreshCurrentPosition()
        {
            var position = qmarker.Position;
            ChangeQuestionColorAt(position, Color.Red);

            ditDahBox.Text = "";
            RefreshTeacher();
        }

        void RefreshTeacher()
        {
            teachingLabel.Text = "";
            var sb = new StringBuilder();
            var nexts = qmarker.GetNextNLetter(2);
            foreach (var next in nexts)
            {
                if (next == "")
                    continue;
                sb.Append(next);
                sb.Append(" : ");
                var code = morseCode.GetCode(next);
                foreach (char dh in code)
                {
                    var dhstr = dh.ToString();
                    if (dhstr.Equals("0"))
                    {
                        sb.Append(" ―");
                    }
                    else
                    {
                        sb.Append(" ・");
                    }
                }
                sb.Append("\r\n");
            }
            teachingLabel.Text = sb.ToString();
        }

        void ChangeQuestionColorAt(int idx, Color color)
        {
            var length = questionTextBox.Text.Length;

            if (idx < 0)
            {
                questionTextBox.Select(0, 1);
            }
            else if (idx >= length)
            {
                questionTextBox.Select(length - 1, 1);
            }
            else
            {
                questionTextBox.Select(idx, 1);
                questionTextBox.SelectionColor = color;
            }
            questionTextBox.ScrollToCaret();
        }

        #region CW用　後で隔離せよ
        // 要実装
        // http://a1club.net/faq/faq-25.htm
        void SetWPM()
        {
            Wpm = int.Parse(wpmBox.Text);
            bufferDurationSeconds = 1.2 / Wpm;
            int freqFac = (int)(Frequency / bufferDurationSeconds);
            Frequency = bufferDurationSeconds * freqFac;
            frequencyBox.Text = Frequency.ToString();
        }

        void SetFrequency()
        {
            Frequency = int.Parse(frequencyBox.Text);
            SetWPM();
        }

        void SetAmplitude()
        {
            Amplitude = amplitudeSlider.Value;
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
                    for (int i = 0; i < numSamples; i+=1)
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
                                if(isUp)
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

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //存在すればセカンダリバッファ破棄
            if (buffer != null)
                buffer.Dispose();
            //デバイス破棄
            device.Dispose();
            //スレッド破棄
            loopThread.Abort();
        }

        private void AppendDitToBox()
        {
            ditDahBox.AppendText(" ・");
        }

        private void AppendDahToBox()
        {
            ditDahBox.AppendText(" ―");
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

            intervalTime = bufferDurationSeconds * (dahunit+spaceunit) * 1000;
            squeezeNext = Dit;
        }

        private void deleteOneLetter()
        {
            string content = textBox1.Text;
            int length = content.Length;
            if (length <= 1) content = "";
            else content = content.Substring(0, length - 1);
            textBox1.Text = content;
        }

        private void writeLetter(string addingLetter)
        {
            textBox1.AppendText(addingLetter);
        }

        void MakeNewLoopThread()
        {
            loopThread = new Thread(new ThreadStart(Loop));
            loopThread.Name = "key watching Thread";
            loopThread.Start();
        }

        void OnKeyDown(int key)
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

        void OnKeyUp(int key)
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

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == dahKey ^ invertMouseCheck.Checked)
            {
                OnKeyDown(Dah);
            }
            else
            {
                OnKeyDown(Dit);
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == dahKey ^ invertMouseCheck.Checked)
            {
                OnKeyUp(Dah);
            }
            else
            {
                OnKeyUp(Dit);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            panel1.BackColor = Color.Gray;
            if (e.Button == mouseDahButton ^ invertMouseCheck.Checked)
            {
                OnKeyUp(Dah);
            }

            else
            {
                OnKeyUp(Dit);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            panel1.BackColor = Color.LightGray;
            if (e.Button == mouseDahButton ^ invertMouseCheck.Checked)
            {
                OnKeyDown(Dah);
            }
            else
            {
                OnKeyDown(Dit);
            }
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
                    if (intervalTime != bufferDurationSeconds * (ditunit+spaceunit) * 1000)
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
                                isLastPosition = qmarker.ProgressNextPosition();
                            }
                            else
                            {
                                textBox1.BeginInvoke(new noargDelegate(deleteOneLetter));
                            }
                            BeepDit();
                            justBeeped = Dit;
                            letter = morseCode.CheckCode();
                            textBox1.BeginInvoke(new writingDelegate(writeLetter), new object[] { letter });

                            sw.Reset();
                            sw.Start();
                        }
                        else  //スクイズでDahを出す
                        {
                            Console.WriteLine("DahKeyDownsqq");
                            if (morseCode.elapsedMilliseconds() > bufferDurationSeconds * 1000 * (morseCode.NetLengthFactor + letterspacefac * spaceunit))
                            {
                                morseCode.Reset();
                                isLastPosition = qmarker.ProgressNextPosition();
                            }
                            else
                            {
                                textBox1.BeginInvoke(new noargDelegate(deleteOneLetter));
                            }
                            BeepDah();
                            justBeeped = Dah;
                            letter = morseCode.CheckCode();
                            textBox1.BeginInvoke(new writingDelegate(writeLetter), new object[] { letter });

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
                            isLastPosition = qmarker.ProgressNextPosition();
                        }
                        else
                        {
                            textBox1.BeginInvoke(new noargDelegate(deleteOneLetter));
                        }
                        BeepDah();
                        justBeeped = Dah;
                        letter = morseCode.CheckCode();
                        textBox1.BeginInvoke(new writingDelegate(writeLetter), new object[] { letter });

                        sw.Reset();
                        sw.Start();
                    }

                    else if (isDitKeyDown)  //Ditのみ押されている
                    {
                        Console.WriteLine("DitKeyDown");
                        if (morseCode.elapsedMilliseconds() > bufferDurationSeconds * 1000 * (morseCode.NetLengthFactor + letterspacefac * spaceunit))
                        {
                            morseCode.Reset();
                            isLastPosition = qmarker.ProgressNextPosition();
                        }
                        else
                        {
                            textBox1.BeginInvoke(new noargDelegate(deleteOneLetter));
                        }
                        BeepDit();
                        justBeeped = Dit;
                        letter = morseCode.CheckCode();
                        textBox1.BeginInvoke(new writingDelegate(writeLetter), new object[] { letter });

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
                            this.BeginInvoke((MethodInvoker)delegate ()
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

        private void amplitudeSlider_Scroll(object sender, EventArgs e)
        {
            SetAmplitude();
        }

        private void frequencyBar_Scroll(object sender, EventArgs e)
        {
            frequencyBox.Text = frequencyBar.Value.ToString();
            SetFrequency();
        }

        private void waveShapeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            WaveShape = waveShapeList.Text;
        }

        private void dahKeyButton_Click(object sender, EventArgs e)
        {
            dahKeyButton.Text = "Press key...";
        }

        private void dahKeyButton_KeyDown(object sender, KeyEventArgs e)
        {
            dahKey = e.KeyCode;
            dahKeyButton.Text = dahKey.ToString();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            if (resultListBox.Items.Count > 0)
            {
                var resultForm = new ResultForm();
                resultForm.ShowResult(qmarker);
                resultForm.Owner = this;
                resultForm.Show();
                resultForm.FormClosed += new FormClosedEventHandler(resultForm_FormClosed);
            }
            else
            {
                InitializePlayMode();
            }
        }

        private void resultForm_FormClosed(object sender, EventArgs e)
        {
            InitializePlayMode();
        }


        #endregion


        private void dictList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReadQuestionFile();
            SetNextQuestion();
            SetQuestionUI();
        }

        private void refreshCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (refreshCheck.Checked)
                textBox1.Text = "";
        }

        private void wpmBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                wpmBar.Value = Convert.ToInt32(wpmBox.Text);
            }
            catch
            {
                //wpmBox.Text = "25";
            }
        }

        private void wpmBar_ValueChanged(object sender, EventArgs e)
        {
            wpmBox.Text = wpmBar.Value.ToString();
            SetWPM();
        }

    }
}
