using Naglyadach.Services;
using Naglyadach.Models;
using System.Globalization;

namespace Naglyadach.Menu
{
    public class ConsoleMenu
    {        
        private readonly MonitoringService Service;
        private readonly SnapshotAnalysisService AnalysisService;
        public ConsoleMenu(MonitoringService service, SnapshotAnalysisService analysisService)
        {
            Service = service;
            AnalysisService = analysisService;
        }
        public bool Run()
        {
            Console.WriteLine("\n АКТИВНЫЙ РЕЖИМ МОНИТОРИНГА");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Доступные команды:");
            Console.WriteLine("  1 (pause)   - Приостановить сбор данных");
            Console.WriteLine("  2 (resume)  - Возобновить сбор данных");
            Console.WriteLine("  3 (stop)    - Остановить мониторинг и выйти");
            Console.WriteLine("  4 - Запустить параллельный анализ снимков (ThreadPool)");
            Console.WriteLine("----------------------------------------");
            Console.Write("\nВведите команду --> ");

            while (true)
            {
                try
                {                    
                    string? input = Console.ReadLine();

                    if (input == null)
                        return false; 
                    string command = input.Trim().ToLower();

                    switch (command)
                    {
                        case "1":
                            Service.PauseMonitoring();
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Статус: Мониторинг на ПАУЗЕ. Данные не собираются");
                            Console.ResetColor();
                            break;

                        case "2":
                            Service.ResumeMonitoring();                            
                            PrintGreen("Статус: Мониторинг ВОЗОБНОВЛЕН. Сбор данных идет");                            
                            break;                        
                        case "4":
                            Service.PauseMonitoring();

                            Console.WriteLine();
                            Console.Write(" Введите дату начала анализа (формат ГГГГ-ММ-ДД): ");
                            string? startInput = Console.ReadLine();
                            Console.WriteLine();
                            Console.Write(" Введите дату окончания анализа (формат ГГГГ-ММ-ДД):");
                            string? endInput = Console.ReadLine();

                            if (string.IsNullOrWhiteSpace(startInput) || string.IsNullOrWhiteSpace(endInput))
                            {
                                PrintError("Даты не могут быть пустыми");
                                break;
                            }

                            Service.ResumeMonitoring();
                            if (DateTime.TryParse(startInput, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime start) &&
                                DateTime.TryParse(endInput, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime end))
                            {
                                if (start > end)
                                {
                                    PrintError("Дата начала не может быть позже даты окончания");
                                    break;
                                }
                                AnalysisService.AnalyzeSnapshotsInParallel(start, end);
                            }
                            else
                            {
                                PrintError("Ошибка формата даты. Формат: ГГГГ-ММ-ДД (напр, 2023-10-01).");
                            }
                            break;                                             

                        case "3":
                        case "0":
                            PrintError("Статус: остановка...");
                            return true; 

                        default:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine($"Неизвестная команда: '{command}', Попробуйте: pause, resume, stop.");
                            Console.ResetColor();
                            break;
                    }
                    
                    Console.Write("Введите команду: ");
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"Ошибка ввода: {ex.Message}");
                    Console.ResetColor();
                    Console.Write("Введите команду: ");
                }
            }
        }
        public void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public void PrintGreen(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
