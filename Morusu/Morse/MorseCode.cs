using System.Collections.Generic;
using System.Linq;
using System.Diagnostics; 

namespace Morusu.Morse
{
    class MorseCode
    {
        private string targetMorse;
        //targetMorse is like "010". 0 = dah, 1 = dit
        private Stopwatch sw = new Stopwatch();
        static readonly double ditunit = 1.0;
        static double dahunit = 3.0;
        static double spaceunit = 1.0;
        public double NetLengthFactor { private set; get; }
        
        static Dictionary<string, string> mh = new Dictionary<string, string>();

        public MorseCode()  //コンストラクタ
        {
            SetMorseHash();
            Reset();
            sw.Start();
        }

        private static void SetMorseHash()
        {
            mh[""] = "";
            mh["0"] = "T";
            mh["1"] = "E";
            mh["00"] = "M";
            mh["01"] = "N";
            mh["10"] = "A";
            mh["11"] = "I";
            mh["000"] = "O";
            mh["001"] = "G";
            mh["010"] = "K";
            mh["011"] = "D";
            mh["100"] = "W";
            mh["101"] = "R";
            mh["110"] = "U";
            mh["111"] = "S";
            //mh["0000"] = "";
            //mh["0001"] = "";
            mh["0010"] = "Q";
            mh["0011"] = "Z";
            mh["0100"] = "Y";
            mh["0101"] = "C";
            mh["0110"] = "X";
            mh["0111"] = "B";
            mh["1000"] = "J";
            mh["1001"] = "P";
            //mh["1010"] = "";
            mh["1011"] = "L";
            //mh["1100"] = "";
            mh["1101"] = "F";
            mh["1110"] = "V";
            mh["1111"] = "H";            
            mh["10000"] = "1";
            mh["11000"] = "2";
            mh["11100"] = "3";
            mh["11110"] = "4";
            mh["11111"] = "5";
            mh["01111"] = "6";
            mh["00111"] = "7";
            mh["00011"] = "8";
            mh["00001"] = "9";
            mh["00000"] = "0";
            mh["110011"] = "?";
            mh["01101"] = "/";
            mh["101010"] = ".";
            mh["001100"] = ",";
            mh["110011"] = "?";
            mh["011110"] = "-";
            mh["100101"] = "@";
            mh["01001"] = "(";
            mh["010010"] = ")";
            mh["11111111"] = "x";
        }

        public static string GetCode(string letter)
        {
            var key = mh.First(x => x.Value == letter).Key;
            return key;
        }

        public string CheckCode()
        {
            if (mh.ContainsKey(targetMorse))
                return (string)mh[targetMorse];
            else 
                return " ";
        }

        public void Dah()
        {
            targetMorse += "0";
            if (NetLengthFactor == 0)
                sw.Start();
            NetLengthFactor += dahunit + spaceunit;
        }

        public void Dit()
        {
            targetMorse += "1";
            if (NetLengthFactor == 0)
                sw.Start();
            NetLengthFactor += ditunit + spaceunit;
        }

        public void Reset()
        {
            targetMorse = "";
            NetLengthFactor = 0;
            sw.Reset();
        }

        public double elapsedMilliseconds()
        {
            return sw.ElapsedMilliseconds;
        }
    }
}
