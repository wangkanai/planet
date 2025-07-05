// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Microsoft.AspNetCore.Identity;

using Wangkanai.Planet.Portal.Identity;

namespace Wangkanai.Planet.Portal.Components.Account;

internal sealed class IdentityUserAccessor(UserManager<PlanetUser> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<PlanetUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
	        redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);

        return user;
    }
}
