using Microsoft.EntityFrameworkCore;
using Naglyadach.Models;

namespace Naglyadach.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            //context.Database.Migrate();

            if (!context.LaunchProfile.Any())
            {
                var profiles = new List<LaunchProfile>
                {
                    new LaunchProfile
                    {
                        Name = "Notepad",
                        FileName = "notepad.exe",
                        Arguments = "",
                        MaxInstances = 1
                    },
                    new LaunchProfile
                    {
                        Name = "Calculator",
                        FileName = "calc.exe",
                        Arguments = "",
                        MaxInstances = 1
                    },
                    new LaunchProfile
                    {
                        Name = "TestApp",
                        FileName = "TestApp.exe",
                        Arguments = "",
                        MaxInstances = 2
                    }
                };
                context.LaunchProfile.AddRange(profiles);
                context.SaveChanges();
            }

            if (!context.WatchRules.Any())
            {
                var rules = new List<WatchRules>
                {
                    new WatchRules
                    {
                        ProcessName = "notepad.exe",
                        MaxMemoryMb = 500,
                        Action = WatchAction.Log
                    },

                    new WatchRules
                    {
                        ProcessName = "chrome.exe",
                        MaxMemoryMb = 2000,
                        Action = WatchAction.Kill
                    }
                };
                    context.WatchRules.AddRange(rules);
                    context.SaveChanges();
            }
        }
    }
}

