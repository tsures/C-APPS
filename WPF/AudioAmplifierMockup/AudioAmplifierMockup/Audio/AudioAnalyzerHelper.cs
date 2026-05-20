using System;
using NAudio.Dsp;
using NAudio.Wave;

namespace AudioAmplifierMockup.Audio
{
    public class AudioAnalyzerHelper : IDisposable
    {
        private const int FftLength = 1024;
        private const int BarCount = 32;

        private readonly Complex[] _fftBuffer = new Complex[FftLength];
        private int _fftPosition;

        private WasapiLoopbackCapture _capture;

        public event Action<float[]> EqualizerUpdated;

        public void Start()
        {
            if (_capture != null)
                return;

            _capture = new WasapiLoopbackCapture();
            _capture.DataAvailable += OnDataAvailable;
            _capture.StartRecording();
        }

        public void Stop()
        {
            if (_capture == null)
                return;

            _capture.DataAvailable -= OnDataAvailable;
            _capture.StopRecording();
            _capture.Dispose();
            _capture = null;
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            const int bytesPerSample = 4;

            for (int i = 0; i < e.BytesRecorded; i += bytesPerSample)
            {
                float sample = BitConverter.ToSingle(e.Buffer, i);

                _fftBuffer[_fftPosition].X =
                    (float)(sample * FastFourierTransform.HammingWindow(_fftPosition, FftLength));

                _fftBuffer[_fftPosition].Y = 0;

                _fftPosition++;

                if (_fftPosition >= FftLength)
                {
                    _fftPosition = 0;

                    var fft = new Complex[FftLength];
                    Array.Copy(_fftBuffer, fft, FftLength);

                    FastFourierTransform.FFT(true, (int)Math.Log(FftLength, 2), fft);

                    EqualizerUpdated?.Invoke(CreateEqualizerValues(fft));
                }
            }
        }

        private float[] CreateEqualizerValues(Complex[] fft)
        {
            var values = new float[BarCount];

            for (int i = 0; i < BarCount; i++)
            {
                int fftIndex = Math.Min((i + 1) * 8, fft.Length - 1);

                double magnitude = Math.Sqrt(
                    fft[fftIndex].X * fft[fftIndex].X +
                    fft[fftIndex].Y * fft[fftIndex].Y);

                values[i] = (float)Math.Min(magnitude * 1000, 200);
            }

            return values;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}