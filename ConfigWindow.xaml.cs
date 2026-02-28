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
            var p = config.GetProyectoActual();
            if (p != null)
            {
                TxtNombreProyecto.Text = p.Nombre;
                TxtVersion.Text = p.VersionInstalador;
                TxtRutaProyecto.Text = p.RutaProyecto;
                TxtRutaPublicacion.Text = p.RutaPublicacion;
            }

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

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            TxtNombreProyecto.Text = "";
            TxtVersion.Text = "1.0";
            TxtRutaProyecto.Text = "";
            TxtRutaPublicacion.Text = "";
            TxtNombreProyecto.Focus();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNombreProyecto.Text) || 
                string.IsNullOrWhiteSpace(TxtRutaProyecto.Text) || 
                string.IsNullOrWhiteSpace(TxtRutaPublicacion.Text))
            {
                System.Windows.MessageBox.Show("Por favor, completa los campos obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Buscar si ya existe un proyecto con ese nombre para actualizarlo, o crear uno nuevo
            var p = config.Proyectos.FirstOrDefault(proj => proj.Nombre == TxtNombreProyecto.Text);
            if (p == null)
            {
                p = new ProyectoConfig { Nombre = TxtNombreProyecto.Text };
                config.Proyectos.Add(p);
            }

            p.VersionInstalador = TxtVersion.Text;
            p.RutaProyecto = TxtRutaProyecto.Text;
            p.RutaPublicacion = TxtRutaPublicacion.Text;
            config.RutaInnoSetup = TxtRutaInnoSetup.Text;
            config.UltimoProyectoSeleccionado = p.Nombre;

            config.Guardar();
            System.Windows.MessageBox.Show("Proyecto guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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
