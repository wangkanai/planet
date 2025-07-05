// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wangkanai.Planet.Portal.Identity;

public class UserConfiguration : IEntityTypeConfiguration<PlanetUser>
{
	public void Configure(EntityTypeBuilder<PlanetUser> builder)
	{
		builder.Property(x => x.Firstname)
		       .HasMaxLength(50)
		       .IsUnicode()
		       .IsRequired();

		builder.Property(x => x.Lastname)
		       .HasMaxLength(50)
		       .IsUnicode()
		       .IsRequired();

		builder.HasData(UserSeed.Users);
	}
}
