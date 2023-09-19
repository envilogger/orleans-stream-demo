using Orleans.Runtime;

namespace OrleansStream.Commons;

public interface IActivityGrain : IGrainWithGuidKey
{
  public Task StopGeneratingUsers();
  public Task StartGeneratingUsers();
  public Task<StreamId> SubscribeForUsers();
}