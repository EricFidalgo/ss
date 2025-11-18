using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Homework2.Maui.Views;

public class ListToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is List<string> list && list.Any())
        {
            return string.Join(", ", list);
        }
        return "No specializations";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public partial class PhysicianListPage : ContentPage
{
    private readonly MedicalDataService _medicalDataService;
    private ObservableCollection<Physician?> _physicians;

    // NEW: Backup dictionary for Cancel functionality
    private Dictionary<int, Physician> _originalPhysicians = new Dictionary<int, Physician>();

    public PhysicianListPage(MedicalDataService medicalDataService)
    {
        InitializeComponent();
        _medicalDataService = medicalDataService;
        
        if (!Resources.ContainsKey("ListToStringConverter"))
        {
            Resources.Add("ListToStringConverter", new ListToStringConverter());
        }
        
        _physicians = new ObservableCollection<Physician?>();
        physiciansCollectionView.ItemsSource = _physicians;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshPhysicianList();
    }

    private void RefreshPhysicianList()
    {
        _physicians.Clear();
        var physicians = _medicalDataService.GetPhysicians();
        foreach (var physician in physicians)
        {
            _physicians.Add(physician);
        }
        
        if (PhysicianCountLabel != null)
            PhysicianCountLabel.Text = $"{_physicians.Count} physician{(_physicians.Count != 1 ? "s" : "")} registered";
    }

    private async void OnAddPhysicianClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PhysicianDetailPage));
    }

    private async void OnDialogEditClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Physician physician)
        {
            var detailPage = new PhysicianDetailPage(_medicalDataService);
            
            // Manually set the ID to trigger loading
            detailPage.PhysicianId = physician.Id.ToString();

            // Push as a Modal
            await Navigation.PushModalAsync(detailPage);
        }
    }

    private async void OnPhysicianSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Physician physician)
        {
            await Shell.Current.GoToAsync($"{nameof(PhysicianDetailPage)}?id={physician.Id}");
            physiciansCollectionView.SelectedItem = null;
        }
    }

    // UPDATED: Edit Logic with Backup
    private void OnInlineEditClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Physician physician)
        {
            if (physician.IsEditing)
            {
                // SAVE ACTION
                _medicalDataService.UpdatePhysician(physician);
                
                // Remove from backup since saved successfully
                if (physician.Id.HasValue) _originalPhysicians.Remove(physician.Id.Value);

                // Switch back to View Mode (Button styling handled by XAML Triggers)
                physician.IsEditing = false;
            }
            else
            {
                // START EDITING - Create Backup
                if (physician.Id.HasValue && !_originalPhysicians.ContainsKey(physician.Id.Value))
                {
                    var clone = new Physician
                    {
                        Id = physician.Id,
                        name = physician.name,
                        license_number = physician.license_number,
                        graduation = physician.graduation,
                        // Deep copy the list of specializations
                        specializations = new List<string>(physician.specializations)
                    };
                    _originalPhysicians[physician.Id.Value] = clone;
                }

                physician.IsEditing = true;
            }
        }
    }

    // NEW: Cancel Logic
    private void OnInlineCancelClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Physician physician)
        {
            // Restore from backup
            if (physician.Id.HasValue && _originalPhysicians.ContainsKey(physician.Id.Value))
            {
                var original = _originalPhysicians[physician.Id.Value];
                physician.name = original.name;
                physician.license_number = original.license_number;
                physician.graduation = original.graduation;
                
                // Restore list and trigger binding update (via property assignment if possible, or directly)
                physician.specializations = new List<string>(original.specializations);
                // Force update of the SpecializationText if the model logic handles it
                // (Assuming SpecializationText is a property in your model that updates on property change)
                
                _originalPhysicians.Remove(physician.Id.Value);
            }

            physician.IsEditing = false;
        }
    }

    private async void OnInlineDeleteClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Physician physician)
        {
            bool confirm = await DisplayAlert("Confirm", $"Delete Dr. {physician.name}?", "Yes", "No");
            if (confirm)
            {
                _medicalDataService.DeletePhysician(physician.Id ?? 0);
                RefreshPhysicianList();
            }
        }
    }
}