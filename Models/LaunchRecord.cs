using Microsoft.EntityFrameworkCore;
using System;

namespace Naglyadach.Models
{
    public class LaunchRecord
    {
        public int Id { get; set; }       
        public int ProfileId { get; set; }
        public int Pid { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? ExitedAt { get; set; }
        public int? ExitCode { get; set; }
       
        public LaunchProfile Profile { get; set; } = null!;      

    }
}