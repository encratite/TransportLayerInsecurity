using System.Text;

using Nil;
using TransportLayerInsecurity;

namespace LeagueOfLegends
{
	class HTTPServer : IServerEventHandler
	{
		Server Server;

		public HTTPServer(ServerConfiguration configuration)
		{
			Server = new Server(configuration, this);
		}

		public void Run()
		{
			Server.Run();
		}

		void WriteLine(string line, params object[] arguments)
		{
			line = string.Format("[HTTP] {0}", line);
			Output.WriteLine(line, arguments);
		}

		public void OnConnect()
		{
			WriteLine("Connected");
		}

		public void OnLocalDisconnect()
		{
			WriteLine("Local disconnect");
		}

		public void OnRemoteDisconnect()
		{
			WriteLine("Remote disconnect");
		}

		public void OnClientToServerData(byte[] data)
		{
			WriteLine("C->S: {0}", ConvertToString(data));
		}

		public void OnServerToClientData(byte[] data)
		{
			WriteLine("S->C: {0}", ConvertToString(data));
		}

		string ConvertToString(byte[] data)
		{
			return Encoding.ASCII.GetString(data);
		}
	}
}
