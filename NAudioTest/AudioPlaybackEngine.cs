using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudioTest.Midi;
using NAudioTest.WaveProviders;
using System;
using System.Collections.Generic;

namespace NAudioTest.Engine
{
	class AudioPlaybackEngine : IDisposable
	{
		private readonly AsioOut outputDevice;
		private readonly MixingSampleProvider mixer;
		private readonly SavingSampleProvider saver;

		private MIDITools midiTools;
		private int sampleRate;

		private NoteProvider[] activeNotes;

		public AudioPlaybackEngine(int driverSelection, int sampleRate = 44100, int channelCount = 2) {
			this.sampleRate = sampleRate;
			midiTools = new MIDITools();

			outputDevice = new AsioOut(AsioOut.GetDriverNames()[driverSelection]);
			mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
			mixer.ReadFully = true;
			saver = new SavingSampleProvider(mixer, "test.wav");


			InitialiseActiveNotes(128);

			outputDevice.Init(saver);
			outputDevice.Play();
		}

		private void InitialiseActiveNotes(int activeNoteSize) {
			activeNotes = new NoteProvider[activeNoteSize];

			for (int i = 0; i < activeNoteSize; i++) {
				activeNotes[i] = new NoteProvider(midiTools.GetFrequencyFromMIDINote(i)) {
					SinVolume = 1.0f,
					SawVolume = 0.0f
				};
				activeNotes[i].SetRampValues(0.2f, 0.2f, 0.2f, 0.18f, 0.2f);
			}
		}

		public void addActiveMIDIKey(int midiNote) {
			NoteProvider output = activeNotes[midiNote];

			if (output.Playing) {
				output.BeginPlay();
			} else {
				output.BeginPlay();
				AddMixerInput(output);
			}
		}

		public void removeActiveMIDIKey(int midiNote) {
			activeNotes[midiNote].BeginRelease();
		}

		public void PlaySound(ISampleProvider input) {
			mixer.AddMixerInput(ConvertToRightChannelCount(input));
		}

		private ISampleProvider ConvertToRightChannelCount(ISampleProvider input) {
			if (input.WaveFormat.Channels == mixer.WaveFormat.Channels) {
				return input;
			}
			if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2) {
				return new MonoToStereoSampleProvider(input);
			}
			throw new NotImplementedException("Not yet implemented this channel count conversion.");
		}

		private void AddMixerInput(ISampleProvider input) {
			mixer.AddMixerInput(ConvertToRightChannelCount(input));
		}

		public void Dispose() {
			outputDevice.Dispose();
			saver.Dispose();
		}

		/*public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);*/
	}
}
