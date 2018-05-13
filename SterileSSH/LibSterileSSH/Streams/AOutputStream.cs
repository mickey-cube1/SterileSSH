using System;
using System.IO;
/*
class LibSterileSSH.AOutputStream:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\java\io\OutputStream.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for InputStream.
	/// </summary>
	public abstract class AOutputStream : Stream
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override int ReadByte()
		{
			return 0;
		}

		public virtual void write(byte[] buffer, int offset, int count)
		{
			Write(buffer, offset, count);
		}

		public virtual void close()
		{
			Close();
		}

		public virtual void flush()
		{
			Flush();
		}

		public override bool CanRead
		{
			get
			{
				return false;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}
		public override void Flush()
		{
		}
		public override long Length
		{
			get
			{
				return 0;
			}
		}
		public override long Position
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}
		public override void SetLength(long value)
		{
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			return 0;
		}
	}
}
