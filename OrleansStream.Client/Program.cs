using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using OrleansStream.Commons;
using OrleansStream.Commons.Model;

var builder = Host.CreateApplicationBuilder();

builder.UseOrleansClient(client =>
{
  client.UseLocalhostClustering();
  client.AddMemoryStreams(Constants.StreamProviderId);
});

var host = builder.Build();

await host.StartAsync();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("starting up...");

var clusterClient = host.Services.GetRequiredService<IClusterClient>();
var activityGrain = clusterClient.GetGrain<IActivityGrain>(Guid.Empty);

logger.LogInformation("getting stream id");
var streamId = await activityGrain.SubscribeForUsers();
logger.LogInformation("stream id is: {StreamId}", streamId);

var streamProvider = clusterClient.GetStreamProvider(Constants.StreamProviderId);
var stream = streamProvider.GetStream<User>(streamId);

await stream.SubscribeAsync(((user, token) =>
{
  logger.LogInformation("got new [{@Token}]: {@User}", token, user);
  return Task.CompletedTask;
}), exception =>
{
  logger.LogError(exception, "failed to process stream");
  return Task.FromException(exception);
}, () =>
{
  logger.LogWarning("stream stopped!");
  return Task.CompletedTask;
});

logger.LogInformation("enabling stream");

await activityGrain.StartGeneratingUsers();

Console.WriteLine("Press ENTER to continue");
Console.ReadLine();

logger.LogInformation("stopping stream");
await activityGrain.StopGeneratingUsers();

Console.WriteLine("Press ENTER to continue");
Console.ReadLine();

await host.StopAsync();