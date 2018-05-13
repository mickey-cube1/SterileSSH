/*
class LibSterileSSH.RuntimeException:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\JSchException.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for RuntimeException.
	/// </summary>
	public class RuntimeException : System.Exception
	{
		public RuntimeException()
			: base()
		{
		}
		public RuntimeException(string msg)
			: base(msg)
		{
		}
	}
}
