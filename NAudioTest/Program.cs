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
		static void Main(string[] args) {
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
