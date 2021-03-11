using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudioTest.Midi;
using NAudioTest.WaveProvider;
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

		public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2) {
			this.sampleRate = sampleRate;

			midiTools = new MIDITools();

			activeMIDIKeys = new Dictionary<int, ISampleProvider>();
			outputDevice = new WasapiOut();
			mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
			mixer.ReadFully = true;
			
			outputDevice.Init(mixer);
			outputDevice.Play();
		}

		public void addActiveMIDIKey(int midiNote) {
			int frequency = midiTools.GetFrequencyFromMIDINote(midiNote);
			SineWaveProvider output = new SineWaveProvider(frequency, sampleRate);

			activeMIDIKeys.Add(midiNote, output);

			PlaySound(output);
		}

		public void removeActiveMIDIKey(int midiNote) {
			ISampleProvider value;
			if (activeMIDIKeys.TryGetValue(midiNote, out value)) {
				if (value is SineWaveProvider) {
					SineWaveProvider output = (SineWaveProvider)value;
					output.StopAudio = true;
					activeMIDIKeys.Remove(midiNote);
				}
			}
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
