using System;
using System.Text;

namespace Test_Args
{
	class Program
	{
		static int Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;

			for (int i = 0; i < args.Length; i++)
				Console.WriteLine("args[{0}]:({1})", i, args[i]);

			return 0;
		}
	}
}
