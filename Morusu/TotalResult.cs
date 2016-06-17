using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morusu
{
    public class TotalResult
    {
        public int Count { set; get; }
        public double Accuracy { set; get; }
        public double Wpm { set; get; }
        public string Name { set; get; }
        public string Date { set; get; }
        public TotalResult(int count, double acc, double wpm)
        {
            Count = count;
            Accuracy = acc;
            Wpm = wpm;
        }
        public TotalResult() { }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(PaddingInBytes(Name, PadType.Char, 18));
            sb.Append(PaddingInBytes(Accuracy.ToString("N0"), PadType.Number, 4));
            sb.Append(PaddingInBytes(Wpm.ToString("N0"), PadType.Number, 5));
            sb.Append(PaddingInBytes(Count.ToString("N0"), PadType.Number, 5));
            sb.Append(PaddingInBytes(Date, PadType.Number, 21));
            return sb.ToString();
        }

        string PaddingInBytes(string value, PadType type, int byteCount)
        {
            Encoding enc = Encoding.GetEncoding("Shift_JIS");

            if (byteCount < enc.GetByteCount(value))
            {
                // valueが既定のバイト数を超えている場合は、切り落とし
                value = value.Substring(0, byteCount);
            }

            switch (type)
            {
                case PadType.Char:
                    // 文字列の場合　左寄せ＋空白埋め
                    return value.PadRight(byteCount - (enc.GetByteCount(value) - value.Length));
                case PadType.Number:
                    // 数値の場合　右寄せ＋0埋め
                    return value.PadLeft(byteCount, ' ');
                default:
                    // 上記以外は全部空白
                    return value.PadLeft(byteCount);
            }
        }

        enum PadType
        {
            Char
            , Number
        }

    }
}
