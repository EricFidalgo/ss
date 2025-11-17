using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Homework2.Maui.Models
{
    public class Physician : INotifyPropertyChanged
    {
        public int? Id { get; set; }

        private string? _name;
        public string? name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string? _license_number;
        public string? license_number
        {
            get => _license_number;
            set { _license_number = value; OnPropertyChanged(); }
        }

        private DateTime _graduation;
        public DateTime graduation
        {
            get => _graduation;
            set { _graduation = value; OnPropertyChanged(); }
        }

        private List<string> _specializations = new List<string>();
        public List<string> specializations
        {
            get => _specializations;
            set 
            { 
                _specializations = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(SpecializationText)); // Update the text view too
            }
        }

        // --- FIX: Helper Property for UI Binding ---
        public string SpecializationText
        {
            get => string.Join(", ", _specializations);
            set
            {
                // Convert the string back to a list when the user types
                if (string.IsNullOrWhiteSpace(value))
                {
                    specializations = new List<string>();
                }
                else
                {
                    specializations = value.Split(',')
                                         .Select(s => s.Trim())
                                         .Where(s => !string.IsNullOrEmpty(s))
                                         .ToList();
                }
                OnPropertyChanged();
            }
        }

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