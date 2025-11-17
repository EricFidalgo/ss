using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Linq;
using System.Collections.Generic;

namespace Homework2.Maui.Views;

[QueryProperty(nameof(AppointmentId), "id")]
public partial class AppointmentDetailPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private Appointment _currentAppointment;
    private DateTime _selectedTime;
    private List<Patient?> _patients;
    private List<Physician?> _availablePhysicians;
    private List<DateTime> _availableSlots;

    public DateTime MinDate => DateTime.Today;
    public DateTime MaxDate => DateTime.Today.AddMonths(3);

    public string AppointmentId { set
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
    }}

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

        LoadPatients();
        AppointmentDatePicker.Date = GetNextWeekday(DateTime.Today);
    }

    private void LoadPatients()
    {
        _patients = _medicalDataService.GetPatients();
        
        // Set ItemsSource and ItemDisplayBinding
        PatientPicker.ItemsSource = _patients;
        PatientPicker.ItemDisplayBinding = new Binding("name");
        
        if (_patients.Any())
        {
            PatientPicker.SelectedIndex = 0;
        }
    }

    private void OnPatientChanged(object sender, EventArgs e)
    {
        UpdateAvailableSlots();
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
        PhysicianPicker.ItemsSource = null;
        PhysicianPicker.SelectedIndex = -1;

        if (PatientPicker.SelectedItem is not Patient selectedPatient)
        {
            TimeSlotsCollectionView.ItemsSource = null;
            return;
        }

        var selectedDate = AppointmentDatePicker.Date;
        var physicians = _medicalDataService.GetPhysicians();
        _availableSlots = _medicalDataService.GetAvailableSlots(selectedDate, selectedPatient, physicians);

        TimeSlotsCollectionView.ItemsSource = _availableSlots;
    }

    private void OnTimeSlotSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is DateTime selectedTime)
        {
            _selectedTime = selectedTime;
            SelectedTimeLabel.Text = $"Selected: {selectedTime:h:mm tt}";

            _availablePhysicians = _medicalDataService.GetAvailablePhysicians(selectedTime);
            
            PhysicianPicker.ItemsSource = _availablePhysicians;
            PhysicianPicker.ItemDisplayBinding = new Binding("name");
            
            if (_availablePhysicians.Any())
            {
                PhysicianPicker.SelectedIndex = 0;
            }
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (PatientPicker.SelectedItem is not Patient selectedPatient)
        {
            await DisplayAlert("Error", "Please select a patient", "OK");
            return;
        }

        if (_selectedTime == default)
        {
            await DisplayAlert("Error", "Please select a time slot", "OK");
            return;
        }

        if (PhysicianPicker.SelectedItem is not Physician selectedPhysician)
        {
            await DisplayAlert("Error", "Please select a physician", "OK");
            return;
        }

        try
        {
            _medicalDataService.CreateAppointment(selectedPatient, selectedPhysician, _selectedTime);
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