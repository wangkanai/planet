// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics.Vectors.Svgs;

/// <summary>Defines the contract for SVG vector graphics.</summary>
public interface ISvgVector : IVector
{
	/// <summary>Gets the SVG metadata.</summary>
	new ISvgMetadata Metadata { get; }

	/// <summary>Gets whether the SVG is compressed.</summary>
	bool IsCompressed { get; }

	/// <summary>Gets the SVG content as XML string.</summary>
	string ToXmlString();

	/// <summary>Saves the SVG to a file.</summary>
	Task SaveToFileAsync(string filePath, bool compressed = false);

	/// <summary>Loads SVG content from a file.</summary>
	Task LoadFromFileAsync(string filePath);

	/// <summary>Optimizes the SVG for performance.</summary>
	void Optimize();

	/// <summary>Validates the SVG document.</summary>
	bool ValidateDocument();
}
