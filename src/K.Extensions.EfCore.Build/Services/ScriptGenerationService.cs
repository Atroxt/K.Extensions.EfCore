using ADotNet.Clients;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets.Tasks;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets;

namespace K.Extensions.EfCore.Build.Services
{
    internal class ScriptGenerationService
    {
        private readonly ADotNetClient _adotNetClient = new();
        public void GenerateBuildScript()
        {
            string branchName = "main";
            string dotNetVersion = "8.0.302";
            string projectName = "K.Extensions.EfCore";

            var githubPipeline = new GithubPipeline()
            {
                Name = "Build",
                OnEvents = new Events()
                {
                    //Push = new PushEvent() { Branches = [branchName] },
                    PullRequest = new PullRequestEvent() { Branches = [branchName] },
                },


                Jobs = new Dictionary<string, Job>()
                {
                    {
                        "build",
                        new Job()
                        {
                            Name = "Build",
                            RunsOn = BuildMachines.WindowsLatest,

                            Steps = new List<GithubTask>()
                            {
                                new GithubTask()
                                {
                                    Name = "Check out",
                                    Uses = "actions/checkout@v4"
                                },
                                new GithubTask()
                                {
                                    Name ="Setup .Net",
                                    Uses = "actions/setup-dotnet@v4",
                                    With = new Dictionary<string, string>()
                                    {
                                        {"dotnet-version",dotNetVersion}
                                    }
                                },
                                new RestoreTask()
                                {
                                    Name = "Restore"
                                },
                                new DotNetBuildTask()
                                {
                                    Name = "Build"
                                },
                                #region Tests
                                new GithubTask()
                                {
                                    Name = "Cobertura Tests",
                                    Run = $"dotnet test --no-restore --verbosity normal --configuration Release --collect \"XPlat Code Coverage\" /p:CollectCodeCoverage=true --settings CodeCoverage.runsettings --results-directory \"CoberturaTestResults-{dotNetVersion}\""
                                },
                                new GithubTask()
                                {
                                    Name = "Upload coverage report artifact",
                                    Uses = "codecov/codecov-action@v4.0.1",
                                    With = new Dictionary<string, string>()
                                    {
                                        {"verbose","true"},
                                        {"token","${{ secrets.CODECOV_TOKEN }}"},
                                        {"directory",$"CoberturaTestResults-{dotNetVersion}"},
                                    }
                                },
                                #endregion
                                new GithubTask()
                                {
                                    Name = "Create the package",
                                    Run = $@"dotnet pack .\src\{projectName}\{projectName}.csproj -v normal --configuration Release -o:nupkg"
                                },
                                new GithubTask()
                                {
                                    Name = "Push generated package to GitHub registry",
                                    Run = "dotnet nuget push **/K.Extensions*.nupkg --source \"https://nuget.pkg.github.com/Atroxt/index.json\" --api-key ${{ secrets.GIT_TOKEN }} --skip-duplicate "
                                },
                                new GithubTask()
                                {
                                    Name = "Push generated package to Nuget registry",
                                    Run = "dotnet nuget push **/K.Extensions*.nupkg --source \"https://api.nuget.org/v3/index.json\" --api-key ${{ secrets.NUGET_APIKEYTOKEN }} --skip-duplicate "
                                },
                                new GithubTask()
                                {
                                    Name = "Upload",
                                    Uses = "actions/upload-artifact@v4",
                                    With = new Dictionary<string, string>()
                                    {
                                        {"name","NugetPackage-Artifacts"},
                                        {"path","./nupkg/*.nupkg"},
                                    },
                                    If = "${{ always() }}"
                                }
                            }
                        }
                    },
                }
            };

            string buildScriptPath = "../../../../../.github/workflows/build.yml";
            string directoryPath = Path.GetDirectoryName(buildScriptPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            _adotNetClient.SerializeAndWriteToFile(
                adoPipeline: githubPipeline,
                path: buildScriptPath);
        }
    }
}
