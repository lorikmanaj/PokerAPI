using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class TokenModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ClientId { get; set; }
        [Required]
        public string Value { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        [Required]
        public DateTime LastModifiedDate { get; set; }
        [Required]
        public DateTime ExpiryTime { get; set; }
        [Required]
        public string EncryptionKeyRt { get; set; }
        [Required]
        public string EncryptionKeyJwt { get; set; }
        
        public virtual ApplicationUser User { get; set; }
    }
}
