using System;
using System.Windows.Forms;

namespace Morusu
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.wpmBox = new System.Windows.Forms.TextBox();
            this.frequencyBox = new System.Windows.Forms.TextBox();
            this.amplitudeSlider = new System.Windows.Forms.TrackBar();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.frequencyBar = new System.Windows.Forms.TrackBar();
            this.wpmBar = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.waveShapeList = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.dahKeyButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.questionText = new System.Windows.Forms.Label();
            this.resultListBox = new System.Windows.Forms.ListBox();
            this.headerLabel = new System.Windows.Forms.Label();
            this.questionTextBox = new System.Windows.Forms.RichTextBox();
            this.invertMouseCheck = new System.Windows.Forms.CheckBox();
            this.exitButton = new System.Windows.Forms.Button();
            this.ditDahBox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dictList = new System.Windows.Forms.ComboBox();
            this.teachingLabel = new System.Windows.Forms.Label();
            this.refreshCheck = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.amplitudeSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frequencyBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wpmBar)).BeginInit();
            this.SuspendLayout();
            // 
            // wpmBox
            // 
            this.wpmBox.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.wpmBox.Location = new System.Drawing.Point(6, 27);
            this.wpmBox.Name = "wpmBox";
            this.wpmBox.Size = new System.Drawing.Size(46, 23);
            this.wpmBox.TabIndex = 0;
            this.wpmBox.Text = "25";
            this.wpmBox.TextChanged += new System.EventHandler(this.wpmBox_TextChanged);
            // 
            // frequencyBox
            // 
            this.frequencyBox.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.frequencyBox.Location = new System.Drawing.Point(6, 90);
            this.frequencyBox.Name = "frequencyBox";
            this.frequencyBox.Size = new System.Drawing.Size(46, 23);
            this.frequencyBox.TabIndex = 1;
            this.frequencyBox.Text = "500";
            // 
            // amplitudeSlider
            // 
            this.amplitudeSlider.LargeChange = 100;
            this.amplitudeSlider.Location = new System.Drawing.Point(58, 153);
            this.amplitudeSlider.Maximum = 10000;
            this.amplitudeSlider.Name = "amplitudeSlider";
            this.amplitudeSlider.Size = new System.Drawing.Size(104, 45);
            this.amplitudeSlider.SmallChange = 10;
            this.amplitudeSlider.TabIndex = 2;
            this.amplitudeSlider.TickFrequency = 1000;
            this.amplitudeSlider.Value = 2000;
            this.amplitudeSlider.Scroll += new System.EventHandler(this.amplitudeSlider_Scroll);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("Meiryo UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.textBox1.Location = new System.Drawing.Point(173, 90);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(357, 142);
            this.textBox1.TabIndex = 0;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            this.textBox1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyUp);
            // 
            // frequencyBar
            // 
            this.frequencyBar.LargeChange = 100;
            this.frequencyBar.Location = new System.Drawing.Point(57, 90);
            this.frequencyBar.Maximum = 5000;
            this.frequencyBar.Minimum = 20;
            this.frequencyBar.Name = "frequencyBar";
            this.frequencyBar.Size = new System.Drawing.Size(105, 45);
            this.frequencyBar.SmallChange = 10;
            this.frequencyBar.TabIndex = 7;
            this.frequencyBar.TickFrequency = 1000;
            this.frequencyBar.Value = 500;
            this.frequencyBar.Scroll += new System.EventHandler(this.frequencyBar_Scroll);
            // 
            // wpmBar
            // 
            this.wpmBar.Location = new System.Drawing.Point(58, 27);
            this.wpmBar.Maximum = 60;
            this.wpmBar.Minimum = 1;
            this.wpmBar.Name = "wpmBar";
            this.wpmBar.Size = new System.Drawing.Size(104, 45);
            this.wpmBar.TabIndex = 8;
            this.wpmBar.TickFrequency = 5;
            this.wpmBar.Value = 25;
            this.wpmBar.ValueChanged += new System.EventHandler(this.wpmBar_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(2, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 15);
            this.label1.TabIndex = 9;
            this.label1.Text = "WPM";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.Location = new System.Drawing.Point(5, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 15);
            this.label2.TabIndex = 10;
            this.label2.Text = "Frequency";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label3.Location = new System.Drawing.Point(3, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 15);
            this.label3.TabIndex = 11;
            this.label3.Text = "Amplitude";
            // 
            // waveShapeList
            // 
            this.waveShapeList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.waveShapeList.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.waveShapeList.FormattingEnabled = true;
            this.waveShapeList.Items.AddRange(new object[] {
            "Sine",
            "Square",
            "Triangle",
            "Sawtooth"});
            this.waveShapeList.Location = new System.Drawing.Point(58, 201);
            this.waveShapeList.Name = "waveShapeList";
            this.waveShapeList.Size = new System.Drawing.Size(104, 23);
            this.waveShapeList.TabIndex = 12;
            this.waveShapeList.SelectedIndexChanged += new System.EventHandler(this.waveShapeList_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label4.Location = new System.Drawing.Point(4, 204);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 15);
            this.label4.TabIndex = 13;
            this.label4.Text = "Wave";
            // 
            // dahKeyButton
            // 
            this.dahKeyButton.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.dahKeyButton.Location = new System.Drawing.Point(57, 234);
            this.dahKeyButton.Name = "dahKeyButton";
            this.dahKeyButton.Size = new System.Drawing.Size(104, 23);
            this.dahKeyButton.TabIndex = 14;
            this.dahKeyButton.Text = "A";
            this.dahKeyButton.UseVisualStyleBackColor = true;
            this.dahKeyButton.Click += new System.EventHandler(this.dahKeyButton_Click);
            this.dahKeyButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dahKeyButton_KeyDown);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label5.Location = new System.Drawing.Point(5, 237);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 15);
            this.label5.TabIndex = 15;
            this.label5.Text = "Dah";
            // 
            // questionText
            // 
            this.questionText.AutoSize = true;
            this.questionText.Font = new System.Drawing.Font("Meiryo UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.questionText.Location = new System.Drawing.Point(173, 20);
            this.questionText.Name = "questionText";
            this.questionText.Size = new System.Drawing.Size(113, 30);
            this.questionText.TabIndex = 16;
            this.questionText.Text = "question";
            // 
            // resultListBox
            // 
            this.resultListBox.Font = new System.Drawing.Font("ＭＳ ゴシック", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.resultListBox.FormattingEnabled = true;
            this.resultListBox.ItemHeight = 15;
            this.resultListBox.Location = new System.Drawing.Point(5, 307);
            this.resultListBox.Name = "resultListBox";
            this.resultListBox.Size = new System.Drawing.Size(632, 199);
            this.resultListBox.TabIndex = 18;
            // 
            // headerLabel
            // 
            this.headerLabel.AutoSize = true;
            this.headerLabel.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.headerLabel.Location = new System.Drawing.Point(5, 289);
            this.headerLabel.Name = "headerLabel";
            this.headerLabel.Size = new System.Drawing.Size(544, 15);
            this.headerLabel.TabIndex = 19;
            this.headerLabel.Text = "Question                                     Ur Keying                           " +
    "                Accuracy       wpm";
            // 
            // questionTextBox
            // 
            this.questionTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.questionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.questionTextBox.Font = new System.Drawing.Font("Meiryo UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.questionTextBox.Location = new System.Drawing.Point(178, 60);
            this.questionTextBox.Multiline = false;
            this.questionTextBox.Name = "questionTextBox";
            this.questionTextBox.ReadOnly = true;
            this.questionTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.questionTextBox.Size = new System.Drawing.Size(324, 23);
            this.questionTextBox.TabIndex = 20;
            this.questionTextBox.Text = "question";
            // 
            // invertMouseCheck
            // 
            this.invertMouseCheck.AutoSize = true;
            this.invertMouseCheck.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.invertMouseCheck.Location = new System.Drawing.Point(99, 263);
            this.invertMouseCheck.Name = "invertMouseCheck";
            this.invertMouseCheck.Size = new System.Drawing.Size(62, 19);
            this.invertMouseCheck.TabIndex = 21;
            this.invertMouseCheck.Text = "Invert";
            this.invertMouseCheck.UseVisualStyleBackColor = true;
            // 
            // exitButton
            // 
            this.exitButton.Font = new System.Drawing.Font("Meiryo UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.exitButton.Location = new System.Drawing.Point(536, 237);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(101, 36);
            this.exitButton.TabIndex = 22;
            this.exitButton.Text = "結果";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // ditDahBox
            // 
            this.ditDahBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ditDahBox.Font = new System.Drawing.Font("ＭＳ ゴシック", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.ditDahBox.Location = new System.Drawing.Point(178, 238);
            this.ditDahBox.Name = "ditDahBox";
            this.ditDahBox.ReadOnly = true;
            this.ditDahBox.Size = new System.Drawing.Size(352, 34);
            this.ditDahBox.TabIndex = 23;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Gray;
            this.panel1.Location = new System.Drawing.Point(536, 90);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(101, 141);
            this.panel1.TabIndex = 24;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseUp);
            // 
            // dictList
            // 
            this.dictList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dictList.FormattingEnabled = true;
            this.dictList.Location = new System.Drawing.Point(480, 3);
            this.dictList.Name = "dictList";
            this.dictList.Size = new System.Drawing.Size(157, 20);
            this.dictList.TabIndex = 25;
            this.dictList.SelectedIndexChanged += new System.EventHandler(this.dictList_SelectedIndexChanged);
            // 
            // teachingLabel
            // 
            this.teachingLabel.AutoSize = true;
            this.teachingLabel.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.teachingLabel.Location = new System.Drawing.Point(509, 31);
            this.teachingLabel.Name = "teachingLabel";
            this.teachingLabel.Size = new System.Drawing.Size(98, 19);
            this.teachingLabel.TabIndex = 0;
            this.teachingLabel.Text = "morse is fun";
            // 
            // refreshCheck
            // 
            this.refreshCheck.AutoSize = true;
            this.refreshCheck.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.refreshCheck.Location = new System.Drawing.Point(7, 263);
            this.refreshCheck.Name = "refreshCheck";
            this.refreshCheck.Size = new System.Drawing.Size(70, 19);
            this.refreshCheck.TabIndex = 26;
            this.refreshCheck.Text = "Refresh";
            this.refreshCheck.UseVisualStyleBackColor = true;
            this.refreshCheck.CheckedChanged += new System.EventHandler(this.refreshCheck_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label6.Location = new System.Drawing.Point(175, 4);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(150, 15);
            this.label6.TabIndex = 27;
            this.label6.Text = "問題文に合わせて打ってみよう";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label7.Location = new System.Drawing.Point(450, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 15);
            this.label7.TabIndex = 28;
            this.label7.Text = "次の文字";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(649, 518);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.refreshCheck);
            this.Controls.Add(this.teachingLabel);
            this.Controls.Add(this.dictList);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.ditDahBox);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.invertMouseCheck);
            this.Controls.Add(this.questionTextBox);
            this.Controls.Add(this.headerLabel);
            this.Controls.Add(this.resultListBox);
            this.Controls.Add(this.questionText);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.dahKeyButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.waveShapeList);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.wpmBar);
            this.Controls.Add(this.frequencyBar);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.amplitudeSlider);
            this.Controls.Add(this.frequencyBox);
            this.Controls.Add(this.wpmBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 305);
            this.Name = "MainForm";
            this.Text = "Morusu";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.amplitudeSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frequencyBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wpmBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox wpmBox;
        private System.Windows.Forms.TextBox frequencyBox;
        private System.Windows.Forms.TrackBar amplitudeSlider;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TrackBar frequencyBar;
        private System.Windows.Forms.TrackBar wpmBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox waveShapeList;
        private System.Windows.Forms.Button dahKeyButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label questionText;
        private System.Windows.Forms.ListBox resultListBox;
        private System.Windows.Forms.Label headerLabel;
        private System.Windows.Forms.RichTextBox questionTextBox;
        private System.Windows.Forms.CheckBox invertMouseCheck;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.TextBox ditDahBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox dictList;
        private System.Windows.Forms.Label teachingLabel;
        private System.Windows.Forms.CheckBox refreshCheck;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
    }
}

