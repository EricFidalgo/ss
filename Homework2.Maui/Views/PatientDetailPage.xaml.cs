using Homework2.Maui.Models;
using Homework2.Maui.Services;

namespace Homework2.Maui.Views;

// This attribute tells the navigation system what "id" means
[QueryProperty(nameof(PatientId), "id")]
public partial class PatientDetailPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private Patient _currentPatient;
    
    // This property will be set by the navigation system
    public string PatientId { set
    {
        int patientId = Convert.ToInt32(value);
        // Load the patient from the service
        _currentPatient = _medicalDataService.GetPatient(patientId);
        
        if (_currentPatient == null)
        {
            // It's a new patient
            Title = "Add Patient";
            _currentPatient = new Patient();
        }
        else
        {
            // It's an existing patient
            Title = "Edit Patient";
            NameEntry.Text = _currentPatient.name;
            AddressEntry.Text = _currentPatient.address;
            BirthdatePicker.Date = _currentPatient.birthdate;
            RaceEntry.Text = _currentPatient.race;
            GenderEntry.Text = _currentPatient.gender;
            DeleteButton.IsVisible = true;
        }
    }}

    public PatientDetailPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Update the patient object from the form fields
        _currentPatient.name = NameEntry.Text;
        _currentPatient.address = AddressEntry.Text;
        _currentPatient.birthdate = BirthdatePicker.Date;
        _currentPatient.race = RaceEntry.Text;
        _currentPatient.gender = GenderEntry.Text;

        if (_currentPatient.Id == null) // New patient
        {
            _medicalDataService.AddPatient(_currentPatient);
        }
        else // Existing patient
        {
            _medicalDataService.UpdatePatient(_currentPatient);
        }

        // Go back to the previous page (the list)
        await Shell.Current.GoToAsync("..");
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_currentPatient.Id == null) return;

        bool answer = await DisplayAlert("Confirm", "Delete this patient?", "Yes", "No");
        if (answer)
        {
            _medicalDataService.DeletePatient(_currentPatient.Id.Value);
            await Shell.Current.GoToAsync("..");
        }
    }
}