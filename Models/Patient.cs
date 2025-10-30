public class Patient
{
  public string? name { get; set; }
  public string? address { get; set; }
  public DateTime birthdate { get; set; }
  public string? race { get; set; }
  public string? gender { get; set; }
  public int? Id { get; set; }
  public List<string> medical_notes { get; set; } = new List<string>();
  public List<string> diagnoses { get; set; } = new List<string>();
  public List<string> prescriptions { get; set; } = new List<string>();
  public List<DateTime> unavailable_hours { get; set; } = new List<DateTime>();
}; 