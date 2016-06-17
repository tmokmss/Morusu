using System;
using System.Collections.Generic;
using System.Text;

namespace Morusu.Quiz
{
    class QuestionMarker
    {
        public Question QCurrent { private set; get; }
        public int Wpm { set; get; }
        List<QuestionResult> resultList;
        StringBuilder typed;
        public int Position { private set; get; }
        int correctCount;
        string lastTypedKey;
        
        public QuestionMarker()
        {
            resultList = new List<QuestionResult>();
            Initialize();
            Wpm = 0;
        }

        void Initialize()
        {
            Position = -1;
            correctCount = 0;
            typed = new StringBuilder();
        }

        public void SetNextQuestion(Question q)
        {
            if (QCurrent != null)
            {
                AddToResult();
            }
            Initialize();
            QCurrent = q;
        }
        
        public bool ProgressNextPosition()
        {
            if (Position >= 0)
            {
                typed.Append(lastTypedKey);
            }
            Position++;

            var isLast = false;
            if (Position >= QCurrent.Alphabet.Length)
            {
                isLast = true;
            }
            return isLast;
        }

        public bool IsTypedkeyCorrect(string key)
        {
            lastTypedKey = key;

            if (Position < 0)
            {
                return false;
            }
            if (QCurrent.Alphabet[Position].ToString() == key)
            {
                correctCount++;
                return true;
            }
            return false;
        }

        public string[] ResultToList()
        {
            var list = new string[resultList.Count];
            for (var i = 0; i < list.Length; i++)
            {
                list[i] = resultList[i].ToString();
            }
            return list;
        }

        public TotalResult GetTotalResult()
        {
            double acc = 0;
            double wpm = 0;
            int n = resultList.Count;
            foreach(var result in resultList)
            {
                acc += result.Accuracy;
                wpm += result.Wpm;
            }
            acc /= n;
            wpm /= n;
            return new TotalResult(n, acc, wpm);
        }

        void AddToResult()
        {
            var typedKey = typed.ToString();
            var ratio = CalcAccuracy(QCurrent.Alphabet, typedKey);
            if (ratio < 0) return;
            var res = new QuestionResult(QCurrent, typedKey, ratio, Wpm);
            resultList.Add(res);
        }

        double CalcAccuracy(string org, string typed)
        {
            var n = org.Length;
            if (typed.Length != n || n == 0)
            {
                return -1;
            }
            double correct = 0;
            for (var i = 0; i < n; i++)
            {
                if (org[i] == typed[i])
                    correct++;
            }
            return correct / n * 100;
        }

        public string[] GetNextNLetter(int n)
        {
            var strs = new string[n];
            for (var i = 0; i < n; i++)
            {
                if (Position + i < 0)
                {
                    strs[i] = "";
                }
                else if (Position + i >= QCurrent.Alphabet.Length)
                {
                    strs[i] = "";
                }
                else
                {
                    strs[i] = QCurrent.Alphabet[Position + i].ToString();
                }
            }
            return strs;
        }
    }

    class QuestionResult
    {
        public Question Question { private set; get; }
        public string TypedKey { private set; get; }
        public double Accuracy { private set; get; }
        public int Wpm { private set; get; }

        public QuestionResult(Question question, string typedKey, double correctRatio, int wpm)
        {
            Question = question;
            TypedKey = typedKey;
            Accuracy = correctRatio;
            Wpm = wpm;
        }

        public override string ToString()
        {
            return string.Format("{0,-25}{1,-25}{2,10:N2}{3,7:N0}", Question.Alphabet, TypedKey, Accuracy, Wpm);
        }
    }

}
