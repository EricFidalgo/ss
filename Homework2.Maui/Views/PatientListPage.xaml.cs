using Homework2.Maui.Models;
using Homework2.Maui.Services;

namespace Homework2.Maui.Views;

public partial class PatientListPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;

    public PatientListPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Reload the list every time the page appears
        patientsCollectionView.ItemsSource = _medicalDataService.GetPatients();
    }

    private async void OnAddPatientClicked(object sender, EventArgs e)
    {
        // Navigate to the detail page with no patient ID, indicating a new patient
        await Shell.Current.GoToAsync(nameof(PatientDetailPage));
    }

    private async void OnPatientSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Patient patient)
        {
            // Navigate to the detail page, passing the patient's ID
            await Shell.Current.GoToAsync($"{nameof(PatientDetailPage)}?id={patient.Id}");
            
            // Clear selection
            patientsCollectionView.SelectedItem = null;
        }
    }
}