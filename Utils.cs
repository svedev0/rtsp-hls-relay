using DotNetEnv;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace RtspHlsRelay;

public class Utils
{
	public static StaticFileOptions GetStaticFileOptions()
	{
		string streamCache = Env.GetString("STREAM_CACHE", string.Empty);

		FileExtensionContentTypeProvider typeProvider = new();
		typeProvider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
		typeProvider.Mappings[".ts"] = "video/mp2t";

		return new StaticFileOptions()
		{
			FileProvider = new PhysicalFileProvider(streamCache),
			RequestPath = string.Empty,
			ContentTypeProvider = typeProvider
		};
	}

	public static bool HasMissingEnvVar(out string missingKey)
	{
		string[] required = ["STREAM_URI", "STREAM_CACHE"];
		missingKey = required.FirstOrDefault(key =>
			string.IsNullOrEmpty(Env.GetString(key, string.Empty)))
			?? string.Empty;
		return missingKey != string.Empty;
	}
}
