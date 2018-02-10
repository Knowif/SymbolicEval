using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symbolic
{
    class Program
	{
		static void Main(string[] args)
		{
			Scanner s = new Scanner("1+2+3/4");
			Parser p = new Parser(s);
			Tree t = p.Parse();
			Console.WriteLine(t.Print());
			t.Simplify();
			Console.WriteLine(t.Print());

			Console.Read();
		}
    }
}
