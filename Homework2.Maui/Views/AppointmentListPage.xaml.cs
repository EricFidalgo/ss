using Homework2.Maui.Models;
using Homework2.Maui.Services;

namespace Homework2.Maui.Views;

public partial class AppointmentListPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;

    public AppointmentListPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        appointmentsCollectionView.ItemsSource = _medicalDataService.GetAppointments();
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