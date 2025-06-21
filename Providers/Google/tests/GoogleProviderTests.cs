namespace Wangkanai.Planet.Providers.Google.Tests;

public class GoogleProviderTests
{
	[Fact]
	public void GoogleProvider_GetTileUrl_ReturnsCorrectUrl()
	{
		// Arrange
		var x        = 1;
		var y        = 2;
		var z        = 3;
		var expected = "https://mt1.google.com/vt/lyrs=s&x=1&y=2&z=3";

		// Act
		var actual = GoogleProvider.GetTileUrl(x, y, z);

		// Assert
		Assert.Equal(expected, actual);
	}
}
