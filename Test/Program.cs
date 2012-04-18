using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Test
{
	class Program
	{
		const string ConfigurationPath = "Configuration.xml";

		static void Main(string[] arguments)
		{
			var serialiser = new Nil.Serialiser<Configuration>(ConfigurationPath);
			Configuration configuration = serialiser.Load();
			ServerHandler handler = new ServerHandler(configuration);
			handler.Run();
			ManualResetEvent resetEvent = new ManualResetEvent(false);
			resetEvent.WaitOne();
		}
	}
}
