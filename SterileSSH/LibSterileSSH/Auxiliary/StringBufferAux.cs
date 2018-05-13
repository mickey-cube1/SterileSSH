using System.Text;
/*
class LibSterileSSH.StringBuilderAux:

This class is based on SharpSSH-1.1.1.13.src\SharpSSH\java\lang\StringBuffer.cs .

*/
namespace LibSterileSSH
{
	/// <summary>
	/// Summary description for StringBuffer.
	/// </summary>
	public class StringBuilderAux
	{
		public static StringBuilder delete(StringBuilder obj, int start, int end)
		{
			obj.Remove(start, end - start);
			return obj;
		}
	}
}
