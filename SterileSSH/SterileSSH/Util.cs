using System;
using System.Collections;

namespace SterileSSH
{
	/// <summary>
	/// Summary description for Util.
	/// </summary>
	public class Util
	{
		/// <summary>
		/// Get input from the user
		/// </summary>
		public static SshConnectionInfo GetInput()
		{
			SshConnectionInfo info = new SshConnectionInfo();
			Console.Write("Enter Remote Host: ");
			info.Host = Console.ReadLine();
			Console.Write("Enter Username: ");
			info.User = Console.ReadLine();

			Console.Write("Use publickey authentication? [Yes|No] :");
			string resp = Console.ReadLine();
			if (resp.ToLower().StartsWith("y")) {
				Console.Write("Enter identity key filename: ");
				info.IdentityFile = Console.ReadLine();
			}
			else {
				Console.Write("Enter Password: ");
				info.Pass = Console.ReadLine();
			}
			Console.WriteLine();
			return info;
		}
	}
}
