using System;

namespace NAudioTest.Midi
{
	class MIDITools
	{
		private readonly float[] midiFrequencyArray;

		public MIDITools() {
			midiFrequencyArray = new float[128];
			GenerateFrequencies();
		}

		private void GenerateFrequencies() {
			for (int midiNote = 0; midiNote < 128; midiNote++) {
				float currFreq = ConvertMIDINoteToFrequency(midiNote);
				midiFrequencyArray[midiNote] = currFreq;
			}
		}

		private float ConvertMIDINoteToFrequency(int midiNote) {
			if (midiNote == 69) 
				return 440;
			else
				return (float)(Math.Pow(2.0, (midiNote - 69.0) / 12.0) * 440.0);
		}

		public float GetFrequencyFromMIDINote(int midiNote) {
			if (midiNote < 0 || midiNote > 127)
				throw new ArgumentOutOfRangeException("Midi note cannot be less than 0 or larger than 127.");

			return midiFrequencyArray[midiNote];
		}
	}
}
