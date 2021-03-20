using System;

namespace NAudioTest.WaveProvider.Tables
{
	interface IWaveTable {
		int SampleRate { get; set; }
		void GenerateWaveTable();

		int GetWaveTableLength();
		float GetWaveSample(int index);
	}

	class SineWaveTable : IWaveTable
	{
		private float[] waveTable;
		private int sampleRate;

		public SineWaveTable(int sampleRate) {
			this.sampleRate = sampleRate;
			this.waveTable = new float[sampleRate];
			GenerateWaveTable();
		}

		public void GenerateWaveTable() {
			for (int index = 0; index < sampleRate; ++index) {
				waveTable[index] = (float)(Math.Sin(2 * Math.PI * (double)index / sampleRate));
			}
		}

		public float GetWaveSample(int index) {
			return waveTable[index];
		}

		public int GetWaveTableLength() {
			return waveTable.Length;
		}

		public int SampleRate { 
			get {
				return sampleRate;
			}
			set {
				sampleRate = value;
			}
		}

		public static readonly SineWaveTable Instance = new SineWaveTable(44100);
	}

	public class SawWaveTable : IWaveTable {
		private float[] waveTable;
		private int sampleRate;
		
		public SawWaveTable(int sampleRate) {
			SampleRate = sampleRate;
			this.waveTable = new float[sampleRate];
			GenerateWaveTable();
		}

		public void GenerateWaveTable() {
			// TODO: This needs to be updated to be more sine wavey so that the sound isn't as sharp.
			float sawWaveGrad = 2.0f / 44100.0f;
			float sampleValue = 0.0f;

			for (int index = 0; index < sampleRate; index++) {
				if (sampleValue > 1.0f)
					sampleValue = -1.0f;

				if (sampleValue < -1.0f)
					sampleValue = 1.0f;

				waveTable[index] = sampleValue;
				sampleValue += sawWaveGrad;
			}
		}

		public float GetWaveSample(int index) {
			return waveTable[index];
		}

		public int GetWaveTableLength() {
			return waveTable.Length;
		}

		public int SampleRate { 
			get {
				return sampleRate;
			}
			set {
				sampleRate = value;
			}
		}

		public static readonly SawWaveTable Instance = new SawWaveTable(44100);
	}
}
