using System.Diagnostics;
using System.IO.Compression;
using System.Net;

namespace OpenRgb.Updater;

internal static class Program
{
	static void Main(string[] args)
	{
		Console.Title = "OpenRGB Updater";

		if(!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
		{
			throw new PlatformNotSupportedException();
		}


		DirectoryInfo tempDir;
		DirectoryInfo localAppDataDir;
		DirectoryInfo appDataDir;

		if(OperatingSystem.IsWindows())
		{
			tempDir = new(Environment.GetEnvironmentVariable("TEMP"));
			localAppDataDir = new(Environment.GetEnvironmentVariable("LOCALAPPDATA"));
			appDataDir = new(Environment.GetEnvironmentVariable("APPDATA"));
		}
		else
		{
			tempDir = new(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
			localAppDataDir = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".applications"));
			appDataDir = new(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
		}


		FileInfo artifactFile = new(Path.Combine(tempDir.FullName, "OpenRGB-Updater.zip"));
		DirectoryInfo extractDir = new(Path.Combine(tempDir.FullName, "OpenRGB"));
		DirectoryInfo installDir = new(Path.Combine(localAppDataDir.FullName, "OpenRGB"));
		DirectoryInfo pluginDir = new(Path.Combine(appDataDir.FullName, "OpenRGB/plugins"));

		WebClient webClient = new();


		var openRgbUrl = "https://gitlab.com/CalcProgrammer1/OpenRGB/-/jobs/artifacts/master/download?job=";
		var pluginUrl = "https://gitlab.com/OpenRGBDevelopers/OpenRGBEffectsPlugin/-/jobs/artifacts/master/download?job=";

		if(OperatingSystem.IsWindows())
		{
			openRgbUrl += "Windows%2064";
			pluginUrl += "Windows%2064";
		}
		else if(OperatingSystem.IsLinux())
		{
			openRgbUrl += "Linux+64+AppImage";
			pluginUrl += "Linux%2064";
		}


		Console.WriteLine("Downloading newest artifact of OpenRGB...");

		if(artifactFile.Exists)
		{
			artifactFile.Delete();
		}

		webClient.DownloadFile(openRgbUrl, artifactFile.FullName);


		Console.WriteLine("Extracting...");

		if(extractDir.Exists)
		{
			extractDir.Delete(true);
		}

		extractDir.Create();
		ZipFile.ExtractToDirectory(artifactFile.FullName, extractDir.FullName);
		artifactFile.Delete();


		Console.WriteLine("Ending all OpenRGB processes...");
		KillProcesses();
		Thread.Sleep(2000);


		Console.WriteLine("Replacing directory...");

		if(installDir.Exists)
		{
			installDir.Delete(true);
			installDir.Create();
		}

		if(OperatingSystem.IsWindows())
		{
			Directory.Move(Path.Combine(extractDir.FullName, "OpenRGB Windows 64-bit"), installDir.FullName);
		}
		else
		{
			const string fileName = "OpenRGB-x86_64.AppImage";
			File.Move(Path.Combine(extractDir.FullName, fileName), Path.Combine(installDir.FullName, fileName));
		}

		extractDir.Delete(true);


		Console.WriteLine("Downloading newest artifact of the effects plugin...");
		webClient.DownloadFile(pluginUrl, artifactFile.FullName);


		Console.WriteLine("Extracting...");
		ZipFile.ExtractToDirectory(artifactFile.FullName, pluginDir.FullName, true);


		Console.WriteLine("Executing OpenRGB...");

		if(OperatingSystem.IsWindows())
		{
			Process.Start("cmd.exe", $@"/C START """" ""{Path.Combine(installDir.FullName, "OpenRGB.exe")}""");
		}
		else
		{
			FileInfo openRgbFile = new(Path.Combine(installDir.FullName, "OpenRGB-x86_64.AppImage"));

			Process.Start("chmod", $@"+x ""{openRgbFile.FullName}""");
			Process.Start(openRgbFile.FullName);
		}


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