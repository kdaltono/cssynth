using NAudio.Wave;
using NAudioTest.WaveProvider.Tables;
using System;

namespace NAudioTest.WaveProvider
{
	class SineWaveProvider : ISampleProvider
	{
		private double phase;
		private double frequency;
		private double indexIncrement;
		private int sampleRate;
		private bool playing;

		public SineWaveProvider(float frequency, int sampleRate = 44100) {
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);

			this.phase = 0;
			this.frequency = frequency;
			this.sampleRate = sampleRate;
			this.playing = false;
			Volume = 0.2f;
		}

		public float Volume { get; set; }

		public double Frequency {
			get {
				return frequency;
			}
			set {
				frequency = value;
			}
		}

		public WaveFormat WaveFormat { get; private set; }

		public bool Playing {
			get {
				return playing;
			}
			set {
				playing = value;
			}
		}

		public int Read(float[] buffer, int offset, int count) {
			if (!playing) return 0;

			SineWaveTable sw = SineWaveTable.Instance;

			double frqTel = sw.GetWaveTableLength() / sampleRate;
			indexIncrement = frqTel * frequency;

			for (int n = 0; n < count; ++n) {
				int index = (int)phase % sw.GetWaveTableLength();

				buffer[n + offset] = sw.GetWaveSample(index) * Volume;
				phase += indexIncrement;
				if (phase >= (double)sw.GetWaveTableLength()) {
					phase -= (double)sw.GetWaveTableLength();
				}
				
			}
			return count;
		}
	}
}
