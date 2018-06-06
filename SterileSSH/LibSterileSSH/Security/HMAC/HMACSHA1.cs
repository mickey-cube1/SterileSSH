using System;
/*
class LibSterileSSH.Security.HMACSHA1:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\jce\HMACSHA1.cs .

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
namespace LibSterileSSH.Security
{
	public class HMACSHA1 : IHMAC
	{
		private const String name = "hmac-sha1";
		private const int bsize = 20;
		private LibSterileSSH.Security.HashTransformer mentalis_mac;
		private System.Security.Cryptography.CryptoStream cs;
		//private Mac mac;
		public int getBlockSize()
		{
			return bsize;
		}
		public void init(byte[] key)
		{
			if (key.Length > bsize) {
				byte[] tmp = new byte[bsize];
				Array.Copy(key, 0, tmp, 0, bsize);
				key = tmp;
			}
			//    SecretKeySpec skey=new SecretKeySpec(key, "HmacSHA1");
			//    mac=Mac.getInstance("HmacSHA1");
			//    mac.init(skey);
			mentalis_mac = new LibSterileSSH.Security.HashTransformer(new System.Security.Cryptography.SHA1CryptoServiceProvider(), key);
			cs = new System.Security.Cryptography.CryptoStream(System.IO.Stream.Null, mentalis_mac, System.Security.Cryptography.CryptoStreamMode.Write);
		}
		private byte[] tmp = new byte[4];
		public void update(int i)
		{
			tmp[0] = (byte)(i >> 24);
			tmp[1] = (byte)(i >> 16);
			tmp[2] = (byte)(i >> 8);
			tmp[3] = (byte)i;
			update(tmp, 0, 4);
		}
		public void update(byte[] foo, int s, int l)
		{
			//mac.update(foo, s, l);      
			cs.Write(foo, s, l);
		}
		public byte[] doFinal()
		{
			//Console.WriteLine("Sha1");
			//return mac.doFinal();
			cs.Close();
			byte[] result = mentalis_mac.Hash;
			byte[] key = mentalis_mac.Key;
			mentalis_mac.Clear();
			init(key);

			return result;
		}
		public String getName()
		{
			return name;
		}
	}

}
