// Copyright (c) 2014-2025 Sarin Na Wangkanai, All Rights Reserved. Apache License, Version 2.0

using System.CommandLine;
using System.Text;

namespace Wangkanai.Planet.Engine;

public static class Program
{
	private static async Task<int> Main(string[] args)
	{
		if (Console.IsInputRedirected)
			Console.OutputEncoding = Encoding.UTF8;

		var rootCommand = new RootCommand("Wangkanai Planet Engine");

		return await rootCommand.InvokeAsync(args);
	}
}
