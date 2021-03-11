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
		private bool stopAudio;

		public SineWaveProvider(float frequency, int sampleRate = 44100) {
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);

			this.phase = 0;
			this.frequency = frequency;
			this.sampleRate = sampleRate;
			this.stopAudio = false;
			Volume = 0.2f;
		}

		public float Volume { get; set; }

		public WaveFormat WaveFormat { get; private set; }

		public bool StopAudio {
			get {
				return stopAudio;
			}
			set {
				stopAudio = value;
			}
		}

		public int Read(float[] buffer, int offset, int count) {
			if (stopAudio) return 0;

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
