using System;
using System.Windows.Forms;

namespace DicomImageViewer
{
    static class MainModule
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FTest());
        }
    }
}
