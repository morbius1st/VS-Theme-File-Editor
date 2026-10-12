using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VS_Theme_Editor;

[ObservableObject]
internal partial class VisualStudioData
{
	public static VisualStudioData Instance { get; private set; }

	static VisualStudioData()
	{
		if (Instance == null)
		{
			Instance = new VisualStudioData();
		}
	}

	private VisualStudioData() { }

	// current Visual Studio instance information
	[ObservableProperty]
	private VsInfo vsInfo;

	// current Visual Studio instance directory
	[ObservableProperty]
	private string vsDirectory;

	// current Visual Studio instance description
	[ObservableProperty]
	private string vsDesc;

	// current Visual Studio instance description border brush
	[ObservableProperty]
	private Brush vsDescBdr;

	// current Visual Studio instance theme directory
	[ObservableProperty]
	public string vsThemeDir;

	// current Visual Studio instance theme registration file name
	public string VsId => vsInfo.VsId;


	// general info

	public string UsrName => Environment.UserName;

	public static readonly string[] VsidList = [VSID_2022COMM, VSID_2026COMM];

	public const string VSD_UDF = "Un-determined";

	public const string VSID_UKN = "VS UnKnown";
	public const string VSID_2022COMM = "VS2022Comm";
	public const string VSID_2026COMM = "VS2026Comm";

	private static readonly Brush Vs2022CommBdrBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x1E, 0x90, 0xFF));
	private static readonly Brush Vs2026CommBdrBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xFF, 0x00));

	// set vs version based information

	public bool GetVsIdForFilePath(string filePath, out string vsId)
	{
		vsId = string.Empty;
		bool result = false;

		if (!File.Exists(filePath)) return false;
		if (!Utilities.DivideFilePath(filePath,
				out string? directory, out string? fileName, out string? fileNameEx)) return false;

		foreach ((string _ , VsInfo vsi) in VsInfoList)
		{
			if (verifyIfUserThemeFilePath(vsi, directory))
			{
				vsId = vsi.VsId;
				result = true;
				break;
			}

			if (verifyIfVsThemeFilePath(vsi, directory))
			{
				vsId = vsi.VsId;
				result = true;
				break;
			}
		}

		return result;
	}

	private bool verifyIfVsThemeFilePath(VsInfo vsi, string dir)
	{
		return vsi.VsThemeDir.Equals(dir, StringComparison.OrdinalIgnoreCase);
	}

	public bool SetVsInfo(string filePath)
	{
		if (!File.Exists(filePath)) return false;

		if (!Utilities.DivideFilePath(filePath,
				out string? directory, out string? fileName, out string? fileNameEx)) return false;

		VsInfo = new();

		foreach ((string _ , VsInfo vsi) in VsInfoList)
		{
			if (setVsInfoIfUser(vsi, directory!)) return true;
			if (setVsInfoIfVs(vsi, directory!)) return true;
		}
		
		return false;
	}

	/// <summary>
	/// based on the filepath, get the vsId
	/// </summary>
	public bool GetVsIdx(string filePath, out string? vsId)
	{
		vsId = null;

		if (!File.Exists(filePath)) return false;

		if (!Utilities.DivideFilePath(filePath,
				out string? directory, out string? fileName, out string? fileNameEx)) return false;

		foreach ((string _ , VsInfo vsi) in VsInfoList)
		{
			if (directory.StartsWith(vsi.VsThemeDir, StringComparison.OrdinalIgnoreCase)
				|| verifyIfUserThemeFilePath(vsi, directory)
				)
			{
				vsId = vsi.VsId;
				return true;
			}
		}

		return false;
	}

	private bool verifyIfUserThemeFilePath(VsInfo vsi, string dir)
	{
		for (int i = 0; i < vsi.UserThemeDirs.Length; i++)
		{
			if (dir.Contains(vsi.UserThemeDirs[i])) return true;
		}

		return false;
	}

	private bool setVsInfoIfUser(VsInfo vsi, string dir)
	{
		if (verifyIfUserThemeFilePath(vsi, dir))
		{
			VsInfo = vsi;
			assignVsInfo();
			return true;
		}

		return false;
	}

	private bool setVsInfoIfVs(VsInfo vsi, string dir)
	{
		if (vsi.VsThemeDir.Equals(dir, StringComparison.OrdinalIgnoreCase))
		{
			VsInfo = vsi;
			assignVsInfo();
			return true;
		}

		return false;
	}

	private void assignVsInfo()
	{
		VsDirectory = vsInfo.VsPath;
		VsDesc = vsInfo.PresentationName;
		VsDescBdr = vsInfo.BdrHighlightBrush;
		VsThemeDir = vsInfo.VsThemeDir;
	}

	public string GetVsThemePath(string vsId)
	{
		VsInfo vsi = VsInfoList[vsId];

		return vsi.VsThemeDir;
	}

	public string GetVsThemeRegFileName(string vsId)
	{
		VsInfo vsi = VsInfoList[vsId];

		return vsi.VsThemeRegFileName;
	}

	public string GetVsThemeRegFilePath(string vsId)
	{
		string name = VsInfoList[vsId].VsThemeRegFileName;
		return Path.Combine(VsInfoList[vsId].VsThemeDir, name);
	}

	// fixed data for each Visual Studio types
	public Dictionary<string, VsInfo> VsInfoList = new ()
	 {
		{ VSID_2022COMM, new VsInfo(VSID_2022COMM, "2022", "Community", ["17", "17.0"], "Visual Studio 2022 Community",
			Vs2022CommBdrBrush,
			@"C:\Program Files\Microsoft Visual Studio\2022\Community",
			@"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Platform",
			"Theme.Registration.pkgdef", "EditorColors.pkgdef",
			[@"Microsoft\VisualStudio\17.0"] ) },

		{ VSID_2026COMM, new VsInfo(VSID_2026COMM, "2026", "Community", ["18", "18.0"], "Visual Studio 2026 Community",
			Vs2026CommBdrBrush,
			@"C:\Program Files\Microsoft Visual Studio\18\Community",
			@"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Platform",
			"Themes.Registration.pkgdef", "EditorColors.pkgdef",
			[@"Microsoft\VisualStudio\18.0"] ) },
	};

	public string XlateVsThemeFileName(string vsId,string testName)
	{
		if (VsThemeFileNameXlate.TryGetValue(vsId, out Dictionary<string, string>? xlateDict))
		{
			if (xlateDict.TryGetValue(testName, out string? xlateName))
			{
				return xlateName;
			}
		}
		
		return testName;
	}

	public Dictionary<string, Dictionary<string, string>> VsThemeFileNameXlate = new ()
	{
		{
			VSID_2026COMM, new Dictionary<string, string>()
			{
				{ "Theme.LightExtraContrast.pkgdef", "Editor.Light.ExtraContrast.pkgdef" },
				{ "Theme.DarkExtraContrast.pkgdef", "Editor.Dark.ExtraContrast.pkgdef" },
			}
		}
	};

}
