using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace Homework2.Maui.Views;

public partial class AppointmentListPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private ObservableCollection<Appointment?> _appointments;
    
    // Backup for Appointment fields (Date, Time, Patient Ref)
    private Dictionary<int, Appointment> _originalAppointments = new Dictionary<int, Appointment>();
    
    // Backup for Reference Types (Diagnoses & Treatments)
    private Dictionary<int, List<string>> _originalDiagnoses = new Dictionary<int, List<string>>();
    private Dictionary<int, List<Treatment>> _originalTreatments = new Dictionary<int, List<Treatment>>();

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
        
        if (AppointmentCountLabel != null)
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

                if (appointment.hour.DayOfWeek == DayOfWeek.Saturday || appointment.hour.DayOfWeek == DayOfWeek.Sunday)
                {
                    await DisplayAlert("Error", "Appointments cannot be scheduled on weekends.", "OK");
                    return;
                }

                var validSlots = _medicalDataService.GetAvailableSlots(appointment.hour.Date, appointment.patients, appointment.Id);
                if (!validSlots.Contains(appointment.hour))
                {
                     await DisplayAlert("Error", "The selected Patient is not available at this time.", "OK");
                     return;
                }

                var validPhysicians = _medicalDataService.GetAvailablePhysicians(appointment.hour, appointment.Id);
                if (appointment.physicians != null && !validPhysicians.Any(p => p?.Id == appointment.physicians.Id))
                {
                    await DisplayAlert("Error", "The selected Physician is not available at this time.", "OK");
                    return;
                }

                // --- SAVE ---
                
                // 1. Save Appointment Changes (Date, Time, Refs, Treatments)
                _medicalDataService.UpdateAppointment(appointment);
                
                // 2. Save Patient Changes (Diagnoses)
                if (appointment.patients != null)
                {
                    _medicalDataService.UpdatePatient(appointment.patients);
                }
                
                // Cleanup Backups
                if (_originalAppointments.ContainsKey(appointment.Id))
                    _originalAppointments.Remove(appointment.Id);
                if (_originalDiagnoses.ContainsKey(appointment.Id))
                    _originalDiagnoses.Remove(appointment.Id);
                if (_originalTreatments.ContainsKey(appointment.Id))
                    _originalTreatments.Remove(appointment.Id);

                appointment.IsEditing = false;
                RefreshAppointmentList(); 
            }
            else
            {
                // --- START EDITING (Create Backups) ---
                
                // 1. Backup Appointment properties
                if (!_originalAppointments.ContainsKey(appointment.Id))
                {
                    var clone = new Appointment
                    {
                        Id = appointment.Id,
                        hour = appointment.hour,
                        patients = appointment.patients,
                        physicians = appointment.physicians
                    };
                    _originalAppointments[appointment.Id] = clone;
                }

                // 2. Backup Diagnoses List
                if (appointment.patients != null && !_originalDiagnoses.ContainsKey(appointment.Id))
                {
                    _originalDiagnoses[appointment.Id] = new List<string>(appointment.patients.diagnoses);
                }

                // 3. Backup Treatments List
                if (!_originalTreatments.ContainsKey(appointment.Id))
                {
                    // Create a shallow copy of the list structure
                    _originalTreatments[appointment.Id] = new List<Treatment>(appointment.Treatments);
                }
                
                appointment.IsEditing = true;
            }
        }
    }

    private void OnInlineCancelClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Appointment appointment)
        {
            // 1. Restore Appointment Properties
            if (_originalAppointments.ContainsKey(appointment.Id))
            {
                var original = _originalAppointments[appointment.Id];
                appointment.hour = original.hour;
                appointment.patients = original.patients;
                appointment.physicians = original.physicians;
                
                _originalAppointments.Remove(appointment.Id);
            }

            // 2. Restore Diagnoses List
            if (_originalDiagnoses.ContainsKey(appointment.Id) && appointment.patients != null)
            {
                var oldDiagnoses = _originalDiagnoses[appointment.Id];
                
                appointment.patients.diagnoses.Clear();
                foreach (var diag in oldDiagnoses)
                {
                    appointment.patients.diagnoses.Add(diag);
                }

                _originalDiagnoses.Remove(appointment.Id);
            }

            // 3. Restore Treatments List
            if (_originalTreatments.ContainsKey(appointment.Id))
            {
                var oldTreatments = _originalTreatments[appointment.Id];
                
                appointment.Treatments.Clear();
                foreach (var t in oldTreatments)
                {
                    appointment.Treatments.Add(t);
                }
                
                _originalTreatments.Remove(appointment.Id);
            }
            
            appointment.IsEditing = false;
        }
    }

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
            var allPatients = _medicalDataService.GetPatients();
            var availablePatients = allPatients.Where(p => 
            {
                if (p.Id == appointment.patients?.Id) return true;
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
            }
            else
            {
                appointment.hour = e.NewDate.Date + appointment.hour.TimeOfDay;
            }
        }
    }
    
    // --- Diagnoses Handlers ---

    private void OnInlineAddDiagnosisClicked(object sender, EventArgs e)
    {
        if (sender is Button addButton && addButton.CommandParameter is Appointment appointment)
        {
            Grid? addGrid = addButton.Parent as Grid;
            Entry? newDiagnosisEntry = addGrid?.Children.OfType<Entry>().FirstOrDefault();

            if (newDiagnosisEntry != null)
            {
                string newDiag = newDiagnosisEntry.Text;

                if (!string.IsNullOrWhiteSpace(newDiag) && appointment.patients != null)
                {
                    appointment.patients.diagnoses.Add(newDiag);
                    newDiagnosisEntry.Text = string.Empty;
                }
            }
        }
    }

    private void OnInlineDeleteDiagnosisClicked(object sender, EventArgs e)
    {
        if (sender is Button deleteButton && deleteButton.CommandParameter is Appointment appointment)
        {
            if (deleteButton.BindingContext is string diagnosisToRemove && appointment.patients != null)
            {
                appointment.patients.diagnoses.Remove(diagnosisToRemove);
            }
        }
    }

    // --- Treatment Handlers (NEW) ---

    private void OnInlineAddTreatmentClicked(object sender, EventArgs e)
    {
        if (sender is Button addButton && addButton.CommandParameter is Appointment appointment)
        {
            Grid? addGrid = addButton.Parent as Grid;
            if (addGrid == null) return;

            Entry? nameEntry = addGrid.Children.OfType<Entry>().FirstOrDefault(x => x.Placeholder == "Treatment Name");
            Entry? costEntry = addGrid.Children.OfType<Entry>().FirstOrDefault(x => x.Placeholder == "Cost");

            if (nameEntry != null && costEntry != null)
            {
                string name = nameEntry.Text;
                if (string.IsNullOrWhiteSpace(name))
                {
                     DisplayAlert("Error", "Please enter a treatment name.", "OK");
                     return;
                }

                if (decimal.TryParse(costEntry.Text, out decimal cost))
                {
                    appointment.Treatments.Add(new Treatment { Name = name, Cost = cost });
                    
                    nameEntry.Text = string.Empty;
                    costEntry.Text = string.Empty;

                    // Force UI Update for the List above this grid
                    if (addGrid.Parent is VerticalStackLayout parentLayout)
                    {
                        // Find the sibling StackLayout that holds the list
                        var listStack = parentLayout.Children.OfType<StackLayout>().FirstOrDefault();
                        if (listStack != null)
                        {
                             // Resetting the source forces the BindableLayout to redraw
                             BindableLayout.SetItemsSource(listStack, null);
                             BindableLayout.SetItemsSource(listStack, appointment.Treatments);
                        }
                    }
                }
                else
                {
                    DisplayAlert("Error", "Please enter a valid numeric cost.", "OK");
                }
            }
        }
    }

    private void OnInlineDeleteTreatmentClicked(object sender, EventArgs e)
    {
        if (sender is Button deleteButton && deleteButton.CommandParameter is Appointment appointment)
        {
            if (deleteButton.BindingContext is Treatment treatmentToRemove)
            {
                appointment.Treatments.Remove(treatmentToRemove);
                
                // Force UI Update: Find the parent StackLayout
                // Button -> Grid -> StackLayout (The list container)
                if (deleteButton.Parent is Grid itemGrid && itemGrid.Parent is StackLayout listStack)
                {
                     BindableLayout.SetItemsSource(listStack, null);
                     BindableLayout.SetItemsSource(listStack, appointment.Treatments);
                }
            }
        }
    }
}