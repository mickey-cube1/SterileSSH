using System;
using System.IO;
/*
class LibSterileSSH.FileAux:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\java\io\File.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for File.
	/// </summary>
	public class FileAux
	{
		public static bool IsDirectory(string file)
		{
			return Directory.Exists(file);
		}

		public static String[] DirList(string file)
		{
			string[] dirs = Directory.GetDirectories(file);
			string[] files = Directory.GetFiles(file);
			String[] _list = new String[dirs.Length + files.Length];
			Array.Copy(dirs, 0, _list, 0, dirs.Length);
			Array.Copy(files, 0, _list, dirs.Length, files.Length);
			return _list;
		}

		public static long Length(string file)
		{
			FileInfo info = new FileInfo(file);
			return info.Length;
		}
	}
}
