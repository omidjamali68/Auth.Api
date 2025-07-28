﻿using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? NationalCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
