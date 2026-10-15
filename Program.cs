using System;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Naglyadach.Data;
using Naglyadach.Models;
using Naglyadach.Services;
using Naglyadach.Menu;
using Naglyadach;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Naglyadach
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string mutexName = @"Global\Naglyadach";
            bool createdNew;

            using (var mutex = new Mutex(true, mutexName, out createdNew))
            {
                if (!createdNew)
                {                    
                    PrintError(" Ошибка: Naglyadach запущен в другом окне");                    
                    return;
                }
                Console.WriteLine(" Mutex получен. Дубликатов нет");
                // =========================================================

                Console.WriteLine("Hello World!");
                var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var configuration = builder.Build();
                                
                var connectionString = configuration.GetConnectionString("NaglyadachDb");

                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("Ошибка: строка подключения не найдена!");
                    return;
                }

                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                var context = new AppDbContext(optionsBuilder.Options);
                try
                {                    
                    PrintGreen(" Подключение к БД установлено");
                    
                    DbInitializer.Initialize(context);
                    Console.WriteLine(" Данные инициализированы\n");

                    var profiles = await context.LaunchProfile.ToListAsync();
                    if (profiles.Count == 0)
                    {
                        Console.WriteLine(" В базе нет профилей");
                        return;
                    }

                    Console.WriteLine(" Список профилей запуска:");
                    foreach (var p in profiles)
                    {
                        Console.WriteLine($"   - {p.Name} (файл: {p.FileName}, лимит: {p.MaxInstances})");
                    }
                    Console.WriteLine(new string('-', 40));

                    // --- ЗАПУСК NOTEPAD ---
                    var notepadProfile = profiles.FirstOrDefault(p =>
                        p.FileName.Contains("notepad.exe", StringComparison.OrdinalIgnoreCase) ||
                        p.Name == "Notepad"
                    );

                    if (notepadProfile != null)
                    {
                        Console.WriteLine(" Запуск первого профиля (Notepad)...");
                        var launcher = new ProcessLauncher(context);
                        bool success = await launcher.LaunchAsync(notepadProfile);

                        if (success)
                        {                            
                            PrintGreen("\n БЛОКНОТ ОТКРЫТ\n");                            
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("\n Запуск не выполнен (лимит или ошибка)");
                            Console.ResetColor();
                        }
                    }
                    else
                    {                        
                        PrintError(" Профиль для Блокнота не найден");                        
                    }
                    //***********************************************************
                    // --- ЗАПУСК Calculator ---
                    var calcProfile = profiles.FirstOrDefault(p =>
                        p.FileName.Contains("calc.exe", StringComparison.OrdinalIgnoreCase) ||
                        p.Name == "Calculator"
                    );

                    if (calcProfile != null)
                    {
                        Console.WriteLine(" Запуск второго профиля (Calculator)...");
                        var launcher = new ProcessLauncher(context);
                        bool success = await launcher.LaunchAsync(calcProfile);

                        if (success)
                        {                            
                            PrintGreen("\n Calculator ОТКРЫТ\n");                            
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("\n Запуск не выполнен (лимит или ошибка)");
                            Console.ResetColor();
                        }
                    }
                    else
                    {                        
                        PrintError(" Профиль для Calculator не найден");                        
                    }
                    // =========================================================

                    Console.WriteLine("----------------------------------------");
                    Console.WriteLine("     ЗАПУСК МОНИТОРИНГА ПРОЦЕССОВ    ");
                    Console.WriteLine("----------------------------------------\n");

                    var monitoringService = new MonitoringService(context);
                    Console.WriteLine(" Мониторинг активен ");

                    //*******
                    var analysisService = new SnapshotAnalysisService(context);
                    //********

                    // меню--
                    var menu = new ConsoleMenu(monitoringService, analysisService);
                    bool shouldStop = menu.Run();

                    // остановка                   
                    if (shouldStop)
                    {
                        monitoringService.Stop();
                        Console.WriteLine(" ОСТАНОВКА. Выход из программы");
                    }
                }
                catch (Exception ex)
                {                    
                    PrintError($"\n ERROR: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($" Детали: {ex.InnerException.Message}");
                    }                    
                }
                finally
                {
                    context.Dispose();
                    Console.WriteLine(" Контекст базы данных закрыт");
                }
            }
        }
        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        static void PrintGreen(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

    }
}
