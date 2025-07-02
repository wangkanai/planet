// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Providers;

public class RemoteProviderTests
{
	[Fact]
	public void RemoteProviders_ShouldHaveCorrectNumberOfValues()
	{
		// Arrange & Act
		var values = Enum.GetValues<RemoteProviders>();

		// Assert
		Assert.Equal(8, values.Length);
	}

	[Fact]
	public void RemoteProviders_ShouldContainAllExpectedValues()
	{
		// Arrange
		var expectedValues = new[]
		{
			RemoteProviders.Google,
			RemoteProviders.Bing,
			RemoteProviders.MapBox,
			RemoteProviders.OpenStreetMap,
			RemoteProviders.OpenTopoMap,
			RemoteProviders.StamenTerrain,
			RemoteProviders.StamenToner,
			RemoteProviders.StamenWatercolor
		};

		// Act
		var actualValues = Enum.GetValues<RemoteProviders>();

		// Assert
		Assert.Equal(expectedValues.Length, actualValues.Length);
		foreach (var expectedValue in expectedValues)
		{
			Assert.Contains(expectedValue, actualValues);
		}
	}

	[Theory]
	[InlineData(RemoteProviders.Google, "Google")]
	[InlineData(RemoteProviders.Bing, "Bing")]
	[InlineData(RemoteProviders.MapBox, "MapBox")]
	[InlineData(RemoteProviders.OpenStreetMap, "OpenStreetMap")]
	[InlineData(RemoteProviders.OpenTopoMap, "OpenTopoMap")]
	[InlineData(RemoteProviders.StamenTerrain, "StamenTerrain")]
	[InlineData(RemoteProviders.StamenToner, "StamenToner")]
	[InlineData(RemoteProviders.StamenWatercolor, "StamenWatercolor")]
	public void RemoteProviders_ToString_ShouldReturnCorrectName(RemoteProviders provider, string expectedName)
	{
		// Act
		var actualName = provider.ToString();

		// Assert
		Assert.Equal(expectedName, actualName);
	}

	[Theory]
	[InlineData("Google", RemoteProviders.Google)]
	[InlineData("Bing", RemoteProviders.Bing)]
	[InlineData("MapBox", RemoteProviders.MapBox)]
	[InlineData("OpenStreetMap", RemoteProviders.OpenStreetMap)]
	[InlineData("OpenTopoMap", RemoteProviders.OpenTopoMap)]
	[InlineData("StamenTerrain", RemoteProviders.StamenTerrain)]
	[InlineData("StamenToner", RemoteProviders.StamenToner)]
	[InlineData("StamenWatercolor", RemoteProviders.StamenWatercolor)]
	public void RemoteProviders_Parse_ShouldReturnCorrectValue(string name, RemoteProviders expectedProvider)
	{
		// Act
		var actualProvider = Enum.Parse<RemoteProviders>(name);

		// Assert
		Assert.Equal(expectedProvider, actualProvider);
	}

	[Theory]
	[InlineData("google", RemoteProviders.Google)]
	[InlineData("BING", RemoteProviders.Bing)]
	[InlineData("mapbox", RemoteProviders.MapBox)]
	public void RemoteProviders_Parse_IgnoreCase_ShouldReturnCorrectValue(string name, RemoteProviders expectedProvider)
	{
		// Act
		var actualProvider = Enum.Parse<RemoteProviders>(name, ignoreCase: true);

		// Assert
		Assert.Equal(expectedProvider, actualProvider);
	}

	[Fact]
	public void RemoteProviders_Parse_InvalidName_ShouldThrowArgumentException()
	{
		// Arrange
		const string invalidName = "InvalidProvider";

		// Act & Assert
		Assert.Throws<ArgumentException>(() => Enum.Parse<RemoteProviders>(invalidName));
	}

	[Theory]
	[InlineData("Google", true, RemoteProviders.Google)]
	[InlineData("InvalidProvider", false, default(RemoteProviders))]
	[InlineData("", false, default(RemoteProviders))]
	public void RemoteProviders_TryParse_ShouldReturnExpectedResult(string name, bool expectedSuccess, RemoteProviders expectedProvider)
	{
		// Act
		var success = Enum.TryParse<RemoteProviders>(name, out var actualProvider);

		// Assert
		Assert.Equal(expectedSuccess, success);
		Assert.Equal(expectedProvider, actualProvider);
	}

	[Fact]
	public void RemoteProviders_IsDefined_ShouldReturnTrueForValidValues()
	{
		// Arrange
		var validValues = Enum.GetValues<RemoteProviders>();

		// Act & Assert
		foreach (var value in validValues)
		{
			Assert.True(Enum.IsDefined(value));
		}
	}

	[Fact]
	public void RemoteProviders_IsDefined_ShouldReturnFalseForInvalidValue()
	{
		// Arrange
		const RemoteProviders invalidValue = (RemoteProviders)999;

		// Act
		var isDefined = Enum.IsDefined(invalidValue);

		// Assert
		Assert.False(isDefined);
	}

	[Fact]
	public void RemoteProviders_GetNames_ShouldReturnAllNames()
	{
		// Arrange
		var expectedNames = new[]
		{
			"Google", "Bing", "MapBox", "OpenStreetMap",
			"OpenTopoMap", "StamenTerrain", "StamenToner", "StamenWatercolor"
		};

		// Act
		var actualNames = Enum.GetNames<RemoteProviders>();

		// Assert
		Assert.Equal(expectedNames.Length, actualNames.Length);
		foreach (var expectedName in expectedNames)
		{
			Assert.Contains(expectedName, actualNames);
		}
	}

	[Theory]
	[InlineData(RemoteProviders.Google, 0)]
	[InlineData(RemoteProviders.Bing, 1)]
	[InlineData(RemoteProviders.MapBox, 2)]
	[InlineData(RemoteProviders.OpenStreetMap, 3)]
	[InlineData(RemoteProviders.OpenTopoMap, 4)]
	[InlineData(RemoteProviders.StamenTerrain, 5)]
	[InlineData(RemoteProviders.StamenToner, 6)]
	[InlineData(RemoteProviders.StamenWatercolor, 7)]
	public void RemoteProviders_ShouldHaveCorrectUnderlyingValues(RemoteProviders provider, int expectedValue)
	{
		// Act
		var actualValue = (int)provider;

		// Assert
		Assert.Equal(expectedValue, actualValue);
	}
}
