public class Appointment
{
  public int Id { get; set; }
  public Patient? patients;
  public Physician? physicians;
  public DateTime hour; 
};