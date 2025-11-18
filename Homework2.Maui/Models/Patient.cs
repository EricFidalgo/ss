using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Homework2.Maui.Models
{
    public class Patient : INotifyPropertyChanged
    {
        public int? Id { get; set; }

        private string? _name;
        public string? name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string? _address;
        public string? address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        private DateTime _birthdate;
        public DateTime birthdate
        {
            get => _birthdate;
            set 
            { 
                _birthdate = value; 
                OnPropertyChanged(); 
                // Notify that the IsUnderage property may have changed
                OnPropertyChanged(nameof(IsUnderage)); 
            }
        }

        // New property to determine if patient is a minor
        public bool IsUnderage
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - birthdate.Year;
                if (birthdate.Date > today.AddYears(-age)) age--;
                return age < 18;
            }
        }

        private string? _race;
        public string? race
        {
            get => _race;
            set { _race = value; OnPropertyChanged(); }
        }

        private string? _gender;
        public string? gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(); }
        }

        public List<string> medical_notes { get; set; } = new List<string>();

        private ObservableCollection<string> _diagnoses = new ObservableCollection<string>();
        public ObservableCollection<string> diagnoses
        {
            get => _diagnoses;
            set
            {
                if (_diagnoses != value)
                {
                    _diagnoses = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<string> prescriptions { get; set; } = new List<string>();
        public List<DateTime> unavailable_hours { get; set; } = new List<DateTime>();

        // --- Properties for Inline Editing ---

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