using System.Windows;
using AdmissionSystem.Data;
using AdmissionSystem.Services;
using AdmissionSystem.ViewModels;
using AdmissionSystem.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdmissionSystem;

public partial class App : Application
{
    private ServiceProvider _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Ensure DB is created
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(ServiceCollection services)
    {
        // DbContext як Transient — кожен сервіс отримує свій екземпляр
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=AdmissionSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true"),
            ServiceLifetime.Transient,
            ServiceLifetime.Transient);

        // Services — Transient щоб кожен мав свій DbContext
        services.AddTransient<ApplicantService>();
        services.AddTransient<ApplicationService>();
        services.AddTransient<SpecialtyService>();
        services.AddTransient<DocumentService>();
        services.AddTransient<RankingService>();
        services.AddTransient<StatisticsService>();

        // ViewModels
        services.AddTransient<ApplicantsViewModel>();
        services.AddTransient<SpecialtiesViewModel>();
        services.AddTransient<ApplicationsViewModel>();
        services.AddTransient<RankingViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<ApplicantDetailsViewModel>();
        services.AddTransient<MainViewModel>();

        // Windows
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}