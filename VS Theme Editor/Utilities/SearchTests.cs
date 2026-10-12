// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             SearchTests.cs
// Created:      2026-09-22 (22:09)

using System.Collections.Immutable;
using System.Diagnostics;
using UtilityLibrary;

// ReSharper disable HeuristicUnreachableCode
#pragma warning disable IDE0051
#pragma warning disable CS0162 // Unreachable code detected

namespace VS_Theme_Editor;

public static class SearchTests
{
	/*
	approx search - find the first key that is before the find string - case sensitive or case insensitive

	eg.  case sensitive  find = Def is between Dee and Deg  => returned will be Dee
		however, since lower case letters > upper case letters - Def is after DZf => returned is DZf


		ignore case  find = Def is between Dee and Deg  => returned is Dee
		however, Def is also after DEe and before Deg (or DEg) => returned is DEe

		if find is before the first key in the list - returned is -1 (before)
		if find is after the last key in the list - returned is testKeys.Length (after)

	*/

	private   const bool SHOW1 = false;
	private   const bool SHOW2 = false;
	private   const bool SHOW3 = true;

	public static void TestFuzzySearchLive(Theme theme)
	{
		foreach (string find in testFindWords)
		{
			BestMatchDbl bmd = theme.Categories.FindFuzzy(find);

			Debug.Write($"finding {find,-10} | ");
			bmd.Show();
		}
	}



	public static void TestFuzzySearchDlMultiple()
	{
		Stopwatch s = new ();

		s.Start();

		Debug.Write($"** starting ... ");

		for (int i = 0; i < 10000; i++)
		{
			TestFuzzySearchDl();
		}

		s.Stop();

		long ms = s.ElapsedMilliseconds;

		Debug.WriteLine($" > elapsed (ms) {ms}");
	}

	public static void TestFuzzySearchMultiple()
	{
		Stopwatch s = new ();

		s.Start();

		Debug.Write($"** starting ... ");

		for (int i = 0; i < 10000; i++)
		{
			TestFuzzySearch();
		}

		s.Stop();

		long ms = s.ElapsedMilliseconds;

		Debug.WriteLine($" > elapsed (ms) {ms}");
	}

	public static void TestFuzzyStringSearch()
	{
		List<string> test = new (testKeys);

		foreach (string s in testData)
		{
			BestMatchDbl bmd = FuzzyStringSearch.FuzzyFind(s, test);

			Debug.Write($"*** search for| {s, -10} | ");

			bmd.Show();
		}
	}

	public static void TestFuzzySearchDl()
	{
		string answer;

		foreach (string find in testData)
		{
			if (SHOW3) bestMatch = new ();

			if (SHOW1) Debug.WriteLine($"*** find this {find}");

			// ReSharper disable once RedundantAssignment
			answer = findFuzzyMatchDl(find);

			if (SHOW1) Debug.WriteLine(" ");

			if (SHOW1) Debug.Write($"*** find| {find, -10}| found| {answer, -20}| \t");

			if (SHOW2 || SHOW3) Debug.Write($"*** search for| {find, -10} | ");

			if (SHOW3) showBestMatches();

			if (SHOW1) Debug.WriteLine("\n\n");
		}
	}

	public static void TestFuzzySearch()
	{
		Tuple<double, string, int> answer;

		// foreach ((string find, string[] value) in testData)
		// foreach (string find in testData3)
		foreach (string find in testData)
		{
			if (SHOW3) bestMatch2 = new ();

			if (SHOW1) Debug.WriteLine($"*** search for >{find}< in the list");
			if (SHOW1) Debug.WriteLine(" ");

			// ReSharper disable once RedundantAssignment
			answer = findFuzzyMatch(find);

			if (SHOW1) Debug.WriteLine(" ");

			if (SHOW1) Debug.Write($"*** search for| {find, -10} | found|  {answer.Item2, -20} [ {answer.Item3} ] | \t");


			if (SHOW2 || SHOW3)  Debug.Write($"*** search for| {find, -10} | ");

			if (SHOW3)
			{
				string result = testKeys[answer.Item3];
				if (result.Length > 18) result = result.Substring(0, 18);

				Debug.Write($" | check {result, -20} ");

				showBestMatches2();
			}

			if (SHOW2) Debug.WriteLine("\n");
			if (SHOW1) Debug.WriteLine("\n\n");
		}
	}

	public static void ShowList()
	{
		Tuple<string, int> answer;

		string key;
		string idx;

		List<Tuple<string, int>> tKeys = new ();

		for (var i = 0; i < testKeys.Length; i++)
		{
			tKeys.Add(new (testKeys[i], i));
		}

		ImmutableSortedSet<Tuple<string, int>> set =  tKeys.ToImmutableSortedSet();

		Debug.Write("** ");

		for (var i = 0; i < set.Count; i++)
		{
			answer =  set.ElementAt(i);
			key = answer.Item1;

			if (key.Length > 18) key = key.Substring(0, 18);

			idx = answer.Item2.ToString();

			Debug.Write($"[ {idx, 3} ] {key, -20}, ");

			if (i % 5 == 4) Debug.Write("\n** ");
		}

		Debug.WriteLine("\n");
	}

	private static string[] testKeys =
	[
		"and",
		"ACDCOverview" ,
		"CodeAnalysis",
		"DataGrid",
		"InfoBar",
		"Locals" ,
		"Regex Editor Results Grid" ,
		"Regex Quick Start Window",
		"ReSharper",
		"TreeView",
		"Watch",
		"BayWatch",
		"ExtendedTreeView"  ,
		"CodeWorks",
		"Pencil Sharper",
		"Quick Start Window",
		"Picture" ,
		"Editor Results Grid" ,
		"Uniform",
		"Zypher",
		"Vortex",
		"Money",
		"money",
		"Makeover" ,
		"Zither",
		"RiverView"  ,
		"Corridor",
		"Minimum",
		"Start Window",
		"Lifetime" ,
		"Grid" ,
		"Panel",
		"zebra",
		"youth" ,
		"Hardly",
		"For Love of"
	];


	private static string[] testKeys2 =
	[
		"And",
		"ACDCOverview" ,
		"CodeAnalysis",
		"DataGrid",
		// "InfoBar",
		// "Locals" ,
		// "Regex Editor Results Grid" ,
		// "Regex Quick Start Window",
		// "ReSharper",
		// "TreeView",
		// "Watch",
		"BayWatch",
		"ExtendedTreeView"  ,
		"CodeWorks",
		// "Pencil Sharper",
		// "Quick Start Window",
		// "Picture" ,
		// "Editor Results Grid" ,
		// "Uniform",
		// "Zypher",
		// "Vortex",
		// "Money",
		// "money",
		// "Makeover" ,
		// "Zither",
		// "RiverView"  ,
		"Corridor",
		// "Minimum",
		// "Start Window",
		// "Lifetime" ,
		"Grid" ,
		// "Panel",
		// "zebra",
		// "youth" ,
		// "Hardly",
		"For Love of"
	];


	// shorter words
	private static string[] testFindWords =
	[
		"Cider",
		"Locals",
		"DataGrid",
		"Locals",
		"Watch",
		"XAML",
		"Fin",
		"Jef",
	];


	// standard test
	private static string[] testData =
	[
		"time",
		"AbcdO",
		"zexy",
		"Moniy",
		"Locals",
		"Regx O",
		"Localsx",
		"babies",
		"zappy",
		"maine",
		"Watt",
		"Flyer",
		"fox and",
		"dog and"
	];

	// shorter words
	private static string[] testData5 =
	[
		"Abcd",
		"zex",
		"Moni",
		"Locals",
		"Regx",
		"Localsx",
		"baby",
		"zap",
		"mane",
		"Wat",
		"Flyer"
	];

	// limited list
	private static string[] testData4=
	[
		"maine",
		// "Abcd",
		// "AbcdOv",
		// "AbcdOver",
	];

	// special list of words with issues
	private static string[] testData3 =
	[
		"AbcdO",
		"Abc",
		"Abcd",
		"AbcOver",
		"zexy",
		"Moniy",
		"Regx O",
		"babies",
		"maine",
		"Watt",
		"dog and"
	];


	private static string findFuzzyMatchDl(string find)
	{
		// this prevents finding very short strings that are likely match to much
		if (find.Length < 3) return string.Empty;

		int minDiatance = int.MaxValue;
		string result = string.Empty;

		int idx = 10;

		find = find.ToLower();

		foreach (string test in testKeys)
		{
			// this prevents comparing very short test strings that may match too easily
			if (test.Length < 3) continue;

			if (SHOW1) Debug.Write($"**  search | ");

			string compareKey = test.ToLower();
			int distance = damerauLevenshtein(find, compareKey);

			if (SHOW1) Debug.Write($" | distance| {distance}");


			if (distance < minDiatance)
			{
				if (SHOW1) Debug.Write($" |  found| {test}");
				minDiatance = distance;
				result = test;
			}

			if (SHOW1) Debug.WriteLine(" <");

			if (SHOW3) addBestMatch(distance, test, idx++);

			if (distance == -1) break;
		}

		return result;
	}

	private static int damerauLevenshtein(string find, string test)
	{
		string strF = find;
		string strT = test;

		int Min(int a, int b, int c)
		{
			int result = a;
			if (b < result) result = b;
			if (c < result) result = c;
			return result;
		}

		// int lenStrF = strF.Length;
		// int lenStrT = strT.Length;

		int len = Math.Min(strF.Length, strT.Length);

		if (SHOW1) Debug.Write($"**  find | [{strF}] ({len}) | test [{strT}] ({len})");

		if (len * len == 0)
			return Math.Max(len, len);

		int[] d = new int[len + 1];

		for (int i = 0; i <= len; i++)
			d[i] = i;

		for (int i = 1; i <= len; i++)
		{
			char s1 = strF[i - 1];
			int prevCost = i - 1;
			int cost = i;

			// Debug.Write(".");

			for (int j = 1; j <= len; j++)
			{
				// Debug.Write(":");

				char s2 = strT[j - 1];

				cost = s1 == s2 || i > 1 && j > 1 && s1 == strT[j - 1] && s2 == strF[i - 1]
					? prevCost
					: 1 + Min(cost, prevCost, d[j]);

				prevCost = d[j];

				if (i == 1 && strF[0] == strT[0]) cost--;

				d[j] = cost;
			}
		}

		return d[len];
	}


	private static  Tuple<double, string, int> findFuzzyMatch(string find)
	{
		Tuple<double, string, int> result =
			Tuple.Create(double.MaxValue, string.Empty, 0);

		if (find.Length < 3) return result;

		Tuple<double, string, int> temp;

		find = find.ToLower();

		double answer = double.MaxValue;

		int idx = 0;

		foreach (string test in testKeys)
		{
			if (test.Length < 3) continue;

			string compareKey = test; //.ToLower();
			double distance = fuzzyCompare(find, compareKey);

			if (SHOW2) Debug.Write($"** {distance, 12:F3}  |  {test} ");

			temp = new (distance, test, idx++);

			if (SHOW3) addBestMatch2(temp);

			if (distance < answer)
			{
				answer = distance;
				result = temp;

				if (SHOW2) Debug.Write($" << found");

				// if (distance == 0)
				// {
				// 	if (SHOW2) Debug.WriteLine($" ");
				// 	break;
				// }
			}

			if (SHOW2) Debug.WriteLine($" ");
		}

		return result;
	}

	// private static string findFuzzyMatch<T>(string find, T? tests)
	// where T : IEnumerable<string>
	// {
	// 	int indexOffset = 10;
	// 	if (tests == null || find.Length < 3) return string.Empty;
	//
	// 	string result = string.Empty;
	// 	find = find.ToLower();
	//
	// 	double answer = double.MaxValue;
	//
	// 	int idx = INDEX_OFFSET;
	//
	// 	foreach (string test in tests)
	// 	{
	// 		if (test.Length < 3) continue;
	//
	// 		string compareKey = test; //.ToLower();
	// 		double distance = fuzzyCompare(find, compareKey);
	//
	// 		if (SHOW2) Debug.Write($"** {distance, 12:F3}  |  {test} ");
	//
	// 		if (distance < answer)
	// 		{
	// 			answer = distance;
	// 			result = test;
	// 			if (SHOW2) Debug.Write($" << found");
	//
	// 			if (distance == 0)
	// 			{
	// 				if (SHOW2) Debug.WriteLine($" ");
	// 				break;
	// 			}
	// 		}
	//
	// 		if (SHOW3) addBestMatch2(distance, test, idx++);
	//
	// 		if (SHOW2) Debug.WriteLine($" ");
	// 	}
	//
	// 	return result;
	// }


	private static double fuzzyCompare2(string find, string test)
	{
		// convert these strings for better matching
		string strF = String.Concat(find.Trim().Split(' ')).ToLower();
		string strT = String.Concat(test.Trim().Split(' ')).ToLower();

		int len = strF.Length > strT.Length ? strT.Length : strF.Length;

		int w1;
		int final = int.MaxValue;
		int i;
		int j;

		int passMax = len - 1;
		passMax = passMax > 3 ? 3 : passMax;
		int loopMax = 1;
		int idxMax = len;


		for (int pass = 0; pass < passMax; pass++)
		{
			for (int loop = 0; loop < loopMax; loop++)
			{
				int o2 = 0;
				int diff = 0;

				for (int idx = 0; idx < idxMax; idx++)
				{
					if (pass > 0 && (idx == loop || idx == loop + pass))
					{
						i = idx;
						j = idx + pass + o2;

						o2 = 2 * -pass;
						w1 = 3; // approx 26/8
					}
					else
					{
						i = idx;
						j = idx;
						w1 = 0; // no weight benefit
					}

					int f = strF[i];
					int t = strT[j];

					int d = t - f;
					d = d < 0 ? -d : d;

					// weight factor compensations
					d += w1;

					if (pass == 0 && f == t)
					{
						if (idx == 0) d += -26; // 26 characters in the EN alphabet
						else d += -6;           // approx 26/4
					}

					diff += d;
				}  // end for idx

				if (diff < final) final = diff;
			} // end for loop

			idxMax = len;
			loopMax = len - pass - 1;
		}

		if (SHOW2) Debug.Write($"** final is {final, 4} | len {len, 4} | ");

		return (final * 100.0) / len;
	}


	private static double fuzzyCompare(string find, string test)
	{
		string strF = String.Concat(find.Trim().Split(' ')).ToLower();
		string strT = String.Concat(test.Trim().Split(' ')).ToLower();

		// the shorter of the two
		int lenF = (strF.Length > strT.Length ? strT.Length : strF.Length) - 1;
		// the longer of the two
		int lenT = (lenF + 2 > strT.Length - 1 ? strT.Length - 1 : lenF + 2);
		// the longer of the two
		int len = lenF > lenT ? lenF : lenT;

		int w1;
		int final = int.MaxValue;
		int i;
		int j;

		int passMax = lenF;
		// passMax = passMax > 3 ? 3 : passMax - 1;
		passMax = passMax > 3 ? 3 : passMax;
		int loopMax = 1;
		int idxMax = len;

		if (SHOW1) Debug.WriteLine($"\n** finding {strF} vs {strT} ");
		if (SHOW1) Debug.WriteLine($"len {len,-4} | lenF {lenF,-4} | lenT {lenT,-4}");
		if (SHOW1) Debug.Write($"pMax {passMax,-4} | ");

		for (int pass = 0; pass < passMax; pass++)
		{
			if (SHOW1) Debug.Write($"lMax {loopMax,-4} | ");

			for (int loop = 0; loop < loopMax; loop++)
			{
				// o1 = -pass;
				int o2 = 0;
				int diff = 0;

				if (SHOW1) Debug.WriteLine($"iMax {idxMax,-4}\n");

				for (int idx = 0; idx < idxMax + 1; idx++)
				{
					if (pass > 0 && (idx == loop || idx == loop + pass))
					{
						i = idx; // + pass + o1;
						j = idx + pass + o2;

						// o1 = -pass;
						o2 = 2 * -pass;
						w1 = 3;
					}
					else
					{
						i = idx;
						j = idx;
						w1 = 0;
					}

					if (i > lenF)
					{
						if (SHOW1) Debug.WriteLine($"idx {idx} | i {i} > lenF {lenF} || j {j} > lenT {lenT}");
						break;
					}


					int f = strF[i];
					int t = strT[j];

					int d = t - f;
					d = d < 0 ? -d : d;

					// weight factor compensations
					d += w1;

					if (pass == 0 && f == t)
					{
						if (idx == 0) d += -26; // 26 characters in the EN alphabet
						else d += -6;           // approx 26/4
					}

					diff += d;

					if (SHOW1) Debug.WriteLine($"*** pass {pass, -4} | loop {loop, -4} | idx {idx, -4} | t [{t, 4}] ({j,2}) vs f [{f, 4}] ({i,2}) | d {d, -4}");
				}  // end for idx

				if (diff < final) final = diff;

				if (SHOW1) Debug.WriteLine($"** diff {diff}\n");
			} // end for loop

			// idxMax = len - pass;
			// idxMax = lenF;

			// loopMax = idxMax - 1;
			// loopMax = lenF - pass - 1;
			loopMax = len - pass;
		}

		if (SHOW1) Debug.WriteLine($"** final is {final}");

		// +1 for comparison old vs new
		if (SHOW2) Debug.Write($"** final is {final, 3} | len {len + 1, 2} | lenF {lenF + 1, 2} | lenT {lenT + 1, 2} | ");

		return (final * 100.0) / (lenF + 1);
	}


	private static SortedList<double, Tuple<int, string>>? bestMatch;
	private static SortedList<double, Tuple<double, string, int>>? bestMatch2;

	private static void addBestMatch(int distance, string answer, int idx)
	{
		double dist = distance + (1.0 / idx);

		#pragma warning disable CS8602 // Dereference of a possibly null reference.
		if (bestMatch.Count < 3)
			#pragma warning restore CS8602 // Dereference of a possibly null reference.
		{
			bestMatch.Add(dist, new (distance, answer));
			return;
		}

		int ix = 0;
		int dx = distance;
		int last = -1;

		foreach ((double _, (int d, string _)) in bestMatch)
		{
			if (d > dx)
			{
				dx = d;
				last = ix;
			}

			ix++;
		}

		if (last >= 0)
		{
			bestMatch.RemoveAt(last);
			bestMatch.Add(dist, new (distance, answer));
		}
	}

	private static void addBestMatch2(Tuple<double, string, int> result)
	{
		// this attempts to prevent duplicate keys - it adds a small value
		// to the score based on the index of the test key
		double score = result.Item1 + (1.0 / (result.Item3 + 10));

		#pragma warning disable CS8602 // Dereference of a possibly null reference.
		if (bestMatch2.Count < 3)
			#pragma warning restore CS8602 // Dereference of a possibly null reference.
		{
			bestMatch2.Add(score, result);
			return;
		}

		int ix = 0;
		double dx = result.Item1;
		int last = -1;

		foreach ((double _, (double d, string _, int _)) in bestMatch2)
		{
			if (d > dx)
			{
				dx = d;
				last = ix;
			}

			ix++;
		}

		if (last >= 0)
		{
			bestMatch2.RemoveAt(last);
			bestMatch2.Add(score, result);
		}
	}

	private static void showBestMatches()
	{
		string answer;
		Debug.Write($"** ");

		#pragma warning disable CS8602 // Dereference of a possibly null reference.
		for (var i = 0; i < bestMatch.Count; i++)
			#pragma warning restore CS8602 // Dereference of a possibly null reference.
		{
			answer = bestMatch.GetValueAtIndex(i).Item2;

			if (answer.Length > 18) answer = answer.Substring(0, 18);

			Debug.Write($" | {i} | {answer,-20} ");
		}

		Debug.Write("\n");
	}

	private static void showBestMatches2()
	{
		string answer;
		int idx;
		double score;
		double prior = 0;
		bool test;
		string result;

		Debug.Write($"** ");

		#pragma warning disable CS8602 // Dereference of a possibly null reference.
		for (var i = 0; i < bestMatch2.Count; i++)
			#pragma warning restore CS8602 // Dereference of a possibly null reference.
		{
			answer = bestMatch2.GetValueAtIndex(i).Item2;
			idx = bestMatch2.GetValueAtIndex(i).Item3;
			score = bestMatch2.GetValueAtIndex(i).Item1;

			if (i == 0)
			{
				test = true;
			}
			else
			{
				test = prior <= score;
			}

			prior = score;

			if (!test) result = "X";
			else result = "|";

			if (answer.Length > 18) answer = answer.Substring(0, 18);

			Debug.Write($" | {i} | {idx, 3} | {score, 8:F2} {result} {answer,-20}");
		}

		Debug.Write("\n");
	}

	/* below good - but required a sorted list


	private const int prefaceWidth = 26;

	// private static int pass;
	private static string[]? keys;
	private static ImmutableSortedSet<string> set;
	private static bool seenList;



		// this is good - but required a sorted list

	public static void TestApxSearch(bool ignoreCase)
	{
		string[] testKeys =
		[
			"ACDCOverview" ,
			"CodeAnalysis",
			"DataGrid",
			"InfoBar",
			"Locals" ,
			"Regex Editor Results Grid" ,
			"Regex Quick Start Window",
			"ReSharper",
			"TreeView"  ,
			"Watch",
			"BWatch",
			"DTreeView"  ,
			"DCodeAnalysis",
			"FReSharper",
			"ORegex Quick Start Window",
			"PLocals" ,
			"QRegex Editor Results Grid" ,
			"UInfoBar",
			"VDataGrid",
			"WACDCOverview" ,
			"ZWatch",
			"RTreeView"  ,
			"SSCodeAnalysis",
			"MNReSharper",
			"JkRegex Quick Start Window",
			"KLocals" ,
			"pRegex Editor Results Grid" ,
			"aInfoBar",
			"zDataGrid",
			"yACDCOverview" ,
			"HCodeAnalysis",
		];

		string[] testKeys1 =
		[
			"Watch",
			"TreeView"  ,
			"CodeAnalysis",
			"ReSharper",
			"Regex Quick Start Window",
			"Locals" ,
			"Regex Editor Results Grid" ,
			"InfoBar",
			"DataGrid",
			"ACDCOverview" ,
		];


		string[] testSearch = ["Abc", "Zex", "Mabc", "Locals", "Regx", "Localsx", "abc", "zax", "mabc", "Wat"];

		string[] answersFalse = ["ACDCOverview", "(after)", "Locals", "Locals", "ReSharp", "Locals", "(after)", "(after)", "(after)", "Watch"];
		string[] answersTrue = ["(before)", "(after)", "Locals", "Locals", "Regex Q", "Locals", "(before)", "(after)", "Locals", "Watch"];

		int t;
		int idx = -1;
		keys = testKeys;

		bool? answer;

		for (int i = 0; i < 2; i++)
		{
			seenList = false;
			idx = -1;

			foreach (string find in testSearch)
			{
				idx++;

				answer = testApxSearch(find, ignoreCase, out t);

				string result = t < 0 ? "(before)" : t == keys.Length ? "(after)" : set[t];

				string ans = ignoreCase ? answersTrue[idx] : answersFalse[idx];

				bool check = result.StartsWith(ans);

				string preface = check ? "**   GOOD " : "***   FAIL";

				Debug.WriteLine($"{preface} search results | idx {idx, -4} | t {t, -4} | find {find, -6} | result {result, -10} | answer {ans}\n\n");
			}

			break;

			keys = testKeys1;
		}
	}

	public static void TestApxSearch1(bool ignoreCase)
	{
		string[] testKeys = ["ACDCOverview" ,  "CodeAnalysis", "DataGrid", "InfoBar", "Locals" ,  "Regex Editor Results Grid" , "Regex Quick Start Window", "ReSharper",  "TreeView"  , "Watch"];
		string[] testSearch = ["Abc", "Zex", "Mabc", "Locals", "Regx", "Localsx", "abc", "zax", "mabc", "Wat"];

		string[] answersFalse = ["ACDCOverview", "(after)", "Locals", "Locals", "ReSharp", "Locals", "(after)", "(after)", "(after)", "Watch"];
		string[] answersTrue = ["(before)", "(after)", "Locals", "Locals", "Regex Q", "Locals", "(before)", "(after)", "Locals", "Watch"];

		int t;
		int idx = -1;

		foreach (string find in testSearch)
		{
			idx++;

			t = findNearest(find, testKeys, ignoreCase);

			string result = t < 0 ? "(before)" : t == testKeys.Length ? "(after)" : testKeys[t];

			string ans = ignoreCase ? answersTrue[idx] : answersFalse[idx];

			bool check = result.StartsWith(ans);

			string preface = check ? "**   GOOD " : "***   FAIL";

			Debug.WriteLine($"{preface} search results | idx {idx} | t {t} | find {find} | result {result} | answer {ans}");
		}
	}

	// find a string, in a sorted list of strings even when not an exact match
	// return the string before the search string is after the test string
	// return -1 if the search string comes before the list or list.count if it comes after the list
	private static int findNearest(string find, string[] testKeys, bool ignoreCase)
	{
		bool b1;
		bool lastCharMatched = false;

		int j;
		int t;
		int m;

		char f1;
		char k1;

		string key;

		for (t = 0; t < testKeys.Length; t++)
		{
			key = testKeys[t];

			b1 = false;

			m = find.Length > key.Length ? key.Length : find.Length;

			for (j = 0; j < m; j++)
			{
				lastCharMatched = false;

				f1 = ignoreCase ? (find[j..(j + 1)].ToLower())[0] : find[j];
				k1 = ignoreCase ? (key[j..(j + 1)].ToLower())[0] : key[j];

				if (f1 > k1) break;

				if (f1 < k1)
				{
					b1 = true;
					break;
				}

				lastCharMatched = f1 == k1;
			}

			if (b1 || lastCharMatched) break;
		}

		if ((t < testKeys.Length && !lastCharMatched ) || (t == testKeys.Length && lastCharMatched)) t--;

		return t;
	}

	public static void TestApxSearch2()
	{
		//                 v = Abc                                                      v = Mabc & Locals                                                     v = Regex                           v = Zex
		string[] testKeys = ["ACDCOverview" ,  "CodeAnalysis", "DataGrid", "InfoBar", "Locals" ,  "Regex Editor Results Grid" , "Regex Quick Start Window", "ReSharper",  "TreeView"  , "Watch"];
		string[] testSearch = ["Mabc", "Locals", "Abc", "Zex", "Regx"];
		string[] answers =  ["Locals", "Locals", "(before)", "(after)", "ReSharp"];

		int pos = -1;
		int idx = -1;
		int i = 0;
		int j = 0;
		int t = 0;

		int l1;

		char f1;
		char k1;


		foreach (string find in testSearch)
		{
			idx++;

			t = 0;
			i = 0;

			l1 = find.Length - 1;

			for (t = 0; t < testKeys.Length; t++)
			{
				string key = testKeys[t];

				f1 = find[i];
				k1 = key[i];

				if (f1 > k1) continue;

				if (f1 < k1) break;

				// for (j = 0; j < l1; j++)
				for (j = 1; j < l1; j++)
				{
					i++;

					f1 = find[i];
					k1 = key[i];

					if (f1 > k1)
					{
						i = 0;
						break;
					}

					if (f1 < k1)
					{
						i = l1 + 1;
						break;
					}

					// they are equal
				}

				if (i > l1) break;

				continue;
			}

			t--;

			string result = t < 0 ? "(before)" : t > testKeys.Length - 1 ? "(after)" : testKeys[t];

			Debug.WriteLine($"** search results | idx {idx} | t {t} | find {find} | result {result} | answer {answers[idx]}");
		}
	}

	public static void TestApxSearch3()
	{
		//                 v = Abc                                                      v = Mabc & Locals                                                     v = Regex                           v = Zex
		string[] testKeys = ["ACDCOverview" ,  "CodeAnalysis", "DataGrid", "InfoBar", "Locals" ,  "Regex Editor Results Grid" , "Regex Quick Start Window", "ReSharper",  "TreeView"  , "Watch"];
		string[] testSearch = ["Abc", "Zex", "Mabc", "Locals", "Regx", "Localsx", "abc", "zax", "mabc", "Waa"];

		string[] answersFalse = ["ACDCOverview", "Watch", "Locals", "Locals", "ReSharp", "Locals", "Watch", "Watch", "Watch", "Watch"];
		string[] answersTrue = ["(before)", "(after)", "Locals", "Locals", "ReSharp", "Locals", "(before)", "(after)", "Locals", "Watch"];


		int idx;
		int i;
		int j;
		int t;

		int l1;

		char f1;
		char k1;

		string key;

		bool ignoreCase = false;

		for (int k = 0; k < 2; k++)
		{
			Debug.WriteLine($"\n\nignore case {ignoreCase}");

			idx = -1;

			foreach (string find in testSearch)
			{
				idx++;

				Debug.WriteLine($"**searching for| {find}");

				l1 = find.Length - 1;

				for (t = 0; t < testKeys.Length; t++)
				{
					i = 0;

					key = testKeys[t];

					Debug.WriteLine($"\t**vs {key}");

					for (j = 0; j < l1 + 1; j++)
					{
						f1 = ignoreCase ? (find[i..(i + 1)].ToLower())[0] : find[i];
						k1 = ignoreCase ? (key[i..(i + 1)].ToLower())[0] : key[i];

						Debug.Write($"\t**{"  ".Repeat(j)} j={j} | i={i} | >{f1}< vs >{k1}<");

						i++;

						if (f1 > k1)
						{
							// i = 0;
							Debug.WriteLine(" >> f1 is greater - next key");

							break;
						}

						if (f1 < k1)
						{
							Debug.WriteLine(" >> f1 is less - done");
							i = l1 + 1;
							break;
						}

						if (i > l1)
						{
							Debug.WriteLine(" >> max character check - break");
							break;
						}

						Debug.WriteLine(" >> f1 is equal - next check character");
					}

					if (i > l1) break;
				}

				t--;

				string result = t < 0 ? "(before)" : t > testKeys.Length - 1 ? "(after)" : testKeys[t];

				string ans = ignoreCase ? answersTrue[idx] : answersFalse[idx];

				Debug.WriteLine($"*** search results | idx {idx} | t {t} | find {find} | result {result} | answer {ans}\n\n");
			}

			ignoreCase = !ignoreCase;
		}
	}

	// good search but does not return (before) or (after) when the find string is before or after the list of strings
	public static void TestApxSearch4(bool ignoreCase)
	{
		List<string> testKeys =  ["ACDCOverview" ,  "CodeAnalysis", "DataGrid", "InfoBar", "Locals" ,  "Regex Editor Results Grid" , "Regex Quick Start Window", "ReSharper",  "TreeView"  , "Watch"];
		string[] testSearch = ["Abc", "Zex", "Mabc", "Locals", "Regx", "Localsx", "abc", "zax", "mabc", "Wat"];

		string[] answersFalse = ["ACDCOverview", "(after)", "Locals", "Locals", "ReSharp", "Locals", "(after)", "(after)", "(after)", "Watch"];
		string[] answersTrue = ["(before)", "(after)", "Locals", "Locals", "Regex Q", "Locals", "(before)", "(after)", "Locals", "Watch"];

		int t;
		int idx = -1;
		int nextNearest;

		foreach (string find in testSearch)
		{
			idx++;
			string result;

			t = testKeys.BinarySearch(find, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

			if (t >= 0)
			{
				result = testKeys[t];
				nextNearest = -1;
			}
			else
			{
				nextNearest = ~t;

				if (nextNearest == 0)
				{
					result = testKeys[0];
				}
				else if (nextNearest >= testKeys.Count)
				{
					result = testKeys[^1];
				}
				else
				{
					result = testKeys[nextNearest - 1];
				}
			}

			// result = t < 0 ? "(before)" : t > testKeys.Count - 1 ? "(after)" : testKeys[t];

			string ans = ignoreCase ? answersTrue[idx] : answersFalse[idx];

			bool check = result.StartsWith(ans);

			string preface = check ? "**   GOOD " : "***   FAIL";

			Debug.WriteLine($"{preface} search results | idx {idx, -3} | t {t, -4} | nextNearest {nextNearest, -3} | find {find, -9} | result {result, -12} | answer {ans}");
		}
	}

	public static void TestApxSearch5()
	{
		string[] testKeys = ["ACDCOverview" ,  "CodeAnalysis", "DataGrid", "InfoBar", "Locals" ,  "Regex Editor Results Grid" , "Regex Quick Start Window", "ReSharper",  "TreeView"  , "Watch"];
		string[] testSearch = ["Abc", "Zex", "Mabc", "Locals", "Regx", "Localsx", "abc", "zax", "mabc", "Wat"];

		string[] answersFalse = ["ACDCOverview", "(after)", "Locals", "Locals", "ReSharp", "Locals", "(after)", "(after)", "(after)", "Watch"];
		string[] answersTrue = ["(before)", "(after)", "Locals", "Locals", "Regex Q", "Locals", "(before)", "(after)", "Locals", "Watch"];

		bool b1;
		bool lastCharMatched = false;

		int idx;
		// int i;
		int j;
		int t;
		int m;

		char f1;
		char k1;

		string key;

		bool ignoreCase = false;

		for (int k = 0; k < 2; k++)
		{
			Debug.WriteLine($"\n\nignore case {ignoreCase}");

			idx = -1;

			foreach (string find in testSearch)
			{
				idx++;

				Debug.WriteLine($"**searching for| {find}");

				//l1 = find.Length - 1;

				for (t = 0; t < testKeys.Length; t++)
				{
					key = testKeys[t];

					b1 = false;

					m = find.Length > key.Length ? key.Length : find.Length;

					Debug.WriteLine($"\t** vs {key} t={t} | max characters to search {m}");

					for (j = 0; j < m; j++)
					{
						lastCharMatched = false;

						f1 = ignoreCase ? (find[j..(j + 1)].ToLower())[0] : find[j];
						k1 = ignoreCase ? (key[j..(j + 1)].ToLower())[0] : key[j];

						Debug.Write($"\t**{"  ".Repeat(j + 1)} j={j} | >{f1}< vs >{k1}<");

						if (f1 > k1)
						{
							// i = 0;
							Debug.Write(" >> f1 is greater");
							break;
						}

						if (f1 < k1)
						{
							Debug.Write(" >> f1 is less");
							b1 = true;
							break;
						}

						lastCharMatched = f1 == k1; // &&  key.Length >= find.Length;

						Debug.WriteLine(" >> check next character");
					}

					if (b1 || lastCharMatched)
					{
						if (b1) Debug.WriteLine("** >> got b1 - break");
						if (lastCharMatched) Debug.WriteLine("** >> got lastCharMatched - break");
						break;
					}

					Debug.WriteLine(" >> next key");
				}

				Debug.WriteLine(" >> done");

				// if (t < testKeys.Length || (t == testKeys.Length && lastCharMatched)) t--;
				if ((t < testKeys.Length && !lastCharMatched ) || (t == testKeys.Length && lastCharMatched)) t--;

				string result = t < 0 ? "(before)" : t > testKeys.Length - 1 ? "(after)" : testKeys[t];

				string ans = ignoreCase ? answersTrue[idx] : answersFalse[idx];

				bool check = result.StartsWith(ans);

				string preface = check ? "**" : "***";

				Debug.WriteLine($"{preface} search results | lastCharMatched {lastCharMatched} | idx {idx} | t {t} | find {find} | got {result} | want {ans}\n\n");
			}

			ignoreCase = !ignoreCase;
		}
	}


	private static bool? testApxSearch(string find, bool ignoreCase, out int foundIndex)
	{
		foundIndex = -1;

		if (keys == null || keys.Length == 0) return false;

		// ImmutableSortedSet<TKey> set =  keys.ToImmutableSortedSet();
		set =  keys.Select(k => k.ToString()!).ToImmutableSortedSet();


		// this causes a round up
		int pos = (set.Count + 1) / 2;
		// this causes a round up
		int jump = (int) ((pos + 1) * 0.5);
		bool done = false;

		string preface;

		int priorDirection = 1;
		int direction = 1;

		int pass = 0;

		if (!seenList)
		{
			Debug.Write("** set list > ");
			foreach (string se in set)
			{
				Debug.Write($" | {se}");
			}

			Debug.WriteLine(" < at end \n");

			Debug.WriteLine($"start pos {pos, -4} | jump {jump, -4}\n");

			seenList = true;
		}

		StringComparison c = ignoreCase ?  StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

		// shortcuts - check for before the first key
		if (String.Compare(find, set[0], c) < 0)
		{
			foundIndex = -1;
			Debug.WriteLine("** >  FOUND < before");
			return null;
		}

		// and after last key
		if (String.Compare(find, set[^1], c) > 0)
		{
			foundIndex = set.Count;
			Debug.WriteLine("** >  FOUND < after");
			return null;
		}


		do
		{
			preface = $"{"   ".Repeat(pass)}pass {pass,-3}";

			Debug.Write($"{preface, -prefaceWidth}");

			priorDirection = direction;

			if (String.Compare(find, set[pos], c) > 0)
			{
				direction = 1;
				Debug.WriteLine($" {"find is larger",-15} | position {pos, -3} | jump {jump, -3} | {find,-10} vs {set[pos], -30} | dir {direction,-3} vs pDir {priorDirection,-3}");
				pos += jump;
			}
			else if (String.Compare(find, set[pos], c) < 0)
			{
				direction = -1;
				Debug.WriteLine($" {"find is smaller",-15} | position {pos,-3} | jump {jump,-3} | {find,-10} vs {set[pos], -30} | dir {direction,-3} vs pDir {priorDirection,-3}");
				pos -= jump;
			}
			else
			{
				Debug.WriteLine($" {"find is equal",-15} | position {pos,-3} | jump {jump,-3} | {find,-10} vs {set[pos], -30} | done and return");
				// found exact match
				foundIndex = pos;
				return true;
			}

			// preface = $"{"   ".Repeat(pass + 1)}pass {pass,-3}";

			// if (jump < 0)
			// {
			// 	Debug.WriteLine($"{preface, -prefaceWidth} at end | position {pos,-3} | jump {jump,-3} | done and return");
			// 	done = true;
			// 	foundIndex = pos;
			// 	foundKey = string.Empty;
			// }
			// else
			// if (jump == 1)
			// {
			//
			// 	Debug.WriteLine($"{preface, -prefaceWidth} jump is 1 | position is {pos,-3} | done and return");
			// 	done = true;
			// 	foundIndex = simpleSearch(pos, find);
			// 	foundKey = set[pos];
			// }
			// else

			{
				pass++;
				// round up
				jump = (int) ((jump + 1) * 0.5);
			}

			if ((jump == 1 && direction != priorDirection) || jump == 0)
			{
				foundIndex = pos - 1;

				Debug.WriteLine($"{preface, - prefaceWidth} {"got end",-15} | fIndex   {foundIndex,-3} | jump {jump,-3} | {find,-10} vs {set[foundIndex], -30} | dir {direction,-3} vs pDir {priorDirection,-3}");

				done = true;
			}
		}
		while (!done);

		Debug.WriteLine($" << done");

		return true;
	}

	// private static int simpleSearch(int startIdx, string find)
	// {
	// 	// starting at the startIdx, compare each item
	// 	// if jump is always 1
	// 	// look for direction change
	//
	// 	bool done = false;
	// 	int idx = startIdx;
	// 	int j = 0;
	//
	// 	string preface;
	//
	// 	while (!done)
	// 	{
	// 		pass++;
	//
	// 		preface = $"{"   ".Repeat(pass + 1)}pass {pass,-3}";
	// 		Debug.Write($"{preface, -prefaceWidth}");
	//
	//
	// 		// is find greater than the current key
	// 		if (String.Compare(find, set[idx]) > 0)
	// 		{
	// 			Debug.Write($" find is greater | position {idx, -3} | test {find,-10} vs {set[idx]}");
	//
	// 			if (j < 0)
	// 			{
	// 				Debug.WriteLine($" | j < 0 - done and return idx = {idx - 1} [ {set[idx - 1]} ]");
	// 				done = true;
	// 				return idx - 1;
	// 			}
	//
	// 			j = 1;
	// 			idx++;
	//
	// 			continue;
	// 		}
	//
	// 		if (String.Compare(find, set[idx]) > 0)
	// 		{
	// 			Debug.WriteLine($" find is smaller | position {idx, -3} | test {find,-10} vs {set[idx]}");
	// 			if (j > 0)
	// 			{
	// 				Debug.WriteLine($" | j > 0 - done and return idx = {idx + 1} [ {set[idx + 1]} ]");
	// 				done = true;
	// 				return idx + 1;
	// 			}
	//
	// 			j = -1;
	// 			idx--;
	// 			continue;
	// 		}
	//
	// 		done = true;
	// 	}
	//
	// 	return idx;
	// }

	*/
}