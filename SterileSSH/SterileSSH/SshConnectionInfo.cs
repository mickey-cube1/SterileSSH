using System;
using System.Collections;

/*
Copyright (c) 2018 micky-cube1 All rights reserved.

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
namespace SterileSSH
{
	public class SshConnectionInfo
	{
		public enum SchemeType
		{
			ERROR,
			ANON,
			SSH,
			SCP,
			SFTP
		}

		public SchemeType Scheme;
		public String Host;
		public String User;
		public String Pass;
		public String IdentityFile;

		public SshConnectionInfo()
		{
		}
		public SshConnectionInfo(String p)
		{
			Scheme = SchemeType.ANON;

			String user = null;
			String host = null;

			if (p.StartsWith("ssh://")) {
				Scheme = SchemeType.SSH;
				p = p.Substring(6);
			}
			else if (p.StartsWith("scp://")) {
				Scheme = SchemeType.SCP;
				p = p.Substring(6);
			}
			else if (p.StartsWith("sftp://")) {
				Scheme = SchemeType.SFTP;
				p = p.Substring(7);
			}

			Int32 ip = p.IndexOf(':');

			Int32 ii = p.IndexOf('@');
			/* Avoid initial @ */
			if (ii == 0) {
				p = p.Substring(1);
				ii = -1;
			}
			if (ii > 0) {
				user = p.Substring(0, ii);
				host = p.Substring(ii + 1);
			}
			else {
				user = null;
				host = p;
			}

			Host = host;
			User = user;
		}
	}
}
