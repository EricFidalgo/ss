namespace Homework2.API.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public DateTime birthdate { get; set; }
        public string race { get; set; }
        public string gender { get; set; }
    }
}