using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Linq;

namespace Homework2.Maui.Views;

[QueryProperty(nameof(AppointmentId), "id")]
public partial class AppointmentDetailPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private Appointment _currentAppointment;
    private DateTime _selectedTime;

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
            // For editing existing appointments (future enhancement)
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

        LoadPatients();
        AppointmentDatePicker.Date = GetNextWeekday(DateTime.Today);
    }

    private void LoadPatients()
    {
        var patients = _medicalDataService.GetPatients();
        PatientPicker.ItemsSource = patients;
        
        if (patients.Any())
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
        // Check if it's a weekend
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
        if (PatientPicker.SelectedItem is not Patient selectedPatient)
        {
            TimeSlotsCollectionView.ItemsSource = null;
            return;
        }

        var selectedDate = AppointmentDatePicker.Date;
        var physicians = _medicalDataService.GetPhysicians();
        var availableSlots = _medicalDataService.GetAvailableSlots(selectedDate, selectedPatient, physicians);

        TimeSlotsCollectionView.ItemsSource = availableSlots;
        SelectedTimeLabel.Text = "No time selected";
        PhysicianPicker.ItemsSource = null;
    }

    private void OnTimeSlotSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is DateTime selectedTime)
        {
            _selectedTime = selectedTime;
            SelectedTimeLabel.Text = $"Selected: {selectedTime:h:mm tt}";

            // Load available physicians for this time slot
            var availablePhysicians = _medicalDataService.GetAvailablePhysicians(selectedTime);
            PhysicianPicker.ItemsSource = availablePhysicians;
            
            if (availablePhysicians.Any())
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
        // For future implementation when editing appointments
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