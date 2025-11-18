using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace Homework2.Maui.Views;

[QueryProperty(nameof(AppointmentId), "id")]
[QueryProperty(nameof(SelectedPatientId), "patientId")]
public partial class AppointmentDetailPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private Appointment? _currentAppointment;
    private DateTime _selectedTime;

    private List<Patient?> _patients;
    private List<Physician?> _allPhysicians;
    
    private Patient? _selectedPatient;
    private Physician? _selectedPhysician;

    private ObservableCollection<string> _diagnosesCollection;
    private ObservableCollection<Treatment> _treatmentsCollection;

    public string SelectedPatientId
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                int patientId = Convert.ToInt32(value);
                _selectedPatient = _patients.FirstOrDefault(p => p?.Id == patientId);
                UpdatePatientButton();
            }
        }
    }

    public string AppointmentId
    {
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                int appointmentId = Convert.ToInt32(value);
                _currentAppointment = _medicalDataService.GetAppointment(appointmentId);
                Title = _currentAppointment != null ? "Edit Appointment" : "New Appointment";
                DeleteButton.IsVisible = _currentAppointment != null;
            }
            else
            {
                Title = "New Appointment";
                DeleteButton.IsVisible = false;
            }
        }
    }

    public AppointmentDetailPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
        _patients = new List<Patient?>();
        _allPhysicians = new List<Physician?>();

        // Initialize Diagnoses
        _diagnosesCollection = new ObservableCollection<string>();
        if (DiagnosesCollectionView != null)
        {
            DiagnosesCollectionView.ItemsSource = _diagnosesCollection;
        }

        // Initialize Treatments
        _treatmentsCollection = new ObservableCollection<Treatment>();
        if (TreatmentsCollectionView != null)
        {
            // Bind specifically to the CollectionView
            TreatmentsCollectionView.ItemsSource = _treatmentsCollection;
        }

        // Default Date
        AppointmentDatePicker.Date = GetNextWeekday(DateTime.Today);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
    }

    private async void LoadData()
    {
        // FIX: Await the API call
        var patientsFromApi = await _medicalDataService.GetPatients();
        
        // FIX: Convert List<Patient> to List<Patient?> to match your class field
        _patients = new List<Patient?>(patientsFromApi);

        // The rest remains the same...
        _allPhysicians = _medicalDataService.GetPhysicians();

        if (_currentAppointment != null)
        {
            _selectedPatient = _patients.FirstOrDefault(p => p?.Id == _currentAppointment.patients?.Id);
            
            AppointmentDatePicker.Date = _currentAppointment.hour.Date;
            _selectedTime = _currentAppointment.hour;
            
            RoomEntry.Text = _currentAppointment.Room;
            
            UpdateAvailablePhysicians();
            UpdatePatientButton(); 
            UpdatePhysicianButton();
            UpdateAvailableSlots();

            _treatmentsCollection.Clear();
            if (_currentAppointment.Treatments != null)
            {
                foreach (var t in _currentAppointment.Treatments)
                {
                    _treatmentsCollection.Add(t);
                }
            }
            UpdateTotalCost();
        }
    }

    private void OnAddTreatmentClicked(object sender, EventArgs e)
    {
        string name = TreatmentNameEntry.Text;
        if (decimal.TryParse(TreatmentCostEntry.Text, out decimal cost) && !string.IsNullOrWhiteSpace(name))
        {
            _treatmentsCollection.Add(new Treatment { Name = name, Cost = cost });
            TreatmentNameEntry.Text = string.Empty;
            TreatmentCostEntry.Text = string.Empty;
            UpdateTotalCost();
        }
        else
        {
            DisplayAlert("Invalid Input", "Please enter a valid name and numeric cost.", "OK");
        }
    }

    private void OnDeleteTreatmentClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Treatment treatment)
        {
            _treatmentsCollection.Remove(treatment);
            UpdateTotalCost();
        }
    }

    private void UpdateTotalCost()
    {
        decimal total = _treatmentsCollection.Sum(t => t.Cost);
        TotalCostLabel.Text = $"${total:F2}";
    }

    private async void OnSelectPatientClicked(object sender, EventArgs e)
    {
        if (_patients == null || !_patients.Any())
        {
            await DisplayAlert("No Patients", "Please add patients first.", "OK");
            return;
        }

        var patientNames = _patients.Select(p => p?.name ?? "Unknown").ToArray();
        
        string action = await DisplayActionSheet(
            "Select Patient",
            "Cancel",
            null,
            patientNames
        );

        if (action != "Cancel" && !string.IsNullOrEmpty(action))
        {
            var index = Array.IndexOf(patientNames, action);
            if (index >= 0 && index < _patients.Count)
            {
                _selectedPatient = _patients[index];
                UpdatePatientButton();
                UpdateAvailablePhysicians();
            }
        }
    }

    private async void OnSelectPhysicianClicked(object sender, EventArgs e)
    {
        var availablePhysicians = GetAvailablePhysiciansForSelectedTime();

        if (availablePhysicians == null || !availablePhysicians.Any())
        {
            await DisplayAlert("No Physicians", "No physicians are available at the selected time.", "OK");
            return;
        }

        var physicianNames = availablePhysicians.Select(p => "Dr. " + (p?.name ?? "Unknown")).ToArray();
        
        string action = await DisplayActionSheet(
            "Select Physician",
            "Cancel",
            null,
            physicianNames
        );

        if (action != "Cancel" && !string.IsNullOrEmpty(action))
        {
            var index = Array.IndexOf(physicianNames, action);
            if (index >= 0 && index < availablePhysicians.Count)
            {
                _selectedPhysician = availablePhysicians[index];
                UpdatePhysicianButton();
            }
        }
    }

    private void UpdatePatientButton()
    {
        if (_selectedPatient != null)
        {
            PatientButton.Text = _selectedPatient.name ?? "Select Patient";
            PatientButton.BackgroundColor = Colors.Green;

            if (DiagnosesFrame != null) DiagnosesFrame.IsVisible = true;
            LoadDiagnoses();
        }
        else
        {
            PatientButton.Text = "Select Patient";
            PatientButton.BackgroundColor = Color.FromArgb("#512BD4");

            if (DiagnosesFrame != null) DiagnosesFrame.IsVisible = false;
            _diagnosesCollection.Clear();
        }
    }

    private void UpdatePhysicianButton()
    {
        if (_selectedPhysician != null)
        {
            PhysicianButton.Text = "Dr. " + (_selectedPhysician.name ?? "Select Physician");
            PhysicianButton.BackgroundColor = Colors.Green;
        }
        else
        {
            PhysicianButton.Text = "Select Physician";
            PhysicianButton.BackgroundColor = Color.FromArgb("#512BD4");
        }
    }

    private void LoadDiagnoses()
    {
        _diagnosesCollection.Clear();
        if (_selectedPatient != null && _selectedPatient.diagnoses != null)
        {
            foreach (var diag in _selectedPatient.diagnoses)
            {
                _diagnosesCollection.Add(diag);
            }
        }
    }

    private void OnAddDiagnosisClicked(object sender, EventArgs e)
    {
        if (NewDiagnosisEntry == null) return;
        string newDiag = NewDiagnosisEntry.Text;

        if (string.IsNullOrWhiteSpace(newDiag))
        {
            DisplayAlert("Error", "Please enter a diagnosis text.", "OK");
            return;
        }

        if (_selectedPatient != null)
        {
            _selectedPatient.diagnoses.Add(newDiag);
            _diagnosesCollection.Add(newDiag);
            NewDiagnosisEntry.Text = string.Empty;
        }
    }
    
    private void OnDeleteDiagnosisClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is string diagnosis)
        {
            _diagnosesCollection.Remove(diagnosis);
            if (_selectedPatient != null && _selectedPatient.diagnoses != null)
            {
                _selectedPatient.diagnoses.Remove(diagnosis);
            }
        }
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        if (e.NewDate.DayOfWeek == DayOfWeek.Saturday || e.NewDate.DayOfWeek == DayOfWeek.Sunday)
        {
            DisplayAlert("Invalid Date", "Appointments only Mon-Fri", "OK");
            AppointmentDatePicker.Date = GetNextWeekday(e.NewDate);
            return;
        }

        _selectedTime = default; 
        SelectedTimeLabel.Text = "No time selected";

        UpdateAvailablePhysicians();
    }

    private void UpdateAvailableSlots()
    {
        if (_selectedPatient == null)
        {
            BindableLayout.SetItemsSource(TimeSlotsLayout, null);
            SelectedTimeLabel.Text = "No time selected";
            return;
        }

        var selectedDate = AppointmentDatePicker.Date;
        int? excludeId = _currentAppointment?.Id;

        var availableSlots = _medicalDataService.GetAvailableSlots(selectedDate, _selectedPatient, excludeId);
        
        BindableLayout.SetItemsSource(TimeSlotsLayout, availableSlots);
        
        Dispatcher.Dispatch(() => RefreshTimeSlotColors());
    }

    private void OnTimeSlotClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedButton && clickedButton.BindingContext is DateTime selectedTime)
        {
            _selectedTime = selectedTime;
            SelectedTimeLabel.Text = $"Selected: {_selectedTime:h:mm tt}";
            UpdateAvailablePhysicians();
            RefreshTimeSlotColors();
        }
    }

    private void RefreshTimeSlotColors()
    {
        foreach (var child in TimeSlotsLayout.Children)
        {
            if (child is Button btn && btn.BindingContext is DateTime time)
            {
                if (time == _selectedTime)
                {
                    btn.BackgroundColor = Colors.Green;
                }
                else
                {
                    if (Application.Current.Resources.TryGetValue("Primary", out var color))
                    {
                         btn.BackgroundColor = (Color)color;
                    }
                    else
                    {
                        btn.BackgroundColor = Color.FromArgb("#512BD4");
                    }
                }
            }
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_selectedPatient == null || _selectedPhysician == null || _selectedTime == default)
        {
            await DisplayAlert("Error", "Please select Patient, Physician, and Time.", "OK");
            return;
        }

        // --- Get Room and Validate ---
        string room = RoomEntry.Text;
        if (!string.IsNullOrWhiteSpace(room))
        {
            if (!_medicalDataService.IsRoomAvailable(room, _selectedTime, _currentAppointment?.Id))
            {
                await DisplayAlert("Room Conflict", $"Room '{room}' is already booked at {_selectedTime:h:mm tt}.", "OK");
                return;
            }
        }
        // -----------------------------

        var treatmentsToSave = _treatmentsCollection.ToList();

        if (_currentAppointment == null)
        {
            // Pass room to create
            var newAppt = _medicalDataService.CreateAppointment(_selectedPatient, _selectedPhysician, _selectedTime, room);
            newAppt.Treatments = treatmentsToSave;
        }
        else
        {
            var updated = new Appointment
            {
                Id = _currentAppointment.Id,
                patients = _selectedPatient,
                physicians = _selectedPhysician,
                hour = _selectedTime,
                Room = room, // Update room
                Treatments = treatmentsToSave
            };
            _medicalDataService.UpdateAppointment(updated);
        }

        _medicalDataService.UpdatePatient(_selectedPatient);
        await Shell.Current.GoToAsync("..");
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_currentAppointment != null)
        {
            bool confirm = await DisplayAlert("Confirm", "Delete appointment?", "Yes", "No");
            if (confirm)
            {
                _medicalDataService.DeleteAppointment(_currentAppointment.Id);
                await Shell.Current.GoToAsync("..");
            }
        }
    }

    private DateTime GetNextWeekday(DateTime date)
    {
        while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            date = date.AddDays(1);
        return date;
    }

    private void UpdateAvailablePhysicians()
    {
        var availablePhysicians = GetAvailablePhysiciansForSelectedTime();
        
        if (_selectedPhysician != null && !availablePhysicians.Contains(_selectedPhysician))
        {
            _selectedPhysician = null;
            UpdatePhysicianButton();
        }
        
        UpdateAvailableSlots();
    }

    private List<Physician?> GetAvailablePhysiciansForSelectedTime()
    {
        return _allPhysicians.Where(physician => 
            _medicalDataService.IsPhysicianAvailable(physician, _selectedTime, _currentAppointment?.Id))
            .ToList();
    }
}