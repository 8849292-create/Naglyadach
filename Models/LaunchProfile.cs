using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;
using System.ComponentModel.DataAnnotations;

namespace Naglyadach.Models
{
    public class LaunchProfile 
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [MaxLength(500)]
        public string FileName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Arguments { get; set; }
        public int MaxInstances { get; set; } = 1;        
        public ICollection<LaunchRecord> LaunchRecords { get; set; } = new List<LaunchRecord>();
        
    }
}