using System;

namespace Morusu
{
    /// <summary>
    /// 音を鳴らすクラスのインターフェイス
    /// </summary>
    interface IBeepEmitter : IDisposable
    {
        double DitLengthSecond { set; get; }
        double Frequency { set; get; }
        int Amplitude { set; get; }
        string WaveShape { set; get; }

        void Initialize();
        void EmitDah();
        void EmitDit();
    }
}
