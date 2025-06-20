using Microsoft.AspNetCore.Identity;
using Wangkanai.Planet.Portal.Data;

namespace Wangkanai.Planet.Portal.Components.Account;

internal sealed class IdentityUserAccessor(UserManager<PlanetUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<PlanetUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }
}
