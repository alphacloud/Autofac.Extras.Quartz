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
        Information("Calculating code coverage for {0} ...", projectFilename);

        Func<string,ProcessArgumentBuilder> buildProcessArgs = (buildCfg) => {
            var pb = new ProcessArgumentBuilder()
                .AppendSwitch("--configuration", buildCfg)
                .AppendSwitch("--filter", "Category!=ManualTests")
                .AppendSwitch("--results-directory", build.Paths.RootDir.Combine(build.Paths.ArtifactsDir).FullPath)
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
                buildProcessArgs("Debug")
            ),
            build.Paths.RootDir.Combine(build.Paths.TestCoverageOutputFile).FullPath,
            openCoverSettings
        );

        // run tests again if Release mode was requested
        if (build.IsRelease)
        {
            var solutionFullPath = build.Paths.RootDir.Combine(build.Paths.SrcDir).Combine(build.Settings.SolutionName) + ".sln";
            Information("Running Release mode tests for {0}", projectFilename);
            DotNetCoreTool(
                solutionFullPath,
                "test",
                buildProcessArgs("Release")
            );
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
            DeleteDirectory(build.Paths.TestCoverageReportDir, recursive: true);
    });

Task("GenerateCoverageReport")
    .WithCriteria<BuildInfo>((ctx, build) => build.IsLocal)
    .Does<BuildInfo>(build =>
    {
        ReportGenerator(build.Paths.TestCoverageOutputFile, build.Paths.TestCoverageReportDir);
    });

Task("UploadCoverage")
    .WithCriteria<BuildInfo>((ctx, build) => !build.IsLocal)
    .Does<BuildInfo>(build =>
    {
        CoverallsIo(build.Paths.TestCoverageOutputFile);
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
            Information("Running {0} build for code coverage", "Debug");
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
            OutputDirectory = build.Paths.PackagesDir,
            NoRestore = true,
            NoBuild = true,
            ArgumentCustomization = args => args.Append($"-p:Version={build.Version.NuGet}")
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
