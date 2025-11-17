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
    private int _currentSortIndex = -1;

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

    private async void OnSortButtonClicked(object sender, EventArgs e)
    {
        var sortOptions = new[]
        {
            "Name (A-Z)",
            "Name (Z-A)",
            "Date of Birth (Oldest)",
            "Date of Birth (Newest)"
        };

        string action = await DisplayActionSheet(
            "Sort Patients By",
            "Cancel",
            "Clear Sort",
            sortOptions
        );

        if (action == "Cancel")
            return;

        if (action == "Clear Sort")
        {
            _currentSortIndex = -1;
            SortButton.Text = "Sort Patients";
            SortButton.BackgroundColor = Color.FromArgb("#512BD4");
            RefreshPatientList();
            return;
        }

        _currentSortIndex = Array.IndexOf(sortOptions, action);
        if (_currentSortIndex >= 0)
        {
            SortButton.Text = $"Sorted: {sortOptions[_currentSortIndex]}";
            SortButton.BackgroundColor = Colors.Green;
            ApplySort();
        }
    }

    private void ApplySort()
    {
        if (_allPatientsCache == null) return;

        IEnumerable<Patient?> sortedList;

        switch (_currentSortIndex)
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

    private async void OnDialogEditClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Patient patient)
        {
            // 1. Create the page instance manually (injecting the service)
            var detailPage = new PatientDetailPage(_medicalDataService);
            
            // 2. Manually trigger the data loading (since QueryProperty won't fire for Modals)
            detailPage.PatientId = patient.Id.ToString();

            // 3. Push as a Modal (Dialog)
            await Navigation.PushModalAsync(detailPage);
        }
    }

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

    private void OnInlineEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Patient patient)
        {
            if (patient.IsEditing)
            {
                // SAVE ACTION
                // Assuming you have an UpdatePatient method in your service
                _medicalDataService.UpdatePatient(patient);
                
                // Switch back to View Mode
                patient.IsEditing = false;
                button.Text = "✏️ Edit Inline";
                button.BackgroundColor = (Color)Application.Current.Resources["Primary"]; 
            }
            else
            {
                // START EDITING ACTION
                patient.IsEditing = true;
                
                // Change button to "Save"
                button.Text = "💾 Save";
                button.BackgroundColor = Colors.Green;
            }
        }
    }
}