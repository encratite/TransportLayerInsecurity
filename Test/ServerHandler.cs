using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TransportLayerInsecurity;

namespace Test
{
	class ServerHandler : IServerEventHandler
	{
		Server Server;

		public ServerHandler(Configuration configuration)
		{
			Server = new Server(configuration.LocalHost, configuration.LocalPort, configuration.RemoteHost, configuration.RemotePort, configuration.CertificatePath, configuration.TargetHost, this);
		}

		public void Run()
		{
			Server.Run();
		}

		void WriteLine(string line, params object[] arguments)
		{
			Nil.Output.WriteLine(line, arguments);
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
			WriteLine("C->S: {0}", data.Length);
		}

		public void OnServerToClientData(byte[] data)
		{
			WriteLine("S->C: {0}", data.Length);
		}
	}
}
