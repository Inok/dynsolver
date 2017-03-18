#tool "nuget:https://dotnet.myget.org/F/nuget-build/?package=NuGet.CommandLine"
#tool "nuget:?package=NUnit.ConsoleRunner"

//---------------------------------
//--- Configuration ---------------
//---------------------------------

//-- arguments
var target = Argument("target", "Default");
var platform = Argument("platform", "Any CPU");
var configuration = Argument("configuration", "Release");

var solutionFilePath = "./DynamicSolver.sln";

var buildFilesDirs = GetDirectories("./DynamicSolver.*/bin/" + configuration)
                   + GetDirectories("./DynamicSolver.*/obj/" + configuration);

var testAssemblies = GetFiles("./DynamicSolver.*/bin/" + configuration + "/*.Tests.dll");

var applicationArtifactsFiles = GetFiles("./DynamicSolver.GUI/bin/" + configuration + "/*.dll")
                              + GetFiles("./DynamicSolver.GUI/bin/" + configuration + "/*.exe");

var artifactsDir = Directory("./artifacts");
var artifactsSolverDir = Directory(artifactsDir.Path + "/solver");
var artifactsSolverZip = File(artifactsDir.Path + "/solver.zip");



//---------------------------------
//--- Setup -----------------------
//---------------------------------

Setup(context =>
{
    Information("Building with configuration {0}|{1}", configuration, platform);
});


//---------------------------------
//--- Tasks -----------------------
//---------------------------------

Task("Clean-Build").Does(() =>
{
    CleanDirectories(buildFilesDirs);
});


Task("Clean-Artifacts").Does(() =>
{
    CleanDirectory(artifactsDir);
});


Task("Restore-NuGet-Packages").Does(() =>
{
    NuGetRestore(solutionFilePath, new NuGetRestoreSettings() {
        ToolPath = "./tools/NuGet.CommandLine/tools/NuGet.exe",
        ToolTimeout = TimeSpan.FromMinutes(1)
     });
});


Task("Build")
    .IsDependentOn("Clean-Build")
    .IsDependentOn("Clean-Artifacts")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(solutionFilePath, settings => {
        settings.SetConfiguration(configuration);
        settings.WithProperty("Platform", "\"" + platform + "\"");
        settings.SetVerbosity(Verbosity.Minimal);
        settings.UseToolVersion(MSBuildToolVersion.VS2017);
        settings.SetNodeReuse(false);
    });
});

Task("Run-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3(testAssemblies, new NUnit3Settings() {
        NoHeader = true,
        NoResults = true
    });
});


Task("Copy-App-Artifacts")
    .IsDependentOn("Build")
    .IsDependentOn("Run-Tests")
    .IsDependentOn("Clean-Artifacts")
    .Does(() =>
{
    CreateDirectory(artifactsSolverDir);
    CopyFiles(applicationArtifactsFiles, artifactsSolverDir);
});


Task("Pack-Zip-Artifacts")
    .IsDependentOn("Copy-App-Artifacts")
    .Does(() =>
{
    Information("Pack directory " + artifactsSolverDir + " to " + artifactsSolverZip);
    Zip(artifactsSolverDir, artifactsSolverZip);
});


//---------------------------------
//--- Targets ---------------------
//---------------------------------

Task("Pack-Artifacts")
    .IsDependentOn("Pack-Zip-Artifacts")
    .Does(() => { });


Task("AppVeyor")
  .IsDependentOn("Pack-Artifacts")
  .Does(() => { });


Task("Default")
  .IsDependentOn("Pack-Artifacts")
  .Does(() => { });


RunTarget(target);