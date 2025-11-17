using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace Homework2.Maui.Views;

[QueryProperty(nameof(AppointmentId), "id")]
public partial class AppointmentDetailPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private Appointment? _currentAppointment;
    private DateTime _selectedTime;
    
    private Patient? _selectedPatient;
    private Physician? _selectedPhysician;

    private List<Patient?> _patients;
    private List<Physician?> _availablePhysicians;
    private List<DateTime> _availableSlots;

    public DateTime MinDate => DateTime.Today;
    public DateTime MaxDate => DateTime.Today.AddMonths(3);

    public string AppointmentId
    {
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Title = "New Appointment";
                _currentAppointment = null;
                DeleteButton.IsVisible = false;
            }
            else
            {
                int appointmentId = Convert.ToInt32(value);
                _currentAppointment = _medicalDataService.GetAppointment(appointmentId);
                
                if (_currentAppointment != null)
                {
                    Title = "Edit Appointment";
                    DeleteButton.IsVisible = true;
                    LoadExistingAppointment();
                }
                else
                {
                    Title = "New Appointment";
                    DeleteButton.IsVisible = false;
                }
            }
        }
    }

    public AppointmentDetailPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;

        _currentAppointment = null;
        Title = "New Appointment";
        DeleteButton.IsVisible = false;

        _patients = new List<Patient?>();
        _availablePhysicians = new List<Physician?>();
        _availableSlots = new List<DateTime>();

        AppointmentDatePicker.Date = GetNextWeekday(DateTime.Today);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadPatients();
    }

    private void LoadPatients()
    {
        _patients = _medicalDataService.GetPatients();
    }

    private void LoadExistingAppointment()
    {
        if (_currentAppointment == null) return;

        // Set the selected patient
        _selectedPatient = _currentAppointment.patients;
        if (_selectedPatient != null)
        {
            PatientSelectButton.Text = $"Patient: {_selectedPatient.name}";
        }

        // Set the date
        AppointmentDatePicker.Date = _currentAppointment.hour.Date;

        // Set the selected time
        _selectedTime = _currentAppointment.hour;
        SelectedTimeLabel.Text = $"Selected: {_selectedTime:h:mm tt}";

        // Set the selected physician
        _selectedPhysician = _currentAppointment.physicians;
        if (_selectedPhysician != null)
        {
            PhysicianSelectButton.Text = $"Physician: {_selectedPhysician.name}";
            PhysicianSelectButton.IsEnabled = true;
        }

        // Load available slots (for editing purposes)
        UpdateAvailableSlotsForEdit();
    }

    private void UpdateAvailableSlotsForEdit()
    {
        if (_selectedPatient == null || _currentAppointment == null)
        {
            UpdateAvailableSlots();
            return;
        }

        var selectedDate = AppointmentDatePicker.Date;
        var physicians = _medicalDataService.GetPhysicians();
        
        // Temporarily remove the current appointment's time from unavailable hours
        var currentTime = _currentAppointment.hour;
        _selectedPatient.unavailable_hours.Remove(currentTime);
        _currentAppointment.physicians?.unavailable_hours.Remove(currentTime);

        _availableSlots = _medicalDataService.GetAvailableSlots(selectedDate, _selectedPatient, physicians);
        
        // Add the current time back if it's on the selected date
        if (currentTime.Date == selectedDate.Date && !_availableSlots.Contains(currentTime))
        {
            _availableSlots.Add(currentTime);
            _availableSlots = _availableSlots.OrderBy(t => t).ToList();
        }
        
        TimeSlotsCollectionView.ItemsSource = _availableSlots;
        
        // Restore the unavailable hours
        _selectedPatient.unavailable_hours.Add(currentTime);
        _currentAppointment.physicians?.unavailable_hours.Add(currentTime);
        
        // Load available physicians for the selected time
        if (_selectedTime != default)
        {
            LoadAvailablePhysiciansForEdit();
        }
    }

    private void LoadAvailablePhysiciansForEdit()
    {
        if (_currentAppointment == null) return;

        var currentTime = _currentAppointment.hour;
        
        // Temporarily remove current physician's unavailable hour
        _currentAppointment.physicians?.unavailable_hours.Remove(currentTime);
        
        _availablePhysicians = _medicalDataService.GetAvailablePhysicians(_selectedTime);
        
        // Make sure current physician is in the list if the time hasn't changed
        if (_selectedTime == currentTime && _currentAppointment.physicians != null)
        {
            if (!_availablePhysicians.Any(p => p?.Id == _currentAppointment.physicians.Id))
            {
                _availablePhysicians.Add(_currentAppointment.physicians);
            }
        }
        
        // Restore the unavailable hour
        _currentAppointment.physicians?.unavailable_hours.Add(currentTime);
        
        PhysicianSelectButton.IsEnabled = _availablePhysicians.Any();
    }

    private async void OnSelectPatientClicked(object sender, EventArgs e)
    {
        if (_patients == null || !_patients.Any())
        {
            await DisplayAlert("Info", "No patients found.", "OK");
            return;
        }

        var names = _patients.Select(p => p?.name ?? "Unknown").ToArray();
        string action = await DisplayActionSheet("Choose a Patient", "Cancel", null, names);

        if (action != "Cancel" && action != null)
        {
            _selectedPatient = _patients.FirstOrDefault(p => p?.name == action);
            PatientSelectButton.Text = $"Patient: {_selectedPatient?.name}";
            
            if (_currentAppointment != null)
            {
                UpdateAvailableSlotsForEdit();
            }
            else
            {
                UpdateAvailableSlots();
            }
        }
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        if (e.NewDate.DayOfWeek == DayOfWeek.Saturday || e.NewDate.DayOfWeek == DayOfWeek.Sunday)
        {
            DisplayAlert("Invalid Date", "Appointments can only be scheduled Monday-Friday", "OK");
            AppointmentDatePicker.Date = GetNextWeekday(e.NewDate);
            return;
        }

        if (_currentAppointment != null)
        {
            UpdateAvailableSlotsForEdit();
        }
        else
        {
            UpdateAvailableSlots();
        }
    }

    private void UpdateAvailableSlots()
    {
        _selectedTime = default;
        SelectedTimeLabel.Text = "No time selected";
        
        _selectedPhysician = null;
        PhysicianSelectButton.Text = "Tap to Choose Physician";
        PhysicianSelectButton.IsEnabled = false;

        if (_selectedPatient == null)
        {
            TimeSlotsCollectionView.ItemsSource = null;
            return;
        }

        var selectedDate = AppointmentDatePicker.Date;
        var physicians = _medicalDataService.GetPhysicians();
        
        _availableSlots = _medicalDataService.GetAvailableSlots(selectedDate, _selectedPatient, physicians);
        TimeSlotsCollectionView.ItemsSource = _availableSlots;
    }

    private void OnTimeSlotSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is DateTime selectedTime)
        {
            _selectedTime = selectedTime;
            SelectedTimeLabel.Text = $"Selected: {selectedTime:h:mm tt}";

            if (_currentAppointment != null)
            {
                LoadAvailablePhysiciansForEdit();
            }
            else
            {
                _availablePhysicians = _medicalDataService.GetAvailablePhysicians(selectedTime);
            }
            
            PhysicianSelectButton.IsEnabled = true;
            PhysicianSelectButton.Text = "Tap to Choose Physician";
            _selectedPhysician = null;
        }
    }

    private async void OnSelectPhysicianClicked(object sender, EventArgs e)
    {
        if (_availablePhysicians == null || !_availablePhysicians.Any())
        {
            await DisplayAlert("Info", "No physicians available at this time.", "OK");
            return;
        }

        var names = _availablePhysicians.Select(p => p?.name ?? "Unknown").ToArray();
        string action = await DisplayActionSheet("Choose a Physician", "Cancel", null, names);

        if (action != "Cancel" && action != null)
        {
            _selectedPhysician = _availablePhysicians.FirstOrDefault(p => p?.name == action);
            PhysicianSelectButton.Text = $"Physician: {_selectedPhysician?.name}";
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_selectedPatient == null)
        {
            await DisplayAlert("Error", "Please select a patient", "OK");
            return;
        }

        if (_selectedTime == default)
        {
            await DisplayAlert("Error", "Please select a time slot", "OK");
            return;
        }

        if (_selectedPhysician == null)
        {
            await DisplayAlert("Error", "Please select a physician", "OK");
            return;
        }

        try
        {
            if (_currentAppointment == null)
            {
                // Create new appointment
                _medicalDataService.CreateAppointment(_selectedPatient, _selectedPhysician, _selectedTime);
                await DisplayAlert("Success", "Appointment created successfully", "OK");
            }
            else
            {
                // Update existing appointment
                _currentAppointment.patients = _selectedPatient;
                _currentAppointment.physicians = _selectedPhysician;
                _currentAppointment.hour = _selectedTime;
                _medicalDataService.UpdateAppointment(_currentAppointment);
                await DisplayAlert("Success", "Appointment updated successfully", "OK");
            }
            
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save appointment: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_currentAppointment == null) return;

        bool answer = await DisplayAlert("Confirm", 
            "Are you sure you want to delete this appointment?", 
            "Yes", "No");
        
        if (answer)
        {
            try
            {
                _medicalDataService.DeleteAppointment(_currentAppointment.Id);
                await DisplayAlert("Success", "Appointment deleted successfully", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete appointment: {ex.Message}", "OK");
            }
        }
    }

    private DateTime GetNextWeekday(DateTime date)
    {
        while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }
        return date;
    }
}