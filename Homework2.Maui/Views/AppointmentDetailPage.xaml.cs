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

    // Data sources for the Pickers
    private List<Patient?> _patients;
    private List<Physician?> _allPhysicians;

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
        }
    }

    public AppointmentDetailPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
        _patients = new List<Patient?>();
        _allPhysicians = new List<Physician?>();

        // Default Date
        AppointmentDatePicker.Date = GetNextWeekday(DateTime.Today);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadData();
    }

    private void LoadData()
    {
        // 1. Load Lists
        _patients = _medicalDataService.GetPatients();
        _allPhysicians = _medicalDataService.GetPhysicians();

        // 2. Bind to Pickers
        PatientPicker.ItemsSource = _patients;
        PhysicianPicker.ItemsSource = _allPhysicians;

        // 3. If Editing, Pre-select values
        if (_currentAppointment != null)
        {
            // Select Patient
            PatientPicker.SelectedItem = _patients.FirstOrDefault(p => p?.Id == _currentAppointment.patients?.Id);

            // Select Date
            AppointmentDatePicker.Date = _currentAppointment.hour.Date;
            _selectedTime = _currentAppointment.hour;

            // Select Physician
            PhysicianPicker.SelectedItem = _allPhysicians.FirstOrDefault(p => p?.Id == _currentAppointment.physicians?.Id);
            
            UpdateAvailableSlots();
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
        UpdateAvailableSlots();
    }

    private void UpdateAvailableSlots()
    {
        // Use the selected patient from the Picker
        var selectedPatient = PatientPicker.SelectedItem as Patient;
        
        if (selectedPatient == null)
        {
            TimeSlotsCollectionView.ItemsSource = null;
            return;
        }

        var selectedDate = AppointmentDatePicker.Date;
        int? excludeId = _currentAppointment?.Id;

        // Logic to get slots based on Patient availability
        var availableSlots = _medicalDataService.GetAvailableSlots(selectedDate, selectedPatient, _allPhysicians, excludeId);
        TimeSlotsCollectionView.ItemsSource = availableSlots;
    }

    private void OnTimeSlotSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is DateTime selectedTime)
        {
            _selectedTime = selectedTime;
            
            // Optional: Filter Physician Picker based on who is free at this time
            // For now, we just keep the full list, but you could filter PhysicianPicker.ItemsSource here
        }
    }

    // This replaces OnSelectPatientClicked and OnSelectPhysicianClicked
    private void OnPhysicianSelectedIndexChanged(object sender, EventArgs e)
    {
        // Logic if you need to do something when physician changes
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var selectedPatient = PatientPicker.SelectedItem as Patient;
        var selectedPhysician = PhysicianPicker.SelectedItem as Physician;

        if (selectedPatient == null || selectedPhysician == null || _selectedTime == default)
        {
            await DisplayAlert("Error", "Please select Patient, Physician, and Time.", "OK");
            return;
        }

        if (_currentAppointment == null)
        {
            _medicalDataService.CreateAppointment(selectedPatient, selectedPhysician, _selectedTime);
        }
        else
        {
            var updated = new Appointment
            {
                Id = _currentAppointment.Id,
                patients = selectedPatient,
                physicians = selectedPhysician,
                hour = _selectedTime
            };
            _medicalDataService.UpdateAppointment(updated);
        }

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

    private void OnPatientSelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateAvailableSlots();
    }
}