using Microsoft.AspNetCore.Identity;
using MiniERP.Models;

namespace MiniERP.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Admin", "Staff" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                    throw new Exception(
                        string.Join("; ",
                            result.Errors.Select(e => e.Description)));
            }
        }

        string? email = configuration["SeedAdmin:Email"];
        string? password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var admin = await userManager.FindByEmailAsync(email);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = "系統管理員",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(
                admin, password);

            if (!result.Succeeded)
                throw new Exception(
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var result = await userManager.AddToRoleAsync(
                admin, "Admin");

            if (!result.Succeeded)
                throw new Exception(
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
        }
    }
}