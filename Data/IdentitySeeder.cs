using Auth.Api.Models;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace Auth.Api.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string roleName = "Admin";
            string adminEmail = "admin@example.com";
            string adminPassword = "Admin@123";
            string adminUserName = "09177870290";

            // 1. ایجاد نقش admin اگر وجود ندارد
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // 2. ایجاد کاربر پیش‌فرض اگر وجود ندارد
            var existingUser = await userManager.FindByNameAsync(adminUserName);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminUserName,
                    NormalizedUserName = adminUserName,
                    Email = adminEmail,
                    NormalizedEmail = adminEmail.ToUpper(),
                    PhoneNumberConfirmed = true,
                    EmailConfirmed = true,
                    PhoneNumber = adminUserName,
                    Name = "Admin",
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(user, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
                else
                {
                    throw new Exception("خطا در ساخت کاربر پیش‌فرض: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

}
