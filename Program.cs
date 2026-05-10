using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace BeTong
{
    static class Program
    {
        // Win32 helpers to bring window to front
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;

        [STAThread]
        static void Main()
        {
            bool created;
            const string mutexName = "Global\\BeTong_SingleInstance_Mutex";

            using (var mutex = new Mutex(true, mutexName, out created))
            {
                if (created)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                {
                    // Another instance is running — bring its main window to front and exit.
                    TryBringOtherInstanceToFront();
                }
            }
        }

        private static void TryBringOtherInstanceToFront()
        {
            try
            {
                var current = Process.GetCurrentProcess();
                var processes = Process.GetProcessesByName(current.ProcessName);
                foreach (var p in processes)
                {
                    if (p.Id == current.Id) continue;
                    try
                    {
                        var h = p.MainWindowHandle;
                        if (h == IntPtr.Zero) continue;

                        // If minimized, restore first
                        if (IsIconic(h))
                        {
                            ShowWindow(h, SW_RESTORE);
                        }

                        SetForegroundWindow(h);
                        return;
                    }
                    catch
                    {
                        // ignore failures for a single process and try next
                    }
                }
            }
            catch
            {
                // swallow all exceptions to avoid crash on startup
            }
        }
    }
}using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace BeTong
{
    static class Program
    {
        // Win32 helpers to bring window to front
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;

        [STAThread]
        static void Main()
        {
            bool created;
            const string mutexName = "Global\\BeTong_SingleInstance_Mutex";

            using (var mutex = new Mutex(true, mutexName, out created))
            {
                if (created)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                {
                    // Another instance is running — bring its main window to front and exit.
                    TryBringOtherInstanceToFront();
                }
            }
        }

        private static void TryBringOtherInstanceToFront()
        {
            try
            {
                var current = Process.GetCurrentProcess();
                var processes = Process.GetProcessesByName(current.ProcessName);
                foreach (var p in processes)
                {
                    if (p.Id == current.Id) continue;
                    try
                    {
                        var h = p.MainWindowHandle;
                        if (h == IntPtr.Zero) continue;

                        // If minimized, restore first
                        if (IsIconic(h))
                        {
                            ShowWindow(h, SW_RESTORE);
                        }

                        SetForegroundWindow(h);
                        return;
                    }
                    catch
                    {
                        // ignore failures for a single process and try next
                    }
                }
            }
            catch
            {
                // swallow all exceptions to avoid crash on startup
            }
        }
    }
}