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

        // --- properties for Inline Editing ---

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditing)); // Update the opposite flag too
            }
        }

        public bool IsNotEditing => !IsEditing;

        // Standard INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}