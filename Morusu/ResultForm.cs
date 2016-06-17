using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Morusu
{
    partial class ResultForm : Form
    {
        static readonly string fileName = "result_backup.xml";

        List<TotalResult> ResultHistory;
        TotalResult myresult;

        public ResultForm()
        {
            InitializeComponent();
            LoadResults();
            SetResultsList();
        }

        public void ShowResult(QuestionMarker qmarker)
        {
            var result = qmarker.GetTotalResult();
            countLabel.Text = string.Format("{0:N0}", result.Count);
            accLabel.Text = string.Format("{0:N2}", result.Accuracy);
            wpmLabel.Text = string.Format("{0:N2}", result.Wpm);

            myresult = result;
        }

        void SaveResults()
        {
            //XmlSerializerオブジェクトを作成
            //オブジェクトの型を指定する
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(typeof(List<TotalResult>));
            //書き込むファイルを開く（UTF-8 BOM無し）
            System.IO.StreamWriter sw = new System.IO.StreamWriter(
                fileName, false, new System.Text.UTF8Encoding(false));
            //シリアル化し、XMLファイルに保存する
            serializer.Serialize(sw, ResultHistory);
            //ファイルを閉じる
            sw.Close();
        }

        void LoadResults()
        {
            try
            {
                //XmlSerializerオブジェクトを作成
                System.Xml.Serialization.XmlSerializer serializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(List<TotalResult>));
                //読み込むファイルを開く
                System.IO.StreamReader sr = new System.IO.StreamReader(
                        fileName, new System.Text.UTF8Encoding(false));
                //XMLファイルから読み込み、逆シリアル化する
                ResultHistory = (List<TotalResult>)serializer.Deserialize(sr);
                //ファイルを閉じる
                sr.Close();
            }
            catch (Exception)
            {
                ResultHistory = new List<TotalResult>();
            }
        }

        private void registerButton_Click(object sender, EventArgs e)
        {
            myresult.Name = nameBox.Text;
            myresult.Date = DateTime.Now.ToString();
            ResultHistory.Insert(0, myresult);
            SaveResults();
            SetResultsList();
            registerButton.Enabled = false;
        }

        void SetResultsList()
        {
            resultListBox.Items.Clear();
            foreach (var result in ResultHistory)
            {
                resultListBox.Items.Add(result.ToString());
            }
            cwerNumLabel.Text = string.Format("{0} CWer", ResultHistory.Count);
        }
        
    }
}
