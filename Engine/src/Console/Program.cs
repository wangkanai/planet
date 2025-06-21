using System.CommandLine;
using System.Text;

namespace Wangkanai.Planet.Engine;

public static class Program
{
	private static async Task<int> Main(string[] args)
	{
		if (System.Console.IsInputRedirected)
			System.Console.OutputEncoding = Encoding.UTF8;

		var rootCommand = new RootCommand("Wangkanai Planet Engine");

		return await rootCommand.InvokeAsync(args);
	}
}
