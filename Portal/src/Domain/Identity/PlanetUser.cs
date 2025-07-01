// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Microsoft.AspNetCore.Identity;

namespace Wangkanai.Planet.Portal.Data;

/// <summary>Represents a user in the system.</summary>
public class PlanetUser : IdentityUser<int>
{
	public required string      Firstname { get; set; }
	public required string      Lastname  { get; set; }
	public          DateOnly    Birthday  { get; set; }
	public          PlanetTheme Theme     { get; set; }
}
