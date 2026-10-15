using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using Naglyadach.Data;
using Naglyadach.Models;

using Timer = System.Timers.Timer;
using System.Threading;

namespace Naglyadach.Services
{
    public class MonitoringService
    {        
        private readonly Timer Timer;
        private readonly AppDbContext Context;
        private readonly ReportService ReportService;

        private bool isHeaderPrinted = false;
        
        private readonly ManualResetEvent MonitoringEvent;

        public MonitoringService(AppDbContext context)
                   : this(context, new ReportService()) 
        {
        }
                
        public MonitoringService(AppDbContext context, ReportService reportService)
        {
            Context = context;
            ReportService = reportService;
            MonitoringEvent = new System.Threading.ManualResetEvent(true);
           
            int intervalSeconds = 5; 

            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var config = builder.Build();
               
                int configValue = config.GetValue<int>("Monitoring:IntervalSeconds");

                if (configValue > 0)
                {
                    intervalSeconds = configValue;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($" Не удалется  прочитать конфиг: {ex.Message}. Интервал по умолчанию: {intervalSeconds} сек.");
                Console.ResetColor();
            }
            
            Console.WriteLine($" Мониторинг запущен. Интервал: {intervalSeconds} сек.");

            Timer = new Timer(intervalSeconds * 1000);
            Timer.AutoReset = true;
            Timer.Elapsed += OnTimerElapsed;
            Timer.Start();            
        }        
        private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {           
            if (!MonitoringEvent.WaitOne(0))
                return;
            try
            {
                var processes = Process.GetProcesses();
                
                var topProcesses = processes
                    .OrderByDescending(p => p.WorkingSet64)
                    .Take(10)
                    .ToList();

                if (!topProcesses.Any())
                {
                    Console.WriteLine(" Мониторинг: Процессы не найдены");
                    return;
                }
                
                double totalMemoryMb = topProcesses.Sum(p => p.WorkingSet64) / (1024.0 * 1024.0);
                int processCount = topProcesses.Count;
                
                var snapshot = new Snapshot
                {
                    TakenAt = DateTime.UtcNow,
                    ProcessCount = processCount,
                    TotalMemoryMb = totalMemoryMb
                };
                
                foreach (var proc in topProcesses)
                {                    
                    double memoryInMb = proc.WorkingSet64 / (1024.0 * 1024.0);

                    snapshot.ProcessSamples.Add(new ProcessSample
                    {
                        Name = proc.ProcessName,
                        Pid = proc.Id,
                        WorkingSetMb = memoryInMb,
                       
                        ThreadCount = proc.Threads.Count,
                        Priority = proc.BasePriority
                    });
                }
                Context.Snapshots.Add(snapshot);
                Context.SaveChanges();                

                //***************************               
                string status = totalMemoryMb > 2048 ? " HIGH LOAD" : "OK";               
                if (!isHeaderPrinted)
                {
                    Console.WriteLine("\n" + new string('-', 65));
                    Console.WriteLine("| Время        | Процессы   |  Память (MB)  |  Статус  |");
                    Console.WriteLine(new string('-', 65));
                    isHeaderPrinted = true; 
                }              
                
                string tableRow = ReportService.FormatTableRow(processCount, totalMemoryMb, status);                
                ReportService.Log(tableRow);                
                ReportService.CheckAndAlert(totalMemoryMb);
                //****************************
               
                bool isAlert = ReportService.CheckAndAlert(totalMemoryMb);
                if (isAlert)
                { }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" Ошибка снимка: {ex.Message}");
                Console.ResetColor();
            }
        }

        public void PauseMonitoring()
        {
            MonitoringEvent.Reset(); 
            Console.WriteLine(" >> Мониторинг ПРИОСТАНОВЛЕН");
        }
        public void ResumeMonitoring()
        {
            MonitoringEvent.Set(); 
            Console.WriteLine(" >> Мониторинг ВОЗОБНОВЛЕН");
        }
        public void Stop()
        {
            Timer.Stop();
            Timer.Dispose();
            MonitoringEvent.Dispose();
            Console.WriteLine(" >> Мониторинг полностью остановлен");
        }
    }
}
