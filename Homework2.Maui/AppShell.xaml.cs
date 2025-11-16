namespace Homework2.Maui;

using Homework2.Maui.Views; // Add this

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for pages we need to navigate to
        Routing.RegisterRoute(nameof(PatientDetailPage), typeof(PatientDetailPage));
        
        // You will add your other detail pages here later
        // Routing.RegisterRoute(nameof(PhysicianDetailPage), typeof(PhysicianDetailPage));
        // Routing.RegisterRoute(nameof(AppointmentDetailPage), typeof(AppointmentDetailPage));
    }
}