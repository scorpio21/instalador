using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Instalador.Models;

namespace Instalador.Services
{
    public interface ILoggingService
    {
        void AddLog(string mensaje);
        void Clear();
        void SaveToFile();
        void LoadFromFile();
        List<LogEntry> GetLogs();
    }

    public class LoggingService : ILoggingService
    {
        private readonly List<LogEntry> _logs = new();
        private readonly string _logsDirectory;
        private readonly string _logFilePath;
        private const int MaxLogEntries = 1000;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

        public LoggingService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var logsDirPreferido = Path.Combine(baseDir, "Log");
            var logsDirFallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Instalador", "logs");

            _logsDirectory = ResolverDirectorioLogs(logsDirPreferido, logsDirFallback);
            _logFilePath = Path.Combine(_logsDirectory, $"instalador_{DateTime.Now:yyyyMMdd}.log");

            LoadFromFile();
        }

        private static string ResolverDirectorioLogs(string preferido, string fallback)
        {
            if (TryCrearDirectorio(preferido))
            {
                return preferido;
            }

            TryCrearDirectorio(fallback);
            return fallback;
        }

        private static bool TryCrearDirectorio(string ruta)
        {
            try
            {
                if (!Directory.Exists(ruta))
                {
                    Directory.CreateDirectory(ruta);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void AddLog(string mensaje)
        {
            var logEntry = new LogEntry 
            { 
                Hora = DateTime.Now.ToString("HH:mm:ss"), 
                Mensaje = mensaje,
                FechaCompleta = DateTime.Now
            };
            
            _logs.Insert(0, logEntry);

            AppendLogLineToFile(logEntry);
            
            // Mantener solo los últimos MaxLogEntries en memoria
            if (_logs.Count > MaxLogEntries)
            {
                _logs.RemoveAt(_logs.Count - 1);
            }
        }

        private void AppendLogLineToFile(LogEntry logEntry)
        {
            try
            {
                // Rotar archivo si excede el tamaño máximo
                if (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length > MaxFileSizeBytes)
                {
                    RotateLogFile();
                }

                var line = $"[{logEntry.FechaCompleta:yyyy-MM-dd HH:mm:ss}] {logEntry.Mensaje}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, line, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Silencioso para no crear bucle de logs
                Console.WriteLine($"Error guardando log: {ex.Message}");
            }
        }

        public void Clear()
        {
            _logs.Clear();
        }

        public void SaveToFile()
        {
            try
            {
                // Rotar archivo si excede el tamaño máximo
                if (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length > MaxFileSizeBytes)
                {
                    RotateLogFile();
                }

                var logLines = _logs.Select(log => $"[{log.FechaCompleta:yyyy-MM-dd HH:mm:ss}] {log.Mensaje}");
                File.WriteAllLines(_logFilePath, logLines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Silencioso para no crear bucle de logs
                Console.WriteLine($"Error guardando log: {ex.Message}");
            }
        }

        private void RotateLogFile()
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string backupPath = Path.Combine(_logsDirectory, $"instalador_{timestamp}.log.bak");
                File.Move(_logFilePath, backupPath);
                
                // Limpiar archivos de respaldo antiguos (mantener solo los últimos 5)
                var backupFiles = Directory.GetFiles(_logsDirectory, "*.log.bak")
                    .OrderByDescending(f => f)
                    .Skip(5);
                
                foreach (var oldBackup in backupFiles)
                {
                    File.Delete(oldBackup);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rotando log: {ex.Message}");
            }
        }

        public void LoadFromFile()
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    var lines = File.ReadAllLines(_logFilePath, Encoding.UTF8);
                    _logs.Clear();
                    
                    foreach (var line in lines.Reverse()) // Invertir para mostrar más recientes primero
                    {
                        if (TryParseLogLine(line, out var logEntry) && logEntry != null)
                        {
                            _logs.Add(logEntry);
                        }
                    }
                    
                    // Limitar a MaxLogEntries
                    if (_logs.Count > MaxLogEntries)
                    {
                        _logs.RemoveRange(MaxLogEntries, _logs.Count - MaxLogEntries);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando logs: {ex.Message}");
            }
        }

        private bool TryParseLogLine(string line, out LogEntry? logEntry)
        {
            logEntry = null;
            
            try
            {
                // Formato esperado: [2026-03-02 14:30:25] Mensaje del log
                if (line.StartsWith("[") && line.Contains("] "))
                {
                    int endIndex = line.IndexOf("] ");
                    if (endIndex > 0)
                    {
                        string dateTimeStr = line.Substring(1, endIndex - 1);
                        string message = line.Substring(endIndex + 2);
                        
                        if (DateTime.TryParse(dateTimeStr, out var dateTime))
                        {
                            logEntry = new LogEntry
                            {
                                FechaCompleta = dateTime,
                                Hora = dateTime.ToString("HH:mm:ss"),
                                Mensaje = message
                            };
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // Ignorar errores de parsing
            }
            
            return false;
        }

        public List<LogEntry> GetLogs()
        {
            return new List<LogEntry>(_logs);
        }
    }
}
