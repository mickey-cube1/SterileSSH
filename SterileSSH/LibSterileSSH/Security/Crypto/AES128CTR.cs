using System;
using System.Security.Cryptography;
/*
Copyright (c) 2017 micky-cube1 All rights reserved.

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
	public class AES128CTR : ICipher
	{
		private int mode;
		private const int ivsize = 16;
		private const int bsize = 16;
		private System.Security.Cryptography.RijndaelManaged rijndael;
		private ICryptoTransform cipher;
		private byte[] counter;
		public int getIVSize()
		{
			return ivsize;
		}
		public int getBlockSize()
		{
			return bsize;
		}
		public void init(int mode, byte[] key, byte[] iv)
		{
			this.mode = mode;
			rijndael = new RijndaelManaged();
			rijndael.Mode = CipherMode.ECB;
			rijndael.Padding = PaddingMode.None;
			byte[] tmp;
			if (iv.Length > ivsize) {
				tmp = new byte[ivsize];
				Array.Copy(iv, 0, tmp, 0, tmp.Length);
				iv = tmp;
			}
			if (key.Length > bsize) {
				tmp = new byte[bsize];
				Array.Copy(key, 0, tmp, 0, tmp.Length);
				key = tmp;
			}

			counter = new byte[iv.Length];
			Array.Copy(iv, counter, iv.Length);

			try {
				cipher = rijndael.CreateEncryptor(key, null);
			}
			catch (Exception e) {
				Console.WriteLine(e);
				cipher = null;
			}
		}
		public void update(byte[] foo, int s1, int len, byte[] bar, int s2)
		{
			int i;
			int j;
			byte[] tmp = new byte[bsize];
			for (i = 0; i < len; i = i + bsize) {
				cipher.TransformBlock(counter, 0, bsize, tmp, 0);
				for (j = 0; j < bsize; j++) {
					bar[s2 + i + j] = (byte)((uint)tmp[j] ^ (uint)foo[s1 + i + j]);
				}
				for (j = bsize - 1; j >= 0; --j) {
					counter[j]++;
					if (counter[j] != 0) {
						break;
					}
				}
			}
		}

		public override string ToString()
		{
			return "aes128-ctr";
		}
	}

}
