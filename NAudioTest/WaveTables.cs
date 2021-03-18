using System;

namespace NAudioTest.WaveProvider.Tables
{
	interface IWaveTable {
		int SampleRate { get; set; }
		void GenerateWaveTable();
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
			// TODO: Generate a 1Hz Saw Wave
		}

		public float GetWaveSample(int index) {
			return waveTable[index];
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
