using Homework2.Maui.Services;
using Homework2.Maui.Views;

namespace Homework2.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register the service as a Singleton (one instance for the whole app)
        builder.Services.AddSingleton<MedicalDataService>();

        // Register the pages
        builder.Services.AddTransient<PatientListPage>();
        builder.Services.AddTransient<PatientDetailPage>();
        
        builder.Services.AddTransient<PhysicianListPage>();
        builder.Services.AddTransient<PhysicianDetailPage>();

        builder.Services.AddTransient<AppointmentListPage>();
        builder.Services.AddTransient<AppointmentDetailPage>();

        return builder.Build();
    }
}