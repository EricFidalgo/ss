using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Homework2.Maui.Views;

[QueryProperty(nameof(PatientId), "id")]
public partial class PatientDetailPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private Patient _currentPatient;

    // Handles the "id" passed from the list page
    public string PatientId
    {
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                SetupCreateMode();
            }
            else
            {
                // Load patient data asynchronously
                LoadPatient(Convert.ToInt32(value));
            }
        }
    }

    public PatientDetailPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
        SetupCreateMode(); // Default to create mode
    }

    private void SetupCreateMode()
    {
        Title = "Add Patient";
        _currentPatient = new Patient();
        DeleteButton.IsVisible = false;
        ClearFields();
    }

    private void ClearFields()
    {
        NameEntry.Text = string.Empty;
        AddressEntry.Text = string.Empty;
        BirthdatePicker.Date = DateTime.Today;
        RaceEntry.Text = string.Empty;
        GenderEntry.Text = string.Empty;
    }

    private async void LoadPatient(int id)
    {
        var patient = await _medicalDataService.GetPatient(id);

        if (patient == null)
        {
            await DisplayAlert("Error", "Patient not found.", "OK");
            await ClosePageAsync();
            return;
        }

        _currentPatient = patient;
        Title = "Edit Patient";
        
        // Populate UI
        NameEntry.Text = _currentPatient.name;
        AddressEntry.Text = _currentPatient.address;
        BirthdatePicker.Date = _currentPatient.birthdate;
        RaceEntry.Text = _currentPatient.race;
        GenderEntry.Text = _currentPatient.gender;
        
        DeleteButton.IsVisible = true;
    }

    private async Task ClosePageAsync()
    {
        if (Navigation.ModalStack.Count > 0 && Navigation.ModalStack.Last() == this)
        {
            await Navigation.PopModalAsync();
        }
        else
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        // Update model from UI
        _currentPatient.name = NameEntry.Text;
        _currentPatient.address = AddressEntry.Text;
        _currentPatient.birthdate = BirthdatePicker.Date;
        _currentPatient.race = RaceEntry.Text;
        _currentPatient.gender = GenderEntry.Text;

        if (_currentPatient.Id == null || _currentPatient.Id == 0)
        {
            await _medicalDataService.AddPatient(_currentPatient);
        }
        else
        {
            await _medicalDataService.UpdatePatient(_currentPatient);
        }

        await ClosePageAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_currentPatient.Id == null) return;

        bool answer = await DisplayAlert("Confirm", "Delete this patient?", "Yes", "No");
        if (answer)
        {
            await _medicalDataService.DeletePatient(_currentPatient.Id.Value);
            await ClosePageAsync();
        }
    }
}