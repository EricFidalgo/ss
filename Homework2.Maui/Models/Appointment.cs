using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Homework2.Maui.Models
{
    public class Appointment : INotifyPropertyChanged
    {
        public int Id { get; set; }

        private Patient? _patients;
        public Patient? patients
        {
            get => _patients;
            set { _patients = value; OnPropertyChanged(); }
        }

        private Physician? _physicians;
        public Physician? physicians
        {
            get => _physicians;
            set { _physicians = value; OnPropertyChanged(); }
        }

        private DateTime _hour;
        public DateTime hour
        {
            get => _hour;
            set { _hour = value; OnPropertyChanged(); }
        }

        // --- NEW ROOM PROPERTY ---
        private string _room;
        public string Room
        {
            get => _room;
            set { _room = value; OnPropertyChanged(); }
        }
        // -------------------------

        public List<Treatment> Treatments { get; set; } = new List<Treatment>();

        // --- properties for Inline Editing ---

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditing)); 
            }
        }

        public bool IsNotEditing => !IsEditing;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}