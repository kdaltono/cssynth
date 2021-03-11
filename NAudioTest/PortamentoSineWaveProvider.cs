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
		private bool audioStopped;

		public SineWaveProvider(float frequency, int sampleRate = 44100) {
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);

			this.phase = 0;
			this.frequency = frequency;
			this.sampleRate = sampleRate;
			this.stopAudio = false;
			this.audioStopped = false;
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

	class PortamentoSineWaveProvider : ISampleProvider
	{
		private double phase;
		private double currentPhaseStep;
		private double targetPhaseStep;
		private double frequency;
		private double phaseStepDelta;
		private bool seekFreq;
		private bool stopAudio;

		public PortamentoSineWaveProvider(int sampleRate = 44100) {
			stopAudio = false;

			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);
			Frequency = 1000f;
			Volume = 0.2f;
			Portamento = 0.2;
		}

		public double Portamento { get; set; }

		public double Frequency {
			get {
				return frequency;
			}
			set {
				frequency = value;
				seekFreq = true;
			}
		}

		public float Volume { get; set; }

		public bool StopAudio {
			get {
				return stopAudio;
			}
			set {
				stopAudio = value;
			}
		}

		public WaveFormat WaveFormat { get; private set; }

		public int Read(float[] buffer, int offset, int count) {
			// If audio has requested stop, return 0:
			// This should get changed so that 
			if (stopAudio) return 0;

			SineWaveTable sw = SineWaveTable.Instance;

			if (seekFreq) {
				targetPhaseStep = sw.GetWaveTableLength() * (frequency / WaveFormat.SampleRate);

				phaseStepDelta = (targetPhaseStep - currentPhaseStep) / (WaveFormat.SampleRate * Portamento);
				seekFreq = false;
			}

			var vol = Volume;
			for (int n = 0; n < count; ++n) {
				int waveTableIndex = (int)phase % sw.GetWaveTableLength();
				buffer[n + offset] = sw.GetWaveSample(waveTableIndex) * vol;
				phase += currentPhaseStep;
				if (this.phase > (double)sw.GetWaveTableLength()) {
					this.phase -= (double)sw.GetWaveTableLength();
				}
				if (currentPhaseStep != targetPhaseStep) {
					currentPhaseStep += phaseStepDelta;
					if (phaseStepDelta > 0.0 && currentPhaseStep > targetPhaseStep)
						currentPhaseStep = targetPhaseStep;
					else if (phaseStepDelta < 0.0 && currentPhaseStep < targetPhaseStep)
						currentPhaseStep = targetPhaseStep;
				}
			}
			return count;
		}
	}
}
