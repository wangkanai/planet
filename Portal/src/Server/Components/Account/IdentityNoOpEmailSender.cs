// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

using Wangkanai.Planet.Portal.Identity;

namespace Wangkanai.Planet.Portal.Components.Account;

// Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
internal sealed class IdentityNoOpEmailSender : IEmailSender<PlanetUser>
{
	private readonly IEmailSender emailSender = new NoOpEmailSender();

	public Task SendConfirmationLinkAsync(PlanetUser user, string email, string confirmationLink) =>
		emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

	public Task SendPasswordResetLinkAsync(PlanetUser user, string email, string resetLink) =>
		emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

	public Task SendPasswordResetCodeAsync(PlanetUser user, string email, string resetCode) =>
		emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
}
