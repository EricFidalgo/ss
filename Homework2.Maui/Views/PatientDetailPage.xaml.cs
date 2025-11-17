using Homework2.Maui.Models;
using Homework2.Maui.Services;

namespace Homework2.Maui.Views;

[QueryProperty(nameof(PatientId), "id")]
public partial class PatientDetailPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private Patient _currentPatient;

    public string PatientId
    {
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Title = "Add Patient";
                _currentPatient = new Patient();
                DeleteButton.IsVisible = false;
            }
            else
            {
                int patientId = Convert.ToInt32(value);
                var patient = _medicalDataService.GetPatient(patientId);

                if (patient == null)
                {
                    Title = "Add Patient";
                    _currentPatient = new Patient();
                    DeleteButton.IsVisible = false;
                }
                else
                {
                    _currentPatient = patient;
                    Title = "Edit Patient";
                    NameEntry.Text = _currentPatient.name;
                    AddressEntry.Text = _currentPatient.address;
                    BirthdatePicker.Date = _currentPatient.birthdate;
                    RaceEntry.Text = _currentPatient.race;
                    GenderEntry.Text = _currentPatient.gender;
                    DeleteButton.IsVisible = true;
                }
            }
        }
    }

    public PatientDetailPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;

        _currentPatient = new Patient();
        Title = "Add Patient";
        DeleteButton.IsVisible = false;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        _currentPatient.name = NameEntry.Text;
        _currentPatient.address = AddressEntry.Text;
        _currentPatient.birthdate = BirthdatePicker.Date;
        _currentPatient.race = RaceEntry.Text;
        _currentPatient.gender = GenderEntry.Text;

        if (_currentPatient.Id == null)
        {
            _medicalDataService.AddPatient(_currentPatient);
        }
        else
        {
            _medicalDataService.UpdatePatient(_currentPatient);
        }

        await Shell.Current.GoToAsync("..");
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
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