using System;
using IO = System.IO;
using LibSterileSSH;
/*
class LibSterileSSH.FileInputStream:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\java\io\FileInputStream.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for FileInputStream.
	/// </summary>
	public class FileInputStream : AInputStream
	{
		IO.FileStream fs;
		public FileInputStream(string file)
		{
			fs = IO.File.OpenRead(file);
		}
		public override void Close()
		{
			fs.Close();
		}


		public override int Read(byte[] buffer, int offset, int count)
		{
			return fs.Read(buffer, offset, count);
		}

		public override bool CanSeek
		{
			get
			{
				return fs.CanSeek;
			}
		}

		public override long Seek(long offset, IO.SeekOrigin origin)
		{
			return fs.Seek(offset, origin);
		}
	}
}
