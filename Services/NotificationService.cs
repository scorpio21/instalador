using System;
using System.Diagnostics;

namespace Instalador.Services
{
    public interface INotificationService
    {
        void Notify(string title, string message);
    }

    public class NotificationService : INotificationService
    {
        public void Notify(string title, string message)
        {
            try
            {
                // Usamos un script de PowerShell simple para mostrar un Toast sin dependencias externas pesadas
                string script = $@"
$title = '{title}'
$msg = '{message}'
[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null
$template = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText02)
$textNodes = $template.GetElementsByTagName('text')
$textNodes.Item(0).AppendChild($template.CreateTextNode($title)) | Out-Null
$textNodes.Item(1).AppendChild($template.CreateTextNode($msg)) | Out-Null
$toast = [Windows.UI.Notifications.ToastNotification]::new($template)
[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('Instalador PRO').Show($toast)
";
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script.Replace("\"", "\\\"")}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error al enviar notificación: " + ex.Message);
            }
        }
    }
}
