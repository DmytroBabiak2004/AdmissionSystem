using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdmissionSystem.Commands;
using AdmissionSystem.Models;
using AdmissionSystem.Services;

namespace AdmissionSystem.ViewModels;

public class SpecialtiesViewModel : BaseViewModel
{
    private readonly SpecialtyService _service;
    private ObservableCollection<Specialty> _specialties = new();
    private Specialty? _selectedSpecialty;
    private Specialty _editingSpecialty = new();
    private bool _isEditPanelVisible;
    private bool _isLoading;

    public ObservableCollection<Specialty> Specialties
    {
        get => _specialties;
        set => SetProperty(ref _specialties, value);
    }

    public Specialty? SelectedSpecialty
    {
        get => _selectedSpecialty;
        set { SetProperty(ref _selectedSpecialty, value); OnPropertyChanged(nameof(HasSelection)); }
    }

    public Specialty EditingSpecialty
    {
        get => _editingSpecialty;
        set => SetProperty(ref _editingSpecialty, value);
    }

    public bool IsEditPanelVisible
    {
        get => _isEditPanelVisible;
        set => SetProperty(ref _isEditPanelVisible, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public bool HasSelection => SelectedSpecialty != null;

    public ICommand LoadCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ToggleActiveCommand { get; }

    public SpecialtiesViewModel(SpecialtyService service)
    {
        _service = service;
        LoadCommand = new RelayCommand(async () => await LoadAsync());
        AddCommand = new RelayCommand(OpenAdd);
        EditCommand = new RelayCommand(OpenEdit, () => SelectedSpecialty != null);
        SaveCommand = new RelayCommand(async () => await SaveAsync());
        CancelCommand = new RelayCommand(() => IsEditPanelVisible = false);
        DeleteCommand = new RelayCommand(async () => await DeleteAsync(), () => SelectedSpecialty != null);
        ToggleActiveCommand = new RelayCommand(async () => await ToggleActiveAsync(), () => SelectedSpecialty != null);

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _service.GetAllAsync();
            Specialties = new ObservableCollection<Specialty>(list);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка завантаження: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }

    private void OpenAdd()
    {
        EditingSpecialty = new Specialty { IsActive = true };
        IsEditPanelVisible = true;
    }

    private void OpenEdit()
    {
        if (SelectedSpecialty == null) return;
        EditingSpecialty = new Specialty
        {
            Id = SelectedSpecialty.Id,
            Name = SelectedSpecialty.Name,
            Code = SelectedSpecialty.Code,
            Faculty = SelectedSpecialty.Faculty,
            BudgetPlaces = SelectedSpecialty.BudgetPlaces,
            ContractPlaces = SelectedSpecialty.ContractPlaces,
            Description = SelectedSpecialty.Description,
            IsActive = SelectedSpecialty.IsActive
        };
        IsEditPanelVisible = true;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditingSpecialty.Name) || string.IsNullOrWhiteSpace(EditingSpecialty.Code))
        {
            MessageBox.Show("Назва та код спеціальності обов'язкові.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (EditingSpecialty.BudgetPlaces < 0 || EditingSpecialty.ContractPlaces < 0)
        {
            MessageBox.Show("Кількість місць не може бути від'ємною.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            if (EditingSpecialty.Id == 0)
                await _service.CreateAsync(EditingSpecialty);
            else
                await _service.UpdateAsync(EditingSpecialty);

            IsEditPanelVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedSpecialty == null) return;
        var result = MessageBox.Show(
            $"Видалити спеціальність «{SelectedSpecialty.Name}»?",
            "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _service.DeleteAsync(SelectedSpecialty.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ToggleActiveAsync()
    {
        if (SelectedSpecialty == null) return;
        SelectedSpecialty.IsActive = !SelectedSpecialty.IsActive;
        await _service.UpdateAsync(SelectedSpecialty);
        await LoadAsync();
    }
}
