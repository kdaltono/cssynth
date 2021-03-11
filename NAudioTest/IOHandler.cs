using NAudio.Midi;
using NAudioTest.Engine;
using System;

namespace NAudioTest.IO
{
	class MIDIHandler : IDisposable
	{
		private int selectedDevice = -1;
		private MidiIn midiIn = null;
		private bool canRead = false;

		public MIDIHandler() {
			// TODO: By default, this will select the 0 index MIDI device. This should be changed so that the user can select which one they want.
			selectDevice();
			if (canRead) {
				handleMidiMessages();
			}
		}

		public bool HasMidiDevice {
			get {
				return canRead;
			}
			private set {
				canRead = value;
			}
		}

		private void selectDevice() {
			// If there are any devices, then we can read (technically)
			if (MidiIn.NumberOfDevices > 0) {
				canRead = true;
				selectedDevice = 0;
				Console.WriteLine(String.Format("Selected MIDI Device: {0}", MidiIn.DeviceInfo(selectedDevice).ProductName));
			} else {
				Console.WriteLine("No MIDI devices found.");
			}
		}

		private void handleMidiMessages() {
			midiIn = new MidiIn(selectedDevice);
			midiIn.MessageReceived += midiIn_MessageReceived;
			midiIn.ErrorReceived += midiIn_ErrorReceived;
			midiIn.Start();
		}

		private void midiIn_ErrorReceived(object sender, MidiInMessageEventArgs e) {
			Console.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
				e.Timestamp, e.RawMessage, e.MidiEvent));
		}

		private void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e) {
			if (e.MidiEvent is NoteOnEvent) {
				NoteOnEvent noteOn = (NoteOnEvent)e.MidiEvent;
				AudioPlaybackEngine.Instance.addActiveMIDIKey(noteOn.NoteNumber);
			} else if (e.MidiEvent is NoteEvent) {
				NoteEvent noteOff = (NoteEvent)e.MidiEvent;
				AudioPlaybackEngine.Instance.removeActiveMIDIKey(noteOff.NoteNumber);
			}
		}

		public void Dispose() {
			if (canRead) {
				midiIn.Stop();
				midiIn.Dispose();
				canRead = false;
			}
		}
	}
}
