using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Linq;

namespace Homework2.Maui.Views;

[QueryProperty(nameof(PhysicianId), "id")]
public partial class PhysicianDetailPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private Physician _currentPhysician;

    public string PhysicianId
    {
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Title = "Add Physician";
                _currentPhysician = new Physician();
                DeleteButton.IsVisible = false;
            }
            else
            {
                int physicianId = Convert.ToInt32(value);
                var physician = _medicalDataService.GetPhysician(physicianId);

                if (physician == null)
                {
                    Title = "Add Physician";
                    _currentPhysician = new Physician();
                    DeleteButton.IsVisible = false;
                }
                else
                {
                    _currentPhysician = physician;
                    Title = "Edit Physician";
                    NameEntry.Text = _currentPhysician.name;
                    LicenseEntry.Text = _currentPhysician.license_number;
                    GraduationPicker.Date = _currentPhysician.graduation;
                    SpecializationsEntry.Text = string.Join(", ", _currentPhysician.specializations);
                    DeleteButton.IsVisible = true;
                }
            }
        }
    }

    public PhysicianDetailPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;

        // Initialize with a new physician by default
        _currentPhysician = new Physician();
        Title = "Add Physician";
        DeleteButton.IsVisible = false;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        _currentPhysician.name = NameEntry.Text;
        _currentPhysician.license_number = LicenseEntry.Text;
        _currentPhysician.graduation = GraduationPicker.Date;

        _currentPhysician.specializations = SpecializationsEntry.Text
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        if (_currentPhysician.Id == null)
        {
            _medicalDataService.AddPhysician(_currentPhysician);
        }
        else
        {
            _medicalDataService.UpdatePhysician(_currentPhysician);
        }

        await Shell.Current.GoToAsync("..");
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_currentPhysician.Id == null) return;

        bool answer = await DisplayAlert("Confirm", "Delete this physician?", "Yes", "No");
        if (answer)
        {
            _medicalDataService.DeletePhysician(_currentPhysician.Id.Value);
            await Shell.Current.GoToAsync("..");
        }
    }
}