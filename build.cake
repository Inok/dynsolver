#tool "nuget:https://dotnet.myget.org/F/nuget-build/?package=NuGet.CommandLine"
#tool "nuget:?package=NUnit.ConsoleRunner"
#addin "nuget:?package=Cake.Git"
#addin "nuget:?package=Polly&version=4.2.0"

using Polly;


//---------------------------------
//--- Configuration ---------------
//---------------------------------

var target = Argument("target", "Default");
var platform = Argument("platform", "Any CPU");
var configuration = Argument("configuration", "Release");

var rootDir = Directory(".");

var solutionFilePath = "./DynamicSolver.sln";
var solutionInfoFile = "./SolutionInfo.cs";

var artifactsDir = Directory("./artifacts");
var artifactsSolverDir = Directory(artifactsDir.Path + "/solver");


//---------------------------------
//--- Version ---------------------
//---------------------------------

var solutionInfo = ParseAssemblyInfo(solutionInfoFile);
var semanticVersion = solutionInfo.AssemblyVersion;

if(BuildSystem.AppVeyor.IsRunningOnAppVeyor)
{
    semanticVersion += "-" + BuildSystem.AppVeyor.Environment.Repository.Branch + "-build" + EnvironmentVariable("APPVEYOR_BUILD_NUMBER");
}
else
{
    var branch = GitBranchCurrent(rootDir).FriendlyName.Replace("feature/", "");
    semanticVersion += "-" + branch;
}



//---------------------------------
//--- Setup -----------------------
//---------------------------------

Setup(context =>
{
    Information("Building with configuration {0}|{1}", configuration, platform);
    Information("Current application version: " + semanticVersion);
    if(BuildSystem.AppVeyor.IsRunningOnAppVeyor)
    {
        Information("Setting AppVeyor build version...");
        BuildSystem.AppVeyor.UpdateBuildVersion(semanticVersion);
    }
});



//---------------------------------
//--- Tasks -----------------------
//---------------------------------

Task("Clean-Build").Does(() =>
{
    var buildFilesDirs = GetDirectories("./DynamicSolver.*/bin/" + configuration)
                       + GetDirectories("./DynamicSolver.*/obj/" + configuration);

    CleanDirectories(buildFilesDirs);
});


Task("Clean-Artifacts").Does(() =>
{
    CleanDirectory(artifactsDir);
});


Task("Restore-NuGet-Packages").Does(() =>
{
    var maxRetryCount = 3;
    var timeout = 1d;
    Policy
        .Handle<Exception>()
        .Retry(maxRetryCount, (exception, retryCount, context) => {
            if (retryCount == maxRetryCount)
            {
                throw exception;
            }
            else
            {
                Information(exception.Message);
                timeout += 1;
            }})
        .Execute(()=> {
                NuGetRestore(solutionFilePath, new NuGetRestoreSettings {
                    ToolPath = "./tools/NuGet.CommandLine/tools/NuGet.exe",
                    ToolTimeout = TimeSpan.FromMinutes(timeout)
                });
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
    var testAssemblies = GetFiles("./DynamicSolver.*/bin/" + configuration + "/*.Tests.dll");

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

    var artifacts = GetFiles("./DynamicSolver.GUI/bin/" + configuration + "/*.dll")
                  + GetFiles("./DynamicSolver.GUI/bin/" + configuration + "/*.exe");
    CopyFiles(artifacts, artifactsSolverDir);
});


Task("Pack-Zip-Artifacts")
    .IsDependentOn("Copy-App-Artifacts")
    .Does(() =>
{
    var artifactsSolverZip = File(artifactsDir.Path + string.Format("/solver-{0}.zip", semanticVersion));
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