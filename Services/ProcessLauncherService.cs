using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
//using System.IO;
using Naglyadach.Data;
using Naglyadach.Models;

namespace Naglyadach
{
    public class ProcessLauncher
    {
        private readonly AppDbContext Context;
        public ProcessLauncher(AppDbContext context)
        {
            Context = context;
        }
        public async Task<bool> LaunchAsync(LaunchProfile profile)
        {            
            string fileName = System.IO.Path.GetFileName(profile.FileName); 
            int currentCount = Process.GetProcessesByName(fileName).Length;

            Console.WriteLine($" Запуск: текущих экземпляров '{fileName}': {currentCount}, Лимит: {profile.MaxInstances}");
                       
            if (currentCount >= profile.MaxInstances)
            {
                Console.WriteLine($" Лимит! Не удается  запустить '{profile.Name}'. Открыто: {currentCount}, лимит: {profile.MaxInstances}");
                return false;
            }
            try
            {               
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = profile.FileName,
                        Arguments = profile.Arguments ?? "",
                        UseShellExecute = true
                    }
                };

                process.Start();
                Console.WriteLine($" Успешно запущен: {profile.Name} ({profile.FileName})");
                
                var record = new LaunchRecord
                {
                    ProfileId = profile.Id,
                    StartedAt = DateTime.UtcNow,
                    Pid = process.Id,
                    ExitedAt = null,  
                    ExitCode = null                    
                };

                Context.LaunchRecord.Add(record);
                await Context.SaveChangesAsync();

                Console.WriteLine($" Запись (о запуске) сохранена в БД (ID: {record.Id})");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: (ошибка при запуске): {ex.Message}");
                return false;
            }
        }
    }
}
