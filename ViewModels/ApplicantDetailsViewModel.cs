using System;
using System.Collections.Generic;
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
    private ApplicantDocument? _selectedDocument;

    private Application _newApplication = new();
    private bool _isAddApplicationVisible;

    // --- Document modal ---
    private bool _isAddDocumentVisible;
    private ApplicantDocument _editingDocument = new();
    private bool _isEditingExistingDocument;

    private string _documentComment = string.Empty;
    private string _documentFileName = string.Empty;

    // Список типів документів для ComboBox
    public Array DocumentTypes => Enum.GetValues(typeof(DocumentType));

    // Форма навчання
    public List<string> FormOfEducationOptions { get; } = new() { "Денна", "Заочна" };

    private string _selectedFormOfEducation = "Денна";
    public string SelectedFormOfEducation
    {
        get => _selectedFormOfEducation;
        set
        {
            if (SetProperty(ref _selectedFormOfEducation, value))
                NewApplication.FormOfEducation = value == "Заочна" ? FormOfEducation.PartTime : FormOfEducation.FullTime;
        }
    }

    // Основа навчання
    public List<string> EducationBasisOptions { get; } = new() { "Бюджет", "Контракт" };

    private string _selectedEducationBasis = "Бюджет";
    public string SelectedEducationBasis
    {
        get => _selectedEducationBasis;
        set
        {
            if (SetProperty(ref _selectedEducationBasis, value))
                NewApplication.EducationBasis = value == "Контракт" ? EducationBasis.Contract : EducationBasis.Budget;
        }
    }

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

    public ApplicantDocument? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            if (SetProperty(ref _selectedDocument, value) && value != null)
            {
                DocumentComment = value.Comment ?? string.Empty;
                DocumentFileName = value.FileName ?? string.Empty;
            }
        }
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

    // Документ, який редагується у модальному вікні
    public ApplicantDocument EditingDocument
    {
        get => _editingDocument;
        set => SetProperty(ref _editingDocument, value);
    }

    public bool IsAddDocumentVisible
    {
        get => _isAddDocumentVisible;
        set => SetProperty(ref _isAddDocumentVisible, value);
    }

    public bool IsEditingExistingDocument
    {
        get => _isEditingExistingDocument;
        set => SetProperty(ref _isEditingExistingDocument, value);
    }

    public string DocumentComment
    {
        get => _documentComment;
        set => SetProperty(ref _documentComment, value);
    }

    public string DocumentFileName
    {
        get => _documentFileName;
        set => SetProperty(ref _documentFileName, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand AddApplicationCommand { get; }
    public ICommand SaveApplicationCommand { get; }
    public ICommand CancelApplicationCommand { get; }

    public ICommand VerifyDocumentCommand { get; }
    public ICommand MarkDocumentProvidedCommand { get; }
    public ICommand MarkDocumentMissingCommand { get; }
    public ICommand SaveDocumentCommentCommand { get; }

    // Нові команди для модального вікна документів
    public ICommand AddDocumentCommand { get; }
    public ICommand EditDocumentCommand { get; }
    public ICommand SaveDocumentCommand { get; }
    public ICommand CancelDocumentCommand { get; }
    public ICommand DeleteDocumentCommand { get; }

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
        MarkDocumentProvidedCommand = new RelayCommand<ApplicantDocument>(async d => await MarkDocumentProvidedAsync(d));
        MarkDocumentMissingCommand = new RelayCommand<ApplicantDocument>(async d => await MarkDocumentMissingAsync(d));
        SaveDocumentCommentCommand = new RelayCommand(async () => await SaveDocumentCommentAsync());

        AddDocumentCommand = new RelayCommand(OpenAddDocument);
        EditDocumentCommand = new RelayCommand<ApplicantDocument>(OpenEditDocument);
        SaveDocumentCommand = new RelayCommand(async () => await SaveDocumentAsync());
        CancelDocumentCommand = new RelayCommand(() => IsAddDocumentVisible = false);
        DeleteDocumentCommand = new RelayCommand<ApplicantDocument>(async d => await DeleteDocumentAsync(d));
    }

    public async Task LoadForApplicantAsync(int applicantId)
    {
        try
        {
            Applicant = await _applicantService.GetByIdAsync(applicantId);
            if (Applicant == null) return;

            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка завантаження: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Внутрішнє оновлення без скидання вибраного документа
    private async Task RefreshDataAsync()
    {
        if (Applicant == null) return;

        var apps = await _appService.GetByApplicantAsync(Applicant.Id);
        Applications = new ObservableCollection<Application>(apps);

        var docs = await _docService.GetByApplicantAsync(Applicant.Id);

        if (docs.Count == 0)
        {
            await _docService.CreateDefaultDocumentsForApplicantAsync(Applicant.Id);
            docs = await _docService.GetByApplicantAsync(Applicant.Id);
        }

        var prevSelectedId = SelectedDocument?.Id;
        Documents = new ObservableCollection<ApplicantDocument>(docs);

        // Відновлюємо вибір після оновлення
        if (prevSelectedId.HasValue)
        {
            var restored = Documents.FirstOrDefault(d => d.Id == prevSelectedId.Value);
            if (restored != null)
                SelectedDocument = restored;
        }

        var specs = await _specialtyService.GetActiveAsync();
        Specialties = new ObservableCollection<Specialty>(specs);
    }

    private async Task LoadAsync()
    {
        if (Applicant != null)
            await RefreshDataAsync();
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

        // Скидаємо вибір форми та основи навчання
        SelectedFormOfEducation = "Денна";
        SelectedEducationBasis = "Бюджет";

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

    // Відкрити модальне вікно для нового документа
    private void OpenAddDocument()
    {
        if (Applicant == null) return;

        EditingDocument = new ApplicantDocument
        {
            ApplicantId = Applicant.Id,
            DocumentType = DocumentType.Passport,
            FileName = string.Empty,
            IsProvided = false,
            IsVerified = false,
            UploadedAt = null,
            Comment = string.Empty
        };

        IsEditingExistingDocument = false;
        IsAddDocumentVisible = true;
    }

    // Відкрити модальне вікно для редагування існуючого документа
    private void OpenEditDocument(ApplicantDocument? doc)
    {
        if (doc == null) return;

        // Копіюємо документ щоб редагувати без side-effects
        EditingDocument = new ApplicantDocument
        {
            Id = doc.Id,
            ApplicantId = doc.ApplicantId,
            DocumentType = doc.DocumentType,
            FileName = doc.FileName,
            IsProvided = doc.IsProvided,
            IsVerified = doc.IsVerified,
            UploadedAt = doc.UploadedAt,
            Comment = doc.Comment
        };

        IsEditingExistingDocument = true;
        IsAddDocumentVisible = true;
    }

    // Зберегти документ (новий або оновлений)
    private async Task SaveDocumentAsync()
    {
        if (string.IsNullOrWhiteSpace(EditingDocument.FileName))
        {
            MessageBox.Show("Введіть назву документа.", "Валідація", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (IsEditingExistingDocument)
            {
                await _docService.UpdateAsync(EditingDocument);
            }
            else
            {
                await _docService.CreateAsync(EditingDocument);
            }

            IsAddDocumentVisible = false;
            await RefreshApplicationStatusesAsync();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка збереження документа: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task DeleteDocumentAsync(ApplicantDocument? doc)
    {
        if (doc == null) return;

        var result = MessageBox.Show(
            $"Видалити документ «{doc.FileName}»?",
            "Підтвердження",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _docService.DeleteAsync(doc.Id);
            await RefreshApplicationStatusesAsync();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ToggleVerifyDocumentAsync(ApplicantDocument? doc)
    {
        if (doc == null) return;

        try
        {
            if (!doc.IsProvided)
            {
                MessageBox.Show(
                    "Не можна підтвердити документ, який ще не подано.",
                    "Попередження",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            doc.IsVerified = !doc.IsVerified;

            await _docService.UpdateAsync(doc);
            await RefreshApplicationStatusesAsync();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка оновлення документа: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task MarkDocumentProvidedAsync(ApplicantDocument? doc)
    {
        if (doc == null) return;

        try
        {
            doc.IsProvided = true;
            doc.UploadedAt = DateTime.Now;

            await _docService.UpdateAsync(doc);
            await RefreshApplicationStatusesAsync();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка оновлення документа: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task MarkDocumentMissingAsync(ApplicantDocument? doc)
    {
        if (doc == null) return;

        try
        {
            doc.IsProvided = false;
            doc.IsVerified = false;
            doc.UploadedAt = null;
            // FileName НЕ чистимо — залишаємо назву для зручності

            await _docService.UpdateAsync(doc);
            await RefreshApplicationStatusesAsync();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка оновлення документа: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SaveDocumentCommentAsync()
    {
        if (SelectedDocument == null) return;

        try
        {
            SelectedDocument.Comment = DocumentComment;
            SelectedDocument.FileName = DocumentFileName;

            await _docService.UpdateAsync(SelectedDocument);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка збереження коментаря: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RefreshApplicationStatusesAsync()
    {
        if (Applicant == null) return;

        var docs = await _docService.GetByApplicantAsync(Applicant.Id);
        var apps = await _appService.GetByApplicantAsync(Applicant.Id);

        bool allProvided = docs.Count > 0 && docs.All(d => d.IsProvided);
        bool allVerified = docs.Count > 0 && docs.All(d => d.IsProvided && d.IsVerified);

        foreach (var app in apps)
        {
            if (allVerified)
                app.CurrentStatus = ApplicationStatus.AdmittedToCompetition;
            else if (allProvided)
                app.CurrentStatus = ApplicationStatus.Submitted;
            else
                app.CurrentStatus = ApplicationStatus.Draft;

            await _appService.UpdateAsync(app);
        }
    }

    public string GetDocumentStatus(ApplicantDocument? doc)
    {
        if (doc == null) return string.Empty;
        if (!doc.IsProvided) return "Не подано";
        if (doc.IsVerified) return "Підтверджено";
        return "Подано";
    }
}