// #define DML0 // not yet used
// #define DML1 // do not use here ** defined in properties *** start and end
// #define DML2 // turns on or off bool flags / button enable flags only / listbox idex set
// #define DML3 // various status messages
// #define DML4 // update status status messages
// #define DML5 // orator routines
// #define TRACE

// the above controls the output of the Debug Message information

// *********************
//  Settings Manager 7.5
// *********************
// USER_SETTINGS; APP_SETTINGS; SUITE_SETTINGS; MACH_SETTINGS; SITE_SETTINGS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;



#if WPFfmt

using Microsoft.WindowsAPICodePack.Dialogs;

#endif

using UtilityLibrary;
using static SettingsManager.SettingMgrStatus;

// ReSharper disable CommentTypo
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo



// version and requirements
// requires a reference to:
// System.Runtime.Serialization
// System.Xaml

// item name:	Config
// username:	jeff s
// created:		12/30/2017 4:42:00 PM

//	ver 1.0		initial version
//	ver 2.0		revise to use DataContract
//	ver 2.1		refine use fewer classes / abstract classes
//	ver	2.2		refine
//	ver 2.3		refine move utility methods to library file
//	ver 2.4		move setting file specific info out of base file
//	ver 2.4.1	revise date format
//	ver 2.5		move static setting classes into base file
//				added delegate to properly reset data
//	ver 2.6		incorporate [OnDeserializing] and proper default values
//	ver 3.0		updated to incorporate checking for setting file being
//				a different version and to upgrade to the current version
//	ver 3.1		added call to SyncData() at the end of a normal read();
//	ver 4.0		updated to allow a machine level and site level setting file
//	ver 5.0 & 6.0	refine support classes to use generics and consolidate code
//	ver 6.1		incorporate a data only component
//	ver	7.0		add support for suites
//	ver	7.0.1	added conditional compilation at primary class definitions
//              adjust site PathAndFile to include company and assembly name
//	ver	7.0.2	enhance path to have bool for rootpath is valid and settingpath is valid
//	ver 7.1		* adjust data file system to allow more than one data file, of a type, 
//				accessible at the same time
//				* remove the version number from class names
//				* adjust names to clarify path to a file "FilePath" versus
//				a path to a folder "FolderPath"
//				* add some summary descriptions
//	ver 7.2		* adjust user, app, and suite settings to allow more than one file per suite of programs
//	ver 7.2.1	* lots of small adjustments to make it work better
//				* adjusted header data to allow on the fly adjustments to data file specific parts
//				* incorporated self-check that the file read is of the correct "file type" (this 
//					is only a basic test that can be bypassed - but an exception would occur)
//				* adjusted setting file and data file to remove most "namespace" identifiers
//				* adjusted root xml element name
//				* added some notes to seed data file
//	ver 7.3		* adjust classes & provide an interface to put Header's Setting File specific
//					information to be initialized from the data class
//	ver 7.4		* adjust to make app settings a non-per user file - that is, per app file between suite level and user level
//				* put header data into data classes
//				* adjust classes / inheritance to remove the header class from the data file & collapse code
//				* remove Save()
//				* rename BaseDataFile class to DataManager
//				* add Create() to DataManager
//				* misc other minor adjustments
//  ver 7.4.1	* adjust the data paths for DataManager
//  ver 7.4.2	* removed protected access
//				* add FilePath<FileNameSimple> to Path class
//  ver 7.4.3	* add "Exists" to path base file
//				* adjusted setting file locations to be in a sub-folder based on the assembly name
//  ver 7.5		* adjust file location paths
//				* change header properties to internal rather than private
//	ver 7.5.1	* add "suite" folder path to each config file type (this is the folder for each assembly folder)
//				* add "IsSuite" to info class
//	ver 7.5.2	* adjust data file settings.  added IDataStore interface.  added DataFileType as a field to all data storage files


#region setting file hireacrhy

/*

file locations:
{ApplicationData} = C:\Users\jeffs\AppData\Roaming
{CommonApplicationData} = C:\ProgramData
{CompanyName} = \CyberStudio
{AssemblyName} = \SettingManager75
{SuiteName} = \SettingManager75  (or, if null, {assemblyname})

$RootPathSuite$ =  {CommonApplicationData}
$RootPathUser$ =  {ApplicationData}


site settings

		C:\ProgramData
		               \CyberStudio
					                \SettingManager
									                  \ site.setting.xml

		default:
		$RootPathSuite$ \ {CompanyName} \ {SuiteName} \ site.setting.xml

		directed:
		{AssignedRootPath} \ {SuiteName} + .site.setting.xml

machine settings

		C:\ProgramData
		               \CyberStudio
					                \SettingManager
									                  \ machine.setting.xml

		$RootPathSuite$ \ {CompanyName} \ {SuiteName} \ machine.setting.xml

suite settings

		C:\ProgramData
		               \CyberStudio
					                \SettingManager
									                 \ suite.setting.xml

		$RootPathSuite$ \ {CompanyName} \ {SuiteName} \ suite.setting.xml


app settings

		C:\ProgramData
		               \CyberStudio
					                \SettingManager
									                  \SettingsManager75 \ app.setting.xml

		$RootPathSuite$ \ {CompanyName} \ {SuiteName} \ {appName} \ app.setting.xml

user settings

		C:\Users\jeffs\AppData\Roaming
									   \CyberStudio
													\SettingManager
																    \SettingsManager75 \ user.setting.xml

		$RootPathUser$ \ {CompanyName} \ {SuiteName} \ {appName} \ user.setting.xml

data store

path and filename {.xml} is user assigned (SubFolders are set to null)





top
^   +-> site level (per suite)
|   |	Site settings:
| ^ |		- applies to all machines & all users
| | |		- holds information needed by all machines and, therefore, all users
| | v		- maybe placed on the local or remote machine (specify location in app setting file)
| | 
| | +--> Mach settings (per suite) [example location/file (by setting mgr): C:\ProgramData\CyberStudio\Andy\Andy.machine.setting.xml]
| | |		- applies to a specific machine / all users on that machine
| | |		- holds information needed by or shared between all users on the machine
| | v		- located in the common app data folder (currently c:\program data)		
| | 
| | per suite settings
+-| ----------------
| | per user settings
| |  
| | +-> Suite settings (per user) [example location/file (by setting mgr): C:\Users\jeffs\AppData\Roaming\CyberStudio\Andy\Andy.suite.setting.xml]
| | |		- applies to a specific app suite (multiple programs)
| | |		- holds information needed by all programs in the suite, but not all users of the suite
| | |		- provides the pointer to the site settings file (why here: allows each user to be associated with different site files)
| | v		- located in the user's app data folder
| |
| +<<<-- common to all apps
| +<<<-- specific to a single app
| | 
| | +-> App settings (per user) [example location/file (by setting mgr): C:\Users\jeffs\AppData\Roaming\CyberStudio\Andy\AndyConfig\AppSettings\AndyConfig.setting.xml]
| | |		- applies to a specific app in the suite
| | |		- holds information specific to the app
| | v		- located in the user's app data folder / app name / AppSettings
| | 
| v +-> User settings (per user) [example location/file (by setting mgr): C:\Users\jeffs\AppData\Roaming\CyberStudio\Andy\AndyConfig\user.setting.xml]
|   | 	- user's settings for a specific app
v   v		- located in the user's app data folder / app name
bottom
*/

#endregion

// ReSharper disable once CheckNamespace

namespace SettingsManager
{
	// ReSharper disable once UnusedType.Global
	internal class Av75 { }

	public enum SettingMgrStatus
	{
		SAVEDISALLOWED = -4,
		INVALIDPATH = -3,
		NOPATH = -2,
		DOESNOTEXIST = -1,

		BEGIN = 0,
		CONSTRUCTED,
		INITIALIZED,
		CREATED,
		READ,
		SAVED,
		EXISTS
	}

	public enum SettingFileType
	{
		SETTING_MGR_DATA  = -1,
		SETTING_MGR_USER  = 0,
		SETTING_MGR_APP   = 1,
		SETTING_MGR_SUITE = 2,
		SETTING_MGR_MACH  = 3,
		SETTING_MGR_SITE  = 4,

		// ReSharper disable once UnusedMember.Global
		LENGTH = 6
	}

#region + Heading

	[DataContract(Namespace = "")]
	public partial class Heading
	{
		public static string ClassVersionName    = nameof(DataClassVersion);
		public static string SettingsVersionName = nameof(SettingsVersion);

		[DataMember(Order = 0)] public string SaveDateTime       { get; internal set; } =
			DateTime.Now.ToString("yyyy-MM-dd - HH:mm zzz");

		[DataMember(Order = 1)] public string SavedBy            { get; internal set; } = Environment.UserName;
		[DataMember(Order = 2)] public string AssemblyVersion    { get; internal set; } = CsUtilities.AssemblyVersion;
		[DataMember(Order = 3)] public string AssemblyName       { get; internal set; } = CsUtilities.AssemblyName;
		[DataMember(Order = 4)] public string SettingsVersion    { get; internal set; } = "7.5.2";
		[DataMember(Order = 6)] public string DataClassVersion   = "Unassigned";
		[DataMember(Order = 5)] public string Notes              = "Unassigned";
		[DataMember(Order = 6)] public string Description        = "Unassigned";
	}

#endregion

	public delegate void RstData();

	public class SettingsMgr<TPath, TInfo, TData>
		where TPath : PathAndFileBase, new()
		where TInfo : SettingInfoBase<TData>, new()
		where TData : IDataFile, new()
	{

	#region + Constructor

		public SettingsMgr(TPath path, RstData rst)
		{
			Status = BEGIN;

			Path = path;

			if (path == null || String.IsNullOrWhiteSpace(Path.SettingFilePath))
			{
				Status = NOPATH;
			}

			_resetData = rst;

			Initialize();

			Status = CONSTRUCTED;
		}

		// ReSharper disable once InconsistentNaming
		private void Initialize()
		{
			if (Status != NOPATH)
			{
				Status = INITIALIZED;


// TODO redo the upgrade system
//				UpgradeRequired = !Info.ClassVersionsMatch(Path.ClassVersionFromFile);
//
//				if (FileExists())
//				{
//					if (!Info.ClassVersionsMatch(Path.ClassVersionFromFile) && CanAutoUpgrade)
//					{
//						Upgrade();
//					}
//				}
			}
		}

	#endregion

	#region + Properties

		public TInfo Info { get; private set; } =
			new TInfo();

		public TPath Path { get; set; }

		public SettingMgrStatus Status { get; private set; }

		public bool CanAutoUpgrade { get; set; } = false;

		public bool UpgradeRequired { get; set; } = false;

	#endregion

	#region + Read

		public void Read()
		{
			// Trace.Assert(DM.Start0(false, "reading", $"path id {Path.id}"),"");

			if (Path == null)
			{
				// Trace.Assert(DM.End0("end 1"),"");
				return;
			}

			if (Path.SettingFilePath.IsVoid() )
			{
				Status = INVALIDPATH;

				// Trace.Assert(DM.End0("end 2"),"");
				return;
			}

			// does the file already exist?
			if (FileExists())
			{
// TODO re-wirite the upgrade system
//				if (Info.ClassVersionsMatch(Path.ClassVersionFromFile))
//				{
				try
				{
					DataContractSerializer ds = new DataContractSerializer(typeof(TInfo));

					// file exists - get the current values

					using (FileStream fs = new FileStream(Path.SettingFilePath, FileMode.Open))
					{
						Info = (TInfo) ds.ReadObject(fs);
					}

					Status = READ;
				}
				// catch malformed XML data file and replace with default
				// change - issue warning and return
				catch (SerializationException)
				{
					msgDialog("ClassificationFile Error", 
						$"Cannot Read the Classification File", 
						$"The requested classification file\n\"{Path.SettingFilePath}\"\nhas errors and cannot be read.  Please fix these errors in order to proceed");

					// Create();
					// Write();
				}
				// catch any other errors
				catch (Exception e)
				{
					throw new MessageException(Environment.NewLine
						+ "Cannot read setting data for file: "
						+ Path.SettingFilePath + Environment.NewLine
						+ "(" + e.Message + ")" + Environment.NewLine
						, e.InnerException);
				}

// TODO re-write the upgrade system
//				}
//				else
//				{
//					Upgrade();
//				}
			}
			else
			{
				Create();
				Write();
			}

			// added in v3.1
			SyncData();

			// Trace.Assert(DM.Stat0($"settingfilepath is void? {Path.SettingFilePath.IsVoid()}"),"");

			// Trace.Assert(DM.End0(),"");
		}

		public dynamic Read(Type type)
		{
			dynamic p = null;

			if (FileExists())
			{
				try
				{
					DataContractSerializer ds = new DataContractSerializer(type);

					using (FileStream fs = new FileStream(Path.SettingFilePath, FileMode.Open))
					{
						p = ds.ReadObject(fs);
					}
				}
				catch
				{
					// ignored
				}
			}

			return p;
		}

	#endregion

	#region + Create

		// create a empty object - initialized with default data
		//  from the app / user setting's class
		public void Create()
		{
			// Trace.Assert(DM.Start0(),"");

			if (Path.SettingFilePath.IsVoid() )
			{
				Status = INVALIDPATH;
				return;
			}

			// since the data is "new" and may not match what
			// has been saved, note as not initialized
			Info = new TInfo();

			Status = CREATED;

			// Trace.Assert(DM.End0(),"");
		}

	#endregion

	#region + Write

		public void Write()
		{
			// // Trace.Assert(DM.Start0(),"");
			// Trace.Assert(DM.InOut0(),"");

			if (Path.IsReadOnly || Path.SettingFilePath.IsVoid())
			{
				Status = SAVEDISALLOWED;
				return;
			}

			XmlWriterSettings xmlSettings = new XmlWriterSettings() { Indent = true };

			DataContractSerializer ds = new DataContractSerializer(typeof(TInfo));

			using (XmlWriter w = XmlWriter.Create(Path.SettingFilePath, xmlSettings))
			{
				ds.WriteObject(w, Info);
			}

			// since file and memory match
			Status = SAVED;

			// // Trace.Assert(DM.End0(),"");
		}

	#endregion

	#region + Upgrade

		// TODO re-write the upgrade system
// upgrade must be re-written to allow for the (5) configuration setting files

//		// upgrade from a prior class version to the current class version
//		// this method is less restricted to allow the user to perform
//		// a manual upgrade - if CanAutoUpgrade is false
//		public void Upgrade()
//		{
//			// is an upgrade needed
//			if (!Info.ClassVersionsMatch(Path.ClassVersionFromFile))
//			{
//				// get a list of prior setting classes from 
//				// which to possible upgrade
//				List<SettingInfoBase<Tdata>> settings = null;
//
//				switch (Info.FileType)
//				{
//				case SettingFileType.APP:
//					{
//						settings =
//							GetMatchingClasses<AppSettingInfoBase<Tdata>>();
//						break;
//					}
//				case SettingFileType.USER:
//					{
//						settings =
//							GetMatchingClasses<UserSettingInfoBase<Tdata>>();
//						break;
//					}
//				}
//
//				// if needed, preform upgrade
//				if (settings != null)
//				{
//					UpgradeList(settings);
//
//					Save();
//
//					UpgradeRequired = false;
//				}
//			}
//		}

	#endregion

	#region + Reset

		private readonly RstData _resetData;

		// reset the data portion to it's default values
		public void Reset()
		{
			Create();

			_resetData?.Invoke();
		}

		public void SyncData()
		{
			_resetData?.Invoke();
		}

	#endregion

	#region + Utilities

		// report whether the setting file does exist
		// ReSharper disable once InconsistentNaming
		private bool FileExists()
		{
			// Trace.Assert(DM.Start0(),"");
			bool result = Path.SettingFileExists;

			Status = result ? EXISTS : DOESNOTEXIST;

			// Trace.Assert(DM.End0(),"");

			return result;
		}

		// use reflection to create a list of classes of matching type
		// that may be used to upgrade the existing class to the
		// current version
		// ReSharper disable once InconsistentNaming
		// ReSharper disable once UnusedMember.Local
		private List<SettingInfoBase<TData>> GetMatchingClasses<TU>()
			where TU : SettingInfoBase<TData>

		{
			Type baseType = typeof(TInfo).BaseType;

			List<SettingInfoBase<TData>> list = new List<SettingInfoBase<TData>> { Info };

			Type[] types = Assembly.GetExecutingAssembly().GetTypes();

			// scan the list of all of the types to find the
			// few of the correct type
			foreach (Type type in types)
			{
				if (type == Info.GetType()) continue;

				Type testType = type.BaseType;

				if (testType == baseType)
				{
					list.Add((TU) Activator.CreateInstance(type));
				}
			}

			return list;
		}

		private void msgDialog(string title, string main, string message)
		{
		#if WPFfmt

			TaskDialog td = new TaskDialog();
			td.Caption = title;
			td.InstructionText = main;
			td.Text = message;
			td.Icon = TaskDialogStandardIcon.Error;
			td.Cancelable = false;
			// td.OwnerWindowHandle = ScreenParameters.GetWindowHandle(Common.GetCurrentWindow());
			td.StartupLocation = TaskDialogStartupLocation.CenterOwner;
			td.Show();
			
		#endif

		}


		// based on a list of matching setting of prior versions
		// upgrade the existing to the current version
		// ReSharper disable once UnusedMember.Local
		// ReSharper disable once InconsistentNaming
		// private void UpgradeList<TData>(List<SettingInfoBase<TData>> settings)
		// 	where TData : HeaderData, new()
		// {
		// 	if (settings == null || settings.Count < 2)
		// 	{
		// 		return;
		// 	}
		//
		// 	if (Info.ClassVersionsMatch(Path.ClassVersionFromFile))
		// 	{
		// 		return;
		// 	}
		//
		// 	settings.Sort(); // must be in the correct order from oldest to newest
		//
		// 	for (int i = 0; i < settings.Count; i++)
		// 	{
		// 		int j = String.Compare(settings[i].DataClassVersion,
		// 			Path.ClassVersionFromFile, StringComparison.Ordinal);
		//
		// 		if (j < 0) continue;
		//
		// 		if (j == 0)
		// 		{
		// 			// found the starting point, read the current setting
		// 			// file into memory
		// 			settings[i] = Read(settings[i].GetType());
		// 		}
		// 		else
		// 		{
		// 			settings[i].UpgradeFromPrior(settings[i - 1]);
		// 		}
		// 	}
		// }
		//

	#endregion

	}

#region + Path File and Utilities

	// this class will be associated with the main class
	public abstract class PathAndFileBase
	{
		protected const string SETTINGFILEBASE = @".setting.xml";
		protected const string SETTINGFILEBASE_SUFFIX = @".setting";
		public const string SETTINGFILEBASE_EXT = @"xml";

		private static int idx = 100;
		public int Id;

		internal string   fileNameRoot;
		internal string   fileNameSuffix;
		internal string   fileNameExtNoSep;
		internal string   fileName;
		internal string   rootFolderPath;
		internal string   suiteFolderPath;
		internal string[] subFolders = new [] { CsUtilities.AssemblyName };
		internal string   settingFolderPath;
		internal string   settingFilePath;

		public int id
		{
			get => Id;
			set
			{
				Id = idx++;
			}
		}

		protected PathAndFileBase()
		{
			// Trace.Assert(DM.Start0(false, "no path info"), "");

			// DM.Start0(false, "no path info");

			id = 0;

			// ReSharper disable once VirtualMemberCallInConstructor
			Configure();
			ConfigureFilePath();

			// Trace.Assert(DM.End0(), "");
		}

		protected PathAndFileBase(string rootPath, string[] subfolders, string filename)
		{
			// Trace.Assert(DM.Start0(false, "has path info"), "");

			id = 0;

			// Trace.Assert(DM.Stat0($"root {rootPath} | filename {filename}"), "");
			rootFolderPath = rootPath;
			subFolders = subfolders;
			fileName = filename;

			Configure();
			ConfigureFilePath();

			// Trace.Assert(DM.Stat0($"at pathandfilebase ({id}) / ctor| setting file path {settingFilePath}"), "");

			// Trace.Assert(DM.End0(), "");
		}

		public FilePath<FileNameSimple> FilePath { get; private set; }


	#region public properties

		public string CompanyName => CsUtilities.CompanyName;

		public string AppName => CsUtilities.AssemblyName;

		/// <summary>
		/// Indicates if the referenced setting FILE exists
		/// </summary>
		public bool Exists
		{
			get
			{
				// Trace.Assert(DM.Start0(false, $"id is {id}",""));

				// Trace.Assert(DM.Stat0($"settingfilepath {settingFilePath ?? "null"}"),"");

				if (SettingFilePath.IsVoid())
				{
					// Trace.Assert(DM.End0("end 1","path is void"),"");
					return false;
				}

				// Trace.Assert(DM.End0("end", $"file exists? {File.Exists(settingFilePath)}"),"");
				return File.Exists(SettingFilePath);
			}
		}

		/// <summary>
		/// The FILENAME with the EXTENSION
		/// </summary>
		public string FileName
		{
			get => fileName;
			// ver 7.4.2
			set
			{
				fileName = value;
				settingFilePath = null;
			}
		}

		/// <summary>
		/// The FILENAME NO extension
		/// </summary>
		public string FileNameNoExt
		{
			get => fileNameRoot;
			set
			{
				fileNameRoot = value;
				settingFilePath = null;
			}
		}

		/// <summary>
		/// The filename SUFFIX
		/// </summary>
		public string FileNameSuffix
		{
			get => fileNameSuffix;
			set
			{
				fileNameSuffix = value;
				settingFilePath = null;
			}
		}

		/// <summary>
		/// The filename's EXTENSION (no sep
		/// </summary>
		public string FileNameExtNoSep
		{
			get => fileNameExtNoSep;
			set
			{
				fileNameExtNoSep = value;
				settingFilePath = null;
			}
		}

		// public string CompanyFolderPath { get; set; }

		/// <summary>
		/// the FOLDER PATH to the root folder for the referenced setting file
		/// </summary>
		public string RootFolderPath
		{
			get => rootFolderPath;
			// ver 7.4.2
			set
			{
				rootFolderPath = value;
				settingFolderPath = null;
				settingFilePath = null;
			}
		}

		/// <summary>
		/// The sub-folders, below the root folder, used to create the<br/>
		/// complete folder path
		/// </summary>
		public string[] SubFolders
		{
			get => subFolders;
			// ver 7.4.2
			set
			{
				subFolders = value;
				settingFolderPath = null;
				settingFilePath = null;
			}
		}

		/// <summary>
		/// when is a suite, the the folder that contains each if the assembly folders
		/// </summary>
		public string SuiteFolderPath
		{
			get
			{
				if (suiteFolderPath.IsVoid())
				{
					if (!Heading.SuiteName.IsVoid())
					{
						suiteFolderPath = CreateSuiteFolderPath();
					} 
					else
					{
						suiteFolderPath = SettingFolderPath;
					}
				}

				return suiteFolderPath;
			}
		}

		/// <summary>
		/// the FOLDER PATH string for the referenced setting file
		/// </summary>
		public string SettingFolderPath
		{
			get
			{
				if (settingFolderPath.IsVoid())
				{
					settingFolderPath = CreateSettingFolderPath();
				}

				return settingFolderPath;
			}
		}

		/// <summary>
		/// The FILE PATH string for the referenced setting file
		/// </summary>
		public string SettingFilePath
		{
			get
			{
				// ConfigureFilePath();

				return settingFilePath;
			}
		}

		/// <summary>
		/// indicates if the ROOT PATH for the referenced setting file is valid
		/// </summary>
		public bool RootFolderPathIsValid => Directory.Exists(RootFolderPath);

		/// <summary>
		/// indicates if the SETTING PATH for the referenced setting file is valid
		/// </summary>
		public bool SettingFolderPathIsValid => Directory.Exists(SettingFolderPath);

		public string ClassVersionFromFile => GetVersionFromFile(Heading.ClassVersionName);

		public string SystemVersionFromFile => GetVersionFromFile(Heading.SettingsVersionName);

		public bool IsReadOnly { get; protected set; }

		// ReSharper disable once MemberCanBeProtected.Global
		public bool IsSettingFile { get; set; }
		
		public bool HasFilePath => !settingFilePath.IsVoid();

		/// <summary>
		/// Indicates if the referenced setting FILE exists
		/// </summary>
		public bool SettingFileExists
		{
			get
			{
				// Trace.Assert(DM.Stat0($"@SettingFileExists void? {settingFilePath.IsVoid()}"),"");

				if (SettingFilePath.IsVoid()) return false;

				// bool a = File.Exists(@"C:\Users\jeffs\Documents\Programming\VisualStudioProjects\PDF SOLUTIONS\_Samples\The Village\text.txt");
				// bool b = File.Exists(@"C:\Users\jeffs\Documents\Programming\VisualStudioProjects\PDF SOLUTIONS\_Samples\The Village\file.pdf");
				// bool c = File.Exists(@"C:\Users\jeffs\Documents\Programming\VisualStudioProjects\PDF SOLUTIONS\_Samples\The Village\ShtSettings.xml");


				return File.Exists(SettingFilePath);
			}
		}

		// public bool FolderExists => Directory.Exists(SettingFolderPath);

	#endregion

	#region public methods

		public virtual void ConfigureFilePath()
		{
			// Trace.Assert(DM.Start0(),"");
			if (settingFilePath.IsVoid())
			{
				// Trace.Assert(DM.Stat0("path is void"),"");
				settingFilePath = CreateSettingFilePath();
			}

			// Trace.Assert(DM.Stat0($"setting file path {settingFilePath}"),"");
			// Trace.Assert(DM.End0(),"");
		}

	#endregion

	#region protected methods

		protected string CreateSettingFolderPath()
		{
			if (subFolders == null) return rootFolderPath;

			return CsUtilities.SubFolder( subFolders.Length - 1,
				rootFolderPath,  subFolders);
		}

		
		protected string CreateSuiteFolderPath()
		{
			if (subFolders == null) return rootFolderPath;

			return CsUtilities.SubFolder( subFolders.Length - 2,
				rootFolderPath,  subFolders);
		}

		protected string CreateSettingFilePath()
		{
			// Trace.Assert(DM.Start0(),"");

			if (!rootFolderPath.IsVoid())
			{
				// Trace.Assert(DM.Stat0($"******  rootFolderPath is not void"),"");

				if (!Directory.Exists(SettingFolderPath))
				{
					// Trace.Assert(DM.Stat0($"******  SettingFolderPath does not exist"),"");

					if (!CsUtilities.CreateSubFolders(rootFolderPath, subFolders))
					{
						throw new DirectoryNotFoundException(
							// ReSharper disable once ConstantNullCoalescingCondition
							"setting file path| " + rootFolderPath ?? "path is null");
					}
				}

				FilePath = new FilePath<FileNameSimple>(settingFolderPath + "\\" + fileName);

				// Trace.Assert(DM.End0("end 1 (good)", $"file path {FilePath}"),"");

				return SettingFolderPath + "\\" + fileName;
			}

			// Trace.Assert(DM.Stat0($"******  rootFolderPath is void"),"");

			// Trace.Assert(DM.End0("end (fail)"),"");

			FilePath = null;
			return null;
		}

		protected abstract void Configure();

		protected string GetVersionFromFile(string elementName) => CsXmlUtilities.ScanXmlForElementValue(SettingFilePath, elementName);

	#endregion
	}

#endregion

#region + Support Classes

	public interface IDataFile
	{
		string DataFileDescription { get; }
		string DataFileNotes { get; }
		string DataFileVersion { get; }
	}

	public interface IDataStore : IDataFile
	{
		string DataFileType { get; }
	}

	[DataContract(Namespace = "")]
	public abstract class SettingInfoBase<TData> : IComparable<SettingInfoBase<TData>>
		where TData : IDataFile, new()
	{
		public SettingInfoBase()
		{
			// ReSharper disable VirtualMemberCallInConstructor
			Header = new Heading();

			DataClassVersion = Data.DataFileVersion;
			Description = Data.DataFileDescription;
			Notes = Data.DataFileNotes;
		}

		private bool? classVersionsMatch;

		[DataMember(Order = 1)]
		public abstract SettingFileType FileType  { get; set; }

		[DataMember(Order = 5)]
		public Heading Header;

		public string Description
		{
			get => Header.Description;
			set => Header.Description = value;
		}

		public string DataClassVersion
		{
			get => Header.DataClassVersion;
			set => Header.DataClassVersion = value;
		}

		public string Notes
		{
			get => Header.Notes;
			set => Header.Notes = value;
		}

		[DataMember(Order = 10)]
		public TData Data { get; set; } = new TData();

		[IgnoreDataMember]
		public bool IsSuite => !Heading.SuiteName.IsVoid();


		public int CompareTo(SettingInfoBase<TData> other) => String.Compare(DataClassVersion, other.DataClassVersion, StringComparison.Ordinal);

		// ReSharper disable once UnusedParameter.Global
		public abstract void UpgradeFromPrior(SettingInfoBase<TData> prior);

		public bool ClassVersionsMatch(string fileClassVersion)
		{
			if (classVersionsMatch == null)
			{
				classVersionsMatch = fileClassVersion?.Equals(DataClassVersion) ?? true;
			}

			return classVersionsMatch.Value;
		}
	}

	[DataContract(Namespace = "")]
	public class BaseSettingsStatic<TPath, TInfo, TData>
		where TPath : PathAndFileBase, new()
		where TInfo : SettingInfoBase<TData>, new()
		where TData : IDataFile, new()
	{
		public static SettingsMgr<TPath, TInfo, TData> Admin { get;  }

		public static TPath Path { get; } = new TPath();
		public static TInfo Info { get; private set; }
		public static TData Data { get; private set; }

		public BaseSettingsStatic() { /*DM.InOut0();*/ }

		static BaseSettingsStatic()
		{
			// DM.Start0();
			Admin = new SettingsMgr<TPath, TInfo, TData>(Path, ResetData);
			ResetData();
			// DM.End0();
		}

		public static void ResetData()
		{
			Info = Admin.Info;
			Data = Info.Data;
		}
	}

#region user settings config / conditional

#if USER_SETTINGS
	// level 1 setting file
	// user specific data is stored in this file
	// this file is stored here:
	// C:\Users\{user}\AppData\Roaming\{company name}\{suite name}\{program name}\
	public class UserSettingPath : PathAndFileBase
	{
		protected override void Configure()
		{
			// Trace.Assert(DM.Start0(false, "user settings: config path"),"");

			string suiteName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}" : null;

			// string rootName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}\\{CsUtilities.AssemblyName}" : CsUtilities.AssemblyName;
			string rootName = CsUtilities.AssemblyName;

			// FileNameNoExt = "\\user";
			FileNameNoExt = "user";

			FileNameSuffix = SETTINGFILEBASE_SUFFIX;
			FileNameExtNoSep = SETTINGFILEBASE_EXT;

			FileName = FileNameNoExt + SETTINGFILEBASE;
			RootFolderPath =  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			SubFolders = new []
			{
				CsUtilities.CompanyName,
				suiteName,
				rootName
			};
			IsReadOnly = false;
			IsSettingFile = true;

			// Trace.Assert(DM.End0(),"");
		}
	}

	// define file type specific information: User
	[DataContract(Name = "UserSettings", Namespace = "")]
	public class UserSettingInfo<TData> : SettingInfoBase<TData>
		where TData : IDataFile, new()
	{
		public UserSettingInfo()
		{
			// Trace.Assert(DM.InOut0("UserSettingInfo: started"),"");
			// DataClassVersion = Data.DataFileVersion;
			// Description = Data.DataFileDescription;
			// Notes = Data.DataFileNotes;
		}

		public override SettingFileType FileType
		{
			get => SettingFileType.SETTING_MGR_USER;
			set
			{
				if (!value.Equals(FileType))
				{
					throw new MessageException("Read FileType does not match| " +
						FileType.ToString(), null);
				}
			}
		}


		public override void UpgradeFromPrior(SettingInfoBase<TData> prior) { }
	}

	public class UserSettings :
		BaseSettingsStatic<UserSettingPath,
		UserSettingInfo<UserSettingDataFile>,
		UserSettingDataFile>
	{
		public UserSettings()
		{
			// Trace.Assert(DM.InOut0("*** UserSettings: started"),"");
		}
	}

#endif

#endregion

#region app settings config / conditional

#if APP_SETTINGS
	// level 2 setting file
	// app specific data is stored in this file
	// this file will provide the locations for the site setting file
	// this file is per user
	// this file is stored here:
	// C:\Users\{user}\AppData\Roaming\{company name}\{suite name}\{program name}\AppSettings\
	public class AppSettingPath : PathAndFileBase
	{
		protected override void Configure()
		{
			string suiteName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}" : null;

			// string rootName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}\\{CsUtilities.AssemblyName}" : CsUtilities.AssemblyName;
			string rootName = CsUtilities.AssemblyName;

			// FileNameNoExt = "\\app";
			FileNameNoExt = "app";

			FileName = FileNameNoExt + SETTINGFILEBASE;

			FileNameSuffix = SETTINGFILEBASE_SUFFIX;
			FileNameExtNoSep = SETTINGFILEBASE_EXT;

			RootFolderPath =  Environment. GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			SubFolders = new []
			{
				CsUtilities.CompanyName,
				suiteName,
				rootName
			};
			IsReadOnly = false;
			IsSettingFile = true;
		}
	}

	[DataContract(Namespace = "")]
	// define file type specific information: app
	public class AppSettingInfo<TData> : SettingInfoBase<TData>
		where TData : IDataFile, new()
	{
		public AppSettingInfo()
		{
			// DataClassVersion = Data.DataFileVersion;
			// Description = Data.DataFileDescription;
			// Notes = Data.DataFileNotes;
		}

		public override SettingFileType FileType
		{
			get => SettingFileType.SETTING_MGR_APP;
			set
			{
				if (!value.Equals(FileType))
				{
					throw new MessageException("Read FileType does not match| " +
						FileType.ToString(), null);
				}
			}
		}

		public override void UpgradeFromPrior(SettingInfoBase<TData> prior) { }
	}

	internal class AppSettings :
		BaseSettingsStatic<AppSettingPath,
		AppSettingInfo<AppSettingDataFile>,
		AppSettingDataFile>
	{ }


#endif

#endregion

#region suite settings config / conditional

#if SUITE_SETTINGS
	// level 3 setting file
	// suite specific data is stored in this file
	// i.e. information to be used by all programs in a suite
	// this file is per user
	// this file is stored here:
	// C:\Users\{user}\AppData\Roaming\{company name}\{suite name}\
	public class SuiteSettingPath : PathAndFileBase
	{
		protected override void Configure()
		{
			string suiteName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}" : null;
			string rootName = suiteName == null ? CsUtilities.AssemblyName : "";

			// string rootName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}\\{CsUtilities.AssemblyName}" : CsUtilities.AssemblyName;
			// string rootName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}" : CsUtilities.AssemblyName;

			// FileNameNoExt = "\\suite";
			FileNameNoExt = "suite";
			
			FileName = FileNameNoExt + SETTINGFILEBASE;

			FileNameSuffix = SETTINGFILEBASE_SUFFIX;
			FileNameExtNoSep = SETTINGFILEBASE_EXT;

			// RootFolderPath =  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			RootFolderPath =  Environment. GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			SubFolders = new []
			{
				CsUtilities.CompanyName,
				suiteName,
				rootName
			};
			IsReadOnly = false;
			IsSettingFile = true;
		}
	}

	// define file type specific information: app
	[DataContract(Namespace = "")]
	public class SuiteSettingInfo<TData> : SettingInfoBase<TData>
		where TData : IDataFile, new()
	{
		public SuiteSettingInfo() { }

		public override SettingFileType FileType
		{
			get => SettingFileType.SETTING_MGR_SUITE;
			set
			{
				if (!value.Equals(FileType))
				{
					throw new MessageException("Read FileType does not match| " +
						FileType.ToString(), null);
				}
			}
		}

		public override void UpgradeFromPrior(SettingInfoBase<TData> prior) { }
	}

	internal class SuiteSettings :
		BaseSettingsStatic<SuiteSettingPath,
		SuiteSettingInfo<SuiteSettingDataFile>,
		SuiteSettingDataFile>
	{
		public static string SiteRootPath => SuiteSettings.Data.SiteRootPath;
	}

#endif

#endregion

#region machine settings config / conditional

#if MACH_SETTINGS
	// level 4a setting file
	// machine specific data is stored in this file
	// i.e. information to be used by all programs by a
	// software developer on the machine (e.g. license info / contact info / web site info)
	// this file is per machine (all users)
	// this file is stored here:
	// C:\ProgramData\{company name}\{program name}\
	public class MachSettingPath : PathAndFileBase
	{
		public MachSettingPath() { }

		protected override void Configure()
		{
			string suiteName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}" : null;
			string rootName = suiteName == null ? CsUtilities.AssemblyName : "";

			// string rootName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}\\{CsUtilities.AssemblyName}" : CsUtilities.AssemblyName;
			// string rootName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}" : CsUtilities.AssemblyName;

			// FileNameNoExt = "\\machine";
			FileNameNoExt = "machine";

			FileName = FileNameNoExt + SETTINGFILEBASE;

			FileNameSuffix = SETTINGFILEBASE_SUFFIX;
			FileNameExtNoSep = SETTINGFILEBASE_EXT;

			RootFolderPath =  Environment. GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			SubFolders = new []
			{
				CsUtilities.CompanyName,
				suiteName,
				rootName
			};
			IsReadOnly = false;
			IsSettingFile = true;
		}
	}

	[DataContract(Namespace = "")]
	// define file type specific information: machine
	public class MachSettingInfo<TData> : SettingInfoBase<TData>
		where TData : IDataFile, new()
	{
		public MachSettingInfo() { }

		public override SettingFileType FileType
		{
			get => SettingFileType.SETTING_MGR_MACH;
			set
			{
				if (!value.Equals(FileType))
				{
					throw new MessageException("Read FileType does not match| " +
						FileType.ToString(), null);
				}
			}
		}

		public override void UpgradeFromPrior(SettingInfoBase<TData> prior) { }
	}

	internal class MachSettings :
		BaseSettingsStatic<MachSettingPath,
		MachSettingInfo<MachSettingDataFile>,
		MachSettingDataFile>
	{
		public MachSettings() { }
	}

#endif

#endregion

#region site settings config / conditional

#if SITE_SETTINGS
	// level 4b setting file
	// site & suite specific data is stored in this file
	// i.e. information used by multiple programs / suites on
	// multiple machines or multiple sites and machines
	// this file is per "site" (all machines & all users)
	// this file is stored here:
	// {as defined in a suite setting file} - could be on a remote machine
	public class SiteSettingPath : PathAndFileBase
	{
#pragma warning disable CS0169 // The field 'SiteSettingPath.localSubFolders' is never used
		private string[] localSubFolders;
#pragma warning restore CS0169 // The field 'SiteSettingPath.localSubFolders' is never used

		public SiteSettingPath() { }

		protected override void Configure()
		{
			string suiteName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}" : null;
			string rootName = suiteName == null ? CsUtilities.AssemblyName : "";

			// string rootName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}\\{CsUtilities.AssemblyName}" : CsUtilities.AssemblyName;
			// string rootName = !Heading.SuiteName.IsVoid() ? $"{Heading.SuiteName}" : CsUtilities.AssemblyName;

			// FileNameNoExt = "\\site";
			FileNameNoExt = "site";

			FileName = FileNameNoExt + SETTINGFILEBASE;

			FileNameSuffix = SETTINGFILEBASE_SUFFIX;
			FileNameExtNoSep = SETTINGFILEBASE_EXT;

			RootFolderPath =  Environment. GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			SubFolders = new []
			{
				CsUtilities.CompanyName,
				suiteName,
				rootName
			};
			IsReadOnly = false;
			IsSettingFile = true;
		}

		public new string RootFolderPath
		{
			get => base.RootFolderPath;
			set
			{
				base.RootFolderPath = value;
			}
		}

		public new string[] SubFolders
		{
			get => base.SubFolders;
			set
			{
				base.SubFolders = value;

			}
		}

		public void SetRootFolder(string rootFolderPath)
		{
			RootFolderPath = rootFolderPath;
		}
	}

	[DataContract(Namespace = "")]
	// define file type specific information: site
	public class SiteSettingInfo<TData> : SettingInfoBase<TData>
		where TData : IDataFile, new()
	{
		public SiteSettingInfo() { }

		public override SettingFileType FileType
		{
			get => SettingFileType.SETTING_MGR_SITE;
			set
			{
				if (!value.Equals(FileType))
				{
					throw new MessageException("Read FileType does not match| " +
						FileType.ToString(), null);
				}
			}
		}

		public override void UpgradeFromPrior(SettingInfoBase<TData> prior) { }
	}

	internal class SiteSettings :
		BaseSettingsStatic<SiteSettingPath,
		SiteSettingInfo<SiteSettingDataFile>,
		SiteSettingDataFile>
	{
		public SiteSettings() { }
	}

#endif

#endregion

#endregion

/*
 to use data storage
 * create datastorageset class using example and derived from IDataStore
 * create filepath<FileNameSimple> that identified the path and name for the storage set
 * create a datamanager based the datastorageset and filepath: "DataManager<StorageSet1> dm1 = new DataManager<StorageSet1>(FilePath1)
 * write the datastorageset to initialize the file: dm1.Write();
 * use like any of the other setting managers
 * to read, create the datamanager to configure.
 * use like any of the other setting managers
*/

#region info class for data

	[DataContract(Name = "DataStorage", Namespace = "")]
	public class StorageMgrInfo<TData> : SettingInfoBase<TData>
		where TData : IDataStore, new ()
	{
		public StorageMgrInfo()
		{
			// DataClassVersion = Data.DataFileVersion;
			// Description = Data.DataFileDescription;
			// Notes = Data.DataFileNotes;
			DataFileType = Data.DataFileType;
		}
		
		[DataMember(Order = 1)]
		public override SettingFileType FileType
		{
			get => SettingFileType.SETTING_MGR_DATA;
			set
			{
				if (!value.Equals(FileType))
				{
					throw new MessageException("Read FileType does not match| " +
						FileType.ToString(), null);
				}
			}
		}

		[DataMember(Order = 0)]
		public string DataFileType { get; set; }

		public override void UpgradeFromPrior(SettingInfoBase<TData> prior) { }
	}

#endregion

#region management classes for data

	public class StorageMgrPath : PathAndFileBase
	{
		public StorageMgrPath()
		{
			// Trace.Assert(DM.InOut0(),"");
		}

		public StorageMgrPath(string rootPath, string filename) : base(rootPath, null, filename)
		{
			// Trace.Assert(DM.InOut0(), "");
		}

		// ver 7.4.2
		// public new string RootFolderPath
		// {
		// 	get => base.RootFolderPath;
		// 	set => base.RootFolderPath = value;
		// }
		//
		// public new string[] SubFolders
		// {
		// 	get => base.SubFolders;
		// 	set => base.SubFolders = value;
		// }
		//
		// public new string FileName
		// {
		// 	get => base.FileName;
		// 	set => base.FileName = value;
		// }

		protected override void Configure() { }

		public override void ConfigureFilePath()
		{
			// Trace.Assert(DM.Start0(),"");

			base.ConfigureFilePath();

			// FileName = FilePath?.FileName ?? null;
			// FileNameNoExt = FilePath?.FileNameNoExt ?? null;
			// FileNameSuffix = "";
			// FileNameExtNoSep = SETTINGFILEBASE_EXT;
			// RootFolderPath = FilePath?.FolderPath ?? null;

			fileName = FilePath?.FileName ?? null;
			fileNameRoot = FilePath?.FileNameNoExt ?? null;
			fileNameSuffix = "";
			fileNameExtNoSep = SETTINGFILEBASE_EXT;
			rootFolderPath = FilePath?.FolderPath ?? null;

			// Trace.Assert(DM.End0(),"");
		}
	}

	/*

	to use the data class

	// create the data class
	// DataSet1is the class with the date that implements "HeaderData"
	DataManager<DataSet1> dm1 = new DataManager<DataSet1>();

	// either:

	// create a data file (if first time)
	dm1.Create(FilePath<FileNameSimple> filePath)

	// configure DataManager (on later usage)
	dm1.Configure(
		new FilePath<FileNameSimple>(
			@"C:\Users\jeffs\AppData\Roaming\CyberStudio\SettingsManager\SettingsManagerv75\DataSet1_1.xml"));
	// then read the date
	dm1.Admin.Read();

	// after the above

	// access the date file
	dm1.Data.property = data;

	// write the data to the disk:
	dm1.Admin.Write();

	*/

	// v7.5 fix member paths / connetions / eliminate duplicates
	// todo implement for settings manager
	[DataContract]
	public class DataManager<TData> : INotifyPropertyChanged
		where TData : class, IDataStore, new()
	{
		public bool IsInitialized => Path.HasFilePath;

		public SettingsMgr<StorageMgrPath, StorageMgrInfo<TData>, TData> Admin { get;  }

		public StorageMgrPath Path
		{
			get => Admin.Path;
			internal set => Admin.Path = value;
		}

		// public StorageMgrInfo<TData> Info { get; set; }
		public StorageMgrInfo<TData> Info => Admin.Info;

		// public TData Data { get; private set; }
		public TData Data => Info.Data;

	#region ctor

		// public DataManager()
		// {
		// 	Path= new StorageMgrPath("", "");
		// 	Admin = new SettingsMgr<StorageMgrPath, StorageMgrInfo<TData>, TData>(Path, ResetData);
		// 	ResetData();
		// }

		public DataManager(FilePath<FileNameSimple> filePath, string folderPath = null, string fileName=null)
		{
			// Trace.Assert(DM.Start0(false, "**** ctor datamanager"),"");
			// todo
			// if (filePath != null)
			// {
			// 	Path = new StorageMgrPath(filePath.FolderPath, filePath.FileName);
			//
			// 	Path.SubFolders = null;
			// }


			string folder = filePath == null ? folderPath : filePath.FolderPath;
			string file = filePath == null ? fileName : filePath.FileName;


			// todo
			Admin = new SettingsMgr<StorageMgrPath, StorageMgrInfo<TData>, TData>(new StorageMgrPath(folder, file), null);
			// Path = new StorageMgrPath(folder, file);

			// todo fix the reset system - set to null here but needs to fixed overall

			OnPropertyChange("Initialized");

			// ConfigData();

			// Reset();

			// Trace.Assert(DM.Stat0($"id via admin {Admin.Path.id}"),"");
			// Trace.Assert(DM.Stat0($"id via path  {Path.id}"),"");

			// Trace.Assert(DM.End0(),"");
		}

	#endregion


	#region public properties

		public void Reset()
		{
			// hold - top is original 
			// Admin.Info.Data = new TData();
			// this may be the right option
			// Admin.Info.Header = new Heading();
			// but using this for now
			Admin.Reset();
		}

		public void ResetPath()
		{
			// Trace.Assert(DM.Start0(),"");
			Configure(null);
			// Trace.Assert(DM.End0(),"");
		}

		public void Close()
		{
			// Trace.Assert(DM.Start0(),"");
			Reset();
			ResetPath();
			// Trace.Assert(DM.End0(),"");
		}

		// public void ConfigData()
		// {
		// 	Info = Admin.Info;
		// 	Data = Info.Data;
		// }

		public void Open(FilePath<FileNameSimple> filePath)
		{
			// Trace.Assert(DM.Start0(),"");

			Configure(filePath);

			if (filePath.Exists)
			{
				Admin.Read();
			}
			else
			{
				Admin.Create();
				Admin.Read();
			}

			// Trace.Assert(DM.End0(),"");
		}

		private void Configure(FilePath<FileNameSimple> filePath)
		{
			// Trace.Assert(DM.Start0(),"");

			Path.SubFolders = null;
			Path.RootFolderPath = filePath?.FolderPath ?? null;
			Path.FileName = filePath?.FileName ?? null;

			Path.ConfigureFilePath();

			// ReSharper disable once ExplicitCallerInfoArgument
			OnPropertyChange("Initialized");

			// Trace.Assert(DM.End0(),"");
		}

		public void Configure( FilePath<FileNameSimple> filePath, string fileName, string[] folders)
		{
			Configure(filePath.FolderPath, fileName, folders);
		}

		public void Configure( string rootPath, string fileName, string[] folders, string filenameext = "")
		{
			// Trace.Assert(DM.Start0(),"");
			// todo
			// if (Path == null) return;

			// Path.SubFolders = null;
			Path.SubFolders = folders;
			Path.RootFolderPath = rootPath;

			// not sure this is correct - may be: Path.FileName = filePath?.FileName ?? null;
			Path.FileName = fileName;

			// Path.FileNameNoExt = fileName;
			Path.FileNameSuffix = null;
			Path.FileNameExtNoSep = PathAndFileBase.SETTINGFILEBASE_EXT;

			Path.ConfigureFilePath();

			// ReSharper disable once ExplicitCallerInfoArgument
			OnPropertyChange("Initialized");

			// Trace.Assert(DM.End0(),"");
		}

		public void Create( FilePath<FileNameSimple> folderPath, string fileName, string[] folders)
		{
			// Trace.Assert(DM.Start0(),"");
			Configure(folderPath, fileName, folders);

			if (IsInitialized)
			{
				Admin.Write();
			}

			// Trace.Assert(DM.End0(),"");
		}

		public void Open( FilePath<FileNameSimple> folderPath, string fileName, string[] folders)
		{
			// Trace.Assert(DM.Start0(false, $@"Open - path info {fileName} - {folderPath.FullFilePath}"),"");
			if (folderPath.Exists)
			{
				// Trace.Assert(DM.Stat0("@Open - path does exists"),"");

				// todo
				// if (Path == null)
				// {
				// 	Path = new StorageMgrPath(folderPath.FolderPath, fileName);
				//
				// 	Path.SubFolders = null;
				// }

				// todo

				// Trace.Assert(DM.Start0(false, "starting StorageMgrPath"),"");

				Path = new StorageMgrPath(folderPath.FolderPath, fileName);

				// Trace.Assert(DM.Stat0($"@settingsmgr / datamanager| path id is {Path.id}"),"");

				// Trace.Assert(DM.Stat0($"setting file path {Path.SettingFilePath}"),"");

				// Trace.Assert(DM.End0("complete StorageMgrPath"),"");

				// Path.SubFolders = null;

				Admin.Read();
			}
			else
			{
				// Trace.Assert(DM.Stat0("@open path does NOT exists"),"");

				Create(folderPath, fileName, folders);
			}
			// Trace.Assert(DM.End0(),"");
		}

	#endregion

	#region event publish

		public event PropertyChangedEventHandler PropertyChanged;

		[DebuggerStepThrough]
		private void OnPropertyChange([CallerMemberName] string memberName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
		}

	#endregion
	}

#endregion
}