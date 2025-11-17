using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace Homework2.Maui.Views;

[QueryProperty(nameof(AppointmentId), "id")]
public partial class AppointmentDetailPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private Appointment _currentAppointment;
    private DateTime _selectedTime;
    
    // We store the selected objects in these variables now
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
                _currentAppointment = new Appointment();
                DeleteButton.IsVisible = false;
            }
            else
            {
                Title = "Edit Appointment";
                DeleteButton.IsVisible = true;
            }
        }
    }

    public AppointmentDetailPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;

        _currentAppointment = new Appointment();
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

    // FIX: New method to handle Patient Button Click
    private async void OnSelectPatientClicked(object sender, EventArgs e)
    {
        if (_patients == null || !_patients.Any())
        {
            await DisplayAlert("Info", "No patients found.", "OK");
            return;
        }

        // Create list of names for the popup
        var names = _patients.Select(p => p?.name ?? "Unknown").ToArray();

        // Show the native Mac popup
        string action = await DisplayActionSheet("Choose a Patient", "Cancel", null, names);

        if (action != "Cancel" && action != null)
        {
            // Find the patient object based on the name selected
            _selectedPatient = _patients.FirstOrDefault(p => p?.name == action);
            
            // Update the Button Text to show who was picked
            PatientSelectButton.Text = $"Patient: {_selectedPatient?.name}";
            
            // Reset downstream selections
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

        UpdateAvailableSlots();
    }

    private void UpdateAvailableSlots()
    {
        _selectedTime = default;
        SelectedTimeLabel.Text = "No time selected";
        
        // Reset Physician
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

            _availablePhysicians = _medicalDataService.GetAvailablePhysicians(selectedTime);
            
            // Enable the physician button now that we have a time
            PhysicianSelectButton.IsEnabled = true;
            PhysicianSelectButton.Text = "Tap to Choose Physician";
            _selectedPhysician = null;
        }
    }

    // FIX: New method to handle Physician Button Click
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
            _medicalDataService.CreateAppointment(_selectedPatient, _selectedPhysician, _selectedTime);
            await DisplayAlert("Success", "Appointment created successfully", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to create appointment: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Not Implemented", "Delete functionality coming soon", "OK");
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