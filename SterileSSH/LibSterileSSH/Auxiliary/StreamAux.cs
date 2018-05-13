using System;
using System.IO;
using LibSterileSSH;
/*
class LibSterileSSH.StreamAux:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\java\io\JStream.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for Stream.
	/// </summary>
	public class StreamAux
	{
		public static long skip(Stream s, long len)
		{
			//Seek doesn't work
			//return Seek(offset, IO.SeekOrigin.Current);
			int i = 0;
			int count = 0;
			byte[] buf = new byte[len];
			while (len > 0)
			{
				i = s.Read(buf, count, (int)len);//tamir: possible lost of pressision
				if (i <= 0)
				{
					throw new RuntimeException("inputstream is closed");
					//return (s-foo)==0 ? i : s-foo;
				}
				count += i;
				len -= i;
			}
			return count;
		}
		public static int available(Stream s)
		{
			if (s is LibSterileSSH.PipedInputStream)
			{
				return ((LibSterileSSH.PipedInputStream)s).available();
			}
			throw new RuntimeException("JStream.available() -- Method not implemented");
		}

		public static void flush(Stream s)
		{
			s.Flush();
		}

		public static void write(Stream s, byte[] buffer)
		{
			s.Write(buffer, 0, buffer.Length);
		}
	}
}
