using System;
using System.Collections;
using System.IO;
/*
class LibSterileSSH.SecureShell.SshClient:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\JSch.cs .

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
	public class SshClient
	{

		static System.Collections.Hashtable config;

		public static void Init()
		{
			config = new System.Collections.Hashtable();

			//  config.Add("kex", "diffie-hellman-group-exchange-sha1");
			config.Add("kex", "diffie-hellman-group14-sha1,diffie-hellman-group1-sha1,diffie-hellman-group-exchange-sha1");
			config.Add("server_host_key", "ssh-rsa,ssh-dss");
			//config.Add("server_host_key", "ssh-dss,ssh-rsa");

			config.Add("cipher.s2c", "aes128-ctr,3des-cbc,aes128-cbc");
			config.Add("cipher.c2s", "aes128-ctr,3des-cbc,aes128-cbc");

			//			config.Add("mac.s2c", "hmac-md5,hmac-sha1,hmac-sha1-96,hmac-md5-96");
			//			config.Add("mac.c2s", "hmac-md5,hmac-sha1,hmac-sha1-96,hmac-md5-96");
			config.Add("mac.s2c", "hmac-md5,hmac-sha1");
			config.Add("mac.c2s", "hmac-md5,hmac-sha1");
			config.Add("compression.s2c", "none");
			config.Add("compression.c2s", "none");
			config.Add("lang.s2c", "");
			config.Add("lang.c2s", "");

			config.Add("diffie-hellman-group-exchange-sha1", "LibSterileSSH.Security.DHGEX");
			config.Add("diffie-hellman-group1-sha1", "LibSterileSSH.Security.DHG1");
			config.Add("diffie-hellman-group14-sha1", "LibSterileSSH.Security.DHG14");

			config.Add("dh", "LibSterileSSH.Security.DH");

			config.Add("3des-cbc", "LibSterileSSH.Security.TripleDESCBC");
			config.Add("aes128-cbc", "LibSterileSSH.Security.AES128CBC");
			config.Add("aes128-ctr", "LibSterileSSH.Security.AES128CTR");

			config.Add("sha-1", "LibSterileSSH.Security.SHA1");
			config.Add("md5", "LibSterileSSH.Security.MD5");

			config.Add("hmac-sha1", "LibSterileSSH.Security.HMACSHA1");
			config.Add("hmac-sha1-96", "LibSterileSSH.Security.HMACSHA196");
			config.Add("hmac-md5", "LibSterileSSH.Security.HMACMD5");
			config.Add("hmac-md5-96", "LibSterileSSH.Security.HMACMD596");

			config.Add("signature.dss", "LibSterileSSH.Security.SignatureDSA");
			config.Add("signature.rsa", "LibSterileSSH.Security.SignatureRSA");
			config.Add("keypairgen.dsa", "LibSterileSSH.Security.KeyPairGenDSA");
			config.Add("keypairgen.rsa", "LibSterileSSH.Security.KeyPairGenRSA");

			config.Add("random", "LibSterileSSH.Security.Random");

			//config.Add("zlib",          "com.jcraft.SecureShell.jcraft.Compression");

			config.Add("StrictHostKeyChecking", "ask");
		}

		internal ArrayList pool = new ArrayList();
		internal ArrayList identities = new ArrayList();
		//private KnownHosts known_hosts=null;
		private IHostKeyRepository known_hosts = null;

		public SshClient()
		{
			//known_hosts=new KnownHosts(this);
			if (config == null)
				Init();
		}

		public Session getSession(String username, String host)
		{
			return getSession(username, host, 22);
		}
		public Session getSession(String username, String host, int port)
		{
			Session s = new Session(this);
			s.setUserName(username);
			s.setHost(host);
			s.setPort(port);
			pool.Add(s);
			return s;
		}

		internal bool removeSession(Session session)
		{
			lock (pool)
			{
				pool.Remove(session);
				return true;
			}
		}

		public void setHostKeyRepository(IHostKeyRepository foo)
		{
			known_hosts = foo;
		}
		public void setKnownHosts(String foo)
		{
			if (known_hosts == null)
				known_hosts = new KnownHosts(this);
			if (known_hosts is KnownHosts)
			{
				lock (known_hosts)
				{
					((KnownHosts)known_hosts).setKnownHosts(foo);
				}
			}
		}
		public void setKnownHosts(StreamReader foo)
		{
			if (known_hosts == null)
				known_hosts = new KnownHosts(this);
			if (known_hosts is KnownHosts)
			{
				lock (known_hosts)
				{
					((KnownHosts)known_hosts).setKnownHosts(foo);
				}
			}
		}
		/*
		HostKeyRepository getKnownHosts(){ 
			if(known_hosts==null) known_hosts=new KnownHosts(this);
			return known_hosts; 
		}
		*/
		public IHostKeyRepository getHostKeyRepository()
		{
			if (known_hosts == null)
				known_hosts = new KnownHosts(this);
			return known_hosts;
		}
		/*
		public HostKey[] getHostKey(){
			if(known_hosts==null) return null;
			return known_hosts.getHostKey(); 
		}
		public void removeHostKey(String foo, String type){
			removeHostKey(foo, type, null);
		}
		public void removeHostKey(String foo, String type, byte[] key){
			if(known_hosts==null) return;
			known_hosts.remove(foo, type, key); 
		}
		*/
		public void addIdentity(String foo)
		{
			addIdentity(foo, (String)null);
		}
		public void addIdentity(String foo, String bar)
		{
			IIdentity identity = new IdentityFile(foo, this);
			if (bar != null)
				identity.setPassphrase(bar);
			identities.Add(identity);
		}
		internal String getConfig(String foo)
		{
			return (String)(config[foo]);
		}

		private System.Collections.ArrayList proxies;
		void setProxy(String hosts, IProxy proxy)
		{
			String[] patterns = StringAux.split(hosts, ",");
			if (proxies == null)
			{
				proxies = new System.Collections.ArrayList();
			}
			lock (proxies)
			{
				for (int i = 0; i < patterns.Length; i++)
				{
					if (proxy == null)
					{
						proxies[0] = null;
						proxies[0] = System.Text.Encoding.Default.GetBytes(patterns[i]);
					}
					else
					{
						proxies.Add(System.Text.Encoding.Default.GetBytes(patterns[i]));
						proxies.Add(proxy);
					}
				}
			}
		}
		internal IProxy getProxy(String host)
		{
			if (proxies == null)
				return null;
			byte[] _host = System.Text.Encoding.Default.GetBytes(host);
			lock (proxies)
			{
				for (int i = 0; i < proxies.Count; i += 2)
				{
					if (StringAux.glob(((byte[])proxies[i]), _host))
					{
						return (IProxy)(proxies[i + 1]);
					}
				}
			}
			return null;
		}
		internal void removeProxy()
		{
			proxies = null;
		}

		public static void setConfig(System.Collections.Hashtable foo)
		{
			lock (config)
			{
				System.Collections.IEnumerator e = foo.Keys.GetEnumerator();
				while (e.MoveNext())
				{
					String key = (String)(e.Current);
					config.Add(key, (String)(foo[key]));
				}
			}
		}
	}

}
