using System;
using System.Collections.Generic;

namespace ModelService
{
    public class ProfileModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public string Birthday { get; set; }
        public string Gender { get; set; }
        public string Displayname { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string UserRole { get; set; }
        public string Lastname { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsTermsAccepted { get; set; }
        public string ProfilePic { get; set; }
        public bool IsAccountLocked { get; set; }
        public ICollection<ActivityModel> Activities { get; set; }

        public ProfileModel() { }

        public ProfileModel(ApplicationUser appUser, List<ActivityModel> activities)
        {
            UserId = appUser.Id;
            Email = appUser.Email;
            Username = appUser.UserName;
            Phone = appUser.PhoneNumber ?? "Phone Number was not added by user.";
            Birthday = appUser.Birthday;
            Gender = appUser.Gender;
            Displayname = appUser.DisplayName;
            Firstname = appUser.FirstName;
            Middlename = appUser.MiddleName;
            Lastname = appUser.LastName;
            UserRole = appUser.UserRole;
            IsEmailVerified = appUser.EmailConfirmed;
            IsTermsAccepted = appUser.Terms;
            ProfilePic = appUser.ProfilePic ?? "Profile pic was not added by user.";
            IsAccountLocked = false;//appUser.LockoutEnabled;
            Activities = activities;
        }
    }
}
