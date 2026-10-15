using Microsoft.EntityFrameworkCore;
using Naglyadach.Data;
using Naglyadach.Models;
using System;
using System.Threading;

namespace Naglyadach.Services 
{
    public class SnapshotAnalysisService
    {
        private readonly AppDbContext dbContext;

        public SnapshotAnalysisService(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
                
        public void AnalyzeSnapshotsInParallel(DateTime start, DateTime end)
        {
            Console.WriteLine($" Запуск параллельного анализа: [{start:yyyy-MM-dd}] — [{end:yyyy-MM-dd}]");

           
            var snapshots = dbContext.Snapshots
                .Include(s => s.ProcessSamples) 
                .Where(s => s.TakenAt >= start && s.TakenAt <= end)
                .ToList();

            if (snapshots.Count == 0)
            {
                Console.WriteLine(" Нет снимков для анализа за этот период");
                return;
            }

            Console.WriteLine($" Найдено снимков: {snapshots.Count}. Раскидываем по потокам ThreadPool...");

            int processedCount = 0; 
            var topProcesses = new List<ProcessSample>(); 
            var lockObj = new object(); 
            
            ManualResetEvent[] events = new ManualResetEvent[snapshots.Count];

            for (int i = 0; i < snapshots.Count; i++)
            {
                events[i] = new ManualResetEvent(false); 
                var snapshot = snapshots[i]; 
                
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {             
                        var topProc = snapshot.ProcessSamples
                            .OrderByDescending(p => p.WorkingSetMb)
                            .FirstOrDefault();

                        if (topProc != null)
                        {                           
                            lock (lockObj)
                            {
                                topProcesses.Add(topProc);                               
                                Interlocked.Increment(ref processedCount);
                                Console.WriteLine($" [Поток #{Thread.CurrentThread.ManagedThreadId}] " +
                                                  $" Обработал снимок от {snapshot.TakenAt:HH:mm:ss}," +
                                                  $" топ-процесс: {topProc.Name}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"ERROR (потока) #{i}] {ex.Message}");
                        Console.ResetColor();
                    }
                    finally
                    {                       
                        events[i].Set();
                    }
                });
            }                        
            WaitHandle.WaitAll(events);
            
            foreach (var evt in events) evt.Dispose();

            Console.WriteLine($" Анализ завершен, обработано снимков: {processedCount}");

            var globalTop5 = topProcesses
                .GroupBy(p => p.Name) 
                .Select(g => new
                {
                    Name = g.Key,
                    MaxMem = g.Max(x => x.WorkingSetMb) 
                })
                .OrderByDescending(x => x.MaxMem)
                .Take(5)
                .ToList();

            Console.WriteLine("\n Топ-5 процессов по пиковой памяти за период:");
            Console.WriteLine("----------------------------------------");
            foreach (var item in globalTop5)
            {                
                Console.WriteLine($"| {item.Name,-20} | {item.MaxMem,10:F2} MB |");
            }
            Console.WriteLine("----------------------------------------\n");
        }
    }
}
