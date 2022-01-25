using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.GameLogModels
{
    public class LogError
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogErrorId { get; set; }
        public DateTime DateOccurred { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string Line { get; set; }
        public string Class { get; set; }
        public string Method { get; set; }
    }
}
