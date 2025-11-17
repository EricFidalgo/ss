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

    private async void OnInlineEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Appointment appointment)
        {
            if (appointment.IsEditing)
            {
                // --- VALIDATION ON SAVE ---

                // 1. Check Date (Weekend)
                if (appointment.hour.DayOfWeek == DayOfWeek.Saturday || appointment.hour.DayOfWeek == DayOfWeek.Sunday)
                {
                    await DisplayAlert("Error", "Appointments cannot be scheduled on weekends.", "OK");
                    return;
                }

                // 2. Check Patient Availability
                // We manually check because the Service doesn't have "IsPatientAvailable" public, 
                // but we can check the list directly.
                if (appointment.patients != null)
                {
                    bool isPatientBusy = appointment.patients.unavailable_hours.Any(dt => 
                        dt == appointment.hour && 
                        dt != appointment.hour // Wait, the patient HAS this hour in their list already because of THIS appointment.
                        // To fix this logic, we should check if the time exists AND it belongs to a DIFFERENT appointment.
                        // But since we don't have back-refs easily, we rely on the fact that "UpdateAppointment"
                        // in the service handles the swap.
                        // However, for VALIDATION, we must check if they are double-booked.
                    );
                    
                    // Simpler check: Use the Service's GetAvailableSlots logic concepts
                    // If the patient has this hour marked unavailable, AND it's not this exact appointment instance...
                    // Since we are editing "in-place", the availability list might already contain this slot.
                    // The only way to be sure is to check if they are available at the NEW time (if changed) 
                    // or if we swapped patients.
                }

                // Let's use the Service methods to validate properly by asking "Is this valid?"
                // The Service helper `GetAvailableSlots` includes the logic to "Ignore" the current appointment ID.
                var validSlots = _medicalDataService.GetAvailableSlots(appointment.hour.Date, appointment.patients, appointment.Id);
                if (!validSlots.Contains(appointment.hour))
                {
                     await DisplayAlert("Error", "The selected Patient is not available at this time.", "OK");
                     return;
                }

                // 3. Check Physician Availability
                var validPhysicians = _medicalDataService.GetAvailablePhysicians(appointment.hour, appointment.Id);
                if (appointment.physicians != null && !validPhysicians.Any(p => p?.Id == appointment.physicians.Id))
                {
                    await DisplayAlert("Error", "The selected Physician is not available at this time.", "OK");
                    return;
                }

                // --- SAVE ---
                _medicalDataService.UpdateAppointment(appointment);
                
                // Switch back to View Mode
                appointment.IsEditing = false;
                button.Text = "✏️ Edit Inline";
                button.BackgroundColor = (Color)Application.Current.Resources["Primary"]; 
                RefreshAppointmentList(); // Refresh to re-sort if date changed
            }
            else
            {
                // START EDITING ACTION
                appointment.IsEditing = true;
                button.Text = "💾 Save";
                button.BackgroundColor = Colors.Green;
            }
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
    
    private async void OnDialogEditClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Appointment appointment)
        {
            await Shell.Current.GoToAsync($"{nameof(AppointmentDetailPage)}?id={appointment.Id}");
        }
    }

    // --- Smart Pickers ---

    private async void OnPatientButtonClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is Appointment appointment)
        {
            // Filter patients: Only show those available at the current appointment time
            // (Passing appointment.Id allows the service to say "The current patient is available for this slot")
            var allPatients = _medicalDataService.GetPatients();
            
            // We manually filter here because we want to see ALL valid patients for this specific time slot
            var availablePatients = allPatients.Where(p => 
            {
                // If this is the patient currently assigned to this appointment, they are "available" (to keep it)
                if (p.Id == appointment.patients?.Id) return true;

                // Otherwise, check if they are free at this time
                return !p.unavailable_hours.Contains(appointment.hour);
            }).ToList();

            if (!availablePatients.Any())
            {
                await DisplayAlert("Info", "No other patients available at this time.", "OK");
                return;
            }

            var names = availablePatients.Select(p => p.name).ToArray();
            string selectedName = await DisplayActionSheet("Select Patient (Available)", "Cancel", null, names);

            if (selectedName != "Cancel" && !string.IsNullOrEmpty(selectedName))
            {
                appointment.patients = availablePatients.FirstOrDefault(p => p.name == selectedName);
            }
        }
    }

    private async void OnPhysicianButtonClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is Appointment appointment)
        {
            // Use Service to get physicians available at this time (excluding current appointment lock)
            var availablePhysicians = _medicalDataService.GetAvailablePhysicians(appointment.hour, appointment.Id);

            if (!availablePhysicians.Any())
            {
                await DisplayAlert("Info", "No physicians available at this time.", "OK");
                return;
            }

            var names = availablePhysicians.Select(p => "Dr. " + p.name).ToArray();
            string selectedName = await DisplayActionSheet("Select Physician (Available)", "Cancel", null, names);

            if (selectedName != "Cancel" && !string.IsNullOrEmpty(selectedName))
            {
                string cleanName = selectedName.Replace("Dr. ", "");
                appointment.physicians = availablePhysicians.FirstOrDefault(p => p.name == cleanName);
            }
        }
    }

    private async void OnTimeButtonClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is Appointment appointment)
        {
            // Use Service to get slots where BOTH the current patient and ANY physician are available
            var slots = _medicalDataService.GetAvailableSlots(appointment.hour.Date, appointment.patients, appointment.Id);
            
            if (!slots.Any())
            {
                await DisplayAlert("Info", "No other time slots available for this date.", "OK");
                return;
            }

            var timeStrings = slots.Select(t => t.ToString("h:mm tt")).ToArray();
            string selectedTimeStr = await DisplayActionSheet("Select Time", "Cancel", null, timeStrings);
            
            if (selectedTimeStr != "Cancel" && DateTime.TryParse(selectedTimeStr, out DateTime selectedTime))
            {
                // Combine the existing Date with the new Time
                appointment.hour = appointment.hour.Date + selectedTime.TimeOfDay;
            }
        }
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        if (sender is DatePicker picker && picker.BindingContext is Appointment appointment)
        {
            if (e.NewDate.DayOfWeek == DayOfWeek.Saturday || e.NewDate.DayOfWeek == DayOfWeek.Sunday)
            {
                DisplayAlert("Invalid Date", "Appointments cannot be scheduled on weekends. Please select a weekday.", "OK");
                // Optional: Revert date or force to Monday?
                // picker.Date = e.OldDate; // This might cause a loop if not careful, usually better to just warn and let user fix.
            }
            else
            {
                // Update the appointment hour (keep time, change date)
                appointment.hour = e.NewDate.Date + appointment.hour.TimeOfDay;
            }
        }
    }
}