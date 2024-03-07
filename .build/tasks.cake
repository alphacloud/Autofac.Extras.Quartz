// TASKS

Task("CleanAll")
    .Does<BuildInfo>(build =>
    {
        CleanDirectories($"{build.Paths.SrcDir}/**/obj");
        CleanDirectories($"{build.Paths.SrcDir}/**/bin");
        CleanDirectories($"{build.Paths.ArtifactsDir}/**");
    });

Task("SetVersion")
    .Does<BuildInfo>(build =>
    {
        CreateAssemblyInfo(build.Paths.CommonAssemblyVersionFile, new AssemblyInfoSettings
        {
            FileVersion = build.Version.Milestone,
            InformationalVersion = build.Version.Informational,
            Version = build.Version.Milestone
        });
    });


Task("UpdateAppVeyorBuildNumber")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .ContinueOnError()
    .Does<BuildInfo>(build =>
    {
        AppVeyor.UpdateBuildVersion(build.Version.Full);
    });


Task("Restore")
    .Does<BuildInfo>(build =>
    {
        DotNetCoreRestore(build.Paths.SrcDir);
    });


Task("RunXunitTests")
    .Does<BuildInfo>(build =>
    {
        var projectPath = build.Paths.SrcDir;
        var projectFilename = build.Settings.SolutionName;
        // keep in sync with src/Directory.Build.props
        var testTargets = new KeyValuePair<string, bool>[] {
            new KeyValuePair<string,bool>("net6.0", true),
            new KeyValuePair<string,bool>("net7.0", true),
            new KeyValuePair<string,bool>("net8.0", true)
        };
        foreach(var targetFw in testTargets)
        {
            Func<string,string,ProcessArgumentBuilder> buildProcessArgs = (buildCfg, targetFramework) => {
                var pb = new ProcessArgumentBuilder()
                    .AppendSwitch("--configuration", buildCfg)
                    .AppendSwitch("--filter", "Category!=ManualTests")
                    .AppendSwitch("--results-directory", build.Paths.RootDir.Combine(build.Paths.ArtifactsDir).FullPath)
                    .AppendSwitch("--framework", targetFramework)
                    .Append("--no-restore")
                    .Append("--no-build");
                if (!build.IsLocal) {
                    pb.AppendSwitch("--test-adapter-path", ".")
                        .AppendSwitch("--logger", "AppVeyor");
                }
                else {
                    pb.AppendSwitch("--logger", $"trx;LogFileName={projectFilename}.trx");
                }
                return pb;
            };

            if (targetFw.Value) // calculate coverage
            {
                Information("Calculating code coverage for {0} ({1}) ...", projectFilename, targetFw.Key);
                
                var openCoverSettings = new OpenCoverSettings
                {
                    OldStyle = true,
                    ReturnTargetCodeOffset = 0,
                    ArgumentCustomization = args => args.Append("-mergeoutput").Append("-hideskipped:File;Filter;Attribute"),
                    WorkingDirectory = projectPath,
                }
                .WithFilter($"{build.Settings.CodeCoverage.IncludeFilter} {build.Settings.CodeCoverage.ExcludeFilter}")
                .ExcludeByAttribute(build.Settings.CodeCoverage.ExcludeByAttribute)
                .ExcludeByFile(build.Settings.CodeCoverage.ExcludeByFile);
                
                // run open cover for debug build configuration
                OpenCover(
                    tool => tool.DotNetCoreTool(
                        projectPath.ToString(),
                        "test",
                        buildProcessArgs("Debug", targetFw.Key)
                    ),
                    build.Paths.RootDir.Combine(build.Paths.TestCoverageOutputFile).FullPath,
                    openCoverSettings
                );
            } else 
            {
                var solutionFullPath = build.Paths.RootDir.Combine(build.Paths.SrcDir).Combine(build.Settings.SolutionName) + ".sln";
                Information("Running Debug mode tests for {0} ({1})", projectFilename, targetFw.Key);
                DotNetCoreTool(
                    solutionFullPath,
                    "test",
                    buildProcessArgs("Debug", targetFw.Key)
                );
            
            }
            
            // run tests again if Release mode was requested
            if (build.IsRelease)
            {
                var solutionFullPath = build.Paths.RootDir.Combine(build.Paths.SrcDir).Combine(build.Settings.SolutionName) + ".sln";
                Information("Running Release mode tests for {0} ({1})", projectFilename, targetFw.Key);
                DotNetCoreTool(
                    solutionFullPath,
                    "test",
                    buildProcessArgs("Release", targetFw.Key)
                );
            }
        }
    })
    .DeferOnError();

Task("CleanPreviousTestResults")
    .Does<BuildInfo>(build =>
    {
        if (FileExists(build.Paths.TestCoverageOutputFile))
            DeleteFile(build.Paths.TestCoverageOutputFile);
        DeleteFiles(build.Paths.ArtifactsDir + "/*.trx");
        if (DirectoryExists(build.Paths.TestCoverageReportDir))
            DeleteDirectory(build.Paths.TestCoverageReportDir, new DeleteDirectorySettings
            {
                Force = true,
                Recursive = true
            });
    });

Task("GenerateCoverageReport")
    .WithCriteria<BuildInfo>((ctx, build) => build.IsLocal)
    .Does<BuildInfo>(build =>
    {
        ReportGenerator((FilePath)build.Paths.TestCoverageOutputFile, build.Paths.TestCoverageReportDir);
    });

Task("UploadCoverage")
    .WithCriteria<BuildInfo>((ctx, build) => !build.IsLocal)
    .Does<BuildInfo>(build =>
    {
        CoverallsNet(build.Paths.TestCoverageOutputFile, CoverallsNetReportType.OpenCover, new CoverallsNetSettings()
        {
            RepoTokenVariable = "COVERALLS_REPO_TOKEN"
        });
    });

Task("RunUnitTests")
    .IsDependentOn("Build")
    .IsDependentOn("CleanPreviousTestResults")
    .IsDependentOn("RunXunitTests")
    .IsDependentOn("GenerateCoverageReport")
    .IsDependentOn("UploadCoverage")
    .Does<BuildInfo>(build =>
    {
        Information("Done Test");
    });

Task("UpdateReleaseNotesLink")
    .WithCriteria<BuildInfo>((ctx, build) => build.Repository.IsTagged)
    .Does<BuildInfo>(build =>
    {
        var releaseNotesUrl = $"https://github.com/{build.Settings.RepoOwner}/{build.Settings.RepoName}/releases/tag/{build.Version.Milestone}";
        Information("Updating ReleaseNotes URL to '{0}'", releaseNotesUrl);
        XmlPoke(build.Paths.BuildPropsFile,
            "/Project/PropertyGroup[@Label=\"Package\"]/PackageReleaseNotes",
            releaseNotesUrl
        );
    });


Task("Build")
    .IsDependentOn("SetVersion")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .IsDependentOn("UpdateReleaseNotesLink")
    .IsDependentOn("Restore")
    .Does<BuildInfo>(build =>
    {
        if (build.IsRelease) {
            Information("Running {0} build to calculate code coverage", "Debug");
            // need Debug build for code coverage
            DotNetCoreBuild(build.Paths.SrcDir, new DotNetCoreBuildSettings {
                NoRestore = true,
                Configuration = "Debug",
            });
        }
        Information("Running {0} build", build.Config);
        DotNetCoreBuild(build.Paths.SrcDir, new DotNetCoreBuildSettings {
            NoRestore = true,
            Configuration = build.Config,
        });
    });


Task("CreateNugetPackages")
    .Does<BuildInfo>(build =>
    {
        DotNetCorePack(build.Paths.SrcDir, new DotNetCorePackSettings {
            Configuration = build.Config,
            NoRestore = true,
            NoBuild = true,
            ArgumentCustomization = args => 
                args.Append($"-p:Version={build.Version.NuGet}")
                    .Append($"--output {build.Paths.PackagesDir}")
        });
    });

Task("CreateRelease")
    .WithCriteria<BuildInfo>((ctx, build) =>
        build.Repository.IsMain && build.Repository.IsReleaseBranch && build.Repository.IsPullRequest == false)
    .Does<BuildInfo>(build =>
    {
        GitReleaseManagerCreate(
            build.GitHubToken,
            build.Settings.RepoOwner, build.Settings.RepoName,
            new GitReleaseManagerCreateSettings {
              Milestone = build.Version.Milestone,
              TargetCommitish = "master"
        });
    });

Task("CloseMilestone")
    .WithCriteria<BuildInfo>((ctx, build) =>
        build.Repository.IsMain && build.Repository.IsTagged && build.Repository.IsPullRequest == false)
    .Does<BuildInfo>(build =>
    {
        GitReleaseManagerClose(
            build.GitHubToken,
            build.Settings.RepoOwner, build.Settings.RepoName,
            build.Version.Milestone
        );
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
