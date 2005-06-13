// cs0120-6.cs: An object reference is required for the nonstatic field, method, or property `MemRefMonoBug.Int32'
// Line: 11

using System;

public class MemRefMonoBug {
	private int Int32;	// this member has the same name as System.Int32 class
	public static void Main ()
	{
		new MemRefMonoBug ().Int32 = 0;	// this line causes no problem
		Int32 = 0;	// mcs crashes in this line
	}
}

