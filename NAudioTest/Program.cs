using NAudio.Wave;
using NAudioTest.Engine;
using NAudioTest.IO;
using NAudioTest.WaveProviders.Tables;
using System;

namespace NAudioTest
{
	class Program
	{
		[STAThread]
		static void Main(string[] args) {

			Console.WriteLine("Please select an ASIO driver to use: ");

			int asioDriverCount = AsioOut.GetDriverNames().Length;

			for (int i = 0; i < asioDriverCount; i++) {
				Console.WriteLine(i + ": " + AsioOut.GetDriverNames()[i]);
			}

			string input= Console.ReadLine();
			int selection = Int32.Parse(input);

			SineWaveTable.Instance.GenerateWaveTable();
			SawWaveTable.Instance.GenerateWaveTable();

			MIDIHandler io = new MIDIHandler(selection);

			Console.WriteLine("To stop audio playback, type \'stop\'");
			input = Console.ReadLine();
			while (input != "stop") {
				Console.WriteLine(String.Format("\'{0}\' is not a recognised command.", input));
				input = Console.ReadLine();
			}

			Console.WriteLine("Exiting program...");
			io.Dispose();
		}
	}
}
