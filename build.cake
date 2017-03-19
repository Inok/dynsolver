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

var solutionFilePath = rootDir + File("DynamicSolver.sln");
var solutionInfoFile = rootDir + File("SolutionInfo.cs");

var artifactsDir = rootDir + Directory("artifacts");
var artifactsSolverDir = artifactsDir + Directory("solver");

var testResultFilePath = rootDir + File("TestResult.xml");


//---------------------------------
//--- Version ---------------------
//---------------------------------

string FormatBranch(string branch)
{
    return branch.Replace("feature/", "");
}

var solutionInfo = ParseAssemblyInfo(solutionInfoFile);
var semanticVersion = solutionInfo.AssemblyVersion;

if(BuildSystem.AppVeyor.IsRunningOnAppVeyor)
{
    var branch = BuildSystem.AppVeyor.Environment.Repository.Branch;
    var build = BuildSystem.AppVeyor.Environment.Build.Number;
    semanticVersion += "." + build + "-" + FormatBranch(branch);
}
else
{
    var branch = GitBranchCurrent(rootDir).FriendlyName;
    semanticVersion += "-" + FormatBranch(branch);
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


Task("Clean-TestResult").Does(() =>
{
    if(FileExists(testResultFilePath))
    {
        DeleteFile(testResultFilePath);
    }
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
    .IsDependentOn("Clean-TestResult")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testAssemblies = GetFiles("./DynamicSolver.*/bin/" + configuration + "/*.Tests.dll");

    NUnit3(testAssemblies, new NUnit3Settings() {
        NoHeader = true,
        OutputFile = testResultFilePath
    });
});


Task("Copy-App-Artifacts")
    .IsDependentOn("Clean-Artifacts")
    .IsDependentOn("Build")
    .IsDependentOn("Run-Tests")
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
    var artifactsSolverZip = artifactsDir + File(string.Format("solver-{0}.zip", semanticVersion));
    Information("Pack directory " + artifactsSolverDir + " to " + artifactsSolverZip);
    Zip(artifactsSolverDir, artifactsSolverZip);
});



//---------------------------------
//--- Targets ---------------------
//---------------------------------

Task("Clean-All")
    .IsDependentOn("Clean-Build")
    .IsDependentOn("Clean-Artifacts")
    .IsDependentOn("Clean-TestResult")
    .Does(() => { });


Task("Pack-Artifacts")
    .IsDependentOn("Pack-Zip-Artifacts")
    .Does(() => { });


Task("AppVeyor")
  .IsDependentOn("Clean-All")
  .IsDependentOn("Pack-Artifacts")
  .Does(() => {
        Information("Uploading test results: " + testResultFilePath);
        BuildSystem.AppVeyor.UploadTestResults(testResultFilePath, AppVeyorTestResultsType.NUnit3);

        var artifacts = GetFiles(artifactsDir.Path + "/*.zip");
        foreach(var artifact in artifacts)
        {
            Information("Uploading artifact: " + artifact);
            BuildSystem.AppVeyor.UploadArtifact(artifact);
        }
    });


Task("Default")
  .IsDependentOn("Clean-All")
  .IsDependentOn("Pack-Artifacts")
  .Does(() => { });


RunTarget(target);