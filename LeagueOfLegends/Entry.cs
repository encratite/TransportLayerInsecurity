using System.Threading;

using Nil;
using TransportLayerInsecurity;

namespace LeagueOfLegends
{
	class Entry
	{
		static ServerConfiguration LoadConfiguration(string path)
		{
			var serialiser = new Serialiser<ServerConfiguration>(path);
			return serialiser.Load();
		}

		static void Main(string[] arguments)
		{
			ManInTheMiddle mitm = new ManInTheMiddle(LoadConfiguration("HTTP.xml"), LoadConfiguration("RTMP.xml"));
			mitm.Run();
			ManualResetEvent resetEvent = new ManualResetEvent(false);
			resetEvent.WaitOne();
		}
	}
}
