using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Instalador.Models;
using Instalador.Services;
using Instalador.ViewModels;

namespace Instalador.Views
{
    public partial class MainWindow : Window
    {
        public const string AppVersion = "1.1.1";
        private MainViewModel _viewModel;
        private DispatcherTimer timerHora = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            
            var configService = new ConfigService();
            var gitService = new GitService();
            var buildService = new BuildService();
            var innoService = new InnoSetupService();
            var notificationService = new NotificationService();

            _viewModel = new MainViewModel(configService, gitService, buildService, innoService, notificationService);
            this.DataContext = _viewModel;

            timerHora.Interval = TimeSpan.FromSeconds(1);
            timerHora.Tick += (s, e) => TxtHora.Text = DateTime.Now.ToString("HH:mm:ss");
            timerHora.Start();
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            var configService = new ConfigService();
            var vm = new ConfigViewModel(configService);
            var cfg = new ConfigWindow();
            cfg.DataContext = vm;
            cfg.Owner = this;
            cfg.ShowDialog();
            
            // Refrescar lista de proyectos tras cerrar config
            _viewModel.Proyectos.Clear();
            var newConfig = configService.CargarConfig();
            foreach (var p in newConfig.Proyectos) _viewModel.Proyectos.Add(p);
        }

        private void MenuInstrucciones_Click(object sender, RoutedEventArgs e)
        {
            string msg = "MANUAL DE USO RÁPIDO (v1.1.1):\n\n" +
                         "1. Selecciona tu proyecto en el selector superior.\n" +
                         "2. Usa 'Limpiar' para preparar el entorno.\n" +
                         "3. 'Compilar' verifica que el código sea correcto.\n" +
                         "4. 'Publicar' aplica tus ajustes (ReadyToRun, Trim, etc).\n" +
                         "5. 'Generar Instalador' crea el setup final.\n" +
                         "6. 'EJECUTAR TODO' automatiza todo el flujo.\n\n" +
                         "Novedades Power User:\n" +
                         "• Recibirás una notificación Toast al finalizar.\n" +
                         "• Usa 'Auto-Detectar' en Ajustes para Inno Setup.\n" +
                         "• El punto naranja indica cambios en Git.";
            System.Windows.MessageBox.Show(msg, "Instrucciones", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuAutor_Click(object sender, RoutedEventArgs e)
        {
            try { Process.Start(new ProcessStartInfo("https://github.com/scorpio21") { UseShellExecute = true }); }
            catch { System.Windows.MessageBox.Show("No se pudo abrir el navegador."); }
        }

        private void MenuAcercaDe_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show($"Instalador PRO v{AppVersion}\nDesarrollado para Scorpio 2026\n\nHerramienta profesional de empaquetado y automatización de procesos .NET.", "Acerca de", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
