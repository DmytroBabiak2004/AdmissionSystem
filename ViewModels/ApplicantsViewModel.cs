using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdmissionSystem.Commands;
using AdmissionSystem.Models;
using AdmissionSystem.Services;

namespace AdmissionSystem.ViewModels;

public class ApplicantsViewModel : BaseViewModel
{
    private readonly ApplicantService _service;
    private ObservableCollection<Applicant> _applicants = new();
    private Applicant? _selectedApplicant;
    private string _searchText = string.Empty;
    private bool _isLoading;

    // Edit form fields
    private Applicant _editingApplicant = new();
    private bool _isEditPanelVisible;

    public ObservableCollection<Applicant> Applicants
    {
        get => _applicants;
        set => SetProperty(ref _applicants, value);
    }

    public Applicant? SelectedApplicant
    {
        get => _selectedApplicant;
        set { SetProperty(ref _selectedApplicant, value); OnPropertyChanged(nameof(HasSelection)); }
    }

    public string SearchText
    {
        get => _searchText;
        set { SetProperty(ref _searchText, value); _ = SearchAsync(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public Applicant EditingApplicant
    {
        get => _editingApplicant;
        set => SetProperty(ref _editingApplicant, value);
    }

    public bool IsEditPanelVisible
    {
        get => _isEditPanelVisible;
        set => SetProperty(ref _isEditPanelVisible, value);
    }

    public bool HasSelection => SelectedApplicant != null;

    public ICommand LoadCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand DeleteCommand { get; }

    public event Action<int>? ViewDetailsRequested;

    public ApplicantsViewModel(ApplicantService service)
    {
        _service = service;
        LoadCommand = new RelayCommand(async () => await LoadAsync());
        AddCommand = new RelayCommand(OpenAdd);
        EditCommand = new RelayCommand(OpenEdit, () => SelectedApplicant != null);
        SaveCommand = new RelayCommand(async () => await SaveAsync());
        CancelEditCommand = new RelayCommand(() => IsEditPanelVisible = false);
        DeleteCommand = new RelayCommand(async () => await DeleteAsync(), () => SelectedApplicant != null);

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _service.GetAllAsync();
            Applicants = new ObservableCollection<Applicant>(list);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка завантаження: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }

    private async Task SearchAsync()
    {
        try
        {
            var list = await _service.SearchAsync(SearchText);
            Applicants = new ObservableCollection<Applicant>(list);
        }
        catch { /* silent */ }
    }

    private void OpenAdd()
    {
        EditingApplicant = new Applicant { DateOfBirth = new DateTime(2000, 1, 1) };
        IsEditPanelVisible = true;
    }

    private void OpenEdit()
    {
        if (SelectedApplicant == null) return;
        EditingApplicant = new Applicant
        {
            Id = SelectedApplicant.Id,
            LastName = SelectedApplicant.LastName,
            FirstName = SelectedApplicant.FirstName,
            MiddleName = SelectedApplicant.MiddleName,
            DateOfBirth = SelectedApplicant.DateOfBirth,
            Gender = SelectedApplicant.Gender,
            Phone = SelectedApplicant.Phone,
            Email = SelectedApplicant.Email,
            Address = SelectedApplicant.Address,
            DocumentSeriesNumber = SelectedApplicant.DocumentSeriesNumber,
            TaxCode = SelectedApplicant.TaxCode,
            AverageGrade = SelectedApplicant.AverageGrade,
            RegistrationDate = SelectedApplicant.RegistrationDate
        };
        IsEditPanelVisible = true;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditingApplicant.LastName) ||
            string.IsNullOrWhiteSpace(EditingApplicant.FirstName))
        {
            MessageBox.Show("Прізвище та ім'я обов'язкові.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (EditingApplicant.Id == 0)
                await _service.CreateAsync(EditingApplicant);
            else
                await _service.UpdateAsync(EditingApplicant);

            IsEditPanelVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            var inner = ex.InnerException;

            // Цикл, щоб дістатися до самого "серця" помилки в базі
            while (inner != null)
            {
                message += "\n\nДеталі: " + inner.Message;
                inner = inner.InnerException;
            }

            MessageBox.Show(message, "Помилка збереження", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private async Task DeleteAsync()
    {
        if (SelectedApplicant == null) return;
        var result = MessageBox.Show(
            $"Видалити абітурієнта «{SelectedApplicant.FullName}»?",
            "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _service.DeleteAsync(SelectedApplicant.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
