namespace TransportLayerInsecurity
{
	public interface IServerEventHandler
	{
		void OnConnect();
		void OnLocalDisconnect();
		void OnRemoteDisconnect();

		void OnClientToServerData(byte[] data);
		void OnServerToClientData(byte[] data);
	}
}
