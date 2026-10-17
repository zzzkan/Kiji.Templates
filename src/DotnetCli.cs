using CliWrap;

namespace Kiji.Templates.Infrastructure;

internal sealed class DotnetCli(string repository, string runDirectory)
{
    public string Logs { get; } = Directory.CreateDirectory(Path.Combine(runDirectory, "logs")).FullName;

    public async Task Run(string label, CancellationToken cancellationToken, params string[] arguments)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(5));
        var command = Cli.Wrap("dotnet")
            .WithArguments(arguments)
            .WithWorkingDirectory(repository)
            .WithEnvironmentVariables(env => env
                .Set("DOTNET_CLI_HOME", Path.Combine(runDirectory, "cli"))
                .Set("NUGET_PACKAGES", Environment.GetEnvironmentVariable("NUGET_PACKAGES")
                    ?? Path.Combine(repository, "artifacts", "verification", "packages"))
                .Set("DOTNET_ADD_GLOBAL_TOOLS_TO_PATH", "0")
                .Set("DOTNET_GENERATE_ASPNET_CERTIFICATE", "0")
                .Set("MSBUILDDISABLENODEREUSE", "1")
                .Set("DOTNET_CLI_USE_MSBUILD_SERVER", "0")
                .Set("UseSharedCompilation", "false"))
            .WithStandardOutputPipe(PipeTarget.ToFile(Path.Combine(Logs, $"{label}.stdout.log")))
            .WithStandardErrorPipe(PipeTarget.ToFile(Path.Combine(Logs, $"{label}.stderr.log")));
        try
        {
            await command.ExecuteAsync(timeout.Token);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"{command} failed. See {Logs}/{label}.*.log", exception);
        }
    }
}
