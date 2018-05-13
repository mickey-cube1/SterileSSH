using System;
/*
class LibSterileSSH.SecureShell.IForwardedTCPIPDaemon:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\jsch\ .

*/
namespace LibSterileSSH.SecureShell
{
	public interface IForwardedTCPIPDaemon : IRunnable
	{
		void setChannel(ChannelForwardedTCPIP channel);
		void setArg(Object[] arg);
	}
}
