namespace Homework2.Maui;

using Homework2.Maui.Views;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for pages we need to navigate to
        Routing.RegisterRoute(nameof(PatientDetailPage), typeof(PatientDetailPage));
        Routing.RegisterRoute(nameof(PhysicianDetailPage), typeof(PhysicianDetailPage));
        Routing.RegisterRoute(nameof(AppointmentDetailPage), typeof(AppointmentDetailPage));
    }
}