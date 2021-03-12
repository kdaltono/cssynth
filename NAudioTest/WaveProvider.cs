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

		// Hann Window Calculation:
		private bool startFade;
		private bool endFade;
		private float fadeTime;

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

					startFade = true;
					endFade = false;
				} else {
					playing = false;

					startFade = false;
					endFade = true;
				}
			}
		}

		// Need to update this:
		//	- When playing = false, and Volume > 0 -> decrease the volume to 0.
		//		- This can also be considered 'delay' and should hopefully remove the pop at the end of a note.

		public int Read(float[] buffer, int offset, int count) {
			if (!playing)
				return 0;

			SineWaveTable sw = SineWaveTable.Instance;

			double frqTel = sw.GetWaveTableLength() / sampleRate;
			indexIncrement = frqTel * frequency;

			int numFadeSamples = (int)fadeTime * sampleRate;
			int fadeCount = 0;
			if (numFadeSamples > sampleRate) numFadeSamples = sampleRate;

			for (int n = 0; n < count; ++n) {
				int index = (int)phase % sw.GetWaveTableLength();

				buffer[n + offset] = sw.GetWaveSample(index) * Volume;

				// Start fade:
				if (startFade && fadeCount < numFadeSamples) {
					float weight = (float)(0.5 * (1 - Math.Cos(Math.PI * fadeCount / (numFadeSamples - 1))));
					buffer[n + offset] *= weight;
					fadeCount++;
				}
				else if (fadeCount >= numFadeSamples)
					startFade = false;

				phase += indexIncrement;
				if (phase >= (double)sw.GetWaveTableLength()) {
					phase -= (double)sw.GetWaveTableLength();
				}
				
			}
			return count;
		}
	}
}
