using NAudio.Wave;
using NAudioTest.Engine;
using NAudioTest.WaveProviders.Tables;
using System;
using System.Diagnostics;

namespace NAudioTest.WaveProviders {

	class NoteProvider : ISampleProvider {
		private double frequency;
		private int sampleRate;
		private bool playing;

		private bool playSin, playSaw;
		private float sinVolume, sawVolume;

		private double phase;
		private double phaseInc;

		// Attack Decay Sustain Release:
		private float attackInc, attackVol, decayInc, sustainVol, sustainInc, releaseInc;

		private const int ATTACK = 0;
		private const int DECAY = 1;
		private const int SUSTAIN = 2;
		private const int RELEASE = 3;
		private int currentStage;

		Stopwatch timer;

		public NoteProvider(float frequency, int sampleRate = 44100) {
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);

			timer = new Stopwatch();
			timer.Start();

			phase = 0.0;
			phaseInc = 0.0;

			Frequency = frequency;
			this.sampleRate = sampleRate;
			this.playing = false;
			
			Volume = 0.0f;
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
				phaseInc = value;
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

			// Need to find a graceful way for this to finally return 0 when the audio is definintely finished. The
			// 'playing' variable doesn't work properly, and this method keeps the read open for more reads. When
			// lots of notes are played, this creates buffering issues with AudioPlaybackEngine trying to read from
			// multiple NoteProviders at the same time. Returning 0 here would remove them and not cause this issue.

			for (int n = 0; n < count; n++) {
				Oscillate();

				buffer[n + offset] = GetBufferValue() * Volume;

				UpdateVolume();
			}
			return count;
		}

		private void Oscillate() {
			phase += phaseInc;
			if (phase >= sampleRate)
				phase -= sampleRate;
		}

		private float GetBufferValue() {
			float returnValue = 0.0f;
			if (playSin) {
				returnValue += GetSinWaveTableValue();
			}
			if (playSaw) {
				returnValue += GetSawWaveTableValue();
			}
			return returnValue;
		}

		private float GetSinWaveTableValue() {
			return SineWaveTable.Instance.GetWaveSample((int)phase) * sinVolume;
		}

		private float GetSawWaveTableValue() {
			return SawWaveTable.Instance.GetWaveSample((int)phase) * sawVolume;
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
