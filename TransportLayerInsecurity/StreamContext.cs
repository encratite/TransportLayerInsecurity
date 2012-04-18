using System.Net.Security;

namespace TransportLayerInsecurity
{
	class StreamContext
	{
		const int BufferSize = 4096;

		public SslStream Stream { get; private set; }
		public byte[] Buffer { get; private set; }

		public StreamContext(SslStream stream)
		{
			Stream = stream;
			Buffer = new byte[BufferSize];
		}
	}
}
