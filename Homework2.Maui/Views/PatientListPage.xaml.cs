using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace Homework2.Maui.Views;

public partial class PatientListPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private ObservableCollection<Patient?> _patients;
    private List<Patient?> _allPatientsCache = new List<Patient?>();
    private int _currentSortIndex = -1;

    // NEW: Dictionary to backup patient data for Cancel functionality
    private Dictionary<int, Patient> _originalPatients = new Dictionary<int, Patient>();

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
            var detailPage = new PatientDetailPage(_medicalDataService);
            detailPage.PatientId = patient.Id.ToString();
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

    // UPDATED: Inline Edit Logic with Backup
    private void OnInlineEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Patient patient)
        {
            if (patient.IsEditing)
            {
                // SAVE ACTION
                _medicalDataService.UpdatePatient(patient);
                
                // Remove from backup since we successfully saved
                if (patient.Id.HasValue) _originalPatients.Remove(patient.Id.Value);

                // Switch back to View Mode (Button appearance handled by XAML Triggers)
                patient.IsEditing = false;
            }
            else
            {
                // START EDITING ACTION - Create Backup
                if (patient.Id.HasValue && !_originalPatients.ContainsKey(patient.Id.Value))
                {
                    var clone = new Patient
                    {
                        Id = patient.Id,
                        name = patient.name,
                        address = patient.address,
                        birthdate = patient.birthdate,
                        race = patient.race,
                        gender = patient.gender
                    };
                    _originalPatients[patient.Id.Value] = clone;
                }
                
                patient.IsEditing = true;
            }
        }
    }

    // NEW: Cancel Logic
    private void OnInlineCancelClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Patient patient)
        {
            // Restore original values from backup
            if (patient.Id.HasValue && _originalPatients.ContainsKey(patient.Id.Value))
            {
                var original = _originalPatients[patient.Id.Value];
                patient.name = original.name;
                patient.address = original.address;
                patient.birthdate = original.birthdate;
                patient.race = original.race;
                patient.gender = original.gender;
                
                // Cleanup
                _originalPatients.Remove(patient.Id.Value);
            }
            
            // Exit edit mode
            patient.IsEditing = false;
        }
    }
}