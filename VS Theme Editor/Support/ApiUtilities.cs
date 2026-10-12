// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             ApiUtilities.cs
// Created:      2026-09-05 (07:09)

using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace VS_Theme_Editor;

public class ApiUtilities
{
	public ApiUtilities()
	{
		MouseMoveHook = new GlobalMouseMove();
	}

	// public GlobalMouseClick MouseClickHook;
	public GlobalMouseMove MouseMoveHook;

	public const int WH_MOUSE_LL = 14;
	public const int WM_MOUSEMOVE = 0x0200;
	public const int WM_LBUTTONDOWN = 0x0201;
	public const int WM_LBUTTONUP = 0x0202;
	public const int WM_RBUTTONDOWN = 0x0204;
	public const int WM_RBUTTONUP = 0x0205;


	public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

	// get the color of the pixel at the specified screen coordinates
	public static Color GetColorAt(int x, int y)
	{
		IntPtr hdc = GetDC(IntPtr.Zero);
		uint pixel = GetPixel(hdc, x, y);
		ReleaseDC(IntPtr.Zero, hdc);

		int red = (int)(pixel & 0x000000FF);
		int green = (int)((pixel & 0x0000FF00) >> 8);
		int blue = (int)((pixel & 0x00FF0000) >> 16);

		return Color.FromArgb(red, green, blue);
	}


	// // Win32 API to broadcast system-wide changes instantly
	// [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
	// private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string? pvParam, uint fWinIni);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SystemParametersInfo(uint uiAction, int uiParam, IntPtr pvParam, uint fWinIni);

	[DllImport("user32.dll")]
	public static extern bool GetCursorPos(out POINT lpPoint);

	[DllImport("user32.dll")]
	static extern IntPtr GetDC(IntPtr hWnd);

	[DllImport("gdi32.dll")]
	// ReSharper disable once InconsistentNaming
	static extern uint GetPixel(IntPtr hDC, int x, int y);

	[DllImport("user32.dll")]
	// ReSharper disable once InconsistentNaming
	static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr GetModuleHandle(string lpModuleName);

	// Import GetSysColor from user32.dll
	[DllImport("user32.dll")]
	public static extern uint GetSysColor(int nIndex);

	// [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
	// private static extern IntPtr LoadImageA(
	// 	IntPtr hInst,
	// 	string name,
	// 	uint type,
	// 	int cx,
	// 	int cy,
	// 	uint fuLoad
	// 	);



	[StructLayout(LayoutKind.Sequential)]
	// ReSharper disable once InconsistentNaming
	public struct POINT
	{
		public int X;
		public int Y;
	}

	[StructLayout(LayoutKind.Sequential)]
	// ReSharper disable once InconsistentNaming
	public struct MSLLHOOKSTRUCT
	{
		public POINT pt;
		public uint mouseData;
		public uint flags;
		public uint time;
		public IntPtr dwExtraInfo;
	}


	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetSystemCursor(IntPtr hcur, uint id);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr LoadCursorFromFile(string lpFileName);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr CopyIcon(IntPtr hIcon);


	// OCR Constants for System Cursors
	private const uint OCR_NORMAL = 32512; // Standard arrow pointer
	// private const uint OCR_IBEAM = 32513;  // Text I-beam
	// private const uint OCR_HAND = 32521;   // Link select hand

	// SPI Constants to restore default cursors
	private const uint SPI_SETCURSORS = 0x0057;

	// private const uint SPIF_UPDATEINIFILE = 0x01;
	// private const uint SPIF_SENDCHANGE = 0x02;

	public const uint SPIF_SENDWININICHANGE = 2;


	// private const uint IMAGE_BITMAP = 0;
	// private const uint IMAGE_CURSOR = 2;
	// private const uint LR_LOADFROMFILE = 0x00000010;
	// private const uint LR_LOADFROMMODULE = 0x00000000;

	public const uint LR_DEFAULTCOLOR = 0x0000;
	public const uint LR_MONOCHROME = 0x0001;
	public const uint LR_DEFAULTSIZE = 0x0040;

	public static void SetGlobalCursorTemp(string resourceUriString)
	{
		// SaveSystemCursor();

		// 1. Get resource stream from WPF pack URI
		var uri = new Uri(resourceUriString, UriKind.RelativeOrAbsolute);
		var resourceStream = Application.GetResourceStream(uri);
		if (resourceStream == null) return;

		// 2. Extract resource stream to a temporary .cur file on disk
		string tempCursorPath = Path.Combine(Path.GetTempPath(), "temp_global_cursor.cur");

		using (var fileStream = new FileStream(tempCursorPath, FileMode.Create, FileAccess.Write))
		{
			resourceStream.Stream.CopyTo(fileStream);
		}

		// temp cursor saved to a file

		IntPtr hCursor = LoadCursorFromFile(tempCursorPath);

		IntPtr hCopy = CopyIcon(hCursor);

		SetSystemCursor(hCopy, OCR_NORMAL);
	}

	public static void ResetSystemCursor()
	{
		SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, SPIF_SENDWININICHANGE);
	}
}

/* voided - did not work

	[DllImport("user32.dll")]
	private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

	private const int IDC_ARROW = 32512;
	private static IntPtr originalCursor = IntPtr.Zero;

	[DllImport("user32.dll", SetLastError = true)]
   private static extern bool DestroyIcon(IntPtr hIcon);

	// Import GetModuleHandleA to get the current module handle
   [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
   private static extern IntPtr GetModuleHandleA(string lpModuleName);
   
   // --- Win32 API Imports ---
   [DllImport("user32.dll", SetLastError = true)]
   private static extern IntPtr CreateIconFromResourceEx(
   	byte[] pbIconBits,
   	uint cbIconBits,
   	bool fIcon,
   	uint dwVersion,
   	int cxDesired,
   	int cyDesired,
   	uint uFlags);




	public static void SetGlobalCursorFromResourceBx(string resourceUriString)
   {
   	try
   	{
   		// SaveSystemCursor();
   		
   		// 1. Get resource stream from WPF pack URI
   		var uri = new Uri(resourceUriString, UriKind.RelativeOrAbsolute);
   		var resourceStream = Application.GetResourceStream(uri);
   		if (resourceStream == null) return;

   		// 2. Extract resource stream to a temporary .cur file on disk
   		string tempCursorPath = Path.Combine(Path.GetTempPath(), "temp_global_cursor.cur");

   		using (var fileStream = new FileStream(tempCursorPath, FileMode.Create, FileAccess.Write))
   		{
   			resourceStream.Stream.CopyTo(fileStream);
   		}

   		// 3. Update the registry mapping for the normal/standard arrow pointer
   		Microsoft.Win32.Registry.SetValue(
   			@"HKEY_CURRENT_USER\Control Panel\Cursors", "Arrow", tempCursorPath);

   		// 4. Broadcast system parameter change so Windows updates globally without a restart
   		SystemParametersInfo(SPI_SETCURSORS, 0, null, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
   	}
   	catch (Exception ex)
   	{
   		MessageBox.Show($"Failed to change global cursor: {ex.Message}");
   	}
   }


	// Save current system normal cursor
   // only do this if not already done
   public static void SaveSystemCursor()
   {
   	Debug.WriteLine("** save system cursor? ");
   	if (originalCursor != IntPtr.Zero) return;
   	
   	Debug.WriteLine("** yes - saveing system cursor? ");
   	IntPtr defaultCursor = LoadCursor(IntPtr.Zero, IDC_ARROW);
   	originalCursor = CopyIcon(defaultCursor);
   }
   
   // Restore original system cursor
   public static void RestoreSystemCursor()
   {
   	if (originalCursor != IntPtr.Zero)
   	{
   		Debug.WriteLine("** restore system cursor from saved");
   		bool result = SetSystemCursor(originalCursor, OCR_NORMAL);
   		// DestroyIcon(originalCursor);
   		originalCursor = IntPtr.Zero;
   	}
   }

*/



public class MouseHookEventArgs : EventArgs
{
	public int Message { get; }
	public int X { get; }
	public int Y { get; }

	public MouseHookEventArgs(int message, int x, int y)
	{
		Message = message;
		X = x;
		Y = y;
	}
}