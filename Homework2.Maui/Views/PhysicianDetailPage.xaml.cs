using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Linq;

namespace Homework2.Maui.Views;

// This attribute tells the navigation system what "id" means
[QueryProperty(nameof(PhysicianId), "id")]
public partial class PhysicianDetailPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private Physician _currentPhysician;

    // This property will be set by the navigation system
    public string PhysicianId { set
    {
        int physicianId = Convert.ToInt32(value);
        // Load the physician from the service
        _currentPhysician = _medicalDataService.GetPhysician(physicianId);
        
        if (_currentPhysician == null)
        {
            // It's a new physician
            Title = "Add Physician";
            _currentPhysician = new Physician();
        }
        else
        {
            // It's an existing physician
            Title = "Edit Physician";
            NameEntry.Text = _currentPhysician.name;
            LicenseEntry.Text = _currentPhysician.license_number;
            GraduationPicker.Date = _currentPhysician.graduation;
            SpecializationsEntry.Text = string.Join(", ", _currentPhysician.specializations);
            DeleteButton.IsVisible = true;
        }
    }}

    public PhysicianDetailPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Update the physician object from the form fields
        _currentPhysician.name = NameEntry.Text;
        _currentPhysician.license_number = LicenseEntry.Text;
        _currentPhysician.graduation = GraduationPicker.Date;
        
        // Convert comma-separated string back to a list
        _currentPhysician.specializations = SpecializationsEntry.Text
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        if (_currentPhysician.Id == null) // New physician
        {
            _medicalDataService.AddPhysician(_currentPhysician);
        }
        else // Existing physician
        {
            _medicalDataService.UpdatePhysician(_currentPhysician);
        }

        // Go back to the previous page (the list)
        await Shell.Current.GoToAsync("..");
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
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