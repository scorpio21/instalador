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

            if (string.IsNullOrWhiteSpace(config.RutaInnoSetup))
            {
                TxtRutaInnoSetup.Text = DetectarInnoSetup();
            }
            else
            {
                TxtRutaInnoSetup.Text = config.RutaInnoSetup;
            }
            
            ValidarTodasLasRutas();
        }

        private string DetectarInnoSetup()
        {
            string[] rutasComunes = {
                @"C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
                @"C:\Program Files\Inno Setup 6\ISCC.exe",
                @"C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
            };

            foreach (var ruta in rutasComunes)
            {
                if (File.Exists(ruta)) return ruta;
            }
            return "";
        }

        private void TxtRuta_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
            {
                ValidarRuta(tb);
            }
        }

        private void ValidarTodasLasRutas()
        {
            ValidarRuta(TxtRutaProyecto);
            ValidarRuta(TxtRutaPublicacion);
            ValidarRuta(TxtRutaInnoSetup);
        }

        private void ValidarRuta(System.Windows.Controls.TextBox tb)
        {
            string path = tb.Text;
            bool esValido = false;

            if (string.IsNullOrWhiteSpace(path))
            {
                tb.BorderBrush = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["BorderBrush"];
                return;
            }

            if (tb == TxtRutaInnoSetup)
            {
                esValido = File.Exists(path) && path.EndsWith("ISCC.exe", StringComparison.OrdinalIgnoreCase);
            }
            else if (tb == TxtRutaPublicacion)
            {
                // El directorio de salida puede no existir, así que lo marcamos como válido 
                // si la ruta tiene un formato correcto (simplemente comprobamos que no sea vacía)
                // O mejor: comprobamos que el directorio padre existe.
                try {
                    string parent = Path.GetDirectoryName(path) ?? "";
                    esValido = string.IsNullOrEmpty(parent) || Directory.Exists(parent) || Directory.Exists(path);
                } catch { esValido = false; }
            }
            else
            {
                esValido = Directory.Exists(path) || File.Exists(path);
            }

            tb.BorderBrush = esValido ? 
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 167, 69)) : // Verde
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 53, 69));   // Rojo
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
            // Validación básica
            if (string.IsNullOrWhiteSpace(TxtNombreProyecto.Text) || 
                string.IsNullOrWhiteSpace(TxtRutaProyecto.Text) || 
                string.IsNullOrWhiteSpace(TxtRutaPublicacion.Text))
            {
                System.Windows.MessageBox.Show("Por favor, completa los campos obligatorios (Nombre y Rutas).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(TxtRutaInnoSetup.Text) && !File.Exists(TxtRutaInnoSetup.Text))
            {
                System.Windows.MessageBox.Show("La ruta de Inno Setup no parece válida o el archivo ISCC.exe no existe.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

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
