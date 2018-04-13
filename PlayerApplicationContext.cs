using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Ayuda.BMS.Player.Shell;
using Ayuda.BMS.Player.Controls;
using Ayuda.BMS.Player.Managers;
using Ayuda.BMS.Player.Shared;
using Ayuda.BMS.Player.Views;
using log4net;

namespace Ayuda.BMS.Player
{
    public class PlayerApplicationContext : ApplicationContext
    {
        private BackgroundWorker _worker = new BackgroundWorker();

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PlayerApplicationContext()
        {
            // Force the static ctor to run and configure logging ASAP
            Engine.Instance.ToString();

            this.ThreadExit += new EventHandler(PlayerApplicationContext_ThreadExit);

            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);

            Shell.Shell.BootShellUILoaded += new Loaded(Shell_BootShellUILoaded);
            Shell.Shell.ShowBootShell();      
        }

        private void Shell_BootShellUILoaded(object sender, EventArgs e)
        {
            _worker.RunWorkerAsync();
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Shell.Shell.CloseBootShell();

            if (e.Error == null)
            {
                Shell.Shell.ShowShell();
            }
            else
            {
                _log.Error("Unable to start player", e.Error);

                throw e.Error;
            }
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Shell.Shell.BootShellMessage = "Initialising...";

            Engine.Instance.Bootstrap();

            Engine.Instance.ShutdownRequested += new OnShutdownRequested(Instance_ShutdownRequested);
            Engine.Instance.RestartRequested += new OnRestartRequested(Instance_RestartRequested);

            if (!Engine.Instance.ConfigurationManager.GetConfigured())
            {
                Shell.Shell.BootShellMessage = "Player is not configured. Deferring start up...";
            }
            else
            {
                Shell.Shell.BootShellMessage = "Player is already configured. Starting up...";

                Engine.Instance.Start();
            }
        }

        private void PlayerApplicationContext_ThreadExit(object sender, EventArgs e)
        {
            try
            {
                Terminate();
            }
            catch { }
        }

        private void Instance_RestartRequested(object sender, EventArgs e)
        {
            try
            {
                this.Terminate();
            }
            catch { }

            if (Engine.Instance.UpdateManager.IsNetworkDeployed)
            {
                Environment.ExitCode = Engine.Instance.UpdateManager.RestartAppReturnValue;
                Application.Exit();
            }
            else
            {
                Application.Restart();
            }
        }

        private void Instance_ShutdownRequested(object sender, ShutdownEventArgs e)
        {
            try
            {
                this.Terminate();
            }
            catch { }

            Environment.ExitCode = e.ExitCode;
            Application.Exit();
        }

        private void Terminate()
        {
            if (Engine.Instance.Started)
            {
                Engine.Instance.Stop();
            }

            Platform.Platform.RestoreCursor();
        }
    }
}
