﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Morusu.Quiz
{
    class Question
    {
        public Question(string orgstr, string alphstr)
        {
            Original = orgstr;
            Alphabet = alphstr;
        }

        public string Alphabet;
        public string Original;


    }
}
