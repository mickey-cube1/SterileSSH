using System;
using LibSterileSSH.Security;
/*
class LibSterileSSH.StringAux:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\java\String.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for String.
	/// </summary>
	public class StringAux
	{
		public static string getString(byte[] arr)
		{
			return System.Text.Encoding.Default.GetString(arr, 0, arr.Length);
		}
		public static string getString(byte[] arr, int offset, int len)
		{
			return System.Text.Encoding.Default.GetString(arr, offset, len);
		}

		public static string getStringUTF8(byte[] arr)
		{
			return System.Text.Encoding.UTF8.GetString(arr, 0, arr.Length);
		}

		public static byte[] getBytes(string str)
		{
			return System.Text.Encoding.Default.GetBytes(str);
		}

		public static byte[] getBytesUTF8(string str)
		{
			return System.Text.Encoding.UTF8.GetBytes(str);
		}

		/**
		* Utility method to delete the leading zeros from the modulus.
		* @param a modulus
		* @return modulus
		*/
		public static byte[] stripLeadingZeros(byte[] a)
		{
			int lastZero = -1;
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] == 0)
				{
					lastZero = i;
				}
				else
				{
					break;
				}
			}
			lastZero++;
			byte[] result = new byte[a.Length - lastZero];
			Array.Copy(a, lastZero, result, 0, result.Length);
			return result;
		}

		internal static string unquote(string _path)
		{
			byte[] path = StringAux.getBytesUTF8(_path);
			int pathlen = path.Length;
			int i = 0;
			while (i < pathlen)
			{
				if (path[i] == '\\')
				{
					if (i + 1 == pathlen)
						break;
					System.Array.Copy(path, i + 1, path, i, path.Length - (i + 1));
					pathlen--;
					continue;
				}
				i++;
			}
			if (pathlen == path.Length)
				return _path;
			byte[] foo = new byte[pathlen];
			System.Array.Copy(path, 0, foo, 0, pathlen);
			return StringAux.getString(foo);
		}

		public static uint ToUInt32(byte[] ptr, int Index)
		{
			uint ui = 0;

			ui = ((uint)ptr[Index++]) << 24;
			ui += ((uint)ptr[Index++]) << 16;
			ui += ((uint)ptr[Index++]) << 8;
			ui += (uint)ptr[Index++];

			return ui;
		}

		public static int ToInt32(byte[] ptr, int Index)
		{
			return (int)ToUInt32(ptr, Index);
		}

		public static bool regionMatches(string orig, bool ignoreCase, int toffset, String other, int ooffset, int len)
		{
			char[] ta = new char[orig.Length];
			char[] pa = new char[other.Length];
			orig.CopyTo(0, ta, 0, orig.Length);
			int to = toffset;
			other.CopyTo(0, pa, 0, other.Length);
			int po = ooffset;
			// Note: toffset, ooffset, or len might be near -1>>>1.
			if ((ooffset < 0) || (toffset < 0) || (toffset > (long)orig.Length - len) ||
				(ooffset > (long)other.Length - len))
			{
				return false;
			}
			while (len-- > 0)
			{
				char c1 = ta[to++];
				char c2 = pa[po++];
				if (c1 == c2)
				{
					continue;
				}
				if (ignoreCase)
				{
					// If characters don't match but case may be ignored,
					// try converting both characters to uppercase.
					// If the results match, then the comparison scan should
					// continue.
					char u1 = char.ToUpper(c1);
					char u2 = char.ToUpper(c2);
					if (u1 == u2)
					{
						continue;
					}
					// Unfortunately, conversion to uppercase does not work properly
					// for the Georgian alphabet, which has strange rules about case
					// conversion.  So we need to make one last check before
					// exiting.
					if (char.ToLower(u1) == char.ToLower(u2))
					{
						continue;
					}
				}
				return false;
			}
			return true;
		}
		internal static string[] split(string foo, string split)
		{
			byte[] buf = StringAux.getBytes(foo);
			System.Collections.ArrayList bar = new System.Collections.ArrayList();
			int start = 0;
			int index;
			while (true)
			{
				index = foo.IndexOf(split, start);
				if (index >= 0)
				{
					bar.Add(StringAux.getString(buf, start, index - start));
					start = index + 1;
					continue;
				}
				bar.Add(StringAux.getString(buf, start, buf.Length - start));
				break;
			}
			string[] result = new string[bar.Count];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = (string)(bar[i]);
			}
			return result;
		}
		internal static bool glob(byte[] pattern, byte[] name)
		{
			return glob(pattern, 0, name, 0);
		}
		private static bool glob(byte[] pattern, int pattern_index, byte[] name, int name_index)
		{
			//System.out.println("glob: "+new String(pattern)+", "+new String(name));
			int patternlen = pattern.Length;
			if (patternlen == 0)
				return false;
			int namelen = name.Length;
			int i = pattern_index;
			int j = name_index;
			while (i < patternlen && j < namelen)
			{
				if (pattern[i] == '\\')
				{
					if (i + 1 == patternlen)
						return false;
					i++;
					if (pattern[i] != name[j])
						return false;
					i++;
					j++;
					continue;
				}
				if (pattern[i] == '*')
				{
					if (patternlen == i + 1)
						return true;
					i++;
					byte foo = pattern[i];
					while (j < namelen)
					{
						if (foo == name[j])
						{
							if (glob(pattern, i, name, j))
							{
								return true;
							}
						}
						j++;
					}
					return false;
					/*
					if(j==namelen) return false;
					i++; j++;
					continue;
					*/
				}
				if (pattern[i] == '?')
				{
					i++;
					j++;
					continue;
				}
				if (pattern[i] != name[j])
					return false;
				i++;
				j++;
				continue;
			}
			if (i == patternlen && j == namelen)
				return true;
			return false;
		}
		internal static String getFingerPrint(IHASH hash, byte[] data)
		{
			try
			{
				hash.Init();
				hash.update(data, 0, data.Length);
				byte[] foo = hash.digest();
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				uint bar;
				for (int i = 0; i < foo.Length; i++)
				{
					bar = (byte)(foo[i] & 0xff);
					sb.AppendFormat("{0:x2}", bar);
					if (i + 1 < foo.Length)
						sb.Append(":");
				}
				return sb.ToString();
			}
			catch
			{
				return "???";
			}
		}
		private static byte[] b64 = System.Text.Encoding.Default.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=");
		private static byte val(byte foo)
		{
			if (foo == '=')
				return 0;
			for (int j = 0; j < b64.Length; j++)
			{
				if (foo == b64[j])
					return (byte)j;
			}
			return 0;
		}
		internal static byte[] fromBase64(byte[] buf, int start, int length)
		{
			byte[] foo = new byte[length];
			int j = 0;
			int l = length;
			for (int i = start; i < start + length; i += 4)
			{
				foo[j] = (byte)((val(buf[i]) << 2) | ((val(buf[i + 1]) & 0x30) >> 4));
				if (buf[i + 2] == (byte)'=')
				{
					j++;
					break;
				}
				foo[j + 1] = (byte)(((val(buf[i + 1]) & 0x0f) << 4) | ((val(buf[i + 2]) & 0x3c) >> 2));
				if (buf[i + 3] == (byte)'=')
				{
					j += 2;
					break;
				}
				foo[j + 2] = (byte)(((val(buf[i + 2]) & 0x03) << 6) | (val(buf[i + 3]) & 0x3f));
				j += 3;
			}
			byte[] bar = new byte[j];
			Array.Copy(foo, 0, bar, 0, j);
			return bar;
		}
		internal static byte[] toBase64(byte[] buf, int start, int length)
		{

			byte[] tmp = new byte[length * 2];
			int i, j, k;

			int foo = (length / 3) * 3 + start;
			i = 0;
			for (j = start; j < foo; j += 3)
			{
				k = (buf[j] >> 2) & 0x3f;
				tmp[i++] = b64[k];
				k = (buf[j] & 0x03) << 4 | (buf[j + 1] >> 4) & 0x0f;
				tmp[i++] = b64[k];
				k = (buf[j + 1] & 0x0f) << 2 | (buf[j + 2] >> 6) & 0x03;
				tmp[i++] = b64[k];
				k = buf[j + 2] & 0x3f;
				tmp[i++] = b64[k];
			}

			foo = (start + length) - foo;
			if (foo == 1)
			{
				k = (buf[j] >> 2) & 0x3f;
				tmp[i++] = b64[k];
				k = ((buf[j] & 0x03) << 4) & 0x3f;
				tmp[i++] = b64[k];
				tmp[i++] = (byte)'=';
				tmp[i++] = (byte)'=';
			}
			else if (foo == 2)
			{
				k = (buf[j] >> 2) & 0x3f;
				tmp[i++] = b64[k];
				k = (buf[j] & 0x03) << 4 | (buf[j + 1] >> 4) & 0x0f;
				tmp[i++] = b64[k];
				k = ((buf[j + 1] & 0x0f) << 2) & 0x3f;
				tmp[i++] = b64[k];
				tmp[i++] = (byte)'=';
			}
			byte[] bar = new byte[i];
			Array.Copy(tmp, 0, bar, 0, i);
			return bar;

			//    return sun.misc.BASE64Encoder().encode(buf);
		}
	}
}
