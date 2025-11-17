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
        // Check for new vs. existing *before* converting
        if (string.IsNullOrEmpty(value))
        {
            // It's a new patient
            Title = "Add Patient";
            _currentPatient = new Patient();
            DeleteButton.IsVisible = false;
        }
        else
        {
            // It's an existing patient
            int patientId = Convert.ToInt32(value);
            _currentPatient = _medicalDataService.GetPatient(patientId);

            if (_currentPatient == null)
            {
                // Fallback for bad ID
                Title = "Add Patient";
                _currentPatient = new Patient();
                DeleteButton.IsVisible = false;
            }
            else
            {
                // Fill form with existing data
                Title = "Edit Patient";
                NameEntry.Text = _currentPatient.name;
                AddressEntry.Text = _currentPatient.address;
                BirthdatePicker.Date = _currentPatient.birthdate;
                RaceEntry.Text = _currentPatient.race;
                GenderEntry.Text = _currentPatient.gender;
                DeleteButton.IsVisible = true;
            }
        }
    }}

    public PatientDetailPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;

        // --- THIS IS THE FIX ---
        // Create a new patient by default. 
        // If we are editing, the 'PatientId' property will
        // overwrite this with the one from the service.
        _currentPatient = new Patient();
        Title = "Add Patient";
        DeleteButton.IsVisible = false;
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