using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VS_Theme_Editor
{
	[INotifyPropertyChanged]
	public partial class MruManager
	{
		public MruManager()
		{
			// dataMgr = DataManager.Instance;
			folders = new MostRecentlyUsedFolderList();
			files = new MostRecentlyUsedFileList();

			Load();

			// initPopulate();
		}

		public ICollectionView FoldersView => folders.View;
		public ICollectionView FilesView => files.View;

		// // [0,0] = Light Theme, [0,1] = Dark Theme / 2022
		// // [1,0] = Light Theme, [1,1] = Dark Theme / 2026
		// private MostRecentlyUsed[,] BaseThemes;

		private MostRecentlyUsedFolderList folders;

		private MostRecentlyUsedFileList files;

		// private DataManager dataMgr;

		public int FoldersCount => folders.Count;
		public int FilesCount => files.Count;

		public void Load()
		{ 
			folders.Load();
			files.Load();

			getThemeNamesAfterLoad();
		}

		public void Save()
		{
			folders.Save();
			files.Save();
		}

		public MostRecentlyUsed Add(string filePath, string themeName)
		{
			folders.Add(new MostRecentlyUsed(filePath, null));
			
			MostRecentlyUsed mru = new MostRecentlyUsed(filePath, false);
			mru.ThemeName = themeName;

			files.Add(mru);

			Save();

			return mru;
		}

		// private void initPopulate()
		// {
		// 	for (int i = 0; i < dataMgr.VsDataList.Length; i++)
		// 	{
		// 		folders.Add(new MostRecentlyUsed(dataMgr.VsDataList[i].VsThemeDir, true));
		// 	}
		//
		// 	foreach (string folder in dataMgr.InitUserThemeFolders)
		// 	{
		// 		folders.Add(new MostRecentlyUsed(string.Format(folder, dataMgr.UsrName), true));
		// 	}
		//
		// 	folders.Save();
		// }

		private void getThemeNamesAfterLoad()
		{
			foreach (MostRecentlyUsed mru in FilesView)
			{
				// mru.ThemeName = "TBD Name";
				mru.ThemeName = ThemeManager.Instance.GetThemeNameFromFile(mru.FilePath);
			}
		}

	}
}
