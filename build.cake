// ADDINS
#addin "Cake.Coveralls"
#addin "Cake.FileHelpers"
#addin "Cake.Incubator"
#addin "Cake.Issues"
#addin nuget:?package=Cake.AppVeyor

// TOOLS
#tool "GitReleaseManager"
#tool "GitVersion.CommandLine"
#tool "coveralls.io"
#tool "OpenCover"
#tool "ReportGenerator"
#tool "nuget:?package=GitVersion.CommandLine&prerelease"

// ARGUMENTS
var target = Argument("target", "Default");
if (string.IsNullOrWhiteSpace(target))
{
    target = "Default";
}

var buildConfig = Argument("buildConfig", "Release");
if (string.IsNullOrEmpty(buildConfig)) {
    buildConfig = "Release";
}

// Build configuration

var repoOwner = "alphacloud";
var repoName = "Autofac.Extras.Quartz";

var local = BuildSystem.IsLocalBuild;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var isRepository = StringComparer.OrdinalIgnoreCase.Equals($"{repoOwner}/{repoName}", AppVeyor.Environment.Repository.Name);

var isDebugBuild = string.Equals(buildConfig, "Debug", StringComparison.OrdinalIgnoreCase);
var isReleaseBuild = string.Equals(buildConfig, "Release", StringComparison.OrdinalIgnoreCase);

var isDevelopBranch = StringComparer.OrdinalIgnoreCase.Equals("develop", AppVeyor.Environment.Repository.Branch);
var isReleaseBranch = AppVeyor.Environment.Repository.Branch.IndexOf("releases/", StringComparison.OrdinalIgnoreCase) >= 0
    || AppVeyor.Environment.Repository.Branch.IndexOf("hotfixes/", StringComparison.OrdinalIgnoreCase) >= 0;

var isTagged = AppVeyor.Environment.Repository.Tag.IsTag;
var appVeyorJobId = AppVeyor.Environment.JobId;

// Solution settings
// Nuget packages to build
var nugetPackages = new [] {
    "Autofac.Extras.Quartz"
};

// Calculate version and commit hash
GitVersion semVersion = GitVersion();
var nugetVersion = semVersion.NuGetVersion;
var buildVersion = semVersion.FullBuildMetaData;
var informationalVersion = semVersion.InformationalVersion;
var nextMajorRelease = $"{semVersion.Major+1}.0.0";
var commitHash = semVersion.Sha;
var milestone = semVersion.MajorMinorPatch;

// Artifacts
var artifactsDir = "./artifacts";

var testCoverageOutputFile = artifactsDir + "/OpenCover.xml";
var codeCoverageReportDir = artifactsDir + "/CodeCoverageReport";


var packagesDir = artifactsDir + "/packages";
var srcDir = "./src";
var testsRootDir = srcDir + "/Tests";
var solutionFile = srcDir + "/Autofac.Extras.Quartz.sln";
var samplesDir = "./samples";

Credentials githubCredentials = null;

public class Credentials {
    public string UserName { get; set; }
    public string Password { get; set; }

    public Credentials(string userName, string password) {
        UserName = userName;
        Password = password;
    }
}


// SETUP / TEARDOWN

Setup((context) =>
{
    Information("Building version {0} (tagged: {1}, local: {2}, release branch: {3})...", nugetVersion, isTagged, local, isReleaseBranch);
    CreateDirectory(artifactsDir);
    CleanDirectory(artifactsDir);
    githubCredentials = new Credentials(
      context.EnvironmentVariable("GITHUB_USER"),
      context.EnvironmentVariable("GITHUB_PASSWORD")
    );
});

Teardown((context) =>
{
    // Executed AFTER the last task.
});

Task("SetVersion")
    .Does(() =>
    {
        CreateAssemblyInfo("./src/common/AssemblyVersion.cs", new AssemblyInfoSettings{
            FileVersion = semVersion.MajorMinorPatch,
            InformationalVersion = semVersion.InformationalVersion,
            Version = semVersion.MajorMinorPatch
        });
    });


Task("UpdateAppVeyorBuildNumber")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
    {
        AppVeyor.UpdateBuildVersion(buildVersion);

    }).ReportError(exception =>
    {
        Warning("Build with version {0} already exists.", buildVersion);
    });


Task("Restore")
    .Does(() =>
    {
        DotNetCoreRestore(srcDir);
    });



Task("RunXunitTests")
    .DoesForEach(GetFiles($"{testsRootDir}/**/*.csproj"), (testProj) => {
        var projectPath = testProj.GetDirectory();
        var projectFilename = testProj.GetFilenameWithoutExtension();
        Information("Calculating code coverage for {0} ...", projectFilename);
        var openCoverSettings = new OpenCoverSettings {
            OldStyle = true,
            ReturnTargetCodeOffset = 0,
            ArgumentCustomization = args => args.Append("-mergeoutput"),
            WorkingDirectory = projectPath,
        }
        .WithFilter("+[Autofac.Extras.Quartz]* -[Autofac.Extras.Quartz.Tests]*")
        .ExcludeByAttribute("*.ExcludeFromCodeCoverage*")
        .ExcludeByFile("*/*Designer.cs");

        var testOutputAbs = MakeAbsolute(File($"{artifactsDir}/xunitTests-{projectFilename}.xml"));

        // run open cover for debug build configuration
        OpenCover(
            tool => tool.DotNetCoreTool(projectPath.ToString(),
                "xunit",
                new ProcessArgumentBuilder()
					.AppendSwitch("-parallel", "none")
                    .AppendSwitchQuoted("-xml", testOutputAbs.FullPath)
                    .AppendSwitch("--configuration", "Debug")
                    .Append("--no-build")
            ),
            testCoverageOutputFile,
            openCoverSettings);

        // run tests again if Release mode was requested
        if (isReleaseBuild) {
            Information("Running Release mode tests for {0}", projectFilename.ToString());
            DotNetCoreTool(testProj.FullPath,
                "xunit",
                new ProcessArgumentBuilder()
					.AppendSwitch("-parallel", "none")
                    .AppendSwitchQuoted("-xml", testOutputAbs.FullPath)
                    .AppendSwitch("--configuration", "Release")
                    .Append("--no-build")
            );
        }
    })
    .DeferOnError();

Task("CleanPreviousTestResults")
    .Does(() =>
    {
        if (FileExists(testCoverageOutputFile))
            DeleteFile(testCoverageOutputFile);
        DeleteFiles(artifactsDir + "/xunitTests-*.xml");
        if (DirectoryExists(codeCoverageReportDir))
            DeleteDirectory(codeCoverageReportDir, recursive: true);
    });

Task("GenerateCoverageReport")
    .WithCriteria(() => local)
    .Does(() =>
    {
        ReportGenerator(testCoverageOutputFile, codeCoverageReportDir);
    });


Task("UploadTestResults")
    .WithCriteria(() => !local)
    .Does(() => {
        CoverallsIo(testCoverageOutputFile);
        foreach(var xunitResult in GetFiles($"{artifactsDir}/xunitTests-*.xml"))
        {
            Information("Uploading xUnit results: {0}", xunitResult);
            UploadFile("https://ci.appveyor.com/api/testresults/xunit/"+appVeyorJobId, xunitResult);
        }
    });


Task("RunUnitTests")
    .IsDependentOn("Build")
    .IsDependentOn("CleanPreviousTestResults")
    .IsDependentOn("RunXunitTests")
    .IsDependentOn("GenerateCoverageReport")
    .IsDependentOn("UploadTestResults")
    .Does(() =>
    {
    });



Task("Build")
    .IsDependentOn("SetVersion")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetCoreBuild(srcDir, new DotNetCoreBuildSettings {
            NoRestore = true,
            Configuration = buildConfig,
        });
    });



Task("CreateNugetPackages")
    .Does(() => {
        Action<string> buildPackage = (string projectName) => {
			var projectFileName = $"{srcDir}/{projectName}/{projectName}.csproj";
			
			if (isTagged) {
				var releaseNotes = $"https://github.com/alphacloud/Autofac.Extras.Quartz/releases/tag/{milestone}";
				Information("Updating ReleaseNotes Link for project {0} to {1}", projectName, releaseNotes);
				XmlPoke(projectFileName,
					"/Project/PropertyGroup[@Label=\"Package\"]/PackageReleaseNotes",
					releaseNotes
				);
			}

            DotNetCorePack(projectFileName, new DotNetCorePackSettings {
                Configuration = buildConfig,
                OutputDirectory = packagesDir,
                NoBuild = true,
                ArgumentCustomization = args => args.Append($"-p:Version={nugetVersion}")
            });
        };

        foreach(var projectName in nugetPackages) {
            buildPackage(projectName);
        };
    });

Task("CreateRelease")
    .WithCriteria(() => isRepository && isReleaseBranch && !isPullRequest)
    .Does(() => {
        GitReleaseManagerCreate(githubCredentials.UserName, githubCredentials.Password, repoOwner, repoName,
            new GitReleaseManagerCreateSettings {
              Milestone = milestone,
              TargetCommitish = "master"
        });
    });

Task("CloseMilestone")
    .WithCriteria(() => isRepository && isTagged && !isPullRequest)
    .Does(() => {
        GitReleaseManagerClose(githubCredentials.UserName, githubCredentials.Password, repoOwner, repoName, milestone);
    });

Task("Default")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("Build")
    .IsDependentOn("RunUnitTests")
    .IsDependentOn("CreateNugetPackages")
    .IsDependentOn("CreateRelease")
    .IsDependentOn("CloseMilestone")
    .Does(
        () => {}
    );


// EXECUTION
RunTarget(target);
