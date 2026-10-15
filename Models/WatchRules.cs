using System;

namespace Naglyadach.Models
{
    public class WatchRules
    {
        public int Id { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public double MaxMemoryMb { get; set; }
        public WatchAction Action { get; set; }
    }
}
