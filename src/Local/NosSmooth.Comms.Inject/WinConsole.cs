//
//  WinConsole.cs
//
//  Copyright (c) František Boháček. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace NosSmooth.Comms.Inject;

/// <summary>
/// A class for managing Windows console.
/// </summary>
internal static class WinConsole
{
    /// <summary>
    /// Initialize a console.
    /// </summary>
    /// <param name="alwaysCreateNewConsole">Whether to create (true) or attach (false) to a console.</param>
    public static void Initialize(bool alwaysCreateNewConsole = true)
    {
        bool consoleAttached = true;
        if (alwaysCreateNewConsole
            || (AttachConsole(ATTACH_PARRENT) == 0
                && Marshal.GetLastWin32Error() != ERROR_ACCESS_DENIED))
        {
            consoleAttached = AllocConsole() != 0;
        }

        if (consoleAttached)
        {
            InitializeOutStream();
            InitializeInStream();
        }
    }

    /// <summary>
    /// Close a console.
    /// </summary>
    public static void Close()
    {
        FreeConsole();
    }

    private static void InitializeOutStream()
    {
        var fs = CreateFileStream("CONOUT$", GENERIC_WRITE, FILE_SHARE_WRITE, FileAccess.Write);
        if (fs != null)
        {
            var writer = new StreamWriter(fs) { AutoFlush = true };
            Console.SetOut(writer);
            Console.SetError(writer);
        }
    }

    private static void InitializeInStream()
    {
        var fs = CreateFileStream("CONIN$", GENERIC_READ, FILE_SHARE_READ, FileAccess.Read);
        if (fs != null)
        {
            Console.SetIn(new StreamReader(fs));
        }
    }

    private static FileStream? CreateFileStream
    (
        string name,
        uint win32DesiredAccess,
        uint win32ShareMode,
        FileAccess dotNetFileAccess
    )
    {
        var file = new SafeFileHandle
        (
            CreateFileW
            (
                name,
                win32DesiredAccess,
                win32ShareMode,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero
            ),
            true
        );
        if (!file.IsInvalid)
        {
            var fs = new FileStream(file, dotNetFileAccess);
            return fs;
        }
        return null;
    }

    [DllImport
    (
        "kernel32.dll",
        EntryPoint = "AllocConsole",
        SetLastError = true,
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall
    )]
    private static extern int AllocConsole();

    [DllImport
    (
        "kernel32.dll",
        EntryPoint = "AttachConsole",
        SetLastError = true,
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall
    )]
    private static extern uint AttachConsole(uint dwProcessId);

    [DllImport
    (
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        SetLastError = true,
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall
    )]
    private static extern IntPtr CreateFileW
    (
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile
    );

    [DllImport("kernel32")]
    private static extern bool FreeConsole();

    private const uint GENERIC_WRITE = 0x40000000;
    private const uint GENERIC_READ = 0x80000000;
    private const uint FILE_SHARE_READ = 0x00000001;
    private const uint FILE_SHARE_WRITE = 0x00000002;
    private const uint OPEN_EXISTING = 0x00000003;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    private const uint ERROR_ACCESS_DENIED = 5;

    private const uint ATTACH_PARRENT = 0xFFFFFFFF;
}