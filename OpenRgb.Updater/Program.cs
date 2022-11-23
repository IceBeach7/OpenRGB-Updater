using System.Diagnostics;
using System.IO.Compression;
using System.Net;

namespace OpenRgb.Updater;

internal static class Program
{
	static void Main(string[] args)
	{
		Console.Title = "OpenRGB Updater";


		var filePathZip = Path.Combine(Path.GetTempPath(), "OpenRGBTemp.zip");
		var dirExtracted = Path.Combine(Path.GetTempPath(), "OpenRGB");
		var dirInstallOpenRgb = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "OpenRGB");
		var dirInstallPlugin = Path.Combine(Environment.GetEnvironmentVariable("APPDATA"), @"OpenRGB\plugins");

		WebClient webClient = new();


		if(File.Exists(filePathZip))
		{
			File.Delete(filePathZip);
		}

		if(Directory.Exists(dirExtracted))
		{
			Directory.Delete(dirExtracted, true);
		}


		Console.WriteLine("Downloading newest artifact of OpenRGB...");
		webClient.DownloadFile("https://gitlab.com/CalcProgrammer1/OpenRGB/-/jobs/artifacts/master/download?job=Windows%2064", filePathZip);

		Console.WriteLine("Extracting...");
		ZipFile.ExtractToDirectory(filePathZip, dirExtracted);
		File.Delete(filePathZip);

		Console.WriteLine("Ending all OpenRGB processes...");
		KillProcesses();
		Thread.Sleep(2000);

		Console.WriteLine("Replacing directory...");
		if(Directory.Exists(dirInstallOpenRgb))
		{
			Directory.Delete(dirInstallOpenRgb, true);
		}

		Directory.Move(Path.Combine(dirExtracted, "OpenRGB Windows 64-bit"), dirInstallOpenRgb);
		Directory.Delete(dirExtracted, true);


		Console.WriteLine("Downloading newest artifact of the effects plugin...");
		webClient.DownloadFile("https://gitlab.com/OpenRGBDevelopers/OpenRGBEffectsPlugin/-/jobs/artifacts/master/download?job=Windows%2064", filePathZip);

		Console.WriteLine("Extracting...");
		ZipFile.ExtractToDirectory(filePathZip, dirInstallPlugin, true);


		Console.WriteLine("Executing OpenRGB...");
		Process.Start("cmd.exe", $@"/C START """" ""{Path.Combine(dirInstallOpenRgb, "OpenRGB.exe")}""");


		Console.Write("Done.");
		Thread.Sleep(2000);
		Environment.Exit(0);
	}

	static void KillProcesses()
	{
		var processes = Process.GetProcessesByName("OpenRGB");

		foreach(var process in processes)
		{
			process.Kill();
		}
	}
}