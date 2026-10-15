using Microsoft.EntityFrameworkCore;
using Naglyadach.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Naglyadach.Models
{
    public class ProcessSample
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double WorkingSetMb { get; set; }
        public int Pid { get; set; }
        public int ThreadCount { get; set; }
        public int Priority { get; set; }
      
        public int SnapshotId { get; set; }        
        public Snapshot Snapshot { get; set; } = null!;
    }
}
