using System.Net;
using System.Net.Sockets;
/*
class LibSterileSSH.SecureShell.ITcpListenerFactory:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\ServerSocketFactory.cs .

*/
namespace LibSterileSSH.SecureShell
{
	/// <summary>
	/// Summary description for ServerSocketFactory.
	/// </summary>
	public interface ITcpListenerFactory
	{
		TcpListener createServerSocket(int port, int backlog, IPAddress bindAddr);
	}
}
