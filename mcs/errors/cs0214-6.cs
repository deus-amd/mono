// cs0214.cs: Pointers may only be used in an unsafe context
// Line: 12

using System;

namespace ConsoleApplication1
{
	class Class1
	{
		static void Main(string[] args)
		{
			string s = typeof(void *).Name;
		}
	}
}






