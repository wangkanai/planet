// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors.Svgs;

/// <summary>Represents an SVG viewBox with x, y, width, and height values.</summary>
public readonly struct SvgViewBox : IEquatable<SvgViewBox>
{
	/// <summary>Gets the x-coordinate of the viewBox.</summary>
	public double X { get; }

	/// <summary>Gets the y-coordinate of the viewBox.</summary>
	public double Y { get; }

	/// <summary>Gets the width of the viewBox.</summary>
	public double Width { get; }

	/// <summary>Gets the height of the viewBox.</summary>
	public double Height { get; }

	/// <summary>Initializes a new instance of the SvgViewBox struct.</summary>
	public SvgViewBox(double x, double y, double width, double height)
	{
		X      = x;
		Y      = y;
		Width  = width;
		Height = height;
	}

	/// <summary>Gets the default viewBox (0, 0, 100, 100).</summary>
	public static SvgViewBox Default
		=> new(0, 0, 100, 100);

	/// <summary>Gets the aspect ratio of the viewBox.</summary>
	public double AspectRatio
		=> Height != 0 ? Width / Height : 0;

	/// <inheritdoc />
	public bool Equals(SvgViewBox other)
		=> X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);

	/// <inheritdoc />
	public override bool Equals(object? obj)
		=> obj is SvgViewBox other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode()
		=> HashCode.Combine(X, Y, Width, Height);

	/// <inheritdoc />
	public override string ToString()
		=> $"{X} {Y} {Width} {Height}";

	/// <summary>Parses a viewBox string into an SvgViewBox.</summary>
	public static SvgViewBox Parse(string viewBoxString)
	{
		var parts = viewBoxString.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length != 4)
			throw new FormatException("ViewBox must contain exactly 4 values");

		return new SvgViewBox(
			double.Parse(parts[0]),
			double.Parse(parts[1]),
			double.Parse(parts[2]),
			double.Parse(parts[3])
		);
	}

	public static bool operator ==(SvgViewBox left, SvgViewBox right) => left.Equals(right);
	public static bool operator !=(SvgViewBox left, SvgViewBox right) => !left.Equals(right);
}

/// <summary>Defines SVG color space options.</summary>
public enum SvgColorSpace
{
	/// <summary>sRGB color space (default).</summary>
	sRGB,

	/// <summary>Linear RGB color space.</summary>
	LinearRGB,

	/// <summary>Display P3 color space.</summary>
	DisplayP3,

	/// <summary>Rec2020 color space.</summary>
	Rec2020,

	/// <summary>Custom color space defined by profile.</summary>
	Custom
}

/// <summary>Defines SVG version specifications.</summary>
public enum SvgVersion
{
	/// <summary>SVG 1.0 specification.</summary>
	V1_0,

	/// <summary>SVG 1.1 specification (most common).</summary>
	V1_1,

	/// <summary>SVG 2.0 specification (modern features).</summary>
	V2_0
}

/// <summary>Defines SVG element types for optimization.</summary>
public enum SvgElementType
{
	/// <summary>Root SVG element.</summary>
	Svg,

	/// <summary>Group element (g).</summary>
	Group,

	/// <summary>Path element.</summary>
	Path,

	/// <summary>Rectangle element.</summary>
	Rectangle,

	/// <summary>Circle element.</summary>
	Circle,

	/// <summary>Ellipse element.</summary>
	Ellipse,

	/// <summary>Line element.</summary>
	Line,

	/// <summary>Polyline element.</summary>
	Polyline,

	/// <summary>Polygon element.</summary>
	Polygon,

	/// <summary>Text element.</summary>
	Text,

	/// <summary>Image element.</summary>
	Image,

	/// <summary>Use element (references).</summary>
	Use,

	/// <summary>Definitions element (defs).</summary>
	Definitions,

	/// <summary>Symbol element.</summary>
	Symbol,

	/// <summary>Marker element.</summary>
	Marker,

	/// <summary>Gradient element.</summary>
	Gradient,

	/// <summary>Pattern element.</summary>
	Pattern,

	/// <summary>Filter element.</summary>
	Filter,

	/// <summary>Animation element.</summary>
	Animation,

	/// <summary>Metadata element.</summary>
	Metadata,

	/// <summary>Other/unknown element type.</summary>
	Other
}

/// <summary>Defines SVG coordinate system types for geospatial integration.</summary>
public enum SvgCoordinateSystem
{
	/// <summary>Standard SVG coordinate system (top-left origin, y increases downward).</summary>
	Standard,

	/// <summary>Cartesian coordinate system (bottom-left origin, y increases upward).</summary>
	Cartesian,

	/// <summary>Geographic coordinate system (latitude/longitude).</summary>
	Geographic,

	/// <summary>Web Mercator projection (EPSG:3857).</summary>
	WebMercator,

	/// <summary>Universal Transverse Mercator (UTM).</summary>
	UTM,

	/// <summary>Custom coordinate system defined by CRS.</summary>
	Custom
}
