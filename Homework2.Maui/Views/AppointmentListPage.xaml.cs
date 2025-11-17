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

    // FIXED: Handles the "Edit" button inside the list row
    private async void OnInlineEditClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Appointment appointment)
        {
            await Shell.Current.GoToAsync($"{nameof(AppointmentDetailPage)}?id={appointment.Id}");
        }
    }

    // FIXED: Handles the "Delete" button inside the list row (if you added it)
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
}