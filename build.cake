#tool "nuget:?package=NUnit.ConsoleRunner"
#addin "nuget:?package=Cake.Git"
#addin "nuget:?package=Polly&version=4.2.0"

using Polly;

#load "./version.cake"

const string APP_TARGET_FRAMEWORK = "net462";
const string TESTS_TARGET_FRAMEWORK = "net462";


//---------------------------------
//--- Arguments -------------------
//---------------------------------

var target = Argument("target", "Default");
var platform = Argument("platform", "Any CPU");
var configuration = Argument("configuration", "Release");


//---------------------------------
//--- Paths -----------------------
//---------------------------------

var rootDir = Directory(".");
var solutionFilePath = rootDir + File("DynamicSolver.sln");
var artifactsDir = rootDir + Directory("artifacts");
var artifactsSolverDir = artifactsDir + Directory("solver");
var testResultFilePath = rootDir + File("TestResult.xml");


//---------------------------------
//--- Version ---------------------
//---------------------------------

string FormatVersion(string branch, int? build)
{
    var version = PACKAGE_VERSION;
    if(!string.Equals(branch, "master", StringComparison.OrdinalIgnoreCase))
    {
        branch = branch.Replace("feature/", "").Replace("_", "-");        
        version += "-" + branch + "." + (build.HasValue ? build.ToString() : DateTime.Now.ToString("yyyyMMddHHmm"));
    }
    return version;
}

string semanticVersion;

if(BuildSystem.AppVeyor.IsRunningOnAppVeyor)
{
    var branch = BuildSystem.AppVeyor.Environment.Repository.Branch;
    var build = BuildSystem.AppVeyor.Environment.Build.Number;
    semanticVersion = FormatVersion(branch, build);
}
else
{
    var branch = GitBranchCurrent(rootDir).FriendlyName;
    semanticVersion = FormatVersion(branch, null);
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
    var buildFilesDirs = GetDirectories("**/bin/") + GetDirectories("**/obj/");
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
    const int maxRetryCount = 3;
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
            }})
        .Execute(()=> {
                DotNetCoreRestore();
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
        settings.WithProperty("PackageVersion", semanticVersion);
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
    var testAssemblies = GetFiles("./DynamicSolver.*/bin/" + configuration + "/" + TESTS_TARGET_FRAMEWORK + "/*.Tests.dll");

    NUnit3(testAssemblies, new NUnit3Settings() {
        NoHeader = true,
        OutputFile = testResultFilePath
    });
})
    .Finally(() =>
{
    if(BuildSystem.AppVeyor.IsRunningOnAppVeyor)
    {
        Information("Uploading test results: " + testResultFilePath);
        BuildSystem.AppVeyor.UploadTestResults(testResultFilePath, AppVeyorTestResultsType.NUnit3);
    }
});


Task("Copy-App-Artifacts")
    .IsDependentOn("Clean-Artifacts")
    .IsDependentOn("Build")
    .IsDependentOn("Run-Tests")
    .Does(() =>
{
    CreateDirectory(artifactsSolverDir);

    var artifacts = GetFiles("./DynamicSolver.App/bin/" + configuration + "/" + APP_TARGET_FRAMEWORK + "/*.dll")
                  + GetFiles("./DynamicSolver.App/bin/" + configuration + "/" + APP_TARGET_FRAMEWORK + "/*.exe");
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