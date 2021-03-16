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

		// Ramp Values:
		private float rampValue;
		private float rampInc;
		private bool ramp;

		// Hann Window Calculation:
		/*private bool startFade;
		private bool endFade;
		private float fadeTime;*/

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
				// May be able to get rid of 'playing' bool.
				if (value == true) {
					playing = true;
					ramp = false;
					rampValue = 0;
					rampInc = 0;

					/*startFade = true;
					endFade = false;*/
				} else {
					playing = false;

					/*startFade = false;
					endFade = true;*/
				}
			}
		}

		public void Ramp(float finalValue, float time) {
			rampValue = finalValue;
			if (finalValue > Volume)
				rampInc = time / sampleRate;
			else
				rampInc = -(time / sampleRate);
			ramp = true;
			Console.WriteLine("Ramp values set: \nRamp Value: " + rampValue + "\nOriginal Value: " + Volume + "\nRamp Inc: " + rampInc);
		}

		public int Read(float[] buffer, int offset, int count) {
			if (!playing)
				return 0;

			if (ramp)
				UpdateVolume();

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
		private void UpdateVolume() {
			if (rampInc > 0) {
				if (Volume < rampValue)
					Volume += rampInc;
				if (Volume > rampValue) {
					Volume = rampValue;
					ramp = false;
				}
			} else if (rampInc < 0) {
				if (Volume > rampValue)
					Volume += rampInc;
				if (Volume < rampValue) {
					Volume = rampValue;
					ramp = false;
				}
				if (Volume == 0) {
					playing = false;
				}
			} else {
				// If it is zero don't waste time doing calculations
				return;
			}
		}
	}
}
