using System;
/*
class LibSterileSSH.SftpException:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\SftpException.cs .

*/
namespace LibSterileSSH
{
	public class SftpException : RuntimeException
	{
		public int id;
		public String message;
		public SftpException(int id, String message)
			: base()
		{
			this.id = id;
			this.message = message;
		}
		public override String ToString()
		{
			return message;
		}
	}
}
