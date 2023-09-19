using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Streams;
using OrleansStream.Commons;
using OrleansStream.Commons.Model;

namespace OrleansStream.Server.Grains;

public class ActivityGrain : Grain, IActivityGrain
{
  private readonly ILogger<ActivityGrain> logger;
  private readonly StreamId streamId = StreamId.Create(ActivityStream, Guid.NewGuid());
  private IAsyncStream<User>? stream;
  private readonly CancellationTokenSource cts = new();

  private const string ActivityStream = "ActivitiesStream";

  public ActivityGrain(ILogger<ActivityGrain> logger)
  {
    this.logger = logger;
  }

  public override Task OnActivateAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("activating");
    var streamProvider = this.GetStreamProvider(Constants.StreamProviderId);
    stream = streamProvider.GetStream<User>(streamId);
    return base.OnActivateAsync(cancellationToken);
  }

  public Task StopGeneratingUsers()
  {
    cts.Cancel();
    return Task.CompletedTask;
  }

  public Task StartGeneratingUsers()
  {
    _ = Task.Run(Loop);
    return Task.CompletedTask;
  }

  public Task<StreamId> SubscribeForUsers()
  {
    return Task.FromResult(streamId);
  }

  private async Task Loop()
  {
    while (!cts.Token.IsCancellationRequested)
    {
      if (stream != null)
      {
        logger.LogInformation("sending new random user...");
        await stream.OnNextAsync(GetRandomUser());
      }

      await Task.Delay(TimeSpan.FromMilliseconds(100));
    }
  }

  private User GetRandomUser()
  {
    var guid = Guid.NewGuid();
    return new User()
    {
      Id = guid,
      Name = $"User {guid}"
    };
  }
}