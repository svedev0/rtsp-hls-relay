using DotNetEnv;
using RtspHlsRelay.Services;
using RtspHlsRelay.Middleware;

namespace RtspHlsRelay;

public class Program
{
	public static void Main(string[] args)
	{
		Env.Load(".env");
		if (Utils.HasMissingEnvVar(out string missingKey))
		{
			throw new Exception($"Missing environment variable: {missingKey}");
		}

		var builder = WebApplication.CreateBuilder(args);
		builder.Services.AddHostedService<FFMpegService>();
		builder.Logging.ClearProviders().AddSimpleConsole();
		builder.Logging.SetMinimumLevel(LogLevel.Warning);

		var app = builder.Build();
		app.UseMiddleware<RequestMiddleware>();
		app.UseStaticFiles(Utils.GetStaticFileOptions());
		app.Run("http://127.0.0.1:8080");
	}
}
