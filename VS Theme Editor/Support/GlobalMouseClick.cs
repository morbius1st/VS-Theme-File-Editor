// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             GlobalMouseClick.cs
// Created:      2026-09-05 (08:09)

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using static VS_Theme_Editor.ApiUtilities;

namespace VS_Theme_Editor;

public class GlobalMouseClick : IDisposable
{
	private IntPtr hookId ; //= IntPtr.Zero;

	// Custom events to subscribe to in your WPF app
	public event EventHandler<Point> GlobalLeftClick = null!;
	public event EventHandler<Point> GlobalRightClick = null!;

	public GlobalMouseClick()
	{
		HookProc proc = hookCallback;
		hookId = setHook(proc);
	}

	private IntPtr setHook(HookProc proc)
	{
		using Process curProcess = Process.GetCurrentProcess();
		using (ProcessModule? curModule = curProcess.MainModule)
		{
			return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule?.ModuleName!), 0);
		}
	}

	private IntPtr hookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0)
		{
			// Parse coordinates from structure
			MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
			Point clickPoint = new Point(hookStruct.pt.X, hookStruct.pt.Y);

			if ((int)wParam == WM_LBUTTONDOWN)
			{
				GlobalLeftClick(this, clickPoint);
			}
			else if ((int)wParam == WM_RBUTTONDOWN)
			{
				GlobalRightClick(this, clickPoint);
			}
		}

		return CallNextHookEx(hookId, nCode, wParam, lParam);
	}

	public void Dispose()
	{
		if (hookId != IntPtr.Zero)
		{
			UnhookWindowsHookEx(hookId);
			hookId = IntPtr.Zero;
		}
	}
}