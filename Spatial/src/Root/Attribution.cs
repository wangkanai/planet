// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Planet.Spatial;

/// <summary>Represents an attribution with a name and a URL.</summary>
/// <param name="Name">The name of the attribution.</param>
/// <param name="Url">The URL of the attribution.</param>
public record struct Attribution(string Name = "", string Url = "");
