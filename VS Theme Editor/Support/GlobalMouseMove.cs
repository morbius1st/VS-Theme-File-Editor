// Solution:     VS Theme Editor
// Project:       VS Theme Editor
// File:             GlobalMouseWrapper.cs
// Created:      2026-09-05 (07:09)

using System.Diagnostics;
using System.Runtime.InteropServices;
using static VS_Theme_Editor.ApiUtilities;

namespace VS_Theme_Editor;

public class GlobalMouseMove : IDisposable
{

	public event EventHandler<MouseHookEventArgs>? MouseAction;

	private readonly HookProc _proc;
	private IntPtr hookId = IntPtr.Zero;

	public GlobalMouseMove()
	{
		_proc = hookCallback;
	}

	public void Start()
	{
		hookId = setHook(_proc);
	}

	public void Stop()
	{
		if (hookId != IntPtr.Zero)
		{
			UnhookWindowsHookEx(hookId);
			hookId = IntPtr.Zero;
		}
	}

	private IntPtr setHook(HookProc proc)
	{
		using (Process curProcess = Process.GetCurrentProcess())
		using (ProcessModule curModule = curProcess.MainModule!)
		{
			return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
		}
	}

	private IntPtr hookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0)
		{
			MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
			int message = wParam.ToInt32();

			// Raise the event on the stream
			MouseAction?.Invoke(this, new MouseHookEventArgs(message, hookStruct.pt.X, hookStruct.pt.Y));
		}

		return CallNextHookEx(hookId, nCode, wParam, lParam);
	}

	public void Dispose()
	{
		Stop();
	}
	
}

