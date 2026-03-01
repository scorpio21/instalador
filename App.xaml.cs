using System.Windows;

namespace Instalador
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogCrash(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is System.Exception ex)
            {
                LogCrash(ex);
            }
        }

        private void LogCrash(System.Exception ex)
        {
            try
            {
                string LogFile = System.IO.Path.Combine(System.AppContext.BaseDirectory, "crash_log.txt");
                string ErrorMsg = $"[{System.DateTime.Now}] CRASH:\n{ex.Message}\n{ex.StackTrace}\n\n";
                System.IO.File.AppendAllText(LogFile, ErrorMsg);
                System.Windows.MessageBox.Show("Fallo Crítico grabado en crash_log.txt:\n" + ex.Message, "Error en Single-File");
            }
            catch { }
        }
    }
}
