using DotNetEnv;
using FFMpegCore;
using FFMpegCore.Enums;

namespace RtspHlsRelay.Services;

public class FFMpegService(IHostApplicationLifetime lifetime) : IHostedService
{
	private Action? cancelAction;

	public Task StartAsync(CancellationToken cancellationToken)
	{
		Task ffmpegTask = new(async () =>
		{
			try
			{
				await RunFFMpeg();
			}
			finally
			{
				lifetime.StopApplication();
			}
		});

		lifetime.ApplicationStarted.Register(ffmpegTask.Start);

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
				.StartAtZero()
				.ShowStats());

		Console.WriteLine($"Arguments: {arguments.Arguments}");

		await arguments
			.WithLogLevel(FFMpegLogLevel.Warning)
			.NotifyOnOutput(Console.WriteLine)
			.NotifyOnError(Console.WriteLine)
			.CancellableThrough(out cancelAction, 0)
			.ProcessAsynchronously();
	}

	private static void CleanupFiles()
	{
		string streamCache = Env.GetString("STREAM_CACHE", string.Empty);
		string[] files = [
			..Directory.GetFiles(streamCache, "*.ts"),
			..Directory.GetFiles(streamCache, "*.m3u8"),
		];
		files.ToList().ForEach(File.Delete);
	}
}
