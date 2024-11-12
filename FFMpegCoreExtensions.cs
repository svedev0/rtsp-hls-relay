using FFAO = FFMpegCore.FFMpegArgumentOptions;

namespace RtspHlsRelay;

public static class FFMpegCoreExtensions
{
	public static FFAO WithNoBanner(this FFAO opts)
	{
		return opts.WithCustomArgument("-hide_banner");
	}

	public static FFAO WithReconnect(this FFAO opts)
	{
		return opts.WithCustomArgument("-reconnect 1 -reconnect_streamed 1");
	}

	public static FFAO WithReconnectDelay(this FFAO opts, int seconds)
	{
		return opts.WithCustomArgument($"-reconnect_delay_max {seconds}");
	}

	public static FFAO WithHlsTime(this FFAO opts, int seconds)
	{
		return opts.WithCustomArgument($"-hls_time {seconds}");
	}

	public static FFAO WithHlsPlaylistSize(this FFAO opts, int size)
	{
		return opts.WithCustomArgument($"-hls_list_size {size}");
	}

	public static FFAO WithHlsFlags(this FFAO opts, IEnumerable<string> flags)
	{
		return opts.WithCustomArgument($"-hls_flags {string.Join("+", flags)}");
	}

	public static FFAO WithHlsSegmentFilename(this FFAO opts, string filename)
	{
		return opts.WithCustomArgument($"-hls_segment_filename \"{filename}\"");
	}

	public static FFAO WithPersistentHttp(this FFAO opts)
	{
		return opts.WithCustomArgument("-http_persistent 1");
	}

	public static FFAO WithKeyframeInterval(this FFAO opts, int interval)
	{
		return opts.WithCustomArgument($"-g {interval}");
	}

	public static FFAO WithFlags(this FFAO opts, IEnumerable<string> flags)
	{
		return opts.WithCustomArgument($"-flags +{string.Join("+", flags)}");
	}

	public static FFAO WithFFlags(this FFAO opts, IEnumerable<string> flags)
	{
		return opts.WithCustomArgument($"-fflags +{string.Join("+", flags)}");
	}

	public static FFAO StartAtZero(this FFAO opts)
	{
		return opts.WithCustomArgument("-start_at_zero");
	}
}
