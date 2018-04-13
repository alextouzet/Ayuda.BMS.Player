using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using log4net;

namespace Ayuda.BMS.Player
{
    static class Program
    {
        private static readonly Guid _px = new Guid("D4C70998-6E8C-41d3-BF57-4ABF4E481D2E");
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread]
        static int Main()
        {
            Environment.ExitCode = 0;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            using (ProcessLock _processLock = new ProcessLock(_px.ToString()))
            {
                if (_processLock.Exists)
                {
                    MessageBox.Show(Properties.Resources.Error_InstanceAlreadyRunning, 
                        Properties.Resources.DefaultFormTitle, 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Stop);

                    return 0; 
                }

                PlayerApplicationContext ctx = new PlayerApplicationContext();
                Application.Run(ctx);

                return Environment.ExitCode;
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //try
            //{
            //    Engine.Instance.AlertManager.SetAlert(e.Exception.Source, e.Exception.Message, Shared.AlertType.Error, Managers.AlertManager.AlertAction.Post, "", e.Exception);
            //}
            //catch { }

            _log.Error("Unexpected error in UI thread", e.Exception);

            Environment.Exit(1);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;

            //try
            //{
            //    Engine.Instance.AlertManager.SetAlert(ex.Source, ex.Message, Shared.AlertType.Error, Managers.AlertManager.AlertAction.Post, "", ex);
            //}
            //catch { }

            _log.Error("Unhandled exception", ex);

            Environment.Exit(1);
        }
    }
}
