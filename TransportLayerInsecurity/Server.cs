using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
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

		public Server(string localHost, int localPort, string remoteHost, int remotePort, string certificatePath, string targetHost, IServerEventHandler serverEventHandler)
		{
			LocalEndpoint = GetEndpoint(localHost, localPort);
			RemoteEndpoint = GetEndpoint(remoteHost, remotePort);

			ServerCertificate = new X509Certificate(certificatePath);

			TargetHost = targetHost;

			Listener = new TcpListener(LocalEndpoint);

			ServerThread = new Thread(RunThread);
			ServerThread.Name = "TransportLayerInsecurity Server";

			Running = false;

			ServerEventHandler = serverEventHandler;
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
			LocalStream.Close();
			RemoteStream.Close();
			ServerThread.Join();
		}

		void RunThread()
		{
			while (Running)
			{
				TcpClient localClient = Listener.AcceptTcpClient();
				ProcessClient(localClient);
			}
		}

		void ProcessClient(TcpClient localClient)
		{
			LocalStream = new SslStream(localClient.GetStream(), false);
			LocalStream.AuthenticateAsServer(ServerCertificate);

			TcpClient remoteClient = new TcpClient(RemoteEndpoint);
			RemoteStream = new SslStream(remoteClient.GetStream(), false, new RemoteCertificateValidationCallback(TrustAllCertificates));
			RemoteStream.AuthenticateAsClient(TargetHost);

			ServerEventHandler.OnConnect();

			StreamContext localContext = new StreamContext(LocalStream);
			StreamContext remoteContext = new StreamContext(RemoteStream);

			Read(remoteContext);

			while (Running)
			{
				int bytesRead = LocalStream.Read(localContext.Buffer, 0, localContext.Buffer.Length);
				if (bytesRead == 0)
				{
					RemoteStream.Close();
					ServerEventHandler.OnLocalDisconnect();
					return;
				}
				RemoteStream.Write(localContext.Buffer, 0, bytesRead);
				byte[] data = localContext.Buffer.Take(bytesRead).ToArray();
				ServerEventHandler.OnClientToServerData(data);
			}
		}

		bool TrustAllCertificates(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			return true;
		}

		void Read(StreamContext context)
		{
			context.Stream.BeginRead(context.Buffer, 0, context.Buffer.Length, OnRead, context);
		}

		void OnRead(IAsyncResult result)
		{
			StreamContext remoteContext = (StreamContext)result.AsyncState;
			int bytesRead = remoteContext.Stream.EndRead(result);
			if (bytesRead == 0)
			{
				LocalStream.Close();
				ServerEventHandler.OnRemoteDisconnect();
				return;
			}
			byte[] data = remoteContext.Buffer.Take(bytesRead).ToArray();
			ServerEventHandler.OnServerToClientData(data);
			Read(remoteContext);
		}
    }
}
