using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService
{
    public class UserIpInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InfoId { get; set; }
        //[ForeignKey(nameof(Player))]
        //public int? PlayerId { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public DateTime RecDate { get; set; }
        public bool BadIp { get; set; }
        //public virtual ApplicationUser Player { get; set; }
    }
}
