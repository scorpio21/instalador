using System.Windows;
using Microsoft.Win32;
using Instalador.ViewModels;
using Forms = System.Windows.Forms;

namespace Instalador.Views
{
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
        }

        private void BtnSeleccionarProyecto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                if (DataContext is ConfigViewModel vm && vm.ProyectoSeleccionado != null)
                {
                    vm.ProyectoSeleccionado.RutaProyecto = dialog.SelectedPath;
                }
            }
        }

        private void BtnSeleccionarPublicacion_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                if (DataContext is ConfigViewModel vm && vm.ProyectoSeleccionado != null)
                {
                    vm.ProyectoSeleccionado.RutaPublicacion = dialog.SelectedPath;
                }
            }
        }

        private void BtnSeleccionarInno_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "ISCC.exe|ISCC.exe|Todos los archivos (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                if (DataContext is ConfigViewModel vm)
                {
                    vm.RutaInnoSetup = dialog.FileName;
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ConfigViewModel vm)
            {
                vm.GuardarCommand.Execute(null);
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => Close();
    }
}
