public class Physician
{
  public string? name { get; set; }
  public int? Id { get; set; }
  public DateTime graduation { get; set; }
  public List<string> specializations { get; set; } = new List<string>();

  public List<DateTime> unavailable_hours { get; set; } = new List<DateTime>();
  
}; 