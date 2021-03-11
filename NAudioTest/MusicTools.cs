using System;
using System.Collections.Generic;

namespace NAudioTest.Midi
{
	class MIDITools
	{
		private int[] midiFrequencyArray;

		public MIDITools() {
			midiFrequencyArray = new int[128];
			GenerateFrequencies();
		}

		private void GenerateFrequencies() {
			for (int midiNote = 0; midiNote < 128; midiNote++) {
				int currFreq = ConvertMIDINoteToFrequency(midiNote);
				midiFrequencyArray[midiNote] = currFreq;
			}
		}

		private int ConvertMIDINoteToFrequency(int midiNote) {
			if (midiNote == 69) 
				return 440;
			else
				return (int)(Math.Pow(2.0, (midiNote - 69.0) / 12.0) * 440.0);
		}

		public int GetFrequencyFromMIDINote(int midiNote) {
			if (midiNote < 0 || midiNote > 127)
				throw new ArgumentOutOfRangeException("Midi note cannot be less than 0 or larger than 127.");

			return midiFrequencyArray[midiNote];
		}
	}
}
