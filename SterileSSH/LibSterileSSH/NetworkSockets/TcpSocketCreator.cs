using System;
using System.Threading;
using LibSterileSSH.Security;
/*
class LibSterileSSH.SecureShell.TcpSocketCreator:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\Util.cs .

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
	public class TcpSocketCreator
	{
		internal static TcpSocket CreateSocket(String host, int port, int timeout)
		{
			TcpSocket socket = null;
			String message = "";
			if (timeout == 0) {
				try {
					socket = new TcpSocket(host, port);
					return socket;
				}
				catch (Exception e) {
					message = e.ToString();
					throw new SshClientException(message);
				}
			}
			String _host = host;
			int _port = port;
			TcpSocket[] sockp = new TcpSocket[1];
			Thread currentThread = Thread.CurrentThread;
			Exception[] ee = new Exception[1];
			message = "";
			createSocketRun runnable = new createSocketRun(sockp, ee, _host, _port);
			Thread tmp = new Thread(new ThreadStart(runnable.run));
			tmp.Name = "Opening Socket " + host;
			tmp.Start();
			try {
				tmp.Join(timeout);
				message = "timeout: ";
			}
			catch (ThreadInterruptedException) {
			}
			if (sockp[0] != null && sockp[0].isConnected()) {
				socket = sockp[0];
			}
			else {
				message += "socket is not established";
				if (ee[0] != null) {
					message = ee[0].ToString();
				}
				tmp.Interrupt();
				tmp = null;
				throw new SshClientException(message);
			}
			return socket;
		}

		private class createSocketRun
		{
			TcpSocket[] sockp;
			Exception[] ee;
			string _host;
			int _port;

			public createSocketRun(TcpSocket[] sockp, Exception[] ee, string _host, int _port)
			{
				this.sockp = sockp;
				this.ee = ee;
				this._host = _host;
				this._port = _port;
			}

			public void run()
			{
				sockp[0] = null;
				try {
					sockp[0] = new TcpSocket(_host, _port);
				}
				catch (Exception e) {
					ee[0] = e;
					if (sockp[0] != null && sockp[0].isConnected()) {
						try {
							sockp[0].close();
						}
						catch (Exception) {
						}
					}
					sockp[0] = null;
				}
			}
		}
	}
}
/* -*-mode:java; c-basic-offset:2; -*- */
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


