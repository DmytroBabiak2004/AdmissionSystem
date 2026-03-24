using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdmissionSystem.Commands;
using AdmissionSystem.Enums;
using AdmissionSystem.Models;
using AdmissionSystem.Services;
using Application = AdmissionSystem.Models.Application;

namespace AdmissionSystem.ViewModels;

public class ApplicationsViewModel : BaseViewModel
{
    private readonly ApplicationService _appService;
    private readonly ApplicantService _applicantService;
    private readonly SpecialtyService _specialtyService;

    private ObservableCollection<Application> _applications = new();
    private ObservableCollection<Applicant> _applicants = new();
    private ObservableCollection<Specialty> _specialties = new();
    private ObservableCollection<ApplicationStatusHistory> _statusHistory = new();

    private Application? _selectedApplication;
    private Application _editingApplication = new();
    private bool _isEditPanelVisible;
    private bool _isLoading;
    private ApplicationStatus _newStatus;
    private string _statusChangeComment = string.Empty;
    private bool _isStatusChangePanelVisible;

    public ObservableCollection<Application> Applications
    {
        get => _applications;
        set => SetProperty(ref _applications, value);
    }

    public ObservableCollection<Applicant> Applicants
    {
        get => _applicants;
        set => SetProperty(ref _applicants, value);
    }

    public ObservableCollection<Specialty> Specialties
    {
        get => _specialties;
        set => SetProperty(ref _specialties, value);
    }

    public ObservableCollection<ApplicationStatusHistory> StatusHistory
    {
        get => _statusHistory;
        set => SetProperty(ref _statusHistory, value);
    }

    public Application? SelectedApplication
    {
        get => _selectedApplication;
        set
        {
            SetProperty(ref _selectedApplication, value);
            OnPropertyChanged(nameof(HasSelection));
            if (value != null)
                StatusHistory = new ObservableCollection<ApplicationStatusHistory>(
                    value.StatusHistory.OrderByDescending(h => h.ChangedAt));
        }
    }

    public Application EditingApplication
    {
        get => _editingApplication;
        set => SetProperty(ref _editingApplication, value);
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

    public bool HasSelection => SelectedApplication != null;

    public ApplicationStatus NewStatus
    {
        get => _newStatus;
        set => SetProperty(ref _newStatus, value);
    }

    public string StatusChangeComment
    {
        get => _statusChangeComment;
        set => SetProperty(ref _statusChangeComment, value);
    }

    public bool IsStatusChangePanelVisible
    {
        get => _isStatusChangePanelVisible;
        set => SetProperty(ref _isStatusChangePanelVisible, value);
    }

    public Array AllStatuses => Enum.GetValues(typeof(ApplicationStatus));
    public Array AllForms => Enum.GetValues(typeof(FormOfEducation));
    public Array AllBases => Enum.GetValues(typeof(EducationBasis));

    public ICommand LoadCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand OpenStatusChangeCommand { get; }
    public ICommand ApplyStatusChangeCommand { get; }
    public ICommand CancelStatusChangeCommand { get; }

    public ApplicationsViewModel(
        ApplicationService appService,
        ApplicantService applicantService,
        SpecialtyService specialtyService)
    {
        _appService = appService;
        _applicantService = applicantService;
        _specialtyService = specialtyService;

        LoadCommand = new RelayCommand(async () => await LoadAsync());
        AddCommand = new RelayCommand(async () => await OpenAddAsync());
        EditCommand = new RelayCommand(OpenEdit, () => SelectedApplication != null);
        SaveCommand = new RelayCommand(async () => await SaveAsync());
        CancelCommand = new RelayCommand(() => IsEditPanelVisible = false);
        DeleteCommand = new RelayCommand(async () => await DeleteAsync(), () => SelectedApplication != null);
        OpenStatusChangeCommand = new RelayCommand(OpenStatusChange, () => SelectedApplication != null);
        ApplyStatusChangeCommand = new RelayCommand(async () => await ApplyStatusChangeAsync());
        CancelStatusChangeCommand = new RelayCommand(() => IsStatusChangePanelVisible = false);

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var apps = await _appService.GetAllAsync();
            Applications = new ObservableCollection<Application>(apps);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка завантаження: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }

    private async Task OpenAddAsync()
    {
        var applicants = await _applicantService.GetAllAsync();
        Applicants = new ObservableCollection<Applicant>(applicants);
        var specialties = await _specialtyService.GetActiveAsync();
        Specialties = new ObservableCollection<Specialty>(specialties);

        EditingApplication = new Application
        {
            SubmissionDate = DateTime.Now,
            Priority = 1,
            FormOfEducation = FormOfEducation.FullTime,
            EducationBasis = EducationBasis.Budget
        };
        IsEditPanelVisible = true;
    }

    private void OpenEdit()
    {
        if (SelectedApplication == null) return;
        EditingApplication = new Application
        {
            Id = SelectedApplication.Id,
            ApplicantId = SelectedApplication.ApplicantId,
            SpecialtyId = SelectedApplication.SpecialtyId,
            FormOfEducation = SelectedApplication.FormOfEducation,
            EducationBasis = SelectedApplication.EducationBasis,
            Priority = SelectedApplication.Priority,
            CompetitiveScore = SelectedApplication.CompetitiveScore,
            Notes = SelectedApplication.Notes,
            SubmissionDate = SelectedApplication.SubmissionDate,
            CurrentStatus = SelectedApplication.CurrentStatus
        };
        IsEditPanelVisible = true;
    }

    private async Task SaveAsync()
    {
        if (EditingApplication.ApplicantId == 0)
        {
            MessageBox.Show("Оберіть абітурієнта.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (EditingApplication.SpecialtyId == 0)
        {
            MessageBox.Show("Оберіть спеціальність.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (EditingApplication.Priority <= 0)
        {
            MessageBox.Show("Пріоритет має бути додатним числом.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (EditingApplication.CompetitiveScore < 0)
        {
            MessageBox.Show("Конкурсний бал не може бути від'ємним.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (EditingApplication.Id == 0)
                await _appService.CreateAsync(EditingApplication, "operator1");
            else
                await _appService.UpdateAsync(EditingApplication);

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
        if (SelectedApplication == null) return;
        var result = MessageBox.Show("Видалити заяву?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _appService.DeleteAsync(SelectedApplication.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenStatusChange()
    {
        if (SelectedApplication == null) return;
        NewStatus = SelectedApplication.CurrentStatus;
        StatusChangeComment = string.Empty;
        IsStatusChangePanelVisible = true;
    }

    private async Task ApplyStatusChangeAsync()
    {
        if (SelectedApplication == null) return;
        try
        {
            var (success, error) = await _appService.ChangeStatusAsync(
                SelectedApplication.Id, NewStatus, "operator1", StatusChangeComment);

            if (!success)
            {
                MessageBox.Show(error, "Помилка зміни статусу", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsStatusChangePanelVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
