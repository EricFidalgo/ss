namespace Homework2.Maui.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        // CHANGE: Add { get; set; } to make these Properties instead of Fields
        public Patient? patients { get; set; }
        public Physician? physicians { get; set; }
        public DateTime hour { get; set; } 
    }
}