﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using Belikov.GenuineChannels.GenuineTcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenuineChannels.UnitTests
{
	[TestClass]
	public class RegressionTests
	{
		[TestMethod]
		public void Issue2_StopLoggingNullReferenceException()
		{
			// enable logging
			Belikov.GenuineChannels.Logbook.GenuineLoggingServices.SetUpLoggingToMemory(1024, null);

			// start server
			var serverChannel = RegisterChannel(server: true);
			var service = new Service();
			var objref = RemotingServices.Marshal(service, "Service");

			// start client
			var clientChannel = RegisterChannel();
			var proxy = (IService)Activator.GetObject(typeof(IService), "gtcp://localhost:8737/Service");
			var greeting = proxy.Greeting("World");

			// stop server
			RemotingServices.Unmarshal(objref);
			ChannelServices.UnregisterChannel(serverChannel);
			ChannelServices.UnregisterChannel(clientChannel);
		}

		private static IChannel RegisterChannel(bool server = false)
		{
			var props = new Dictionary<string, string>
			{
				{ "name", "GTCP1" + Guid.NewGuid() },
				{ "priority", "100" }
			};

			if (server)
			{
				props["port"] = "8737";
			}

			var channel = new GenuineTcpChannel(props, null, null);
			ChannelServices.RegisterChannel(channel, false);
			return channel;
		}

		internal class Service : MarshalByRefObject, IService
		{
			public string Greeting(string s)
			{
				Belikov.GenuineChannels.Logbook.GenuineLoggingServices.StopLogging();
				var result = $"Hello, {s}!";
				Console.WriteLine("Replying to client: {0}", result);
				return result;
			}
		}

		public interface IService
		{
			string Greeting(string s);
		}

	}
}
