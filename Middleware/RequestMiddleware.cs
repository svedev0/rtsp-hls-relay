﻿using System.Diagnostics;

namespace RtspHlsRelay.Middleware;

public class RequestMiddleware(RequestDelegate next)
{
	public async Task InvokeAsync(HttpContext context)
	{
		long startTime = Stopwatch.GetTimestamp();

		Console.WriteLine("{0,-8}  {1,-6}  {2}{3}",
			context.Request.Protocol,
			context.Request.Method,
			context.Request.Path,
			context.Request.QueryString);

		context.Response.OnStarting(() =>
		{
			context.Response.Headers.Server = "CustomServer";
			return Task.CompletedTask;
		});

		context.Response.OnCompleted(() =>
		{
			TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);
			int elapsedMs = (int)elapsed.TotalMilliseconds;
			Console.WriteLine($"Request processed in {elapsedMs} ms");
			return Task.CompletedTask;
		});

		await next(context);
	}
}
