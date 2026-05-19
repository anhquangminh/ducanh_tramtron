using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using BeTong.Data;
using BeTong.Forms;
using BeTong.Repositories;

namespace BeTong
{
    internal static class Program
    {
        private const string SingleInstanceMutexName = "Global\\BeTong_SingleInstance_Mutex";
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [STAThread]
        private static void Main()
        {
            bool created;
            using (var mutex = new Mutex(true, SingleInstanceMutexName, out created))
            {
                if (!created)
                {
                    TryBringOtherInstanceToFront();
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var connectionFactory = new SqlConnectionFactory();

                if (!Models.CurrentUserContext.IsLoggedIn)
                {
                    using (var loginForm = new LoginForm(new UserRepository(connectionFactory)))
                    {
                        var dlg = loginForm.ShowDialog();
                        if (dlg != DialogResult.OK)
                        {
                            return;
                        }

                        Models.CurrentUserContext.Save(loginForm.LoggedInUser);
                    }
                }

                Application.Run(new CapPhoiHieuChinhForm());
            }
        }

        private static void TryBringOtherInstanceToFront()
        {
            try
            {
                var current = Process.GetCurrentProcess();
                var processes = Process.GetProcessesByName(current.ProcessName);
                foreach (var process in processes)
                {
                    if (process.Id == current.Id)
                    {
                        continue;
                    }

                    IntPtr handle = process.MainWindowHandle;
                    if (handle == IntPtr.Zero)
                    {
                        continue;
                    }

                    if (IsIconic(handle))
                    {
                        ShowWindow(handle, SW_RESTORE);
                    }

                    SetForegroundWindow(handle);
                    return;
                }
            }
            catch
            {
                // Startup should never fail because focusing the existing instance failed.
            }
        }
    }
}
