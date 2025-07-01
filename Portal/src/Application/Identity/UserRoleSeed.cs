// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Microsoft.AspNetCore.Identity;

namespace Wangkanai.Planet.Portal.Identity;

internal static class UserRoleSeed
{
	internal static List<IdentityUserRole<int>> UserRoles =>
	[
		new() { UserId = 1, RoleId = 1 },// Sarin
		new() { UserId = 2, RoleId = 1 },// Admin
		new() { UserId = 3, RoleId = 2 },// Moderator
		new() { UserId = 4, RoleId = 3 },// Editor
		new() { UserId = 5, RoleId = 4 },// Contributor
		new() { UserId = 6, RoleId = 5 },// User
	];
}
