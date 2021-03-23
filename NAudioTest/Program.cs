using NAudioTest.Engine;
using NAudioTest.IO;
using NAudioTest.WaveProviders.Tables;
using System;

namespace NAudioTest
{
	class Program
	{
		static void Main(string[] args) {
			SineWaveTable.Instance.GenerateWaveTable();
			SawWaveTable.Instance.GenerateWaveTable();

			MIDIHandler io = new MIDIHandler();

			Console.WriteLine("To stop audio playback, type \'stop\'");
			string input = Console.ReadLine();
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
