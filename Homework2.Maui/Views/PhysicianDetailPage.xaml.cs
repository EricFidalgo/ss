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
                SetupNewPhysician();
            }
            else
            {
                int physicianId = Convert.ToInt32(value);
                var physician = _medicalDataService.GetPhysician(physicianId);

                if (physician == null)
                {
                    SetupNewPhysician();
                }
                else
                {
                    _currentPhysician = physician;
                    Title = "Edit Physician";
                    NameEntry.Text = _currentPhysician.name;
                    LicenseEntry.Text = _currentPhysician.license_number;
                    GraduationPicker.Date = _currentPhysician.graduation;
                    // Join the list into a string for display
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
        SetupNewPhysician();
    }

    private void SetupNewPhysician()
    {
        _currentPhysician = new Physician();
        Title = "Add Physician";
        DeleteButton.IsVisible = false;
        // Clear fields
        if (NameEntry != null) NameEntry.Text = string.Empty;
        if (LicenseEntry != null) LicenseEntry.Text = string.Empty;
        if (SpecializationsEntry != null) SpecializationsEntry.Text = string.Empty;
        if (GraduationPicker != null) GraduationPicker.Date = DateTime.Today;
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
        _currentPhysician.name = NameEntry.Text;
        _currentPhysician.license_number = LicenseEntry.Text;
        _currentPhysician.graduation = GraduationPicker.Date;

        // Safe split handling
        var specsText = SpecializationsEntry.Text ?? string.Empty;
        _currentPhysician.specializations = specsText
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();

        if (_currentPhysician.Id == null)
        {
            _medicalDataService.AddPhysician(_currentPhysician);
        }
        else
        {
            _medicalDataService.UpdatePhysician(_currentPhysician);
        }

        await ClosePageAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_currentPhysician.Id == null) return;

        bool answer = await DisplayAlert("Confirm", "Delete this physician?", "Yes", "No");
        if (answer)
        {
            _medicalDataService.DeletePhysician(_currentPhysician.Id.Value);
            await ClosePageAsync();
        }
    }
}