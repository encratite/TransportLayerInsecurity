using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace TransportLayerInsecurity
{
    public class Server
    {
		string TargetHost;

		IPEndPoint LocalEndpoint;
		IPEndPoint RemoteEndpoint;

		TcpListener Listener;

		X509Certificate ServerCertificate;

		Thread ServerThread;
		bool Running;

		IServerEventHandler ServerEventHandler;

		SslStream LocalStream;
		SslStream RemoteStream;

		public Server(ServerConfiguration configuration, IServerEventHandler serverEventHandler)
		{
			LocalEndpoint = GetEndpoint(configuration.LocalHost, configuration.LocalPort);
			RemoteEndpoint = GetEndpoint(configuration.RemoteHost, configuration.RemotePort);

			ServerCertificate = new X509Certificate(configuration.CertificatePath, "");

			TargetHost = configuration.TargetHost;

			Listener = new TcpListener(LocalEndpoint);

			ServerThread = new Thread(RunThread);
			ServerThread.Name = "TransportLayerInsecurity Server";

			Running = false;

			ServerEventHandler = serverEventHandler;
		}

		public void Run()
		{
			Listener.Start();
			ServerThread.Start();
			Running = true;
		}

		public void Terminate()
		{
			Running = false;
			Listener.Stop();
			CloseStreams();
			ServerThread.Join();
		}

		IPEndPoint GetEndpoint(string host, int port)
		{
			IPAddress address;
			if (host == string.Empty || host == null)
				address = IPAddress.Any;
			else
				address = Dns.GetHostAddresses(host)[0];
			return new IPEndPoint(address, port);
		}

		void RunThread()
		{
			while (Running)
			{
				TcpClient localClient = Listener.AcceptTcpClient();
				ProcessClient(localClient);
			}
		}

		void CloseStreams()
		{
			LocalStream.Close();
			RemoteStream.Close();
		}

		void ProcessClient(TcpClient localClient)
		{
			LocalStream = new SslStream(localClient.GetStream(), false);
			LocalStream.AuthenticateAsServer(ServerCertificate);

			TcpClient remoteClient = new TcpClient();
			remoteClient.Connect(RemoteEndpoint);
			RemoteStream = new SslStream(remoteClient.GetStream(), false, new RemoteCertificateValidationCallback(TrustAllCertificates));
			RemoteStream.AuthenticateAsClient(TargetHost);

			ServerEventHandler.OnConnect();

			StreamContext localContext = new StreamContext(LocalStream);
			StreamContext remoteContext = new StreamContext(RemoteStream);

			ReadAsynchronously(remoteContext);

			while (Running)
			{
				int bytesRead;
				try
				{
					bytesRead = LocalStream.Read(localContext.Buffer, 0, localContext.Buffer.Length);
					if (bytesRead == 0)
					{
						CloseStreams();
						ServerEventHandler.OnLocalDisconnect();
						return;
					}
				}
				catch (IOException)
				{
					CloseStreams();
					ServerEventHandler.OnLocalDisconnect();
					return;
				}
				try
				{
					byte[] data = localContext.Buffer.Take(bytesRead).ToArray();
					RemoteStream.Write(data);
					ServerEventHandler.OnClientToServerData(data);
				}
				catch (IOException)
				{
					CloseStreams();
					ServerEventHandler.OnRemoteDisconnect();
					return;
				}
			}
		}

		bool TrustAllCertificates(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			return true;
		}

		void ReadAsynchronously(StreamContext context)
		{
			context.Stream.BeginRead(context.Buffer, 0, context.Buffer.Length, OnAsynchronousRead, context);
		}

		void OnAsynchronousRead(IAsyncResult result)
		{
			try
			{
				StreamContext remoteContext = (StreamContext)result.AsyncState;
				int bytesRead = remoteContext.Stream.EndRead(result);
				if (bytesRead == 0)
				{
					CloseStreams();
					ServerEventHandler.OnRemoteDisconnect();
					return;
				}
				byte[] data = remoteContext.Buffer.Take(bytesRead).ToArray();
				LocalStream.Write(data);
				ServerEventHandler.OnServerToClientData(data);
				ReadAsynchronously(remoteContext);
			}
			catch (ObjectDisposedException)
			{
				//This occurs after a disconnect
			}
		}
    }
}
