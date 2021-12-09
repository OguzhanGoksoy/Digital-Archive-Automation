using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace ArchiveManager {
    static class Program {
        /// <summary>
        /// Uygulamanın ana girdi noktası.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            #region Uygulama Dilini Türkçe Olarak Ayarla

            Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("tr-TR");

            #endregion

            db.open_connection();

            LoginForm lf = new LoginForm();
            lf.ShowDialog();
            bool login = lf.login;
            lf.Dispose();

            if(login) Application.Run(new MainForm());

            db.close_connection();
        }
    }
}
