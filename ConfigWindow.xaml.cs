using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace Instalador
{
    public partial class ConfigWindow : Window
    {
        private Config config;

        public ConfigWindow()
        {
            InitializeComponent();
            config = Config.Cargar();
            LoadData();
        }

        private void LoadData()
        {
            TxtNombreProyecto.Text = config.NombreProyecto;
            TxtVersion.Text = config.VersionInstalador;
            TxtRutaProyecto.Text = config.RutaProyecto;
            TxtRutaPublicacion.Text = config.RutaPublicacion;
            TxtRutaInnoSetup.Text = config.RutaInnoSetup;
        }

        private void BtnBuscarProyecto_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == Forms.DialogResult.OK)
                TxtRutaProyecto.Text = dlg.SelectedPath;
        }

        private void BtnBuscarPublicacion_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new Forms.FolderBrowserDialog();
            if (dlg.ShowDialog() == Forms.DialogResult.OK)
                TxtRutaPublicacion.Text = dlg.SelectedPath;
        }

        private void BtnBuscarInno_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Inno Setup Compiler|ISCC.exe";
            if (dlg.ShowDialog() == true)
                TxtRutaInnoSetup.Text = dlg.FileName;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            config.NombreProyecto = TxtNombreProyecto.Text;
            config.VersionInstalador = TxtVersion.Text;
            config.RutaProyecto = TxtRutaProyecto.Text;
            config.RutaPublicacion = TxtRutaPublicacion.Text;
            config.RutaInnoSetup = TxtRutaInnoSetup.Text;

            config.Guardar();
            System.Windows.MessageBox.Show("Configuración guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
