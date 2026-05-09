using System;
using System.Windows.Forms;
using BeTong.Data;
using BeTong.Forms;
using BeTong.Repositories;

namespace BeTong
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var connectionFactory = new SqlConnectionFactory();

            // If no user is logged in, show the LoginForm modally BEFORE creating the main form.
            if (!Models.CurrentUserContext.IsLoggedIn)
            {
                using (var loginForm = new LoginForm(new UserRepository(connectionFactory)))
                {
                    var dlg = loginForm.ShowDialog();
                    if (dlg != DialogResult.OK)
                    {
                        // User cancelled or closed the login dialog — exit the app.
                        return;
                    }

                    // Save the logged in user into the static context (LoginForm already validated).
                    Models.CurrentUserContext.Save(loginForm.LoggedInUser);
                }
            }

            // Only after a successful login (or if already logged in) create and run the main form.
            Application.Run(new CapPhoiHieuChinhForm());
        }
    }
}
