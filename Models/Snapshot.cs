using Microsoft.EntityFrameworkCore;
using Naglyadach.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Naglyadach.Models
{
    public class Snapshot
    {
        public int Id { get; set; }
        public DateTime TakenAt { get; set; }
        public int ProcessCount { get; set; }
        public double TotalMemoryMb { get; set; }        
        public ICollection<ProcessSample> ProcessSamples { get; set; } = new List<ProcessSample>();
    }
}
