using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Collections.ObjectModel;

namespace Homework2.Maui.Views;

public partial class PatientListPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private ObservableCollection<Patient?> _patients;

    public PatientListPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
        _patients = new ObservableCollection<Patient?>();
        patientsCollectionView.ItemsSource = _patients;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshPatientList();
    }

    private void RefreshPatientList()
    {
        _patients.Clear();
        var patients = _medicalDataService.GetPatients();
        foreach (var patient in patients)
        {
            _patients.Add(patient);
        }
    }

    private async void OnAddPatientClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PatientDetailPage));
    }

    private async void OnPatientSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Patient patient)
        {
            await Shell.Current.GoToAsync($"{nameof(PatientDetailPage)}?id={patient.Id}");
            patientsCollectionView.SelectedItem = null;
        }
    }
}