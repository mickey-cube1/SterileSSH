using System;
using System.IO;
using System.Text;
/*
class LibSterileSSH.SecureShell.ProxyHTTP:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\ProxyHTTP.cs .

*/
namespace LibSterileSSH.SecureShell
{
	public class ProxyHTTP : IProxy
	{
		private static int DEFAULTPORT = 80;
		private String proxy_host;
		private int proxy_port;
		private Stream ins;
		private Stream outs;
		private TcpSocket socket;

		private String user;
		private String passwd;

		public ProxyHTTP(String proxy_host)
		{
			int port = DEFAULTPORT;
			String host = proxy_host;
			if (proxy_host.IndexOf(':') != -1) {
				try {
					host = proxy_host.Substring(0, proxy_host.IndexOf(':'));
					port = int.Parse(proxy_host.Substring(proxy_host.IndexOf(':') + 1));
				}
				catch (RuntimeException) {
				}
			}
			this.proxy_host = host;
			this.proxy_port = port;
		}
		public ProxyHTTP(String proxy_host, int proxy_port)
		{
			this.proxy_host = proxy_host;
			this.proxy_port = proxy_port;
		}
		public void setUserPasswd(String user, String passwd)
		{
			this.user = user;
			this.passwd = passwd;
		}
		public void connect(ISocketFactory socket_factory, String host, int port, int timeout)
		{
			try {
				if (socket_factory == null) {
					socket = TcpSocketCreator.CreateSocket(proxy_host, proxy_port, timeout);
					ins = socket.getInputStream();
					outs = socket.getOutputStream();
				}
				else {
					socket = socket_factory.createSocket(proxy_host, proxy_port);
					ins = socket_factory.getInputStream(socket);
					outs = socket_factory.getOutputStream(socket);
				}
				if (timeout > 0) {
					socket.setSoTimeout(timeout);
				}
				socket.setTcpNoDelay(true);

				StreamAux.write(outs, StringAux.getBytesUTF8("CONNECT " + host + ":" + port + " HTTP/1.0\r\n"));

				if (user != null && passwd != null) {
					byte[] _code = StringAux.getBytesUTF8((user + ":" + passwd));
					_code = StringAux.toBase64(_code, 0, _code.Length);
					StreamAux.write(outs, StringAux.getBytesUTF8("Proxy-Authorization: Basic "));
					StreamAux.write(outs, _code);
					StreamAux.write(outs, StringAux.getBytesUTF8("\r\n"));
				}

				StreamAux.write(outs, StringAux.getBytesUTF8("\r\n"));
				StreamAux.flush(outs);

				int foo = 0;

				StringBuilder sb = new StringBuilder();
				while (foo >= 0) {
					foo = ins.ReadByte();
					if (foo != 13) {
						sb.Append((char)foo);
						continue;
					}
					foo = ins.ReadByte();
					if (foo != 10) {
						continue;
					}
					break;
				}
				if (foo < 0) {
					throw new System.IO.IOException();
				}

				String response = sb.ToString();
				String reason = "Unknow reason";
				int code = -1;
				try {
					foo = response.IndexOf(' ');
					int bar = response.IndexOf(' ', foo + 1);
					code = int.Parse(response.Substring(foo + 1, bar - (foo + 1)));
					reason = response.Substring(bar + 1);
				}
				catch//(JException e)
				{
				}
				if (code != 200) {
					throw new System.IO.IOException("proxy error: " + reason);
				}

				/*
				while(foo>=0){
				  foo=in.read(); if(foo!=13) continue;
				  foo=in.read(); if(foo!=10) continue;
				  foo=in.read(); if(foo!=13) continue;
				  foo=in.read(); if(foo!=10) continue;
				  break;
				}
				*/

				int count = 0;
				while (true) {
					count = 0;
					while (foo >= 0) {
						foo = ins.ReadByte();
						if (foo != 13) {
							count++;
							continue;
						}
						foo = ins.ReadByte();
						if (foo != 10) {
							continue;
						}
						break;
					}
					if (foo < 0) {
						throw new System.IO.IOException();
					}
					if (count == 0)
						break;
				}
			}
			catch (RuntimeException e) {
				try {
					if (socket != null)
						socket.close();
				}
				catch//(JException eee)
				{
				}
				String message = "ProxyHTTP: " + e.ToString();
				throw e;
			}
		}
		public Stream getInputStream()
		{
			return ins;
		}
		public Stream getOutputStream()
		{
			return outs;
		}
		public TcpSocket getSocket()
		{
			return socket;
		}
		public void close()
		{
			try {
				if (ins != null)
					ins.Close();
				if (outs != null)
					outs.Close();
				if (socket != null)
					socket.close();
			}
			catch (RuntimeException) {
			}
			ins = null;
			outs = null;
			socket = null;
		}
		public static int getDefaultPort()
		{
			return DEFAULTPORT;
		}
	}
}
