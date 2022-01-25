using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class Session
    {
        [Key]
        public int SessionId { get; set; }
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public string JwtPassphrase { get; set; }
        public DateTime Expiration { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}
