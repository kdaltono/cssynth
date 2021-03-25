using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAudioTest.Recording {

	// TODO: Need to find a way to test this properly. Currently this won't
	// work and will only be able to record audio for a single note. Need to find a way to support ISampleProvider instead.

	class SavingProvider : IWaveProvider, IDisposable {

		private readonly IWaveProvider sourceWaveProvider;
		private readonly WaveFileWriter writer;
		private bool isWriterDisposed;

		public SavingProvider(IWaveProvider sourceWaveProvider, string wavFilePath) {
			this.sourceWaveProvider = sourceWaveProvider;
			writer = new WaveFileWriter(wavFilePath, sourceWaveProvider.WaveFormat);
		}

		public int Read(byte[] buffer, int offset, int count) {
			var read = sourceWaveProvider.Read(buffer, offset, count);
			if (count > 0 && !isWriterDisposed) {
				writer.Write(buffer, offset, read);
			}
			if (count == 0) {
				Dispose();
			}
			return read;
		}

		public WaveFormat WaveFormat {
			get {
				return sourceWaveProvider.WaveFormat;
			}
		}

		public void Dispose() {
			if (!isWriterDisposed) {
				isWriterDisposed = true;
				writer.Dispose();
			}
		}

	}
}
