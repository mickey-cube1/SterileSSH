using System.Threading;
using Threading = System.Threading;
/*
class LibSterileSSH.ThreadAux:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\java\lang\Thread.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for Thread.
	/// </summary>
	public class ThreadAux
	{
		Threading.Thread t;

		public ThreadAux(Threading.Thread t)
		{
			this.t = t;
		}
		public ThreadAux(ThreadStart ts)
			: this(new Threading.Thread(ts))
		{
		}

		public ThreadAux(IRunnable r)
			: this(new ThreadStart(r.run))
		{
		}

		public void setName(string name)
		{
			t.Name = name;
		}

		public void start()
		{
			t.Start();
		}

		public bool isAlive()
		{
			return t.IsAlive;
		}

		public void yield()
		{
		}

		public void interrupt()
		{
			try
			{
				t.Interrupt();
			}
			catch
			{
			}
		}

		public void notifyAll()
		{
			Monitor.PulseAll(this);
		}

		public static void Sleep(int t)
		{
			Threading.Thread.Sleep(t);
		}

		public static ThreadAux currentThread()
		{
			return new ThreadAux(Threading.Thread.CurrentThread);
		}
	}
}
