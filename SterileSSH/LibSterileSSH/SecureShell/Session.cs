using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LibSterileSSH.SecureShell;
using LibSterileSSH.Security;
using Exception = System.Exception;
using NullReferenceException = System.NullReferenceException;
using ThreadInterruptedException = System.Threading.ThreadInterruptedException;
/*
class LibSterileSSH.SecureShell.Session:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\Session.cs .

*/
namespace LibSterileSSH.SecureShell
{


	public class Session : IRunnable
	{
		private static String version = "LibSterileSSH-SSH-" + LibSterileSSH.SshBase.Version.ToString();

		// http://ietf.org/internet-drafts/draft-ietf-secsh-assignednumbers-01.txt
		internal const int SSH_MSG_DISCONNECT = 1;
		internal const int SSH_MSG_IGNORE = 2;
		internal const int SSH_MSG_UNIMPLEMENTED = 3;
		internal const int SSH_MSG_DEBUG = 4;
		internal const int SSH_MSG_SERVICE_REQUEST = 5;
		internal const int SSH_MSG_SERVICE_ACCEPT = 6;
		internal const int SSH_MSG_KEXINIT = 20;
		internal const int SSH_MSG_NEWKEYS = 21;
		internal const int SSH_MSG_KEXDH_INIT = 30;
		internal const int SSH_MSG_KEXDH_REPLY = 31;
		internal const int SSH_MSG_KEX_DH_GEX_GROUP = 31;
		internal const int SSH_MSG_KEX_DH_GEX_INIT = 32;
		internal const int SSH_MSG_KEX_DH_GEX_REPLY = 33;
		internal const int SSH_MSG_KEX_DH_GEX_REQUEST = 34;
		internal const int SSH_MSG_USERAUTH_REQUEST = 50;
		internal const int SSH_MSG_USERAUTH_FAILURE = 51;
		internal const int SSH_MSG_USERAUTH_SUCCESS = 52;
		internal const int SSH_MSG_USERAUTH_BANNER = 53;
		internal const int SSH_MSG_USERAUTH_INFO_REQUEST = 60;
		internal const int SSH_MSG_USERAUTH_INFO_RESPONSE = 61;
		internal const int SSH_MSG_USERAUTH_PK_OK = 60;
		internal const int SSH_MSG_GLOBAL_REQUEST = 80;
		internal const int SSH_MSG_REQUEST_SUCCESS = 81;
		internal const int SSH_MSG_REQUEST_FAILURE = 82;
		internal const int SSH_MSG_CHANNEL_OPEN = 90;
		internal const int SSH_MSG_CHANNEL_OPEN_CONFIRMATION = 91;
		internal const int SSH_MSG_CHANNEL_OPEN_FAILURE = 92;
		internal const int SSH_MSG_CHANNEL_WINDOW_ADJUST = 93;
		internal const int SSH_MSG_CHANNEL_DATA = 94;
		internal const int SSH_MSG_CHANNEL_EXTENDED_DATA = 95;
		internal const int SSH_MSG_CHANNEL_EOF = 96;
		internal const int SSH_MSG_CHANNEL_CLOSE = 97;
		internal const int SSH_MSG_CHANNEL_REQUEST = 98;
		internal const int SSH_MSG_CHANNEL_SUCCESS = 99;
		internal const int SSH_MSG_CHANNEL_FAILURE = 100;

		private byte[] V_S;                                 // server version
		private byte[] V_C = StringAux.getBytesUTF8("SSH-2.0-" + version); // client version

		private byte[] I_C; // the payload of the client's SSH_MSG_KEXINIT
		private byte[] I_S; // the payload of the server's SSH_MSG_KEXINIT
		//		private byte[] K_S; // the host key

		private byte[] session_id;

		private byte[] IVc2s;
		private byte[] IVs2c;
		private byte[] Ec2s;
		private byte[] Es2c;
		private byte[] MACc2s;
		private byte[] MACs2c;

		private int seqi = 0;
		private int seqo = 0;

		private ICipher s2ccipher;
		private ICipher c2scipher;
		private IHMAC s2cmac;
		private IHMAC c2smac;
		private byte[] mac_buf;

		private ICompression deflater;
		private ICompression inflater;

		private IO io;
		private TcpSocket socket;
		private int timeout = 0;

		private bool _isConnected = false;

		private bool isAuthed = false;

		private ThreadAux connectThread = null;

		internal bool x11_forwarding = false;

		internal Stream In = null;
		internal Stream Out = null;

		internal static IRandom random;

		internal Buffer buf;
		internal Packet packet;

		internal ISocketFactory socket_factory = null;

		private Hashtable config = null;

		private IProxy proxy = null;
		private IUserInfo userinfo;

		internal String host = "127.0.0.1";
		internal int port = 22;

		internal String username = null;
		internal String password = null;

		internal SshClient jsch;

		internal Session(SshClient jsch)
		{
			;
			this.jsch = jsch;
			buf = new Buffer();
			packet = new Packet(buf);
		}

		public void connect()
		{
			connect(timeout);
		}

		public void connect(int connectTimeout)
		{
			if (_isConnected) {
				throw new SshClientException("session is already connected");
			}
			io = new IO();
			if (random == null) {
				try {
					System.Type c = System.Type.GetType(getConfig("random"));
					random = (IRandom)(System.Activator.CreateInstance(c));
				}
				catch (Exception e) {
					System.Console.Error.WriteLine("connect: random " + e);
				}
			}
			Packet.setRandom(random);

			try {
				int i, j;
				//int pad=0;

				if (proxy == null) {
					proxy = jsch.getProxy(host);
					if (proxy != null) {
						lock (proxy) {
							proxy.close();
						}
					}
				}

				if (proxy == null) {
					Stream In;
					Stream Out;
					if (socket_factory == null) {
						socket = TcpSocketCreator.CreateSocket(host, port, connectTimeout);
						In = socket.getInputStream();
						Out = socket.getOutputStream();
					}
					else {
						socket = socket_factory.createSocket(host, port);
						In = socket_factory.getInputStream(socket);
						Out = socket_factory.getOutputStream(socket);
					}
					//if(timeout>0){ socket.setSoTimeout(timeout); }
					socket.setTcpNoDelay(true);
					io.setInputStream(In);
					io.setOutputStream(Out);
				}
				else {
					lock (proxy) {
						proxy.connect(socket_factory, host, port, connectTimeout);
						io.setInputStream(proxy.getInputStream());
						io.setOutputStream(proxy.getOutputStream());
						socket = proxy.getSocket();
					}
				}

				if (connectTimeout > 0 && socket != null) {
					socket.setSoTimeout(connectTimeout);
				}

				_isConnected = true;

				while (true) {

					i = 0;
					j = 0;
					while (i < buf.buffer.Length) {
						j = io.getByte();
						if (j < 0)
							break;
						buf.buffer[i] = (byte)j;
						i++;
						if (j == 10)
							break;
					}
					if (j < 0) {
						throw new SshClientException("connection is closed by foreign host");
					}

					if (buf.buffer[i - 1] == 10) {    // 0x0a
						i--;
						if (buf.buffer[i - 1] == 13) {  // 0x0d
							i--;
						}
					}

					if (i > 4 && (i != buf.buffer.Length) &&
						(buf.buffer[0] != 'S' || buf.buffer[1] != 'S' ||
						buf.buffer[2] != 'H' || buf.buffer[3] != '-')) {
						//System.err.println(new String(buf.buffer, 0, i);
						continue;
					}

					if (i == buf.buffer.Length ||
						i < 7 ||                                      // SSH-1.99 or SSH-2.0
						(buf.buffer[4] == '1' && buf.buffer[6] != '9')  // SSH-1.5
						) {
						throw new SshClientException("invalid server's version String");
					}
					break;
				}

				V_S = new byte[i];
				System.Array.Copy(buf.buffer, 0, V_S, 0, i);
				//System.Console.WriteLine("V_S: ("+i+") ["+new String(V_S)+"]");

				//io.put(V_C, 0, V_C.Length); io.put("\n".getBytes(), 0, 1);
				{
					// Some Cisco devices will miss to read '\n' if it is sent separately.
					byte[] foo = new byte[V_C.Length + 1];
					System.Array.Copy(V_C, 0, foo, 0, V_C.Length);
					foo[foo.Length - 1] = (byte)'\n';
					io.put(foo, 0, foo.Length);
				}

				buf = read(buf);
				//System.Console.WriteLine("read: 20 ? "+buf.buffer[5]);
				if (buf.buffer[5] != SSH_MSG_KEXINIT) {
					throw new SshClientException("invalid protocol: " + buf.buffer[5]);
				}
				KeyExchange kex = receive_kexinit(buf);

				while (true) {
					buf = read(buf);
					if (kex.getState() == buf.buffer[5]) {
						bool result = kex.next(buf);
						if (!result) {
							//System.Console.WriteLine("verify: "+result);
							in_kex = false;
							throw new SshClientException("verify: " + result);
						}
					}
					else {
						in_kex = false;
						throw new SshClientException("invalid protocol(kex): " + buf.buffer[5]);
					}
					if (kex.getState() == KeyExchange.STATE_END) {
						break;
					}
				}

				try {
					checkHost(host, kex);
				}
				catch (SshClientException ee) {
					in_kex = false;
					throw ee;
				}

				send_newkeys();

				// receive SSH_MSG_NEWKEYS(21)
				buf = read(buf);
				//System.Console.WriteLine("read: 21 ? "+buf.buffer[5]);
				if (buf.buffer[5] == SSH_MSG_NEWKEYS) {
					receive_newkeys(buf, kex);
				}
				else {
					in_kex = false;
					throw new SshClientException("invalid protocol(newkyes): " + buf.buffer[5]);
				}

				bool auth = false;
				bool auth_cancel = false;

				UserAuthNone usn = new UserAuthNone(userinfo);
				auth = usn.start(this);

				String methods = null;
				if (!auth) {
					methods = usn.getMethods();
					if (methods != null) {
						methods = methods.ToLower();
					}
					else {
						// methods: publickey,password,keyboard-interactive
						methods = "publickey,password,keyboard-interactive";
					}
				}

				//loop:
				while (true) {

					//System.Console.WriteLine("methods: "+methods);

					while (!auth &&
						methods != null && methods.Length > 0) {

						//System.Console.WriteLine("  methods: "+methods);

						UserAuth us = null;
						if (methods.StartsWith("publickey")) {
							//System.Console.WriteLine("   SecureShell.identities.size()="+SecureShell.identities.size());
							lock (jsch.identities) {
								if (jsch.identities.Count > 0) {
									us = new UserAuthPublicKey(userinfo);
								}
							}
						}
						else if (methods.StartsWith("keyboard-interactive")) {
							if (userinfo is IUIKeyboardInteractive) {
								us = new UserAuthKeyboardInteractive(userinfo);
							}
						}
						else if (methods.StartsWith("password")) {
							us = new UserAuthPassword(userinfo);
						}
						if (us != null) {
							try {
								auth = us.start(this);
								auth_cancel = false;
							}
							catch (SshClientAuthCancelException) {
								//System.Console.WriteLine(ee);
								auth_cancel = true;
							}
							catch (SshClientPartialAuthException ee) {
								methods = ee.getMethods();
								//System.Console.WriteLine("PartialAuth: "+methods);
								auth_cancel = false;
								continue;//loop;
							}
							catch (Exception ee) {
								System.Console.WriteLine("ee: " + ee); // SSH_MSG_DISCONNECT: 2 Too many authentication failures
							}
						}
						if (!auth) {
							int comma = methods.IndexOf(",");
							if (comma == -1)
								break;
							methods = methods.Substring(comma + 1);
						}
					}
					break;
				}

				if (connectTimeout > 0 || timeout > 0) {
					socket.setSoTimeout(timeout);
				}

				if (auth) {
					isAuthed = true;
					connectThread = new ThreadAux(this);
					connectThread.setName("Connect thread " + host + " session");
					connectThread.start();
					return;
				}
				if (auth_cancel)
					throw new SshClientException("Auth cancel");
				throw new SshClientException("Auth fail");
			}
			catch (Exception e) {
				in_kex = false;
				if (_isConnected) {
					try {
						packet.reset();
						buf.putByte((byte)SSH_MSG_DISCONNECT);
						buf.putInt(3);
						buf.putString(StringAux.getBytesUTF8(e.ToString()));
						buf.putString(StringAux.getBytesUTF8("en"));
						write(packet);
						disconnect();
					}
					catch (Exception) {
					}
				}
				_isConnected = false;
				//e.printStackTrace();
				if (e is SshClientException)
					throw (SshClientException)e;
				throw new SshClientException("Session.connect: " + e);
			}
		}

		private KeyExchange receive_kexinit(Buffer buf)
		{
			int j = buf.getInt();
			if (j != buf.getLength()) {    // packet was compressed and
				buf.getByte();           // j is the size of deflated packet.
				I_S = new byte[buf.index - 5];
			}
			else {
				I_S = new byte[j - 1 - buf.getByte()];
			}
			System.Array.Copy(buf.buffer, buf.s, I_S, 0, I_S.Length);
			/*
			try{
			byte[] tmp=new byte[I_S.Length];
			LibSterileSSH.java.System.arraycopy(I_S, 0, tmp, 0, I_S.Length);
			Buffer tmpb=new Buffer(tmp);
			System.Console.WriteLine("I_S: len="+I_S.Length);
			tmpb.setOffSet(17);
			System.Console.WriteLine("kex: "+new String(tmpb.getString()));
			System.Console.WriteLine("server_host_key: "+new String(tmpb.getString()));
			System.Console.WriteLine("cipher.c2s: "+new String(tmpb.getString()));
			System.Console.WriteLine("cipher.s2c: "+new String(tmpb.getString()));
			System.Console.WriteLine("mac.c2s: "+new String(tmpb.getString()));
			System.Console.WriteLine("mac.s2c: "+new String(tmpb.getString()));
			System.Console.WriteLine("compression.c2s: "+new String(tmpb.getString()));
			System.Console.WriteLine("compression.s2c: "+new String(tmpb.getString()));
			System.Console.WriteLine("lang.c2s: "+new String(tmpb.getString()));
			System.Console.WriteLine("lang.s2c: "+new String(tmpb.getString()));
			System.Console.WriteLine("?: "+(tmpb.getByte()&0xff));
			System.Console.WriteLine("??: "+tmpb.getInt());
			}
			catch(Exception e){
			System.Console.WriteLine(e);
			}
			*/

			send_kexinit();
			String[] guess = KeyExchange.guess(I_S, I_C);
			if (guess == null) {
				throw new SshClientException("Algorithm negotiation fail");
			}

			if (!isAuthed &&
				(guess[KeyExchange.PROPOSAL_ENC_ALGS_CTOS].Equals("none") ||
				(guess[KeyExchange.PROPOSAL_ENC_ALGS_STOC].Equals("none")))) {
				throw new SshClientException("NONE Cipher should not be chosen before authentification is successed.");
			}

			KeyExchange kex = null;
			try {
				System.Type c = System.Type.GetType(getConfig(guess[KeyExchange.PROPOSAL_KEX_ALGS]));
				kex = (KeyExchange)(System.Activator.CreateInstance(c));
			}
			catch (Exception e) {
				System.Console.Error.WriteLine("kex: " + e);
			}
			kex._guess = guess;
			kex.init(this, V_S, V_C, I_S, I_C);
			return kex;
		}

		private bool in_kex = false;
		public void rekey()
		{
			send_kexinit();
		}
		private void send_kexinit()
		{
			if (in_kex)
				return;
			in_kex = true;

			// byte      SSH_MSG_KEXINIT(20)
			// byte[16]  cookie (random bytes)
			// String    kex_algorithms
			// String    server_host_key_algorithms
			// String    encryption_algorithms_client_to_server
			// String    encryption_algorithms_server_to_client
			// String    mac_algorithms_client_to_server
			// String    mac_algorithms_server_to_client
			// String    compression_algorithms_client_to_server
			// String    compression_algorithms_server_to_client
			// String    languages_client_to_server
			// String    languages_server_to_client
			packet.reset();
			buf.putByte((byte)SSH_MSG_KEXINIT);
			lock (random) {
				random.fill(buf.buffer, buf.index, 16);
				buf.skip(16);
			}
			buf.putString(StringAux.getBytesUTF8(getConfig("kex")));
			buf.putString(StringAux.getBytesUTF8(getConfig("server_host_key")));
			buf.putString(StringAux.getBytesUTF8(getConfig("cipher.c2s")));
			buf.putString(StringAux.getBytesUTF8(getConfig("cipher.s2c")));
			buf.putString(StringAux.getBytesUTF8(getConfig("mac.c2s")));
			buf.putString(StringAux.getBytesUTF8(getConfig("mac.s2c")));
			buf.putString(StringAux.getBytesUTF8(getConfig("compression.c2s")));
			buf.putString(StringAux.getBytesUTF8(getConfig("compression.s2c")));
			buf.putString(StringAux.getBytesUTF8(getConfig("lang.c2s")));
			buf.putString(StringAux.getBytesUTF8(getConfig("lang.s2c")));
			buf.putByte((byte)0);
			buf.putInt(0);

			buf.setOffSet(5);
			I_C = new byte[buf.getLength()];
			buf.getByte(I_C);

			write(packet);
		}

		private void send_newkeys()
		{
			// send SSH_MSG_NEWKEYS(21)
			packet.reset();
			buf.putByte((byte)SSH_MSG_NEWKEYS);
			write(packet);
		}

		private void checkHost(String host, KeyExchange kex)
		{
			String shkc = getConfig("StrictHostKeyChecking");

			//System.Console.WriteLine("shkc: "+shkc);

			byte[] K_S = kex.getHostKey();
			String key_type = kex.getKeyType();
			String key_fprint = kex.getFingerPrint();

			hostkey = new HostKey(host, K_S);

			IHostKeyRepository hkr = jsch.getHostKeyRepository();
			int i = 0;
			lock (hkr) {
				i = hkr.check(host, K_S);
			}

			bool insert = false;

			if ((shkc.Equals("ask") || shkc.Equals("yes")) &&
				i == HostKeyRepositoryCheckResult.CHANGED) {
				String file = null;
				lock (hkr) {
					file = hkr.getKnownHostsRepositoryID();
				}
				if (file == null) {
					file = "known_hosts";
				}
				String message =
					"WARNING: REMOTE HOST IDENTIFICATION HAS CHANGED!\n" +
					"IT IS POSSIBLE THAT SOMEONE IS DOING SOMETHING NASTY!\n" +
					"Someone could be eavesdropping on you right now (man-in-the-middle attack)!\n" +
					"It is also possible that the " + key_type + " host key has just been changed.\n" +
					"The fingerprint for the " + key_type + " key sent by the remote host is\n" +
					key_fprint + ".\n" +
					"Please contact your system administrator.\n" +
					"Add correct host key in " + file + " to get rid of this message.";

				bool b = false;

				if (userinfo != null) {
					//userinfo.showMessage(message);
					b = userinfo.promptYesNo(message +
						"\nDo you want to delete the old key and insert the new key?");
				}
				//throw new JSchException("HostKey has been changed: "+host);
				if (!b) {
					throw new SshClientException("HostKey has been changed: " + host);
				}
				else {
					lock (hkr) {
						hkr.remove(host,
								  (key_type.Equals("DSA") ? "ssh-dss" : "ssh-rsa"),
								   null);
						insert = true;
					}
				}
			}

			//    bool insert=false;

			if ((shkc.Equals("ask") || shkc.Equals("yes")) &&
				(i != HostKeyRepositoryCheckResult.OK) && !insert) {
				if (shkc.Equals("yes")) {
					throw new SshClientException("reject HostKey: " + host);
				}
				//System.Console.WriteLine("finger-print: "+key_fprint);
				if (userinfo != null) {
					bool foo = userinfo.promptYesNo(
						"The authenticity of host '" + host + "' can't be established.\n" +
						key_type + " key fingerprint is " + key_fprint + ".\n" +
						"Are you sure you want to continue connecting?"
						);
					if (!foo) {
						throw new SshClientException("reject HostKey: " + host);
					}
					insert = true;
				}
				else {
					if (i == HostKeyRepositoryCheckResult.NOT_INCLUDED)
						throw new SshClientException("UnknownHostKey: " + host + ". " + key_type + " key fingerprint is " + key_fprint);
					else
						throw new SshClientException("HostKey has been changed: " + host);
				}
			}

			if (shkc.Equals("no") &&
				HostKeyRepositoryCheckResult.NOT_INCLUDED == i) {
				insert = true;
			}

			if (insert) {
				lock (hkr) {
					hkr.add(host, K_S, userinfo);
				}
			}

		}

		//public void start(){ (new Thread(this)).start();  }

		public AChannel openChannel(String type)
		{
			if (!_isConnected) {
				throw new SshClientException("session is down");
			}
			try {
				AChannel channel = AChannel.getChannel(type);
				addChannel(channel);
				channel.init();
				return channel;
			}
			catch (Exception e) {
				System.Console.WriteLine(e);
			}
			return null;
		}

		// encode will bin invoked in write with synchronization.
		public void encode(Packet packet)
		{
			//System.Console.WriteLine("encode: "+packet.buffer.buffer[5]);
			//System.Console.WriteLine("        "+packet.buffer.index);
			//if(packet.buffer.buffer[5]==96){
			//Thread.dumpStack();
			//}
			if (deflater != null) {
				packet.buffer.index = deflater.compress(packet.buffer.buffer,
					5, packet.buffer.index);
			}
			if (c2scipher != null) {
				packet.padding(c2scipher.getIVSize());
				int pad = packet.buffer.buffer[4];
				lock (random) {
					random.fill(packet.buffer.buffer, packet.buffer.index - pad, pad);
				}
			}
			else {
				packet.padding(8);
			}
			byte[] mac = null;
			if (c2smac != null) {
				c2smac.update(seqo);
				c2smac.update(packet.buffer.buffer, 0, packet.buffer.index);
				mac = c2smac.doFinal();
			}
			if (c2scipher != null) {
				byte[] buf = packet.buffer.buffer;
				c2scipher.update(buf, 0, packet.buffer.index, buf, 0);
			}
			if (mac != null) {
				packet.buffer.putByte(mac);
			}
		}

		int[] uncompress_len = new int[1];

		private int cipher_size = 8;
		public Buffer read(Buffer buf)
		{
			int j = 0;
			while (true) {
				buf.reset();
				io.getByte(buf.buffer, buf.index, cipher_size);
				buf.index += cipher_size;
				if (s2ccipher != null) {
					s2ccipher.update(buf.buffer, 0, cipher_size, buf.buffer, 0);
				}
				j = StringAux.ToInt32(buf.buffer, 0);
				j = j - 4 - cipher_size + 8;
				if (j < 0 || (buf.index + j) > buf.buffer.Length) {
					throw new IOException("invalid data");
				}
				if (j > 0) {
					io.getByte(buf.buffer, buf.index, j);
					buf.index += (j);
					if (s2ccipher != null) {
						s2ccipher.update(buf.buffer, cipher_size, j, buf.buffer, cipher_size);
					}
				}

				if (s2cmac != null) {
					s2cmac.update(seqi);
					s2cmac.update(buf.buffer, 0, buf.index);
					byte[] result = s2cmac.doFinal();
					io.getByte(mac_buf, 0, mac_buf.Length);
					if (!result.SequenceEqual(mac_buf)) {
						throw new IOException("MAC Error");
					}
				}
				seqi++;

				if (inflater != null) {
					//inflater.uncompress(buf);
					int pad = buf.buffer[4];
					uncompress_len[0] = buf.index - 5 - pad;
					byte[] foo = inflater.uncompress(buf.buffer, 5, uncompress_len);
					if (foo != null) {
						buf.buffer = foo;
						buf.index = 5 + uncompress_len[0];
					}
					else {
						System.Console.Error.WriteLine("fail in inflater");
						break;
					}
				}

				int type = buf.buffer[5] & 0xff;
				//System.Console.WriteLine("read: "+type);
				if (type == SSH_MSG_DISCONNECT) {
					buf.rewind();
					buf.getInt();
					buf.getShort();
					int reason_code = buf.getInt();
					byte[] description = buf.getString();
					byte[] language_tag = buf.getString();
					/*
						System.Console.Error.WriteLine("SSH_MSG_DISCONNECT:"+
											   " "+reason_code+
								   " "+new String(description)+
								   " "+new String(language_tag));
					*/
					throw new SshClientException("SSH_MSG_DISCONNECT:" +
						" " + reason_code +
						" " + StringAux.getString(description) +
						" " + StringAux.getString(language_tag));
					//break;
				}
				else if (type == SSH_MSG_IGNORE) {
				}
				else if (type == SSH_MSG_DEBUG) {
					buf.rewind();
					buf.getInt();
					buf.getShort();
					/*
						byte always_display=(byte)buf.getByte();
						byte[] message=buf.getString();
						byte[] language_tag=buf.getString();
						System.Console.Error.WriteLine("SSH_MSG_DEBUG:"+
								   " "+new String(message)+
								   " "+new String(language_tag));
					*/
				}
				else if (type == SSH_MSG_CHANNEL_WINDOW_ADJUST) {
					buf.rewind();
					buf.getInt();
					buf.getShort();
					AChannel c = AChannel.getChannel(buf.getInt(), this);
					if (c == null) {
					}
					else {
						c.addRemoteWindowSize(buf.getInt());
					}
				}
				else {
					break;
				}
			}
			buf.rewind();
			return buf;
		}

		internal byte[] getSessionId()
		{
			return session_id;
		}

		private void receive_newkeys(Buffer buf, KeyExchange kex)
		{
			//    send_newkeys();
			updateKeys(kex);
			in_kex = false;
		}
		private void updateKeys(KeyExchange kex)
		{
			byte[] K = kex.getK();
			byte[] H = kex.getH();
			IHASH hash = kex.getHash();

			String[] guess = kex._guess;

			if (session_id == null) {
				session_id = new byte[H.Length];
				System.Array.Copy(H, 0, session_id, 0, H.Length);
			}

			/*
			  Initial IV client to server:     HASH (K || H || "A" || session_id)
			  Initial IV server to client:     HASH (K || H || "B" || session_id)
			  Encryption key client to server: HASH (K || H || "C" || session_id)
			  Encryption key server to client: HASH (K || H || "D" || session_id)
			  Integrity key client to server:  HASH (K || H || "E" || session_id)
			  Integrity key server to client:  HASH (K || H || "F" || session_id)
			*/

			buf.reset();
			buf.putMPInt(K);
			buf.putByte(H);
			buf.putByte((byte)0x41);
			buf.putByte(session_id);
			hash.update(buf.buffer, 0, buf.index);
			IVc2s = hash.digest();

			int j = buf.index - session_id.Length - 1;

			buf.buffer[j]++;
			hash.update(buf.buffer, 0, buf.index);
			IVs2c = hash.digest();

			buf.buffer[j]++;
			hash.update(buf.buffer, 0, buf.index);
			Ec2s = hash.digest();

			buf.buffer[j]++;
			hash.update(buf.buffer, 0, buf.index);
			Es2c = hash.digest();

			buf.buffer[j]++;
			hash.update(buf.buffer, 0, buf.index);
			MACc2s = hash.digest();

			buf.buffer[j]++;
			hash.update(buf.buffer, 0, buf.index);
			MACs2c = hash.digest();

			try {
				System.Type c;

				c = System.Type.GetType(getConfig(guess[KeyExchange.PROPOSAL_ENC_ALGS_STOC]));
				s2ccipher = (ICipher)(System.Activator.CreateInstance(c));
				while (s2ccipher.getBlockSize() > Es2c.Length) {
					buf.reset();
					buf.putMPInt(K);
					buf.putByte(H);
					buf.putByte(Es2c);
					hash.update(buf.buffer, 0, buf.index);
					byte[] foo = hash.digest();
					byte[] bar = new byte[Es2c.Length + foo.Length];
					System.Array.Copy(Es2c, 0, bar, 0, Es2c.Length);
					System.Array.Copy(foo, 0, bar, Es2c.Length, foo.Length);
					Es2c = bar;
				}
				s2ccipher.init(CipherDirectionMode.DECRYPT_MODE, Es2c, IVs2c);
				cipher_size = s2ccipher.getIVSize();
				c = System.Type.GetType(getConfig(guess[KeyExchange.PROPOSAL_MAC_ALGS_STOC]));
				s2cmac = (IHMAC)(System.Activator.CreateInstance(c));
				s2cmac.init(MACs2c);
				mac_buf = new byte[s2cmac.getBlockSize()];

				c = System.Type.GetType(getConfig(guess[KeyExchange.PROPOSAL_ENC_ALGS_CTOS]));
				c2scipher = (ICipher)(System.Activator.CreateInstance(c));
				while (c2scipher.getBlockSize() > Ec2s.Length) {
					buf.reset();
					buf.putMPInt(K);
					buf.putByte(H);
					buf.putByte(Ec2s);
					hash.update(buf.buffer, 0, buf.index);
					byte[] foo = hash.digest();
					byte[] bar = new byte[Ec2s.Length + foo.Length];
					System.Array.Copy(Ec2s, 0, bar, 0, Ec2s.Length);
					System.Array.Copy(foo, 0, bar, Ec2s.Length, foo.Length);
					Ec2s = bar;
				}
				c2scipher.init(CipherDirectionMode.ENCRYPT_MODE, Ec2s, IVc2s);

				c = System.Type.GetType(getConfig(guess[KeyExchange.PROPOSAL_MAC_ALGS_CTOS]));
				c2smac = (IHMAC)(System.Activator.CreateInstance(c));
				c2smac.init(MACc2s);

				if (!guess[KeyExchange.PROPOSAL_COMP_ALGS_CTOS].Equals("none")) {
					String foo = getConfig(guess[KeyExchange.PROPOSAL_COMP_ALGS_CTOS]);
					if (foo != null) {
						try {
							c = System.Type.GetType(foo);
							deflater = (ICompression)(System.Activator.CreateInstance(c));
							int level = 6;
							try {
								level = int.Parse(getConfig("compression_level"));
							}
							catch (Exception) {
							}
							deflater.init(CompressionMode.DEFLATER, level);
						}
						catch (Exception) {
							System.Console.Error.WriteLine(foo + " isn't accessible.");
						}
					}
				}
				else {
					if (deflater != null) {
						deflater = null;
					}
				}
				if (!guess[KeyExchange.PROPOSAL_COMP_ALGS_STOC].Equals("none")) {
					String foo = getConfig(guess[KeyExchange.PROPOSAL_COMP_ALGS_STOC]);
					if (foo != null) {
						try {
							c = System.Type.GetType(foo);
							inflater = (ICompression)(System.Activator.CreateInstance(c));
							inflater.init(CompressionMode.INFLATER, 0);
						}
						catch (Exception) {
							System.Console.Error.WriteLine(foo + " isn't accessible.");
						}
					}
				}
				else {
					if (inflater != null) {
						inflater = null;
					}
				}
			}
			catch (Exception e) {
				System.Console.Error.WriteLine("updatekeys: " + e);
			}
		}

		public void write(Packet packet, AChannel c, int length)
		{
			while (true) {
				if (in_kex) {
					try {
						ThreadAux.Sleep(10);
					}
					catch (ThreadInterruptedException) {
					};
					continue;
				}
				lock (c) {
					if (c.rwsize >= length) {
						c.rwsize -= length;
						break;
					}
				}
				if (c._close || !c.isConnected()) {
					throw new IOException("channel is broken");
				}

				bool sendit = false;
				int s = 0;
				byte command = 0;
				int recipient = -1;
				lock (c) {
					if (c.rwsize > 0) {
						int len = c.rwsize;
						if (len > length) {
							len = length;
						}
						if (len != length) {
							s = packet.shift(len, (c2smac != null ? c2smac.getBlockSize() : 0));
						}
						command = packet.buffer.buffer[5];
						recipient = c.getRecipient();
						length -= len;
						c.rwsize -= len;
						sendit = true;
					}
				}
				if (sendit) {
					_write(packet);
					if (length == 0) {
						return;
					}
					packet.unshift(command, recipient, s, length);
					lock (c) {
						if (c.rwsize >= length) {
							c.rwsize -= length;
							break;
						}
					}
				}

				try {
					ThreadAux.Sleep(100);
				}
				catch (ThreadInterruptedException) {
				};
			}
			_write(packet);
		}
		/*
		public lockpublic void write(Packet packet) {
		   encode(packet);
		   if(io!=null){
			 io.put(packet);
			 seqo++;
		   }
		}
		*/
		public void write(Packet packet)
		{
			// System.Console.WriteLine("in_kex="+in_kex+" "+(packet.buffer.buffer[5]));
			while (in_kex) {
				byte command = packet.buffer.buffer[5];
				//System.Console.WriteLine("command: "+command);
				if (command == SSH_MSG_KEXINIT ||
					command == SSH_MSG_NEWKEYS ||
					command == SSH_MSG_KEXDH_INIT ||
					command == SSH_MSG_KEXDH_REPLY ||
					command == SSH_MSG_DISCONNECT ||
					command == SSH_MSG_KEX_DH_GEX_GROUP ||
					command == SSH_MSG_KEX_DH_GEX_INIT ||
					command == SSH_MSG_KEX_DH_GEX_REPLY ||
					command == SSH_MSG_KEX_DH_GEX_REQUEST) {
					break;
				}
				try {
					ThreadAux.Sleep(10);
				}
				catch (ThreadInterruptedException) {
				};
			}
			_write(packet);
		}
		[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.Synchronized)]
		private void _write(Packet packet)
		{
			encode(packet);
			if (io != null) {
				io.put(packet);
				seqo++;
			}
		}

		IRunnable thread;
		public void run()
		{
			thread = this;

			byte[] foo;
			Buffer buf = new Buffer();
			Packet packet = new Packet(buf);
			int i = 0;
			AChannel channel;
			int[] start = new int[1];
			int[] length = new int[1];
			KeyExchange kex = null;

			try {
				while (_isConnected &&
					thread != null) {
					buf = read(buf);
					int msgType = buf.buffer[5] & 0xff;
					//      if(msgType!=94)
					//System.Console.WriteLine("read: 94 ? "+msgType);

					if (kex != null && kex.getState() == msgType) {
						bool result = kex.next(buf);
						if (!result) {
							throw new SshClientException("verify: " + result);
						}
						continue;
					}

					switch (msgType) {
						case SSH_MSG_KEXINIT:
							//System.Console.WriteLine("KEXINIT");
							kex = receive_kexinit(buf);
							break;

						case SSH_MSG_NEWKEYS:
							//System.Console.WriteLine("NEWKEYS");
							send_newkeys();
							receive_newkeys(buf, kex);
							kex = null;
							break;

						case SSH_MSG_CHANNEL_DATA:
							buf.getInt();
							buf.getByte();
							buf.getByte();
							i = buf.getInt();
							channel = AChannel.getChannel(i, this);
							foo = buf.getString(start, length);
							if (channel == null) {
								break;
							}
							try {
								channel.write(foo, start[0], length[0]);
							}
							catch (Exception) {
								//System.Console.WriteLine(e);
								try {
									channel.disconnect();
								}
								catch (Exception) {
								}
								break;
							}
							int len = length[0];
							channel.setLocalWindowSize(channel.lwsize - len);
							if (channel.lwsize < channel.lwsize_max / 2) {
								packet.reset();
								buf.putByte((byte)SSH_MSG_CHANNEL_WINDOW_ADJUST);
								buf.putInt(channel.getRecipient());
								buf.putInt(channel.lwsize_max - channel.lwsize);
								write(packet);
								channel.setLocalWindowSize(channel.lwsize_max);
							}
							break;

						case SSH_MSG_CHANNEL_EXTENDED_DATA:
							buf.getInt();
							buf.getShort();
							i = buf.getInt();
							channel = AChannel.getChannel(i, this);
							buf.getInt();                   // data_type_code == 1
							foo = buf.getString(start, length);
							//System.Console.WriteLine("stderr: "+new String(foo,start[0],length[0]));
							if (channel == null) {
								break;
							}
							//channel.write(foo, start[0], length[0]);
							channel.write_ext(foo, start[0], length[0]);

							len = length[0];
							channel.setLocalWindowSize(channel.lwsize - len);
							if (channel.lwsize < channel.lwsize_max / 2) {
								packet.reset();
								buf.putByte((byte)SSH_MSG_CHANNEL_WINDOW_ADJUST);
								buf.putInt(channel.getRecipient());
								buf.putInt(channel.lwsize_max - channel.lwsize);
								write(packet);
								channel.setLocalWindowSize(channel.lwsize_max);
							}
							break;

						case SSH_MSG_CHANNEL_WINDOW_ADJUST:
							buf.getInt();
							buf.getShort();
							i = buf.getInt();
							channel = AChannel.getChannel(i, this);
							if (channel == null) {
								break;
							}
							channel.addRemoteWindowSize(buf.getInt());
							break;

						case SSH_MSG_CHANNEL_EOF:
							buf.getInt();
							buf.getShort();
							i = buf.getInt();
							channel = AChannel.getChannel(i, this);
							if (channel != null) {
								//channel._eof_remote=true;
								//channel.eof();
								channel.eof_remote();
							}
							/*
							packet.reset();
							buf.putByte((byte)SSH_MSG_CHANNEL_EOF);
							buf.putInt(channel.getRecipient());
							write(packet);
							*/
							break;
						case SSH_MSG_CHANNEL_CLOSE:
							buf.getInt();
							buf.getShort();
							i = buf.getInt();
							channel = AChannel.getChannel(i, this);
							if (channel != null) {
								//	      channel.close();
								channel.disconnect();
							}
							/*
								if(Channel.pool.size()==0){
							  thread=null;
							}
							*/
							break;
						case SSH_MSG_CHANNEL_OPEN_CONFIRMATION:
							buf.getInt();
							buf.getShort();
							i = buf.getInt();
							channel = AChannel.getChannel(i, this);
							if (channel == null) {
								//break;
							}
							channel.setRecipient(buf.getInt());
							channel.setRemoteWindowSize(buf.getInt());
							channel.setRemotePacketSize(buf.getInt());
							break;
						case SSH_MSG_CHANNEL_OPEN_FAILURE:
							buf.getInt();
							buf.getShort();
							i = buf.getInt();
							channel = AChannel.getChannel(i, this);
							if (channel == null) {
								//break;
							}
							int reason_code = buf.getInt();
							//foo=buf.getString();  // additional textual information
							//foo=buf.getString();  // language tag
							channel.exitstatus = reason_code;
							channel._close = true;
							channel._eof_remote = true;
							channel.setRecipient(0);
							break;
						case SSH_MSG_CHANNEL_REQUEST:
							buf.getInt();
							buf.getShort();
							i = buf.getInt();
							foo = buf.getString();
							bool reply = (buf.getByte() != 0);
							channel = AChannel.getChannel(i, this);
							if (channel != null) {
								byte reply_type = (byte)SSH_MSG_CHANNEL_FAILURE;
								if (StringAux.getString(foo).Equals("exit-status")) {
									i = buf.getInt();             // exit-status
									channel.setExitStatus(i);
									//	    System.Console.WriteLine("exit-stauts: "+i);
									//          channel.close();
									reply_type = (byte)SSH_MSG_CHANNEL_SUCCESS;
								}
								if (reply) {
									packet.reset();
									buf.putByte(reply_type);
									buf.putInt(channel.getRecipient());
									write(packet);
								}
							}
							else {
							}
							break;
						case SSH_MSG_CHANNEL_OPEN:
							buf.getInt();
							buf.getShort();
							foo = buf.getString();
							String ctyp = StringAux.getString(foo);
							//System.Console.WriteLine("type="+ctyp);
							if (!"forwarded-tcpip".Equals(ctyp) &&
								!("x11".Equals(ctyp) && x11_forwarding)) {
								System.Console.WriteLine("Session.run: CHANNEL OPEN " + ctyp);
								throw new IOException("Session.run: CHANNEL OPEN " + ctyp);
							}
							else {
								channel = AChannel.getChannel(ctyp);
								addChannel(channel);
								channel.getData(buf);
								channel.init();

								packet.reset();
								buf.putByte((byte)SSH_MSG_CHANNEL_OPEN_CONFIRMATION);
								buf.putInt(channel.getRecipient());
								buf.putInt(channel.id);
								buf.putInt(channel.lwsize);
								buf.putInt(channel.lmpsize);
								write(packet);
								ThreadAux tmp = new ThreadAux(channel);
								tmp.setName("Channel " + ctyp + " " + host);
								tmp.start();
								break;
							}
						case SSH_MSG_CHANNEL_SUCCESS:
							buf.getInt();
							buf.getShort();
							i = buf.getInt();
							channel = AChannel.getChannel(i, this);
							if (channel == null) {
								break;
							}
							channel.reply = 1;
							break;
						case SSH_MSG_CHANNEL_FAILURE:
							buf.getInt();
							buf.getShort();
							i = buf.getInt();
							channel = AChannel.getChannel(i, this);
							if (channel == null) {
								break;
							}
							channel.reply = 0;
							break;
						case SSH_MSG_GLOBAL_REQUEST:
							buf.getInt();
							buf.getShort();
							foo = buf.getString();       // request name
							reply = (buf.getByte() != 0);
							if (reply) {
								packet.reset();
								buf.putByte((byte)SSH_MSG_REQUEST_FAILURE);
								write(packet);
							}
							break;
						case SSH_MSG_REQUEST_FAILURE:
						case SSH_MSG_REQUEST_SUCCESS:
							ThreadAux t = grr.getThread();
							if (t != null) {
								grr.setReply(msgType == SSH_MSG_REQUEST_SUCCESS ? 1 : 0);
								t.interrupt();
							}
							break;
						default:
							System.Console.WriteLine("Session.run: unsupported type " + msgType);
							throw new IOException("Unknown SSH message type " + msgType);
					}
				}
			}
			catch (Exception) {
				//System.Console.WriteLine("# Session.run");
				//e.printStackTrace();
			}
			try {
				disconnect();
			}
			catch (NullReferenceException) {
				//System.Console.WriteLine("@1");
				//e.printStackTrace();
			}
			catch (Exception) {
				//System.Console.WriteLine("@2");
				//e.printStackTrace();
			}
			_isConnected = false;
		}

		/*
		public void finalize() throws Throwable{
		  disconnect();
		  SecureShell=null;
		}
		*/

		public void disconnect()
		{
			if (!_isConnected)
				return;

			//System.Console.WriteLine(this+": disconnect");
			//Thread.dumpStack();
			/*
			for(int i=0; i<Channel.pool.size(); i++){
			  try{
				Channel c=((Channel)(Channel.pool.elementAt(i)));
			if(c.session==this) c.eof();
			  }
			  catch(Exception e){
			  }
			}
			*/

			AChannel.disconnect(this);

			_isConnected = false;

			PortWatcher.delPort(this);
			ChannelForwardedTCPIP.delPort(this);

			lock (connectThread) {
				connectThread.yield();
				connectThread.interrupt();
				connectThread = null;
			}
			thread = null;
			try {
				if (io != null) {
					if (io.ins != null)
						io.ins.Close();
					if (io.outs != null)
						io.outs.Close();
					if (io.outs_ext != null)
						io.outs_ext.Close();
				}
				if (proxy == null) {
					if (socket != null)
						socket.close();
				}
				else {
					lock (proxy) {
						proxy.close();
					}
					proxy = null;
				}
			}
			catch (Exception) {
				//      e.printStackTrace();
			}
			io = null;
			socket = null;
			//    lock(SecureShell.pool){
			//      SecureShell.pool.removeElement(this);
			//    }

			jsch.removeSession(this);

			//System.gc();
		}

		public void setPortForwardingL(int lport, String host, int rport)
		{
			setPortForwardingL("127.0.0.1", lport, host, rport);
		}
		public void setPortForwardingL(String boundaddress, int lport, String host, int rport)
		{
			setPortForwardingL(boundaddress, lport, host, rport, null);
		}
		public void setPortForwardingL(String boundaddress, int lport, String host, int rport, ITcpListenerFactory ssf)
		{
			PortWatcher pw = PortWatcher.addPort(this, boundaddress, lport, host, rport, ssf);
			ThreadAux tmp = new ThreadAux(pw);
			tmp.setName("PortWatcher Thread for " + host);
			tmp.start();
		}
		public void delPortForwardingL(int lport)
		{
			delPortForwardingL("127.0.0.1", lport);
		}
		public void delPortForwardingL(String boundaddress, int lport)
		{
			PortWatcher.delPort(this, boundaddress, lport);
		}
		public String[] getPortForwardingL()
		{
			return PortWatcher.getPortForwarding(this);
		}

		public void setPortForwardingR(int rport, String host, int lport)
		{
			setPortForwardingR(rport, host, lport, (ISocketFactory)null);
		}
		public void setPortForwardingR(int rport, String host, int lport, ISocketFactory sf)
		{
			ChannelForwardedTCPIP.addPort(this, rport, host, lport, sf);
			setPortForwarding(rport);
		}

		public void setPortForwardingR(int rport, String daemon)
		{
			setPortForwardingR(rport, daemon, null);
		}
		public void setPortForwardingR(int rport, String daemon, System.Object[] arg)
		{
			ChannelForwardedTCPIP.addPort(this, rport, daemon, arg);
			setPortForwarding(rport);
		}

		private class GlobalRequestReply
		{
			private ThreadAux thread = null;
			private int reply = -1;
			internal void setThread(ThreadAux thread)
			{
				this.thread = thread;
				this.reply = -1;
			}
			internal ThreadAux getThread()
			{
				return thread;
			}
			internal void setReply(int reply)
			{
				this.reply = reply;
			}
			internal int getReply()
			{
				return this.reply;
			}
		}
		private GlobalRequestReply grr = new GlobalRequestReply();
		private void setPortForwarding(int rport)
		{
			lock (grr) {
				Buffer buf = new Buffer(100); // ??
				Packet packet = new Packet(buf);

				try {
					// byte SSH_MSG_GLOBAL_REQUEST 80
					// String "tcpip-forward"
					// bool want_reply
					// String  address_to_bind
					// uint32  port number to bind
					packet.reset();
					buf.putByte((byte)SSH_MSG_GLOBAL_REQUEST);
					buf.putString(StringAux.getBytesUTF8("tcpip-forward"));
					//      buf.putByte((byte)0);
					buf.putByte((byte)1);
					buf.putString(StringAux.getBytesUTF8("0.0.0.0"));
					buf.putInt(rport);
					write(packet);
				}
				catch (Exception e) {
					throw new SshClientException(e.ToString());
				}

				grr.setThread(ThreadAux.currentThread());
				try {
					ThreadAux.Sleep(10000);
				}
				catch (Exception) {
				}
				int reply = grr.getReply();
				grr.setThread(null);
				if (reply == 0) {
					throw new SshClientException("remote port forwarding failed for listen port " + rport);
				}
			}
		}
		public void delPortForwardingR(int rport)
		{
			ChannelForwardedTCPIP.delPort(this, rport);
		}

		internal void addChannel(AChannel channel)
		{
			channel.session = this;
		}
		public String getConfig(object name)
		{
			System.Object foo = null;
			if (config != null) {
				foo = config[name];
				if (foo is String)
					return (String)foo;
				if (foo is string)
					return (string)foo;
			}
			foo = jsch.getConfig(name.ToString());
			if (foo is string)
				return (string)foo;
			if (foo is String)
				return (String)foo;
			return null;
		}
		//  public Channel getChannel(){ return channel; }
		public void setProxy(IProxy proxy)
		{
			this.proxy = proxy;
		}
		public void setHost(String host)
		{
			this.host = host;
		}
		public void setPort(int port)
		{
			this.port = port;
		}
		internal void setUserName(String foo)
		{
			this.username = foo;
		}
		public void setPassword(String foo)
		{
			this.password = foo;
		}
		public void setUserInfo(IUserInfo userinfo)
		{
			this.userinfo = userinfo;
		}
		public void setInputStream(Stream In)
		{
			this.In = In;
		}
		public void setOutputStream(Stream Out)
		{
			this.Out = Out;
		}
		public void setX11Host(String host)
		{
			ChannelX11.setHost(host);
		}
		public void setX11Port(int port)
		{
			ChannelX11.setPort(port);
		}
		public void setX11Cookie(String cookie)
		{
			ChannelX11.setCookie(cookie);
		}
#if false
		public void setConfig(System.Collections.Hashtable foo)
		{
			setConfig( new JHashtable( foo ) );
		}
#endif
		public void setConfig(System.Collections.Hashtable foo)
		{
			if (config == null)
				config = new Hashtable();
			IEnumerator ee = foo.Keys.GetEnumerator();
			while (ee.MoveNext()) {
				object key = ee.Current;
				config.Add(key, foo[key]);

			}
#if false
            for (Enumeration e = new Enumeration(foo.Keys.GetEnumerator()); e.hasMoreElements(); ) 
			{
				object key=e.nextElement();
				config.Add(key, foo[key]);
			}
#endif
		}
		public void setSocketFactory(ISocketFactory foo)
		{
			socket_factory = foo;
		}
		public bool isConnected()
		{
			return _isConnected;
		}
		public int getTimeout()
		{
			return timeout;
		}
		public void setTimeout(int foo)
		{
			if (socket == null) {
				if (foo < 0) {
					throw new SshClientException("invalid timeout value");
				}
				this.timeout = foo;
				return;
			}
			try {
				socket.setSoTimeout(foo);
				timeout = foo;
			}
			catch (Exception e) {
				throw new SshClientException(e.ToString());
			}
		}
		public String getServerVersion()
		{
			return StringAux.getString(V_S);
		}
		public String getClientVersion()
		{
			return StringAux.getString(V_C);
		}
		public void setClientVersion(String cv)
		{
			V_C = StringAux.getBytesUTF8(cv);
		}

		public void sendIgnore()
		{
			Buffer buf = new Buffer();
			Packet packet = new Packet(buf);
			packet.reset();
			buf.putByte((byte)SSH_MSG_IGNORE);
			write(packet);
		}

		private static byte[] keepalivemsg = StringAux.getBytesUTF8("keepalive@jcraft.com");
		public void sendKeepAliveMsg()
		{
			Buffer buf = new Buffer();
			Packet packet = new Packet(buf);
			packet.reset();
			buf.putByte((byte)SSH_MSG_GLOBAL_REQUEST);
			buf.putString(keepalivemsg);
			buf.putByte((byte)1);
			write(packet);
		}

		private HostKey hostkey = null;
		public HostKey getHostKey()
		{
			return hostkey;
		}
		public String getHost()
		{
			return host;
		}
		public String getUserName()
		{
			return username;
		}
		public int getPort()
		{
			return port;
		}

		public String getMac()
		{
			String mac = "";
			if (s2cmac != null)
				mac = s2cmac.getName();
			return mac;

		}
		public String getCipher()
		{
			String cipher = "";
			if (s2ccipher != null)
				cipher = s2ccipher.ToString();
			return cipher;
		}

		public int BufferLength
		{
			get
			{
				return this.buf.buffer.Length;
			}
		}
	}

}
