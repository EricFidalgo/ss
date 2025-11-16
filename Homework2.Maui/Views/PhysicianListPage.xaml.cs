using Homework2.Maui.Models;
using Homework2.Maui.Services;

namespace Homework2.Maui.Views;

public partial class PhysicianListPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;

    public PhysicianListPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Reload the list every time the page appears
        physiciansCollectionView.ItemsSource = _medicalDataService.GetPhysicians();
    }

    private async void OnAddPhysicianClicked(object sender, EventArgs e)
    {
        // Navigate to the detail page with no physician ID, indicating a new physician
        await Shell.Current.GoToAsync(nameof(PhysicianDetailPage));
    }

    private async void OnPhysicianSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Physician physician)
        {
            // Navigate to the detail page, passing the physician's ID
            await Shell.Current.GoToAsync($"{nameof(PhysicianDetailPage)}?id={physician.Id}");
            
            // Clear selection
            physiciansCollectionView.SelectedItem = null;
        }
    }
}