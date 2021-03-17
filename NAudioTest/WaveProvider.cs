using NAudio.Wave;
using NAudioTest.WaveProvider.Tables;
using System;

namespace NAudioTest.WaveProvider {
	class SineWaveProvider : ISampleProvider {
		public enum SoundStage {
			Attack,
			Decay,
			Sustain,
			Release
		}

		private double phase;
		private double frequency;
		private double indexIncrement;
		private int sampleRate;
		private bool playing;

		// Attack Decay Sustain Release:
		private SoundStage stage;
		private float attackInc;
		private float attackVol;
		private float decayInc;
		private float sustainVol;
		private float releaseInc;

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

		public void SetRampValues(float attackLength, float attackVolume, float decayLength, float sustainVolume, 
			float releaseLength) {
			SetAttackValues(attackLength, attackVolume);
			SetSustainValues(sustainVolume);
			SetDecayValues(decayLength);
			SetReleaseValues(releaseLength);
		}

		// TODO:
		public void SetAttackValues(float attackLength, float attackVolume) {
			attackVol = attackVolume;
			attackInc = (attackVol) / (attackLength * sampleRate);
		}

		public void SetSustainValues(float sustainVolume) {
			sustainVol = sustainVolume;
		}

		public void SetDecayValues(float decayLength) { 
			if (attackVol != sustainVol)
				decayInc = (sustainVol - attackVol) / (decayLength * sampleRate);
			else
				decayInc = 0.0f;
		}

		public void SetReleaseValues(float releaseLength) {
			releaseInc = (-sustainVol) / (releaseLength * sampleRate);
		}

		public void BeginPlay() {
			stage = SoundStage.Attack;
		}

		public void BeginRelease() {
			stage = SoundStage.Release;
		}

		public int Read(float[] buffer, int offset, int count) {
			if (!playing)
				return 0;

			SineWaveTable sw = SineWaveTable.Instance;

			double frqTel = sw.GetWaveTableLength() / sampleRate;
			indexIncrement = frqTel * frequency;

			for (int n = 0; n < count; ++n) {
				UpdateVolume();
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
			// Update with ADSR values:

			switch (stage) {
				case SoundStage.Attack:
					UpdateAttackVolume();
					break;
				case SoundStage.Decay:
					UpdateDecayVolume();
					break;
				case SoundStage.Sustain:
					UpdateSustainVolume();
					break;
				case SoundStage.Release:
					UpdateReleaseVolume();
					break;
				default:
					// This shouldn't happen.
					throw new NullReferenceException("Wave Provider audio stage not set!");
			}
		}

		private void UpdateAttackVolume() {
			if (attackInc == 0.0f) return;

			Volume += attackInc;
			if ((attackInc > 0 && Volume > attackVol) ||
				(attackInc < 0 && Volume < attackVol)) {
				Volume = attackVol;
				stage = SoundStage.Decay;
			}
		}

		private void UpdateDecayVolume() {
			if (decayInc == 0.0f) return;

			Volume += decayInc;
			if ((decayInc > 0 && Volume > sustainVol) || 
				(decayInc < 0 && Volume < sustainVol)) {
				Volume = sustainVol;
				stage = SoundStage.Sustain;
			}
		}

		private void UpdateSustainVolume() {
			if (Volume != sustainVol)
				Volume = sustainVol;
		}

		private void UpdateReleaseVolume() {
			if (releaseInc == 0.0f) return; // This shouldn't ever be the case, but if it is then do it.

			Volume += releaseInc;
			bool releaseComplete = releaseInc < 0 && Volume < 0.0f;
			if (releaseComplete) {
				Volume = 0.0f;
				playing = false;
			}
		}
	}
}
