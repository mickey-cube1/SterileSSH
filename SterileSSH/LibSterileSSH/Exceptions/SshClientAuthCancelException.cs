using System;
/*
class LibSterileSSH.SshClientAuthCancelException:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\JSchAuthCancelException.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for  SshClientAuthCancelException.
	/// </summary>
	public class SshClientAuthCancelException : SshClientException
	{
		public SshClientAuthCancelException()
			: base()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public SshClientAuthCancelException(string msg)
			: base(msg)
		{
		}
	}
}
