using System;
/*
class LibSterileSSH.SshClientPartialAuthException:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\JSchPartialAuthException.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for SshClientPartialAuthException.
	/// </summary>
	public class SshClientPartialAuthException : SshClientException
	{
		string methods;
		public SshClientPartialAuthException()
			: base()
		{
			methods = null;
		}

		public SshClientPartialAuthException(string msg)
			: base(msg)
		{
			methods = msg;
		}

		public String getMethods()
		{
			return methods;
		}
	}
}
