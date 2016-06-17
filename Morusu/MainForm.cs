using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Morusu.Morse;
using Morusu.Quiz;

namespace Morusu
{
    public partial class MainForm : Form
    {
        // 見栄え用定数
        private static readonly int Dit = 1;
        private static readonly int Dah = 2;

        MorsePlayer mp;
        
        //押下キー判別に使うものたち
        MouseButtons mouseDahButton = MouseButtons.Left;
        Keys dahKey = Keys.A;

        // 問題出題用
        QuestionMaster qmaster;
        QuestionMarker qmarker;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var be = new DxBeemEmitter(this);
            mp = new MorsePlayer(be);
            mp.Beeped += OnBeeped;
            mp.LetterCorrected += OnLetterCorrected;
            mp.SingleLetterFinished += OnSingleLetterFinished;

            SetFrequencyAndWPM();
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
            qmarker.Wpm = mp.Wpm;
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
                var code = MorseCode.GetCode(next);
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

        #region CW用
        void SetFrequencyAndWPM()
        {
            var wpm = int.Parse(wpmBox.Text);
            var frequency = double.Parse(frequencyBox.Text);
            mp.SetFrequencyAndWPM(wpm, frequency);
            frequencyBox.Text = mp.Frequency.ToString();
        }

        void SetAmplitude()
        {
            mp.Amplitude = amplitudeSlider.Value;
        }

        private void AppendDitToBox()
        {
            ditDahBox.AppendText(" ・");
        }

        private void AppendDahToBox()
        {
            ditDahBox.AppendText(" ―");
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

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == dahKey ^ invertMouseCheck.Checked)
            {
                mp.OnKeyDown(Dah);
            }
            else
            {
                mp.OnKeyDown(Dit);
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == dahKey ^ invertMouseCheck.Checked)
            {
                mp.OnKeyUp(Dah);
            }
            else
            {
                mp.OnKeyUp(Dit);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            panel1.BackColor = Color.Gray;
            if (e.Button == mouseDahButton ^ invertMouseCheck.Checked)
            {
                mp.OnKeyUp(Dah);
            }

            else
            {
                mp.OnKeyUp(Dit);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            panel1.BackColor = Color.LightGray;
            if (e.Button == mouseDahButton ^ invertMouseCheck.Checked)
            {
                mp.OnKeyDown(Dah);
            }
            else
            {
                mp.OnKeyDown(Dit);
            }
        }

        private void OnBeeped(object sender, BeepEventArgs e)
        {
            var letter = mp.LetterNow;
            BeginInvoke((Action)(() => writeLetter(letter)));

            var beepType = e.Type;

            if (letter != "")
            {
                // 何らかのキーが押された場合
                qmarker.IsTypedkeyCorrect(letter);
            }
            if (beepType == BeepType.SqueezeDit || beepType == BeepType.OnlyDit)
            {
                // ditが鳴らされた場合
                ditDahBox.BeginInvoke((Action)(() => AppendDitToBox()));
            }
            else// if (beepType == Dah)
            {
                // dahが鳴らされた場合
                ditDahBox.BeginInvoke((Action)(() => AppendDahToBox()));
            }
        }

        private void OnSingleLetterFinished(object sender, EventArgs e)
        {
            // 1文字打ち終えた場合
            var isLastPosition = qmarker.ProgressNextPosition();
            BeginInvoke((Action)(()=>RefreshCurrentPosition()));

            if (isLastPosition == true)
            {
                // 問題1個打ち終えた場合
                SetNextQuestion();  // 後の処理のため、ここはこのスレッドでしっかり済ませる
                BeginInvoke((Action)(() => SetQuestionUI()));
                BeginInvoke((Action)(() => RefreshResultList()));
            }
        }
        
        private void OnLetterCorrected(object sender, EventArgs e)
        {
            BeginInvoke((Action)(() => deleteOneLetter()));
        }

        private void amplitudeSlider_Scroll(object sender, EventArgs e)
        {
            SetAmplitude();
        }

        private void frequencyBar_Scroll(object sender, EventArgs e)
        {
            frequencyBox.Text = frequencyBar.Value.ToString();
            SetFrequencyAndWPM();
        }

        private void waveShapeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            mp.WaveShape = waveShapeList.Text;
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
            SetFrequencyAndWPM();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(mp!= null)
            {
                mp.Dispose();
            }
        }

    }
}
