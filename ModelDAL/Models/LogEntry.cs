using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gzDAL.Models
{
    public class LogEntry
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Application { get; set; }

        [Required]
        public DateTime Logged { get; set; }

        [Required]
        public string Level { get; set; }

        [Required]
        [StringLength(Int32.MaxValue)]
        public string Message { get; set; }

        [StringLength(250)]
        public string UserName { get; set; }

        [StringLength(Int32.MaxValue)]
        public string ServerName { get; set; }

        [StringLength(Int32.MaxValue)]
        public string Port { get; set; }

        [StringLength(Int32.MaxValue)]
        public string Url { get; set; }
        
        public bool Https { get; set; }

        [StringLength(Int32.MaxValue)]
        public string ServerAddress { get; set; }

        [StringLength(Int32.MaxValue)]
        public string RemoteAddress { get; set; }

        [StringLength(Int32.MaxValue)]
        public string Logger { get; set; }

        [StringLength(Int32.MaxValue)]
        public string CallSite { get; set; }

        [StringLength(Int32.MaxValue)]
        public string Exception { get; set; }
    }
}