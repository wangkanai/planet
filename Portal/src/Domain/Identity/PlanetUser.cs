// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Microsoft.AspNetCore.Identity;

namespace Wangkanai.Planet.Portal.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class PlanetUser : IdentityUser
{
	public string Firstname { get; set; }
	public string Lastname  { get; set; }
}
