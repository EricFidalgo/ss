using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace Homework2.Maui.Views;

public partial class PatientListPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private ObservableCollection<Patient?> _patients;
    private List<Patient?> _allPatientsCache = new List<Patient?>();

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
        _allPatientsCache = _medicalDataService.GetPatients();
        ApplySort(); 
    }

    private void OnSortChanged(object sender, EventArgs e)
    {
        ApplySort();
    }

    private void OnClearSort(object sender, EventArgs e)
    {
        var sortPicker = this.FindByName<Picker>("SortPicker");
        if (sortPicker != null)
        {
            sortPicker.SelectedIndex = -1;
        }
        RefreshPatientList();
    }

    private void ApplySort()
    {
        if (_allPatientsCache == null) return;

        var sortPicker = this.FindByName<Picker>("SortPicker");
        int selectedIndex = sortPicker?.SelectedIndex ?? -1;

        IEnumerable<Patient?> sortedList;

        switch (selectedIndex)
        {
            case 0: // Name (A-Z)
                sortedList = _allPatientsCache.OrderBy(p => p?.name);
                break;
            case 1: // Name (Z-A)
                sortedList = _allPatientsCache.OrderByDescending(p => p?.name);
                break;
            case 2: // DOB (Oldest First)
                sortedList = _allPatientsCache.OrderBy(p => p?.birthdate);
                break;
            case 3: // DOB (Youngest First)
                sortedList = _allPatientsCache.OrderByDescending(p => p?.birthdate);
                break;
            default:
                sortedList = _allPatientsCache;
                break;
        }

        _patients.Clear();
        foreach (var patient in sortedList)
        {
            _patients.Add(patient);
        }

        var countLabel = this.FindByName<Label>("PatientCountLabel");
        if (countLabel != null)
        {
            countLabel.Text = $"{_patients.Count} patient{(_patients.Count != 1 ? "s" : "")} registered";
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

    private async void OnInlineEditClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Patient patient)
        {
            await Shell.Current.GoToAsync($"{nameof(PatientDetailPage)}?id={patient.Id}");
        }
    }

    private async void OnDialogEditClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Patient patient)
        {
            await Shell.Current.GoToAsync($"{nameof(PatientDetailPage)}?id={patient.Id}");
        }
    }

    // FIXED: Added the missing Delete handler here
    private async void OnInlineDeleteClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Patient patient)
        {
            bool confirm = await DisplayAlert("Confirm", $"Delete patient {patient.name}?", "Yes", "No");
            if (confirm)
            {
                _medicalDataService.DeletePatient(patient.Id ?? 0);
                RefreshPatientList();
            }
        }
    }
}