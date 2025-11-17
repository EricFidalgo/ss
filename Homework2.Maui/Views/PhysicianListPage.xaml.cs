using Homework2.Maui.Models;
using Homework2.Maui.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Homework2.Maui.Views;

// Converter to display list of specializations
public class ListToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is List<string> list && list.Any())
        {
            return string.Join(", ", list);
        }
        return "No specializations";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
        
        // Add converter to resources
        Resources.Add("ListToStringConverter", new ListToStringConverter());
        
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
        
        PhysicianCountLabel.Text = $"{_physicians.Count} physician{(_physicians.Count != 1 ? "s" : "")} registered";
    }

    private async void OnAddPhysicianClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(PhysicianDetailPage));
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