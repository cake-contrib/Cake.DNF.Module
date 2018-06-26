#tool "nuget:?package=GitVersion.CommandLine"
#load "helpers.cake"
#tool nuget:?package=DocFx.Console&version=2.33.2
#addin nuget:?package=Cake.DocFx&version=0.7.0

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutionPath = File("./src/Cake.DNF.Module.sln");
var projects = GetProjects(solutionPath);
var artifacts = "./dist/";
var testResultsPath = MakeAbsolute(Directory(artifacts + "./test-results"));
GitVersion versionInfo = null;
var frameworks = new List<string> { "netstandard2.0" };

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
	versionInfo = GitVersion();
	Information("Building for version {0}", versionInfo.FullSemVer);
	Verbose("Building for " + string.Join(", ", frameworks));
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
{
	// Clean solution directories.
	foreach(var path in projects.AllProjectPaths)
	{
		Information("Cleaning {0}", path);
		CleanDirectories(path + "/**/bin/" + configuration);
		CleanDirectories(path + "/**/obj/" + configuration);
	}
	Information("Cleaning common files...");
	CleanDirectory(artifacts);
});

Task("Restore")
	.Does(() =>
{
	// Restore all NuGet packages.
	Information("Restoring solution...");
	//NuGetRestore(solutionPath);
	foreach (var project in projects.AllProjectPaths) {
		DotNetCoreRestore(project.FullPath);
	}
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
	.Does(() =>
{
	Information("Building solution...");
	foreach(var framework in frameworks) {
		foreach (var project in projects.SourceProjectPaths) {
			var settings = new DotNetCoreBuildSettings {
				Framework = framework,
				Configuration = configuration,
				NoIncremental = true,
			};
			DotNetCoreBuild(project.FullPath, settings);
		}
	}
	
});

Task("Run-Unit-Tests")
	.IsDependentOn("Build")
	.Does(() =>
{
	if (projects.TestProjects.Any()) {
		CreateDirectory(testResultsPath);

		/*var settings = new XUnit2Settings {
			NoAppDomain = true,
			XmlReport = true,
			HtmlReport = true,
			OutputDirectory = testResultsPath,
		};
		settings.ExcludeTrait("Category", "Integration"); */

		var settings = new DotNetCoreTestSettings {
			Configuration = configuration,
			//ArgumentCustomization = args => args.AppendSwitchQuoted("--logger", "trx;LogFilePath=" + testResultsPath + "tests.trx")
		};

		//XUnit2(testAssemblies, settings);
		foreach(var project in projects.TestProjects) {
			DotNetCoreTest(project.Path.FullPath, settings);
		}
	}
});

Task("Generate-Docs").Does(() => {
	DocFx("./docfx/docfx.json");
	Zip("./docfx/_site/", artifacts + "/docfx.zip");
});

Task("Post-Build")
	.IsDependentOn("Build")
	.IsDependentOn("Run-Unit-Tests")
	.IsDependentOn("Generate-Docs")
	.Does(() =>
{
	CreateDirectory(artifacts + "build");
	CreateDirectory(artifacts + "modules");
	foreach (var project in projects.SourceProjects) {
		CreateDirectory(artifacts + "build/" + project.Name);
		foreach (var framework in frameworks) {
			var frameworkDir = artifacts + "build/" + project.Name + "/" + framework;
			CreateDirectory(frameworkDir);
			var files = GetFiles(project.Path.GetDirectory() + "/bin/" + configuration + "/" + framework + "/" + project.Name +".*");
			CopyFiles(files, frameworkDir);
		}
	}
});

Task("Pack")
	.IsDependentOn("Post-Build")
	.Does(() =>
{
	CreateDirectory(artifacts + "/package");
	foreach(var project in projects.SourceProjects)
    {
        Information("\nPacking {0}...", project.Name);
        DotNetCorePack(project.Path.FullPath, new DotNetCorePackSettings 
        {
            Configuration = configuration,
            OutputDirectory = artifacts + "/package/",
            NoBuild = true,
            ArgumentCustomization = args => args
                .Append("--include-symbols --include-source")
        });
    }
});

Task("NuGet")
	.IsDependentOn("Post-Build")
	.Does(() => 
{
	CreateDirectory(artifacts + "package");
	Information("Building NuGet package");
	var versionNotes = ParseAllReleaseNotes("./ReleaseNotes.md").FirstOrDefault(v => v.Version.ToString() == versionInfo.MajorMinorPatch);
	var content = GetContent(frameworks, projects);
	var settings = new NuGetPackSettings {
		Id				= "Cake.DNF.Module",
		Version			= versionInfo.NuGetVersionV2,
		Title			= "Cake DNF Module",
		Authors		 	= new[] { "Alistair Chapman" },
		Owners			= new[] { "achapman", "cake-contrib" },
		Description		= "This Cake module adds support for the DNF package manager when installing tools in your Cake build scripts.",
		ReleaseNotes	= versionNotes != null ? versionNotes.Notes.ToList() : new List<string>(),
		Summary			= "Adds DNF support to Cake builds.",
		ProjectUrl		= new Uri("https://github.com/cake-contrib/Cake.DNF.Module"),
		IconUrl			= new Uri("https://cdn.rawgit.com/cake-contrib/graphics/a5cf0f881c390650144b2243ae551d5b9f836196/png/cake-contrib-medium.png"),
		LicenseUrl		= new Uri("https://raw.githubusercontent.com/agc93/Cake.DNF.Module/master/LICENSE"),
		Copyright		= "Alistair Chapman 2017",
		Tags			= new[] { "cake", "build", "ci", "build", "dnf", "linux" },
		OutputDirectory = artifacts + "/package",
		Files			= content,
		//KeepTemporaryNuSpecFile = true
	};

	NuGetPack(settings);
});

Task("Default")
	.IsDependentOn("Post-Build");

RunTarget(target);
