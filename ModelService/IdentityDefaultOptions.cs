using System;
using System.Collections.Generic;
using System.Text;

namespace ModelService
{
    public class IdentityDefaultOptions
    {
        public bool PasswordRequireDigit { get; set; }
        public int PasswordRequiredLength { get; set; }
        public bool PasswordRequireNonAphanumeric { get; set; }
        public bool PasswordRequireUppercase { get; set; }
        public bool PasswordRequireLowercase { get; set; }
        public int PasswordRequiredUniqueChars { get; set; }

        public double LockoutDefaultLockoutTimeSpanInMinutes { get; set; }
        public int LockoutMaxFailedAccessAttempts { get; set; }
        public bool LockoutAllowedForNewUsers { get; set; }

        public bool UserRequireUniqueEmail { get; set; }
        public bool SignInRequireConfirmedEmail { get; set; }
        public string AccessDeniedPath { get; set; }
    }
}
