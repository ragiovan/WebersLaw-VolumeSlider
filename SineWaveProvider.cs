using NAudio.Wave;
using System;

namespace WeberSine
{
    internal sealed class SineWaveProvider : ISampleProvider
    {
        private double _phase;

        public SineWaveProvider(float frequency = 440f, int sampleRate = 44100)
        {
            Frequency = frequency;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
        }

        public WaveFormat WaveFormat { get; }
        public float Frequency { get; set; }

        // Written from UI thread, read from audio thread — float reads/writes are atomic on x86/x64
        public float Amplitude { get; set; }

        public int Read(float[] buffer, int offset, int count)
        {
            float amp = Amplitude;
            double phaseStep = 2.0 * Math.PI * Frequency / WaveFormat.SampleRate;
            for (int n = 0; n < count; n++)
            {
                buffer[offset + n] = amp * (float)Math.Sin(_phase);
                _phase += phaseStep;
                if (_phase >= 2.0 * Math.PI)
                    _phase -= 2.0 * Math.PI;
            }
            return count;
        }
    }
}
