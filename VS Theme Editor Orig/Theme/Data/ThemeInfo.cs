using System.IO;

using static VS_Theme_Editor_Orig.DataManager;

namespace VS_Theme_Editor_Orig;



public struct ThemeInfo
{
	private Guid guid;

	public ThemeInfo(List<ThemeFile> vsThemeInfo, ThemeStyle style)
	{
		ThemeList = new ();

		initThemeList(vsThemeInfo);

		Name = Path.GetFileNameWithoutExtension(vsThemeInfo[0].FilePath);
		Guid = Guid.Empty;
		Style = style;
		IsInit = false;

		Source = DataManager.Instance.GetVsThemeSource(style);
	}

	public Guid Guid
	{
		readonly get => guid;
		set
		{
			guid = value;
		}
	}

	public string Name { get; private set; }

	public ThemeStyle Style { get; set; }

	public ThemeSource Source { get; set; }

	/// <summary>
	/// flag for struct status<br/>
	/// true, good and completed
	/// false, good but incomplete
	/// null, created but no good / is invalid
	/// </summary>
	public bool? IsInit { get; private set; } = null;

	public List<ThemeFile> ThemeList { get; private set; }

	private void initThemeList(List<ThemeFile> vsThemeInfo)
	{
		int cnt = (ThemeIndex.RT_SHELL - ThemeIndex.RT_THEME) + 1;

		// create all of the possible elements in this list
		for (int i = 0; i < cnt; i++)
		{
			ThemeList.Add(new ());
		}

		foreach (ThemeFile vtf in vsThemeInfo)
		{
			if (vtf.ThemeIndex < 0 || ((int) vtf.ThemeIndex) > (cnt - 1)) continue;

			ThemeList[(int) vtf.ThemeIndex] = vtf;
		}
	}

	public ThemeInfo Clone()
	{
		ThemeInfo copy = new ();

		copy.Name = Name;
		copy.Guid = Guid;
		copy.IsInit = IsInit;
		copy.Style = Style;
		copy.ThemeList = new ();

		return copy;
	}
}
