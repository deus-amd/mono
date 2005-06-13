// cs1570.cs: XML comment on 'F:Testing.Test.PrivateField2' has non-well-formed XML (unmatched closing element: expected summary but found incorrect  Line 3, position 12.).
// Line: 23
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	public class Test
	{
		public static void Main ()
		{
		}

		/// <summary>
		/// comment for private field
		/// </summary>
		private string PrivateField;

		/// <summary>
		/// incorrect markup comment for private field
		/// </incorrect>
		private string PrivateField2;

		/**
		<summary>
		Javadoc comment for private field
		</summary>
		*/
		private string PrivateField3;
	}
}

