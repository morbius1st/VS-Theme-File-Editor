using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
// using EnvDTE;


// note GetBitmapImage moved to the file CsUtilitiesMedia
// note GetImage moved to the file CsUtilitiesMedia



namespace UtilityLibrary
{
	public class MessageException : Exception
	{
		public MessageException(string msg,
			Exception inner) : base(msg, inner) { }
	}


	/// <summary>
	/// Utilities - list of functions<br/>
	/// nl        - system new line<br/>
	/// UserName  - the user's login name<br/>
	/// MachineName - the name of the machine<br/>
	/// CompanyName - the name of the user's company (if any)<br/>
	/// AssemblyName - the name of the assembly<br/>
	/// AssemblyVersion - the version of the assembly<br/>
	/// AssemblyDirectory - the directory of the assembly<br/>
	/// CreateSubFolders - based a root path, create sub-folders based on an array<br/>
	/// DictionariesEqual - method to compare two dictionaries { TKey, TValue } for equality<br/>
	/// SubFolder - based on a root path, create a sub-folder path<br/>
	/// ValidateStringChars - search the test string for "invalid" characters<br/>
	/// ValidateChar - determine if the char is in a list of invalid char's<br/>
	/// RandomString - create a random string of a defined length (do I need this?)<br/>
	/// GetCallingMember - get the method name of a prior caller (privide int for how far back)<br/>
	/// Extn: IndexOf(T Array) - get the index of an element in a generic array<br/>
	/// <br/>
	/// INVALID_FILE_NAME_CHARACTERS - array of char that are not allowed in a file name<br/>
	/// INVALID_FILE_NAME_STRING  - string of characters that are not allowed in a file name
	/// </summary>
	public static class CsUtilities
	{
		public static readonly string nl = System.Environment.NewLine;

		public static readonly char[] INVALID_FILE_NAME_CHARACTERS =
			new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }; //"<>:\"/\\|?*";

		public static readonly string INVALID_FILE_NAME_STRING = "<>:\"/\\|?*";

		public static string UserName=> Environment.UserName;

		public static string MachineName => Environment.MachineName;

		public static string CompanyName
		{
			get
			{
				object[] att = Assembly.GetExecutingAssembly()
				.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				
				if (att.Length > 0)
				{
					return CsExtensions.IfNullOrWhiteSpace(((AssemblyCompanyAttribute)att[0]).Company, "CyberStudio");
				}

				throw new MissingFieldException("Company is Missing from Assembly Information");
			}
		}

		public static string AssemblyName =>
			CsExtensions.IfNullOrWhiteSpace(typeof(CsUtilities).Assembly.GetName().Name, "DefaultAssembly");

		public static string AssemblyVersion => typeof(CsUtilities).Assembly.GetName().Version.ToString();

		public static string AssemblyDirectory
		{
			get
			{
				string codebase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codebase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public static string[][] CopyStringArray(string[][] source)
		{
			if (source == null) return null;

			string[][] result = new string[source.Length][];

			for (int i = 0; i < source.Length; i++)
			{
				if (source[i] != null)
				{
					result[i] = new string[source[i].Length];
					Array.Copy(source[i], result[i], source[i].Length);
				}
			}
			return result;
		}

		public static bool CreateSubFolders(string rootPath,
			string[] subFolders)
		{
			if (!Directory.Exists(rootPath))
			{
				try
				{
					Directory.CreateDirectory(rootPath);
				}
				catch
				{
					return false;
				}

				if (!Directory.Exists(rootPath)) { return false; }
			}
			
			if (subFolders != null)
			{
				for (int i = 0; i < subFolders.Length; i++)
				{
					string path = SubFolder(i, rootPath, subFolders);

					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
				}
			}

			return true;
		}

		public static string SubFolder(int i,
			string rootPath,
			string[] subFolders)
		{
			if (i < 0 ||
				i >= subFolders.Length) return null;

			string path = rootPath;
			for (int j = 0; j < i + 1; j++)
			{
				if (subFolders[j].IsVoid()) continue;

				path += "\\" + subFolders[j];
			}

			return path;
		}

		// search test string for any characters in the "invalid" char array
		// true means that the string is valid / OK
		public static bool ValidateStringChars(string test,
			char[] invalid)
		{
			bool result = true;
			// by validate - 
			// flag any basic non-print characters < \x20
			// flag any characters in invalid
			foreach (char c in invalid)
			{
				if (test.IndexOf(c) >= 0)
				{
					result = false;
					break;
				}
			}

			return result;
		}

		// return true if char is OK (not control and not in the invalid string)
		public static bool ValidateChar(char c,
			char[] invalid)
		{
			return !(Char.IsControl(c) || Extensions.IndexOf(invalid, c) >= 0);
		}

		public static string RandomString(int length, char fill = 'x')
		{
			StringBuilder temp = new StringBuilder();

			do
			{
				temp = temp.Append(Path.GetRandomFileName());
				temp.Replace('.', fill);
			}
			while (temp.Length < length + 1);

			return temp.ToString().Remove(length + 1);
		}

		// public static void VsClearImeadWin()
		// {
		// 	EnvDTE.DTE dte;
		//
		// 	System.Type dteType = Type.GetTypeFromProgID("VisualStudio.DTE.17.0", true);
		// 	dte = (EnvDTE.DTE) System.Activator.CreateInstance(dteType);
		//
		// 	Window w = dte.ActiveWindow;
		//
		// 	dte.Windows.Item("{ECB7191A-597B-41F5-9843-03A4CF275DDE}").Activate();
		// 	dte.ExecuteCommand("Edit.SelectAll");
		// 	dte.ExecuteCommand("Edit.ClearAll") ;
		// 	w.Activate();
		// }

		/// <summary>
		/// method to compare two dictionaries { TKey, TValue } for equality
		/// they are equal if they have the same keys and values (including null values)
		/// </summary>
		public static bool DictionariesEqual<TKey, TValue>(
			Dictionary<TKey, TValue> dict1, 
			Dictionary<TKey, TValue> dict2) 
		{
			if (dict1 == null || dict2 == null) return dict1 == dict2;
			if (dict1.Count != dict2.Count) return false;

			TValue tv;

			foreach (KeyValuePair<TKey, TValue> kvp in dict1)
			{
				if (!dict2.TryGetValue(kvp.Key, out tv)) return false;

				if (!EqualityComparer<TValue>.Default.Equals(kvp.Value, tv)) return false;
			}

			return true;
		}

		/* verification / test method
		private static void testDictEqual()
		{
			Dictionary<string, string?> t1 = new () {
				{"a", "1"},
				{"b", null},
				{"c", "3"},
			};

			Dictionary<string, string?> t2 = new () {
				{"a", "1"},
				{"b", null},
				{"c", "3"},
			};

			Dictionary<string, string?> t3 = new () {
				{"a", "1"},
				{"b", null},
				{"c", "4"},
			};

			Dictionary<string, string?> t4 = new () {
				{"a", "1"},
				{"b", "2"},
				{"c", "4"},
			};

			Dictionary<string, string?> t5 = new () {
				{"a", "1"},
				{"b", null},
				{"d", "3"},
			};


			bool test2 = CsUtilities.DictionariesEqual(t1, t2);
			bool test3 = CsUtilities.DictionariesEqual(t1, t3);
			bool test4 = CsUtilities.DictionariesEqual(t1, t4);
			bool test5 = CsUtilities.DictionariesEqual(t1, t5);


			Debug.WriteLine($"*** equal?  t1 vs t2 (true)  | {test2}");
			Debug.WriteLine($"*** equal?  t1 vs t3 (false) | {test3}");
			Debug.WriteLine($"*** equal?  t1 vs t4 (false) | {test4}");
			Debug.WriteLine($"*** equal?  t1 vs t5 (false) | {test5}");

		}
		*/


		public static string GetCallingInfo(int which, out string file)
		{
			StackFrame st = new StackFrame(which, false);

			file = Path.GetFileNameWithoutExtension(st.GetMethod()?.DeclaringType?.Name) ?? "UKN";

			return st.GetMethod()?.Name ?? "UKN";
		}


	}

	// extension to get the index of an element within an array of type T
	// this is here because one of the utilities above depends on this
	// and I did not want to depend on CsExtensions
	public static class Extensions
	{
		public static int IndexOf<T>(this T[] array, T find)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Equals(find)) return i;
			}

			return -1;
		}
	}
}