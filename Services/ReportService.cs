using System;
using System.IO;

namespace Naglyadach.Services
{
    public class ReportService
    {
        private readonly string logPath;
        private const double CriticalThresholdMb = 2048; 

        public ReportService()
        {            
            logPath = Path.Combine("logs", "monitor.log");                        
            Directory.CreateDirectory("logs");
        }

        public void Log(string message)
        {            
            if (message.StartsWith("|"))
            {
                Console.WriteLine(message);
                File.AppendAllText(logPath, message + Environment.NewLine);
            }
            else
            {                
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                Console.WriteLine(logEntry);
                File.AppendAllText(logPath, logEntry + Environment.NewLine);
            }
        }
        public bool CheckAndAlert(double totalMemoryMb)
        {
            if (totalMemoryMb > CriticalThresholdMb)
            {
                string alertMessage = $" КРИТИЧЕСКАЯ НАГРУЗКА: {totalMemoryMb:F2} MB! (Лимит: {CriticalThresholdMb} MB)";

                Console.ForegroundColor = ConsoleColor.Red;
                Log(alertMessage); 
                Console.ResetColor();

                return true;
            }
            return false;
        }        
        public string FormatTableRow(int processCount, double totalMemory, string status)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            return $"| {time,-12} | {processCount,-10} | {totalMemory,-10:F2} MB | {status,-8} |";
        }
    }
}