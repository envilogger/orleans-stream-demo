using Microsoft.Extensions.Hosting;
using OrleansStream.Commons;

var builder = Host.CreateApplicationBuilder();

builder.UseOrleans(silo =>
{
  silo.UseLocalhostClustering();
  silo.AddMemoryStreams(Constants.StreamProviderId)
    .AddMemoryGrainStorage(Constants.GrainStorageName);

  
});

var host = builder.Build();

await host.RunAsync();
