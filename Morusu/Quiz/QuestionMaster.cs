using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Morusu.Quiz
{
    class QuestionMaster
    {
        List<Question> qlist;

        public void SetQuestion(string qpath)
        {
            var reader = new QuestionReader();
            qlist = reader.ReadFile(qpath);
        }

        public Question GetNextQuestion()
        {
            var rnd = new Random();
            var idx = rnd.Next(0, qlist.Count-1);
            return qlist[idx];
        }
    }

    class QuestionReader
    {
        string encode = "Shift_JIS";
        public List<Question> ReadFile(string filepath)
        {
            var line = "";
            var list = new List<Question>();

            using (var sr = new StreamReader(
                filepath, Encoding.GetEncoding(encode)))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    string[] strs = line.Split(',');
                    if (strs.Length == 2)
                    {
                        list.Add(new Question(strs[0], strs[1]));
                    }
                }
            }
            return list;
        }
    }
}
