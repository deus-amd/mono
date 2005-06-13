// cs1574.cs: XML comment on 'Test' has cref attribute 'Goo' that could not be resolved in 'Test'.
// Line: 8
// Compiler options: -doc:dummy.xml -warnaserror -warn:1

using System;

/// <see cref="Goo"/> ... does not exist
public class Test
{
	string Foo {
		get { return null; }
	}
}

class X
{
	static void Main ()
	{ }
}
