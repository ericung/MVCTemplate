using Microsoft.AspNetCore.Identity;

namespace MVCTemplate.Web.Data
{
    public static class UserInitializer
    {
        public static async Task Seed(
            RoleManager<IdentityRole> roleManager, 
            UserManager<IdentityUser> userManager, 
            string adminPassword)
        {
            foreach (var role in Enum.GetNames(typeof(Roles)))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (await userManager.FindByIdAsync("admin") == null)
            {
                var result = await userManager.CreateAsync(new IdentityUser
                {
                    Id = "admin",
                    Email = "admin@admin.com",
                    UserName = "admin@admin.com"
                },
                adminPassword);

                if (result.Succeeded)
                {
                    var user = userManager.FindByIdAsync("admin").Result;
                    var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmed = await userManager.ConfirmEmailAsync(user, confirmationToken);

                    if (confirmed.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
                    }
                }
            }
        }
    }
}
