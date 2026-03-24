using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdmissionSystem.Commands;
using AdmissionSystem.Services;

namespace AdmissionSystem.ViewModels;

public class StatisticsViewModel : BaseViewModel
{
    private readonly StatisticsService _service;

    private int _totalApplicants;
    private int _totalApplications;
    private int _budgetRecommended;
    private int _contractRecommended;
    private int _reserved;
    private ObservableCollection<SpecialtyStats> _bySpecialty = new();
    private ObservableCollection<StatusStats> _byStatus = new();
    private bool _isLoading;

    public int TotalApplicants { get => _totalApplicants; set => SetProperty(ref _totalApplicants, value); }
    public int TotalApplications { get => _totalApplications; set => SetProperty(ref _totalApplications, value); }
    public int BudgetRecommended { get => _budgetRecommended; set => SetProperty(ref _budgetRecommended, value); }
    public int ContractRecommended { get => _contractRecommended; set => SetProperty(ref _contractRecommended, value); }
    public int Reserved { get => _reserved; set => SetProperty(ref _reserved, value); }

    public ObservableCollection<SpecialtyStats> BySpecialty
    {
        get => _bySpecialty;
        set => SetProperty(ref _bySpecialty, value);
    }

    public ObservableCollection<StatusStats> ByStatus
    {
        get => _byStatus;
        set => SetProperty(ref _byStatus, value);
    }

    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

    public ICommand RefreshCommand { get; }

    public StatisticsViewModel(StatisticsService service)
    {
        _service = service;
        RefreshCommand = new RelayCommand(async () => await LoadAsync());
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var stats = await _service.GetStatisticsAsync();
            TotalApplicants = stats.TotalApplicants;
            TotalApplications = stats.TotalApplications;
            BudgetRecommended = stats.BudgetRecommended;
            ContractRecommended = stats.ContractRecommended;
            Reserved = stats.Reserved;
            BySpecialty = new ObservableCollection<SpecialtyStats>(stats.BySpecialty);
            ByStatus = new ObservableCollection<StatusStats>(stats.ByStatus);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка завантаження статистики: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }
}
