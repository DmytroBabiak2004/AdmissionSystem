using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdmissionSystem.Commands;
using AdmissionSystem.Models;
using AdmissionSystem.Services;

namespace AdmissionSystem.ViewModels;

public class RankingViewModel : BaseViewModel
{
    private readonly RankingService _rankingService;
    private readonly SpecialtyService _specialtyService;

    private ObservableCollection<Specialty> _specialties = new();
    private ObservableCollection<RankingEntry> _ranking = new();
    private Specialty? _selectedSpecialty;
    private bool _isLoading;
    private string _statusMessage = string.Empty;

    public ObservableCollection<Specialty> Specialties
    {
        get => _specialties;
        set => SetProperty(ref _specialties, value);
    }

    public ObservableCollection<RankingEntry> Ranking
    {
        get => _ranking;
        set => SetProperty(ref _ranking, value);
    }

    public Specialty? SelectedSpecialty
    {
        get => _selectedSpecialty;
        set
        {
            SetProperty(ref _selectedSpecialty, value);
            OnPropertyChanged(nameof(HasSelection));
            if (value != null) _ = LoadRankingAsync();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool HasSelection => SelectedSpecialty != null;

    public ICommand LoadSpecialtiesCommand { get; }
    public ICommand RecalculateCommand { get; }
    public ICommand RefreshRankingCommand { get; }

    public RankingViewModel(RankingService rankingService, SpecialtyService specialtyService)
    {
        _rankingService = rankingService;
        _specialtyService = specialtyService;

        LoadSpecialtiesCommand = new RelayCommand(async () => await LoadSpecialtiesAsync());
        RecalculateCommand = new RelayCommand(async () => await RecalculateAsync(), () => SelectedSpecialty != null);
        RefreshRankingCommand = new RelayCommand(async () => await LoadRankingAsync(), () => SelectedSpecialty != null);

        _ = LoadSpecialtiesAsync();
    }

    private async Task LoadSpecialtiesAsync()
    {
        try
        {
            var list = await _specialtyService.GetActiveAsync();
            Specialties = new ObservableCollection<Specialty>(list);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadRankingAsync()
    {
        if (SelectedSpecialty == null) return;
        IsLoading = true;
        try
        {
            var entries = await _rankingService.GetRankingAsync(SelectedSpecialty.Id);
            Ranking = new ObservableCollection<RankingEntry>(entries);
            StatusMessage = $"Рейтинг сформовано: {entries.Count} заяв допущено до конкурсу";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }

    private async Task RecalculateAsync()
    {
        if (SelectedSpecialty == null) return;
        IsLoading = true;
        try
        {
            await _rankingService.RecalculateAsync(SelectedSpecialty.Id);
            await LoadRankingAsync();
            MessageBox.Show("Рейтинг перераховано успішно.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }
}
