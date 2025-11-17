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

    private async void OnInlineEditClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Physician physician)
        {
            await Shell.Current.GoToAsync($"{nameof(PhysicianDetailPage)}?id={physician.Id}");
        }
    }

    private async void OnDialogEditClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement button && button.BindingContext is Physician physician)
        {
            await Shell.Current.GoToAsync($"{nameof(PhysicianDetailPage)}?id={physician.Id}");
        }
    }

    // FIXED: Added missing Delete handler
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

    private async void OnPhysicianSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Physician physician)
        {
            await Shell.Current.GoToAsync($"{nameof(PhysicianDetailPage)}?id={physician.Id}");
            physiciansCollectionView.SelectedItem = null;
        }
    }
}