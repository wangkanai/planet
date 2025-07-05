// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.Diagnostics.CodeAnalysis;

namespace Wangkanai.Spatial;

/// <summary>Represents a tile index with column, row, and level information.</summary>
/// <param name="col">The column index.</param>
/// <param name="row">The row index.</param>
/// <param name="level">The level index.</param>
public readonly struct TileIndex(int col, int row, int level) : IComparable
{
	public int Col   { get; } = col;
	public int Row   { get; } = row;
	public int Level { get; } = level;

	/// <summary>Compares the current TileIndex with another TileIndex.</summary>
	/// <param name="obj">The TileIndex to compare with.</param>
	/// <returns>A value that indicates the relative order of the objects being compared.</returns>
	/// <exception cref="ArgumentException">Thrown when the object to compare is not a TileIndex.</exception>
	public int CompareTo(object? obj)
	{
		if (obj is not TileIndex index)
			throw new ArgumentException("Object of type TileIndex was expected", nameof(obj));
		return CompareTo(index);
	}

	/// <summary>Compares the current TileIndex with another TileIndex.</summary>
	/// <param name="index">The TileIndex to compare with.</param>
	/// <returns>A value that indicates the relative order of the objects being compared.</returns>
	public int CompareTo(TileIndex index)
	{
		if (Col < index.Col) return -1;
		if (Col > index.Col) return 1;
		if (Row < index.Row) return -1;
		if (Row > index.Row) return 1;
		if (Level < index.Level) return -1;
		if (Level > index.Level) return 1;
		return 0;
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="obj">The object to compare with the current object.</param>
	/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is not TileIndex index)
			return false;
		return base.Equals(obj);
	}

	/// <summary>Determines whether the specified object is equal to the current object.</summary>
	/// <param name="index">TileIndex instance to compare with</param>
	/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
	public bool Equals(TileIndex index)
		=> Col == index.Col && Row == index.Row && Level == index.Level;

	/// <summary>Serves as a hash function for the TileIndex.</summary>
	/// <returns>A hash code for the TileIndex.</returns>
	public override int GetHashCode()
		=> Col ^ Row ^ Level;

	/// <summary>Determines if two TileIndex objects are equal.</summary>
	/// <param name="left">The first TileIndex to compare.</param>
	/// <param name="right">The second TileIndex to compare.</param>
	/// <returns>true if the TileIndex objects are equal; otherwise, false.</returns>
	public static bool operator ==(TileIndex left, TileIndex right)
		=> Equals(left, right);

	/// <summary>Determines if two TileIndex objects are not equal.</summary>
	/// <param name="left">The first TileIndex to compare.</param>
	/// <param name="right">The second TileIndex to compare.</param>
	/// <returns>true if the TileIndex objects are not equal; otherwise, false.</returns>
	public static bool operator !=(TileIndex left, TileIndex right)
		=> !(left == right);

	/// <summary>Determines if one TileIndex object is less than another.</summary>
	/// <param name="left">The first TileIndex to compare.</param>
	/// <param name="right">The second TileIndex to compare.</param>
	/// <returns>true if the first TileIndex is less than the second; otherwise, false.</returns>
	public static bool operator <(TileIndex left, TileIndex right)
		=> left.CompareTo(right) < 0;

	/// <summary>Determines if one TileIndex is greater than another.</summary>
	/// <param name="left">The first TileIndex to compare.</param>
	/// <param name="right">The second TileIndex to compare.</param>
	/// <returns>true if the left TileIndex is greater than the right TileIndex; otherwise, false.</returns>
	public static bool operator >(TileIndex left, TileIndex right)
		=> left.CompareTo(right) > 0;
}
