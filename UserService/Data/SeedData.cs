namespace UserService.Data;

using Microsoft.AspNetCore.Identity;

public static class SeedData
{
    public static async Task InitializeRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        string[] roleNames = { "ADMIN", "USER", "ORGANIZER" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Create the role if it doesn't exist
                await roleManager.CreateAsync(new IdentityRole(roleName));
                Console.WriteLine($"Role '{roleName}' created."); 
            }
        }
    }
}