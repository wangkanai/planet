// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Wangkanai.Planet.Portal.Data;

public class PlanetDbContext(DbContextOptions<PlanetDbContext> options)
	: IdentityDbContext<PlanetUser, PlanetRole, int>(options), IDataProtectionKeyContext
{
	public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder builder)
	{
		//builder.ApplyConfigurationsFromAssembly(typeof(PlanetConstants).Assembly);
		base.OnModelCreating(builder);
	}
}
