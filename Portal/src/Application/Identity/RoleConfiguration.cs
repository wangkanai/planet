// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wangkanai.Planet.Portal.Identity;

public class RoleConfiguration : IEntityTypeConfiguration<PlanetRole> {
	public void Configure(EntityTypeBuilder<PlanetRole> builder)
	{
		builder.HasData(RoleSeed.Roles);
	}
}
