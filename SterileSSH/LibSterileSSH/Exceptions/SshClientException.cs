using System;
/*
class LibSterileSSH.SshClientException:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\JSchException.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for SshClientException.
	/// </summary>
	public class SshClientException : RuntimeException
	{
		public SshClientException()
			: base()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public SshClientException(string msg)
			: base(msg)
		{
		}
	}
}
