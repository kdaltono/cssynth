using NAudio.Wave;
using NAudioTest.Engine;
using NAudioTest.IO;
using NAudioTest.WaveProvider;
using NAudioTest.WaveProvider.Tables;
using System;

namespace NAudioTest
{
	class Program
	{
		// TODO: Modify the MIDI input stuff to implement a round robin approach with atleast 10 fixed wave providers 
		//		 that will update themselves depending on the key pressed. Might help performance rather than creating
		//		 new wave providers every time a key is pressed.

		static void Main(string[] args) {
			// Generate Wave Tables:
			SineWaveTable.Instance.GenerateWaveTable();
			MIDIHandler io = new MIDIHandler();

			Console.WriteLine("To stop audio playback, type \'stop\'");
			String input = Console.ReadLine();
			while (input != "stop") {
				Console.WriteLine(String.Format("\'{0}\' is not a recognised command.", input));
				input = Console.ReadLine();
			}

			Console.WriteLine("Exiting program...");
			AudioPlaybackEngine.Instance.Dispose();
			io.Dispose();
		}
	}
}
