using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSterileSSH;

/*
Copyright (c) 2017 micky-cube1 All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

  1. Redistributions of source code must retain the above copyright notice,
	 this list of conditions and the following disclaimer.

  2. Redistributions in binary form must reproduce the above copyright 
	 notice, this list of conditions and the following disclaimer in 
	 the documentation and/or other materials provided with the distribution.

  3. The names of the authors may not be used to endorse or promote products
	 derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
namespace SterileSSH
{
	/// <summary>
	/// Summary description for sharpSshTest.
	/// 
	/// </summary>
	public class MiniSshTest
	{
		const int SHARP_SHELL = 13;
		const int SHARP_TRANSFER = 16;
		const int EXIT = 17;

		/*
		 * Short description of parameters.
		 */
		public static void usage()
		{
			Console.Write(
					"SterileSSH: command-line connection utility\n" +
					"{0}\n" +
					"Usage: SterileSSH [options] [ssh://][user@]host [command]\n" +
					"        (\"host\" can also be a PASSER saved session name)\n" +
					"Options:\n" +
					"  -V        print version information and exit\n" +
					"  -k        print PGP key fingerprints and exit\n" +
					"  -v        show verbose messages\n" +
					"  -S name   Load settings from saved session\n" +
					"  -P port   connect to specified port\n" +
					"  -l user   connect with specified username\n" +
					"  -B        disable all interactive prompts\n" +
					"  -w passw  login with specified password\n" +
					"  -t -T     enable / disable pty allocation\n" +
					"  -4 -6     force use of IPv4 or IPv6\n" +
					"  -C        enable compression\n" +
					"  -i key    private key file for user authentication\n" +
					"  -hostkey aa:bb:cc:...\n" +
					"            manually specify a host key (may be repeated)\n" +
					"  -m file   read remote command(s) from file\n" +
					"  -s        remote command is an SSH subsystem\n" +
					"  -N        don't start a shell/command\n" +
					"  -z file\n" +
					"  -Z file\n" +
					"            log protocol details to a file\n"
					, Env.Version());
			System.Environment.Exit(1);
		}
		/*
		 * 
		 */
		public static void version()
		{
			Console.Write("Sterile-ssh: {0}\n", Env.Version());
			System.Environment.Exit(1);
		}
		/*
		 * 
		 */
		public static void Main(String[] argv)
		{
			Boolean got_host = false;
			SshConnectionInfo coninfo = new SshConnectionInfo();

			int optch;
			Getopt getopt = new Getopt();
			while ((optch = getopt.getopt(argv, "Bhko:sV46CD:H:K:L:NP:R:S:TW:Z:d:i:l:m:tvw:z:")) != -1)
			{
				switch (optch)
				{
					case 'B':				/* --batch */
						/* FIXME: */
						break;

					case 'h':				/* --help */
					case '?':
						usage();
						Environment.Exit(0);
						break;

					case 'k':				/* --pgpfp */
						/* FIXME: */
						Environment.Exit(1);
						break;

					case 'o':
						/* FIXME: */
						break;

					case 's':
						/* FIXME: Save Status to write to conf later. */
						break;

					case 'V':				/* --version */
						version();
						break;

					case '4':
						/* FIXME: */
						break;

					case '6':
						/* FIXME: */
						break;

					case 'C':
						/* FIXME: */
						break;

					case 'H':				/* --hostkey key */
						/* FIXME: */
						break;

					case 'K':				/* --loghost logical-host-name */
						/* FIXME: */
						break;

					case 'N':
						/* FIXME: */
						break;

					case 'P':
						/* FIXME: */
						break;

					case 'S':				/* --load filename */
						/* FIXME: */
						break;

					case 'T':
						/* FIXME: */
						break;

					case 'Z':				/* --sshrawlog filename  */
						/* FIXME: */
						break;

					case 'd':				/* --sessionlog filename */
						/* FIXME: */
						break;

					case 'i':
						/* FIXME: */
						break;

					case 'l':
						/* FIXME: */
						break;

					case 'm':
						/* FIXME: */
						break;

					case 't':
						/* FIXME: */
						break;

					case 'v':
						/* FIXME: */
						break;

					case 'w':				/* --pw password */
						coninfo.Pass = getopt.optarg;
						break;

					case 'z':				/* --sshlog filename */
						/* FIXME: */
						break;

					default:
						Console.Error.Write("{0}: unknown option \"{1}\"\n", Env.appname, optch);
						break;
				}
			}

			//Console.OutputEncoding = System.Text.Encoding.UTF8;
			//Console.OutputEncoding = Encoding.GetEncoding(932);
			//Console.OutputEncoding = Encoding.GetEncoding("Shift_JIS");

			Int32 argidx = getopt.optind;
			while (argidx < argv.Length) {
				String p = argv[argidx];
				if (p.Length > 0)
				{
					if (!got_host)
					{
						String user;
						String host;
						Int32 ii = p.IndexOf('@');
						/* Avoid initial @ */
						if (ii == 0)
						{
							p = p.Substring(1);
							ii = -1;
						}
						if (ii > 0)
						{
							user = p.Substring(0, ii);
							host = p.Substring(ii + 1);
						}
						else
						{
							user = null;
							host = p;
						}

						if (host != null && host.Length > 0)
						{
							coninfo.Host = host;
							got_host = true;
						}
						if (user != null && user.Length > 0)
						{
							coninfo.User = user;
							got_host = true;
						}
					}
					else
					{
					}
				}
				argidx++;
			}

			if (!got_host)
			{
				usage();
				Environment.Exit(0);
			}
			RunSsh(coninfo);
		}

		public static void RunSsh(SshConnectionInfo input)
		{
			try
			{
				SshShell shell = new SshShell(input.Host, input.User);
				if (input.Pass != null)
					shell.Password = input.Pass;
				if (input.IdentityFile != null)
					shell.AddIdentityFile(input.IdentityFile);

				//This statement must be prior to connecting
				shell.RedirectToConsole();

				Console.Write("Connecting...");
				shell.Connect();
				Console.WriteLine("OK");

				while (shell.ShellOpened)
				{
					System.Threading.Thread.Sleep(500);
				}
				Console.Write("Disconnecting...");
				shell.Close();
				Console.WriteLine("OK");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		public static string[] GetArgs(string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				Console.Write("Enter {0}: ", args[i]);
				args[i] = Console.ReadLine();
			}
			return args;
		}

	}
}

