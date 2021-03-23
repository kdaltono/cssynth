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
		private readonly IWavePlayer outputDevice;
		private readonly MixingSampleProvider mixer;
		private Dictionary<int, ISampleProvider> activeMIDIKeys;
		private MIDITools midiTools;
		private int sampleRate;

		private SawWaveProvider[] activeNotes;

		public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2) {
			this.sampleRate = sampleRate;
			midiTools = new MIDITools();

			activeMIDIKeys = new Dictionary<int, ISampleProvider>();
			outputDevice = new WasapiOut();
			mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
			mixer.ReadFully = true;

			InitialiseActiveNotes(128);

			outputDevice.Init(mixer);
			outputDevice.Play();
		}

		private void InitialiseActiveNotes(int activeNoteSize) {
			activeNotes = new SawWaveProvider[activeNoteSize];

			for (int i = 0; i < activeNoteSize; i++) {
				activeNotes[i] = new SawWaveProvider(440) {
					Volume = 0.0f
				};

				mixer.AddMixerInput(activeNotes[i]);
			}
		}

		private int getInactiveNoteIndex() {
			bool found = false;
			int index = 0;
			while (!found) {
				if (index >= 10)
					break;

				if (!activeNotes[index].Playing)
					return index;
				else
					index++;
			}
			return -1;
		}

		public void addActiveMIDIKey(int midiNote) {
			int frequency = midiTools.GetFrequencyFromMIDINote(midiNote);

			SawWaveProvider output = activeNotes[midiNote];
			output.SetRampValues(0.01f, 0.25f, 0.4f, 0.2f, 0.2f);
			output.Frequency = frequency;
			output.Volume = 0.0f;

			output.BeginPlay();

			PlaySound(output);
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
		}

		public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
	}
}
