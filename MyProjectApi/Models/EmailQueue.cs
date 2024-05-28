using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyProjectApi.Models
{
    public class EmailQueue
    {
        [Key]
        public int ID { get; set; }
        public int UserId { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool sent { get; set; }
        public DateTime createdAt { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }
    }
}