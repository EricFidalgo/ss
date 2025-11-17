using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace Homework2.Maui.Views;

public partial class AppointmentListPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private ObservableCollection<Appointment?> _appointments;

    public AppointmentListPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
        
        _appointments = new ObservableCollection<Appointment?>();
        appointmentsCollectionView.ItemsSource = _appointments;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshAppointmentList();
    }

    private void RefreshAppointmentList()
    {
        _appointments.Clear();
        
        var appointments = _medicalDataService.GetAppointments()
            .OrderBy(a => a?.hour)
            .ToList();
        
        foreach (var appointment in appointments)
        {
            _appointments.Add(appointment);
        }
        
        AppointmentCountLabel.Text = $"{_appointments.Count} appointment{(_appointments.Count != 1 ? "s" : "")} scheduled";
    }

    private async void OnAddAppointmentClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AppointmentDetailPage));
    }

    // Handles the "Edit Dialog" button (Navigate to Detail Page)
    private async void OnDialogEditClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Appointment appointment)
        {
            await Shell.Current.GoToAsync($"{nameof(AppointmentDetailPage)}?id={appointment.Id}");
        }
    }

    // Handles the "Delete" button
    private async void OnInlineDeleteClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Appointment appointment)
        {
            bool confirm = await DisplayAlert("Confirm", "Delete this appointment?", "Yes", "No");
            if (confirm)
            {
                _medicalDataService.DeleteAppointment(appointment.Id);
                RefreshAppointmentList();
            }
        }
    }

    private async void OnAppointmentSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Appointment appointment)
        {
            await Shell.Current.GoToAsync($"{nameof(AppointmentDetailPage)}?id={appointment.Id}");
            appointmentsCollectionView.SelectedItem = null;
        }
    }

    // --- Missing Event Handlers that caused the build error ---

    // 1. Handle the "Inline Edit" / "Save" toggle
    private void OnInlineEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Appointment appointment)
        {
            if (appointment.IsEditing)
            {
                // SAVE ACTION
                _medicalDataService.UpdateAppointment(appointment);
                
                // Switch back to View Mode
                appointment.IsEditing = false;
                button.Text = "✏️ Edit Inline";
                button.BackgroundColor = (Color)Application.Current.Resources["Primary"]; // Reset color
            }
            else
            {
                // START EDITING ACTION
                appointment.IsEditing = true;
                
                // Change button to "Save"
                button.Text = "💾 Save";
                button.BackgroundColor = Colors.Green;
            }
        }
    }

    // 2. Handle Patient Selection (Action Sheet)
    private async void OnPatientButtonClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is Appointment appointment)
        {
            var patients = _medicalDataService.GetPatients();
            var names = patients.Select(p => p.name).ToArray();

            string selectedName = await DisplayActionSheet("Select Patient", "Cancel", null, names);

            if (selectedName != "Cancel" && !string.IsNullOrEmpty(selectedName))
            {
                appointment.patients = patients.FirstOrDefault(p => p.name == selectedName);
            }
        }
    }

    // 3. Handle Physician Selection (Action Sheet)
    private async void OnPhysicianButtonClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is Appointment appointment)
        {
            var physicians = _medicalDataService.GetPhysicians();
            // Optional: Filter by availability if you want strictly valid choices
            // var available = physicians.Where(p => _medicalDataService.IsPhysicianAvailable(p, appointment.hour)).ToList();

            var names = physicians.Select(p => "Dr. " + p.name).ToArray();

            string selectedName = await DisplayActionSheet("Select Physician", "Cancel", null, names);

            if (selectedName != "Cancel" && !string.IsNullOrEmpty(selectedName))
            {
                // Strip "Dr. " prefix to find the object
                string cleanName = selectedName.Replace("Dr. ", "");
                appointment.physicians = physicians.FirstOrDefault(p => p.name == cleanName);
            }
        }
    }

    // 4. Handle Time Selection
    private async void OnTimeButtonClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is Appointment appointment)
        {
            // Simple approach: Ask for a time string or use a TimePicker. 
            // Since Maui doesn't have a modal TimePicker easily callable from C#, 
            // we can use a simple ActionSheet for slots or just assume the DatePicker covers the Date part.
            
            // Let's use the available slots logic you used in the DetailPage
            var slots = _medicalDataService.GetAvailableSlots(appointment.hour.Date, appointment.patients);
            
            var timeStrings = slots.Select(t => t.ToString("h:mm tt")).ToArray();
            
            string selectedTimeStr = await DisplayActionSheet("Select Time", "Cancel", null, timeStrings);
            
            if (selectedTimeStr != "Cancel" && DateTime.TryParse(selectedTimeStr, out DateTime selectedTime))
            {
                // Combine the existing Date with the new Time
                appointment.hour = appointment.hour.Date + selectedTime.TimeOfDay;
            }
        }
    }
}