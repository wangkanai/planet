// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

namespace Wangkanai.Graphics;

/// <summary>
/// Base interface for all metadata implementations in the Graphics library.
/// Provides a common contract for resource cleanup through disposable patterns.
/// </summary>
public interface IMetadata : IDisposable, IAsyncDisposable { }
