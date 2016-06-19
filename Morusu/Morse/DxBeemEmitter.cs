using System;
using System.Windows.Forms;
using Microsoft.DirectX.DirectSound;

namespace Morusu.Morse
{
    /// <summary>
    /// DirectSoundによるIBeepEmitterの実装
    /// </summary>
    class DxBeemEmitter : IBeepEmitter
    {
        static Device device;
        SecondaryBuffer buffer;

        public double DitLengthSecond { set; get; }
        public double Frequency { set; get; }
        public int Amplitude { set; get; }
        public string WaveShape { set; get; }

        readonly double ditunit = 1.0;
        int samplesPerSecond = 44100;
        double dahunit = 3.0;

        public DxBeemEmitter(Form parent)
        {
            if (device == null)
            {
                device = new Device();
                device.SetCooperativeLevel(parent, CooperativeLevel.Priority);
            }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void EmitDah()
        {
            setBufferAndWave(dahunit);
            buffer.Write(0, GenerateOneUnitSound(dahunit), LockFlag.EntireBuffer);
            buffer.Volume = (int)Volume.Max;
            buffer.Play(0, BufferPlayFlags.Default);
        }

        public void EmitDit()
        {
            setBufferAndWave(ditunit);
            buffer.Write(0, GenerateOneUnitSound(ditunit), LockFlag.EntireBuffer);
            buffer.Volume = (int)Volume.Max;
            buffer.Play(0, BufferPlayFlags.Default);
        }

        char[] GenerateOneUnitSound(double lengthFactor)
        {
            var waveFormat = buffer.Format;
            int numSamples = Convert.ToInt32(lengthFactor * DitLengthSecond *
                samplesPerSecond * waveFormat.BlockAlign);
            char[] sampleData = new char[numSamples];
            double angle = (Math.PI * 2 * Frequency) /
                (samplesPerSecond * waveFormat.Channels);

            switch (WaveShape)
            {
                default: //case "Square":
                    for (int i = 0; i < numSamples; i += 1)
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
                        int numOneWave = (int)(numSamples / (Frequency * lengthFactor * DitLengthSecond));
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
                        int numOneWave = (int)(numSamples / (Frequency * lengthFactor * DitLengthSecond));
                        int cycle = 1;
                        int ii = 0;
                        bool isUp = true;   //傾きが正か負か
                        while (ii < numSamples)
                        {
                            for (int jj = numOneWave * (cycle - 1) / 2; jj < numOneWave * cycle / 2; jj++)
                            {
                                if (ii == numSamples) break;    //配列のインデックスが上限を超えてしまう場合、脱出
                                if (isUp)
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
            var waveFormat = new WaveFormat();
            waveFormat.SamplesPerSecond = samplesPerSecond;
            waveFormat.Channels = 2;
            waveFormat.FormatTag = WaveFormatTag.Pcm;
            waveFormat.BitsPerSample = 16;
            waveFormat.BlockAlign = (short)(waveFormat.Channels * waveFormat.BitsPerSample / 8);
            waveFormat.AverageBytesPerSecond = waveFormat.BlockAlign * waveFormat.SamplesPerSecond;

            var bufferDesc = new BufferDescription(waveFormat);
            bufferDesc.DeferLocation = true;
            bufferDesc.Control3D = false;
            bufferDesc.ControlEffects = false;
            bufferDesc.ControlFrequency = true;
            bufferDesc.ControlPan = true;
            bufferDesc.ControlVolume = true;
            bufferDesc.GlobalFocus = true;
            bufferDesc.BufferBytes = Convert.ToInt32(lengthFactor * DitLengthSecond *
                waveFormat.AverageBytesPerSecond);

            if (buffer != null)
            {
                buffer.Stop();
                buffer.Dispose();
                buffer = null;
            }

            buffer = new SecondaryBuffer(bufferDesc, device);
            bufferDesc.Dispose();
            bufferDesc = null;
        }

        public void Dispose()
        {
            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
            //デバイス破棄
            device.Dispose();
        }
    }
}
