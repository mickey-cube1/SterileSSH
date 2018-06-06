using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
/*
class LibSterileSSH.TcpSocket:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\java\net\Socket.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for Socket.
	/// </summary>
	public class TcpSocket
	{
		internal Socket sock;

		protected void SetSocketOption(SocketOptionLevel level, SocketOptionName name, int val)
		{
			try {
				sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, val);
			}
			catch {
			}
		}

		//		public Socket(AddressFamily af, SocketType st, ProtocolType pt)
		//		{
		//			this.sock = new Sock(af, st, pt);
		//			this.sock.Connect();
		//		}

		public TcpSocket(string host, int port)
		{
			IPEndPoint ep = null;
			try {
				ep = new IPEndPoint(IPAddress.Parse(host), port);
			}
			catch (Exception) {
				ep = new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], port);
			}
			this.sock = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			this.sock.Connect(ep);
		}

		public TcpSocket(Socket sock)
		{
			this.sock = sock;
		}

		public Stream getInputStream()
		{
			return new System.Net.Sockets.NetworkStream(sock);
		}

		public Stream getOutputStream()
		{
			return new System.Net.Sockets.NetworkStream(sock);
		}

		public bool isConnected()
		{
			return sock.Connected;
		}

		public void setTcpNoDelay(bool b)
		{
			if (b) {
				SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);
			}
			else {
				SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 0);
			}
		}

		public void setSoTimeout(int t)
		{
			SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, t);
			SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, t);
		}

		public void close()
		{
			sock.Close();
		}

		public IPAddress getInetAddress()
		{
			return ((IPEndPoint)sock.RemoteEndPoint).Address;
		}

		public int getPort()
		{
			return ((IPEndPoint)sock.RemoteEndPoint).Port;
		}
	}
}
