using System;
using System.Collections;
/*
class LibSterileSSH.SecureShell.ChannelForwardedTCPIP:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\ChannelForwardedTCPIP.cs .

*/
/*
Copyright (c) 2002,2003,2004 ymnk, JCraft,Inc. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

  1. Redistributions of source code must retain the above copyright notice,
	 this list of conditions and the following disclaimer.

  2. Redistributions in binary form must reproduce the above copyright 
	 notice, this list of conditions and the following disclaimer in 
	 the documentation and/or other materials provided with the distribution.

  3. The names of the authors may not be used to endorse or promote products
	 derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
namespace LibSterileSSH.SecureShell
{
	public class ChannelForwardedTCPIP : AChannel
	{

		internal static ArrayList pool = new ArrayList();

		//  static private final int LOCAL_WINDOW_SIZE_MAX=0x20000;
		static private int LOCAL_WINDOW_SIZE_MAX = 0x100000;
		static private int LOCAL_MAXIMUM_PACKET_SIZE = 0x4000;

		internal ISocketFactory factory = null;
		internal String target;
		internal int lport;
		internal int rport;

		internal ChannelForwardedTCPIP()
			: base()
		{
			setLocalWindowSizeMax(LOCAL_WINDOW_SIZE_MAX);
			setLocalWindowSize(LOCAL_WINDOW_SIZE_MAX);
			setLocalPacketSize(LOCAL_MAXIMUM_PACKET_SIZE);
		}

		public override void init()
		{
			try
			{
				io = new IO();
				if (lport == -1)
				{
					Type c = Type.GetType(target);
					IForwardedTCPIPDaemon daemon = (IForwardedTCPIPDaemon)Activator.CreateInstance(c);
					daemon.setChannel(this);
					Object[] foo = getPort(session, rport);
					daemon.setArg((Object[])foo[3]);
					new ThreadAux(daemon).start();
					connected = true;
					return;
				}
				else
				{
					TcpSocket socket = (factory == null) ?
						new TcpSocket(target, lport) :
						factory.createSocket(target, lport);
					socket.setTcpNoDelay(true);
					io.setInputStream(socket.getInputStream());
					io.setOutputStream(socket.getOutputStream());
					connected = true;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("target={0},port={1}", target, lport);
				Console.WriteLine(e);
			}
		}

		public override void run()
		{
			thread = ThreadAux.currentThread();
			Buffer buf = new Buffer(rmpsize);
			Packet packet = new Packet(buf);
			int i = 0;
			try
			{
				while (thread != null && io != null && io.ins != null)
				{
					i = io.ins.Read(buf.buffer,
						14,
						buf.buffer.Length - 14
						- 32 - 20 // padding and mac
						);
					if (i <= 0)
					{
						eof();
						break;
					}
					packet.reset();
					if (_close)
						break;
					buf.putByte((byte)Session.SSH_MSG_CHANNEL_DATA);
					buf.putInt(recipient);
					buf.putInt(i);
					buf.skip(i);
					session.write(packet, this, i);
				}
			}
			catch//(Exception e)
			{
				//System.out.println(e);
			}

			//thread=null;
			//eof();
			disconnect();
		}
		internal override void getData(Buffer buf)
		{
			setRecipient(buf.getInt());
			setRemoteWindowSize(buf.getInt());
			setRemotePacketSize(buf.getInt());
			byte[] addr = buf.getString();
			int port = buf.getInt();
			byte[] orgaddr = buf.getString();
			int orgport = buf.getInt();

			/*
			System.out.println("addr: "+new String(addr));
			System.out.println("port: "+port);
			System.out.println("orgaddr: "+new String(orgaddr));
			System.out.println("orgport: "+orgport);
			*/

			lock (pool)
			{
				for (int i = 0; i < pool.Count; i++)
				{
					Object[] foo = (Object[])(pool[i]);
					if (foo[0] != session)
						continue;
					if (((int)foo[1]) != port)
						continue;
					this.rport = port;
					this.target = (String)foo[2];
					if (foo[3] == null || (foo[3] is Object[]))
					{
						this.lport = -1;
					}
					else
					{
						this.lport = ((int)foo[3]);
					}
					if (foo.Length >= 5)
					{
						this.factory = ((ISocketFactory)foo[4]);
					}
					break;
				}
				if (target == null)
				{
					Console.WriteLine("??");
				}
			}
		}

		internal static Object[] getPort(Session session, int rport)
		{
			lock (pool)
			{
				for (int i = 0; i < pool.Count; i++)
				{
					Object[] bar = (Object[])(pool[i]);
					if (bar[0] != session)
						continue;
					if (((int)bar[1]) != rport)
						continue;
					return bar;
				}
				return null;
			}
		}

		internal static String[] getPortForwarding(Session session)
		{
			ArrayList foo = new ArrayList();
			lock (pool)
			{
				for (int i = 0; i < pool.Count; i++)
				{
					Object[] bar = (Object[])(pool[i]);
					if (bar[0] != session)
						continue;
					if (bar[3] == null)
					{
						foo.Add(bar[1] + ":" + bar[2] + ":");
					}
					else
					{
						foo.Add(bar[1] + ":" + bar[2] + ":" + bar[3]);
					}
				}
			}
			String[] bar2 = new String[foo.Count];
			for (int i = 0; i < foo.Count; i++)
			{
				bar2[i] = (String)(foo[i]);
			}
			return bar2;
		}

		internal static void addPort(Session session, int port, String target, int lport, ISocketFactory factory)
		{
			lock (pool)
			{
				if (getPort(session, port) != null)
				{
					throw new SshClientException("PortForwardingR: remote port " + port + " is already registered.");
				}
				Object[] foo = new Object[5];
				foo[0] = session;
				foo[1] = port;
				foo[2] = target;
				foo[3] = lport;
				foo[4] = factory;
				pool.Add(foo);
			}
		}
		internal static void addPort(Session session, int port, String daemon, Object[] arg)
		{
			lock (pool)
			{
				if (getPort(session, port) != null)
				{
					throw new SshClientException("PortForwardingR: remote port " + port + " is already registered.");
				}
				Object[] foo = new Object[4];
				foo[0] = session;
				foo[1] = port;
				foo[2] = daemon;
				foo[3] = arg;
				pool.Add(foo);
			}
		}
		internal static void delPort(ChannelForwardedTCPIP c)
		{
			delPort(c.session, c.rport);
		}
		internal static void delPort(Session session, int rport)
		{
			lock (pool)
			{
				Object[] foo = null;
				for (int i = 0; i < pool.Count; i++)
				{
					Object[] bar = (Object[])(pool[i]);
					if (bar[0] != session)
						continue;
					if (((int)bar[1]) != rport)
						continue;
					foo = bar;
					break;
				}
				if (foo == null)
					return;
				pool.Remove(foo);
			}

			Buffer buf = new Buffer(100); // ??
			Packet packet = new Packet(buf);

			try
			{
				// byte SSH_MSG_GLOBAL_REQUEST 80
				// string "cancel-tcpip-forward"
				// boolean want_reply
				// string  address_to_bind (e.g. "127.0.0.1")
				// uint32  port number to bind
				packet.reset();
				buf.putByte((byte)80/*SSH_MSG_GLOBAL_REQUEST*/);
				buf.putString(StringAux.getBytesUTF8("cancel-tcpip-forward"));
				buf.putByte((byte)0);
				buf.putString(StringAux.getBytesUTF8("0.0.0.0"));
				buf.putInt(rport);
				session.write(packet);
			}
			catch//(Exception e)
			{
				//    throw new JSchException(e.toString());
			}
		}
		internal static void delPort(Session session)
		{
			int[] rport = null;
			int count = 0;
			lock (pool)
			{
				rport = new int[pool.Count];
				for (int i = 0; i < pool.Count; i++)
				{
					Object[] bar = (Object[])(pool[i]);
					if (bar[0] == session)
					{
						rport[count++] = ((int)bar[1]);
					}
				}
			}
			for (int i = 0; i < count; i++)
			{
				delPort(session, rport[i]);
			}
		}
		public int getRemotePort()
		{
			return rport;
		}
		void setSocketFactory(ISocketFactory factory)
		{
			this.factory = factory;
		}
	}

}
