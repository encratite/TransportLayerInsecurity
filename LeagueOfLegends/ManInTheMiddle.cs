using TransportLayerInsecurity;

namespace LeagueOfLegends
{
	class ManInTheMiddle
	{
		HTTPServer HTTPServer;
		RTMPServer RTMPServer;

		public ManInTheMiddle(ServerConfiguration httpConfiguration, ServerConfiguration rtmpConfiguration)
		{
			HTTPServer = new HTTPServer(httpConfiguration);
			RTMPServer = new RTMPServer(rtmpConfiguration);
		}

		public void Run()
		{
			HTTPServer.Run();
			RTMPServer.Run();
		}
	}
}
