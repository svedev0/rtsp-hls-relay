using DotNetEnv;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace RtspHlsRelay;

/*
http-server 'C:\path\to\stream' -p 8080
dotnet run
*/

public class Program
{
	public static void Main(string[] args)
	{
		Env.Load(".env");

		var builder = Host.CreateDefaultBuilder(args);
		builder.ConfigureServices((_, s) => s.AddHostedService<FFMpegService>());

		var app = builder.Build();
		app.Run();
	}
}

public class FFMpegService(IHostApplicationLifetime lifetime) : IHostedService
{
	private Action? cancelAction;

	public Task StartAsync(CancellationToken cancellationToken)
	{
		lifetime.ApplicationStarted.Register(() =>
		{
			Task.Run(async () =>
			{
				try
				{
					await RunFFMpeg();
				}
				catch { }
				finally
				{
					lifetime.StopApplication();
				}
			});
		});

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken _)
	{
		Console.WriteLine("Stopping FFMpeg...");
		cancelAction?.Invoke();
		CleanupFiles();
		return Task.CompletedTask;
	}

	private async Task RunFFMpeg()
	{
		string streamUri = Env.GetString("STREAM_URI", string.Empty);
		string streamCache = Env.GetString("STREAM_CACHE", string.Empty);

		var arguments = FFMpegArguments
			.FromUrlInput(new Uri(streamUri))
			.OutputToFile($"{streamCache}\\stream.m3u8", true, opts => opts
				.WithVideoCodec("copy")
				.WithAudioCodec("aac")
				.UsingThreads(4)
				.WithNoBanner()
				.WithReconnect()
				.WithReconnectDelay(2)
				.ForceFormat("hls")
				.WithHlsTime(4)
				.WithHlsPlaylistSize(4)
				.WithHlsSegmentFilename($"{streamCache}\\segment%04d.ts")
				.WithHlsFlags(["delete_segments", "append_list", "split_by_time"])
				.WithoutMetadata()
				.WithFastStart()
				.WithPersistentHttp()
				.WithKeyframeInterval(8)
				.WithFFlags(["nobuffer"])
				.WithFlags(["low_delay"])
				.StartAtZero());

		Console.WriteLine($"Arguments: {arguments.Arguments}");

		await arguments
			.WithLogLevel(FFMpegLogLevel.Info)
			.NotifyOnOutput(Console.WriteLine)
			.NotifyOnError(Console.WriteLine)
			.CancellableThrough(out cancelAction, 0)
			.ProcessAsynchronously();
	}

	private static void CleanupFiles()
	{
		string streamCache = Env.GetString("STREAM_CACHE", string.Empty);
		string[] files = Directory.GetFiles(streamCache, "*.ts");
		files.ToList().ForEach(File.Delete);
	}
}
