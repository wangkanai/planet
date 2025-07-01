// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved.Apache License, Version 2.0

using Microsoft.AspNetCore.Identity;

namespace Wangkanai.Planet.Portal.Data;

public class PlanetUser : IdentityUser<int>
{
	public string   Firstname { get; set; }
	public string   Lastname  { get; set; }
	public DateOnly Birthday  { get; set; }
	public PlanetTheme    Theme     { get; set; }
}
