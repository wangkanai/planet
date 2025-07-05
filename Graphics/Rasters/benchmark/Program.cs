// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using BenchmarkDotNet.Running;
using Wangkanai.Graphics.Rasters.Benchmark;

// Check if we should run the simple demo or full benchmarks
if (args.Length > 0 && args[0] == "--demo")
{
	PerformanceDemo.RunDemo();
}
else if (args.Length > 0 && args[0] == "--realistic")
{
	RealisticPerformanceDemo.RunDemo();
}
else
{
	BenchmarkRunner.Run<TiffRasterBenchmark>();
}

