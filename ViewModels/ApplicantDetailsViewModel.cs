using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdmissionSystem.Commands;
using AdmissionSystem.Enums;
using AdmissionSystem.Models;
using AdmissionSystem.Services;
using Application = AdmissionSystem.Models.Application;

namespace AdmissionSystem.ViewModels;

public class ApplicantDetailsViewModel : BaseViewModel
{
    private readonly ApplicantService _applicantService;
    private readonly ApplicationService _appService;
    private readonly DocumentService _docService;
    private readonly SpecialtyService _specialtyService;

    private Applicant? _applicant;
    private ObservableCollection<Application> _applications = new();
    private ObservableCollection<ApplicantDocument> _documents = new();
    private ObservableCollection<Specialty> _specialties = new();
    private Application? _selectedApplication;

    // New application form
    private Application _newApplication = new();
    private bool _isAddApplicationVisible;

    public Applicant? Applicant
    {
        get => _applicant;
        set => SetProperty(ref _applicant, value);
    }

    public ObservableCollection<Application> Applications
    {
        get => _applications;
        set => SetProperty(ref _applications, value);
    }

    public ObservableCollection<ApplicantDocument> Documents
    {
        get => _documents;
        set => SetProperty(ref _documents, value);
    }

    public ObservableCollection<Specialty> Specialties
    {
        get => _specialties;
        set => SetProperty(ref _specialties, value);
    }

    public Application? SelectedApplication
    {
        get => _selectedApplication;
        set => SetProperty(ref _selectedApplication, value);
    }

    public Application NewApplication
    {
        get => _newApplication;
        set => SetProperty(ref _newApplication, value);
    }

    public bool IsAddApplicationVisible
    {
        get => _isAddApplicationVisible;
        set => SetProperty(ref _isAddApplicationVisible, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand AddApplicationCommand { get; }
    public ICommand SaveApplicationCommand { get; }
    public ICommand CancelApplicationCommand { get; }
    public ICommand VerifyDocumentCommand { get; }

    public ApplicantDetailsViewModel(
        ApplicantService applicantService,
        ApplicationService appService,
        DocumentService docService,
        SpecialtyService specialtyService)
    {
        _applicantService = applicantService;
        _appService = appService;
        _docService = docService;
        _specialtyService = specialtyService;

        LoadCommand = new RelayCommand(async () => await LoadAsync());
        AddApplicationCommand = new RelayCommand(OpenAddApplication);
        SaveApplicationCommand = new RelayCommand(async () => await SaveApplicationAsync());
        CancelApplicationCommand = new RelayCommand(() => IsAddApplicationVisible = false);
        VerifyDocumentCommand = new RelayCommand<ApplicantDocument>(async d => await ToggleVerifyDocumentAsync(d));
    }

    public async Task LoadForApplicantAsync(int applicantId)
    {
        try
        {
            Applicant = await _applicantService.GetByIdAsync(applicantId);
            if (Applicant == null) return;

            var apps = await _appService.GetByApplicantAsync(applicantId);
            Applications = new ObservableCollection<Application>(apps);

            var docs = await _docService.GetByApplicantAsync(applicantId);
            Documents = new ObservableCollection<ApplicantDocument>(docs);

            var specs = await _specialtyService.GetActiveAsync();
            Specialties = new ObservableCollection<Specialty>(specs);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка завантаження: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadAsync()
    {
        if (Applicant != null)
            await LoadForApplicantAsync(Applicant.Id);
    }

    private void OpenAddApplication()
    {
        if (Applicant == null) return;
        NewApplication = new Application
        {
            ApplicantId = Applicant.Id,
            Priority = Applications.Count + 1,
            SubmissionDate = DateTime.Now
        };
        IsAddApplicationVisible = true;
    }

    private async Task SaveApplicationAsync()
    {
        if (NewApplication.SpecialtyId == 0)
        {
            MessageBox.Show("Оберіть спеціальність.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            await _appService.CreateAsync(NewApplication, "operator1");
            IsAddApplicationVisible = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ToggleVerifyDocumentAsync(ApplicantDocument? doc)
    {
        if (doc == null) return;
        doc.IsVerified = !doc.IsVerified;
        await _docService.UpdateAsync(doc);
        await LoadAsync();
    }
}
