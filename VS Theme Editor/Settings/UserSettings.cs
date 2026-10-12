using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows;
using VS_Theme_Editor;

// User settings (per user) 
//  - user's settings for a specific app
//	- located in the user's app data folder / app name

// ReSharper disable once CheckNamespace

namespace SettingsManager;

#region user data class

// this is the actual data set saved to the user's configuration file
// this is unique for each program
[DataContract(Namespace = "")]
public class UserSettingDataFile : IDataFile
{
	[IgnoreDataMember]
	public string DataFileVersion => "user 7.5u";

	[IgnoreDataMember]
	public string DataFileDescription => "user setting file VS Theme Editor";

	[IgnoreDataMember]
	public string DataFileNotes => "Contains the User's MRU list";

	[DataMember(Order = 1)]
	public int FolderListItemsMax { get; set; } = 10;

	[DataMember(Order = 2)]
	public int FileListItemsMax { get; set; } = 10;

	[DataMember(Order = 3)]
	public ObservableCollection<MostRecentlyUsed>? MruFolderList { get; set; } = new ();

	[DataMember(Order = 4)]
	public ObservableCollection<MostRecentlyUsed>? MruFileList { get; set; } = new ();

	[DataMember(Order = 5)]
	// ReSharper disable once InconsistentNaming
	public Point WinThemeEditorTL { get; set; }

	[DataMember(Order = 6)]
	// ReSharper disable once InconsistentNaming
	public Point WinColorChooserTL { get; set; }
}

#endregion


// , APP_SETTINGS, SUITE_SETTINGS, MACH_SETTINGS, SITE_SETTINGS