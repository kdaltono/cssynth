using NAudio.Wave;
using NAudioTest.WaveProviders.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAudioTest.WaveProviders {

	// TODO: Test this.

	class NoteProvider : ISampleProvider {
		protected double frequency;
		protected int sampleRate;
		protected bool playing;

		private bool playSin, playSaw;
		private float sinVolume, sawVolume;
		private double sinPhase, sawPhase;

		// Attack Decay Sustain Release:
		protected float attackInc, attackVol, decayInc, sustainVol, sustainInc, releaseInc;

		protected const int ATTACK = 0;
		protected const int DECAY = 1;
		protected const int SUSTAIN = 2;
		protected const int RELEASE = 3;
		protected int currentStage;

		public NoteProvider(float frequency, int sampleRate = 44100) {
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);

			this.sawPhase = 0;
			this.sinPhase = 0;

			this.frequency = frequency;
			this.sampleRate = sampleRate;
			this.playing = false;
			Volume = 0.2f;
		}

		public void BeginPlay() {
			currentStage = ATTACK;
			Playing = true;
		}

		public void BeginRelease() {
			currentStage = RELEASE;
		}

		public float Volume { get; set; }

		public float SinVolume { 
			get {
				return sinVolume;
			}
			set {
				sinVolume = value;
				if (sinVolume == 0.0f) playSin = false;
				else if (sinVolume > 0.0f) playSin = true;
			}
		}

		public float SawVolume {
			get {
				return sawVolume;
			}
			set {
				sawVolume = value;
				if (sawVolume == 0.0f) playSaw = false;
				else if (sawVolume > 0.0f) playSaw = true;
			}
		}

		public double Frequency {
			get {
				return frequency;
			}
			set {
				frequency = value;
			}
		}

		public bool Playing {
			get {
				return playing;
			}
			set {
				playing = value;
			}
		}

		public WaveFormat WaveFormat { get; set; }

		public int Read(float[] buffer, int offset, int count) {
			if (!playing)
				return 0;

			SineWaveTable sin = SineWaveTable.Instance;

			double sinFrqTel = sin.GetWaveTableLength() / sampleRate;

			double sinIndexInc = sinFrqTel * frequency;

			for (int n = 0; n < count; ++n) {
				UpdateVolume();

				buffer[n + offset] = GetSinWaveTableValue(sinIndexInc);
			}
			return count;
		}

		private float GetBufferValue(double sinIndexInc) {
			float returnValue = 0.0f;
			if (playSin) {
				returnValue += GetSinWaveTableValue(sinIndexInc);
			}

			return returnValue;
		}

		private float GetSinWaveTableValue(double sinIndexInc) {
			SineWaveTable sin = SineWaveTable.Instance;

			int sinIndex = (int)sinPhase % sin.GetWaveTableLength();

			sinPhase += sinIndexInc;
			if (sinPhase >= (double)sin.GetWaveTableLength()) {
				sinPhase -= (double)sin.GetWaveTableLength();
			}
			return sin.GetWaveSample(sinIndex) * Volume;
		}

		public void SetRampValues(float attackLength, float attackVolume, float decayLength, float sustainVolume,
			float releaseLength) {
			SetAttackValues(attackLength, attackVolume);
			SetSustainValues(sustainVolume);
			SetDecayValues(decayLength);
			SetReleaseValues(releaseLength);
		}

		public void SetAttackValues(float attackLength, float attackVolume) {
			attackVol = attackVolume;
			attackInc = (attackVol - Volume) / (attackLength * sampleRate);
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

		public void SetSustainValues(float sustainVolume) {
			sustainInc = (Volume - sustainVol) / (0.01f / sampleRate);
			sustainVol = sustainVolume;
		}

		public void UpdateVolume() {
			switch (currentStage) {
				case ATTACK:
					UpdateAttackVolume();
					break;
				case DECAY:
					UpdateDecayVolume();
					break;
				case SUSTAIN:
					UpdateSustainVolume();
					break;
				case RELEASE:
					UpdateReleaseVolume();
					break;
				default:
					throw new NullReferenceException("Wave Provider audio stage not set!");
			}
		}

		private void UpdateAttackVolume() {
			if (attackInc == 0.0f) return;

			Volume += attackInc;
			if (attackInc > 0 && Volume > attackVol) {
				Volume = attackVol;
				currentStage = DECAY;
			}
		}

		private void UpdateDecayVolume() {
			if (decayInc == 0.0f) return;

			Volume += decayInc;
			if ((decayInc > 0 && Volume > sustainVol) ||
				(decayInc < 0 && Volume < sustainVol)) {
				Volume = sustainVol;
				currentStage = SUSTAIN;
			}
		}

		private void UpdateSustainVolume() {
			if (sustainInc == 0.0f) return;

			Volume += sustainInc;

			if ((sustainInc < 0 && Volume < sustainVol) || (sustainInc > 0 && Volume > sustainVol)) {
				Volume = sustainVol;
			}
		}

		private void UpdateReleaseVolume() {
			if (releaseInc == 0.0f) return;

			Volume += releaseInc;
			bool releaseComplete = releaseInc < 0 && Volume <= 0.0f;
			if (releaseComplete) {
				Volume = 0.0f;
				playing = false;
			}
		}
	}
}
