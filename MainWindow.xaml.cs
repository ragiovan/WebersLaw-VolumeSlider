using System;
using System.Windows;
using System.Windows.Controls;
using NAudio.Wave;

namespace WeberSine
{
    public partial class MainWindow : Window
    {
        private readonly WaveOutEvent _waveOut;
        private readonly SineWaveProvider _sineWave;
        private bool _playing;

        public MainWindow()
        {
            InitializeComponent();
            _sineWave = new SineWaveProvider(frequency: 440f);
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_sineWave);
            UpdateVolume();
        }

        // Maps slider (0–100) → linear amplitude (0–1).
        //
        // Linear mode: amplitude scales 1:1 with the slider position.
        //
        // Logarithmic mode: slider positions map to the decibel range –40 dB … 0 dB,
        // then converted to amplitude.  This matches how human loudness perception
        // works (Weber–Fechner law): equal slider steps feel equally loud because
        // each step represents the same *ratio* of change in physical energy.
        //   slider =   0 → –40 dB → amplitude ≈ 0.010
        //   slider =  50 → –20 dB → amplitude ≈ 0.100
        //   slider =  75 → –10 dB → amplitude ≈ 0.316
        //   slider = 100 →   0 dB → amplitude = 1.000
        private float ComputeAmplitude(double sliderValue)
        {
            if (sliderValue <= 0.0) return 0f;

            double normalized = sliderValue / 100.0;

            if (LogToggle.IsChecked == true)
            {
                double dB = 40.0 * normalized - 40.0; // –40 dB at 0, 0 dB at 100
                return (float)Math.Pow(10.0, dB / 20.0);
            }

            return (float)normalized;
        }

        private void UpdateVolume()
        {
            float amp = ComputeAmplitude(VolumeSlider.Value);
            _sineWave.Amplitude = amp;

            SliderValueText.Text = ((int)Math.Round(VolumeSlider.Value)).ToString();
            LinearValueText.Text = amp.ToString("F4");
            LinearBar.Value = amp;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Guard: control may fire during InitializeComponent before LogToggle exists
            if (LogToggle is null) return;
            UpdateVolume();
        }

        private void LogToggle_Changed(object sender, RoutedEventArgs e) => UpdateVolume();

        private void PlayStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playing)
            {
                _waveOut.Pause();
                PlayStopButton.Content = "▶  Play";
                _playing = false;
            }
            else
            {
                _waveOut.Play();
                PlayStopButton.Content = "⏸  Pause";
                _playing = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _waveOut.Stop();
            _waveOut.Dispose();
            base.OnClosed(e);
        }
    }
}
