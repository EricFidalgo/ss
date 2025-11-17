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
                    // Load will happen in OnAppearing
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
        
        // Load existing appointment data if editing
        if (_currentAppointment != null)
        {
            LoadExistingAppointment();
        }
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

        // Set the date (this will trigger OnDateSelected)
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

        // Update the UI with current data
        UpdateAvailableSlots();
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
            
            // Reset time selection when patient changes
            _selectedTime = default;
            SelectedTimeLabel.Text = "No time selected";
            _selectedPhysician = null;
            PhysicianSelectButton.Text = "Tap to Choose Physician";
            PhysicianSelectButton.IsEnabled = false;
            
            UpdateAvailableSlots();
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

        // When date changes, reset time and physician selection (unless loading existing)
        if (_currentAppointment == null || e.NewDate.Date != _currentAppointment.hour.Date)
        {
            _selectedTime = default;
            SelectedTimeLabel.Text = "No time selected";
            _selectedPhysician = null;
            PhysicianSelectButton.Text = "Tap to Choose Physician";
            PhysicianSelectButton.IsEnabled = false;
        }

        UpdateAvailableSlots();
    }

    private void UpdateAvailableSlots()
    {
        if (_selectedPatient == null)
        {
            TimeSlotsCollectionView.ItemsSource = null;
            return;
        }

        var selectedDate = AppointmentDatePicker.Date;
        var physicians = _medicalDataService.GetPhysicians();
        
        // Pass the current appointment ID if editing, so it can be excluded from conflicts
        int? excludeId = _currentAppointment?.Id;
        _availableSlots = _medicalDataService.GetAvailableSlots(selectedDate, _selectedPatient, physicians, excludeId);
        
        TimeSlotsCollectionView.ItemsSource = _availableSlots;
        
        // If we're editing and the current time is still valid, keep it selected
        if (_currentAppointment != null && _availableSlots.Contains(_currentAppointment.hour))
        {
            // Find and select the current time in the collection view
            TimeSlotsCollectionView.SelectedItem = _currentAppointment.hour;
        }
    }

    private void OnTimeSlotSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is DateTime selectedTime)
        {
            _selectedTime = selectedTime;
            SelectedTimeLabel.Text = $"Selected: {selectedTime:h:mm tt}";

            // Reset physician selection when time changes
            _selectedPhysician = null;
            PhysicianSelectButton.Text = "Tap to Choose Physician";
            
            // Get available physicians for this time slot
            int? excludeId = _currentAppointment?.Id;
            _availablePhysicians = _medicalDataService.GetAvailablePhysicians(selectedTime, excludeId);
            
            // If editing and the current physician is still available, keep them selected
            if (_currentAppointment != null && 
                _selectedTime == _currentAppointment.hour &&
                _availablePhysicians.Any(p => p?.Id == _currentAppointment.physicians?.Id))
            {
                _selectedPhysician = _currentAppointment.physicians;
                PhysicianSelectButton.Text = $"Physician: {_selectedPhysician?.name}";
            }
            
            PhysicianSelectButton.IsEnabled = _availablePhysicians.Any();
            
            if (!PhysicianSelectButton.IsEnabled)
            {
                PhysicianSelectButton.Text = "No physicians available";
            }
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
                var updatedAppointment = new Appointment
                {
                    Id = _currentAppointment.Id,
                    patients = _selectedPatient,
                    physicians = _selectedPhysician,
                    hour = _selectedTime
                };
                
                _medicalDataService.UpdateAppointment(updatedAppointment);
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