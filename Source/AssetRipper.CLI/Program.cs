using System.ComponentModel;

using Ookii.CommandLine;
using Ookii.CommandLine.Validation;

using AssetRipper.GUI.Web;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Utils;
using AssetRipper.IO.Files.Utils;

static int Quit(string message)
{
	Logger.Error(message);
	Environment.Exit(1);
	return 1;
}

Console.WriteLine(WelcomeMessage.AsciiArt);
var argsParsed = Arguments.Parse(args);
if (argsParsed is null)
{
	return Quit("failed to parse command-line arguments");
}
if (argsParsed.Log)
{
	var logPath = argsParsed.LogPath;
	if (string.IsNullOrEmpty(logPath))
	{
		logPath = ExecutingDirectory.Combine($"AssetRipper_{DateTime.Now:yyyyMMdd_HHmmss}.log");
		WebApplicationLauncher.RotateLogs(logPath);
	}
	Logger.Add(new FileLogger(logPath));
}
Logger.LogSystemInformation("AssetRipper");
Logger.Add(new ConsoleLogger());

DirectoryInfo diSource = new(argsParsed.SourceDir);
if (!diSource.Exists)
{
	return Quit($"no such source dir \"{argsParsed.SourceDir}\"");
}

DirectoryInfo diOutput = new(argsParsed.OutputDir);
if (diOutput.Exists)
{
	if (diOutput.EnumerateFileSystemInfos().Any())
	{
		return Quit($"output dir \"{argsParsed.OutputDir}\" not empty");
	}
	// else fall through
}
else
{
	if (File.Exists(argsParsed.OutputDir))
	{
		return Quit($"output \"{argsParsed.OutputDir}\" exists but is not an empty directory");
	}
	diOutput.Create();
}

GameFileLoader.LoadAndProcess([ diSource.FullName ]);
if (argsParsed.Mode is ExtractMode.UnityProject)
{
	GameFileLoader.ExportUnityProject(diOutput.FullName);
}
else
{
	GameFileLoader.ExportPrimaryContent(diOutput.FullName);
}

return 0;

[GeneratedParser]
[ParseOptions(IsPosix = true)]
internal sealed partial class Arguments
{
	[CommandLineArgument(IsPositional = true)]
	[Description("Selects what to extract.")]
	[ValidateEnumValue]
	public required ExtractMode Mode { get; set; }

	[CommandLineArgument(IsPositional = true)]
	[Description("The path of the folder containing the game's executable.")]
	public required string SourceDir { get; set; }

	[CommandLineArgument(IsPositional = true)]
	[Description("The path to extract to. It may be an empty folder, or it may point to a nonexistent folder.")]
	public required string OutputDir { get; set; }

	[CommandLineArgument(DefaultValue = false)]
	[Description("If true, the application will log to a file.")]
	public bool Log { get; set; }

	[CommandLineArgument(DefaultValue = null)]
	[Description("The file location at which to save the log, or a sensible default if not provided.")]
	public string? LogPath { get; set; }
}

internal enum ExtractMode
{
	UnityProject = 0,
	PrimaryContent = 1,
}
