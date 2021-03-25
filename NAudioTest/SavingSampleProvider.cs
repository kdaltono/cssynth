using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAudioTest {
	class SavingSampleProvider : ISampleProvider, IDisposable {
		private readonly ISampleProvider sourceSampleProvider;
		private readonly WaveFileWriter writer;
		private bool isWriterDisposed;

		public SavingSampleProvider(ISampleProvider sourceSampleProvider, string wavFilePath) {
			this.sourceSampleProvider = sourceSampleProvider;
			writer = new WaveFileWriter(wavFilePath, sourceSampleProvider.WaveFormat);
		}

		public WaveFormat WaveFormat { get { return sourceSampleProvider.WaveFormat; } }

		public int Read(float[] buffer, int offset, int count) {
			var read = sourceSampleProvider.Read(buffer, offset, count);
			if (count > 0 && !isWriterDisposed) {
				writer.WriteSamples(buffer, offset, count);
			}
			return read;
		}

		public void Dispose() {
			if (!isWriterDisposed) {
				isWriterDisposed = true;
				writer.Dispose();
			}
		}
	}
}
