using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Collections.ObjectModel;

namespace Homework2.Maui.Views;

public partial class AppointmentListPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private ObservableCollection<Appointment?> _appointments;

    public AppointmentListPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
        
        // Create the ObservableCollection and bind it ONCE in the constructor
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
        // Clear and repopulate the ObservableCollection
        _appointments.Clear();
        var appointments = _medicalDataService.GetAppointments();
        foreach (var appointment in appointments)
        {
            _appointments.Add(appointment);
        }
    }

    private async void OnAddAppointmentClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AppointmentDetailPage));
    }

    private void OnAppointmentSelected(object sender, SelectionChangedEventArgs e)
    {
        // Logic for editing an appointment can be added here later.
        // For now, it does nothing.
    }
}