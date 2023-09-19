namespace OrleansStream.Commons.Model;

[GenerateSerializer]
public class User
{
  [Id(0)]
  public Guid Id { get; set; }  
  
  [Id(1)]
  public string? Name { get; set; }
}