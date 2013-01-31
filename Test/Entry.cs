using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Nil;
using TransportLayerInsecurity;

namespace Test
{
	class Entry
	{
		const string ConfigurationPath = "Configuration.xml";

		static void Main(string[] arguments)
		{
			var serialiser = new Serialiser<ServerConfiguration>(ConfigurationPath);
			ServerConfiguration configuration = serialiser.Load();
			ServerHandler handler = new ServerHandler(configuration);
			handler.Run();
			ManualResetEvent resetEvent = new ManualResetEvent(false);
			resetEvent.WaitOne();
		}
	}
}
