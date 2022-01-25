using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ModelService
{
    public class ApplicationUser : IdentityUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlayerId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string UserEmail { get; set; }
        public decimal Chips { get; set; }
        public string Gender { get; set; }
        public string Notes { get; set; }
        public string ProfilePic { get; set; }
        public string Birthday { get; set; }
        public bool IsProfileComplete { get; set; }
        public bool Terms { get; set; }
        public string UserRole { get; set; }
        public DateTime AccountCreatedOn { get; set; }
        public bool RememberMe { get; set; }
        public bool IsActive { get; set; }
        public int BadLogins { get; set; }
        public string ValidationCode { get; set; }
        public bool Validated { get; set; }
    }
}
