using System.Windows.Input;
using AdmissionSystem.Commands;

namespace AdmissionSystem.ViewModels;

public class MainViewModel : BaseViewModel
{
    private BaseViewModel _currentView;
    private string _currentPageTitle = "Головна";

    public ApplicantsViewModel ApplicantsVM { get; }
    public SpecialtiesViewModel SpecialtiesVM { get; }
    public ApplicationsViewModel ApplicationsVM { get; }
    public RankingViewModel RankingVM { get; }
    public StatisticsViewModel StatisticsVM { get; }
    public ApplicantDetailsViewModel ApplicantDetailsVM { get; }

    public BaseViewModel CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public string CurrentPageTitle
    {
        get => _currentPageTitle;
        set => SetProperty(ref _currentPageTitle, value);
    }

    public ICommand NavigateApplicantsCommand { get; }
    public ICommand NavigateSpecialtiesCommand { get; }
    public ICommand NavigateApplicationsCommand { get; }
    public ICommand NavigateRankingCommand { get; }
    public ICommand NavigateStatisticsCommand { get; }

    public MainViewModel(
        ApplicantsViewModel applicantsVM,
        SpecialtiesViewModel specialtiesVM,
        ApplicationsViewModel applicationsVM,
        RankingViewModel rankingVM,
        StatisticsViewModel statisticsVM,
        ApplicantDetailsViewModel applicantDetailsVM)
    {
        ApplicantsVM = applicantsVM;
        SpecialtiesVM = specialtiesVM;
        ApplicationsVM = applicationsVM;
        RankingVM = rankingVM;
        StatisticsVM = statisticsVM;
        ApplicantDetailsVM = applicantDetailsVM;

        _currentView = applicantsVM;

        NavigateApplicantsCommand = new RelayCommand(() =>
        {
            CurrentView = ApplicantsVM;
            CurrentPageTitle = "Абітурієнти";
        });
        NavigateSpecialtiesCommand = new RelayCommand(() =>
        {
            CurrentView = SpecialtiesVM;
            CurrentPageTitle = "Спеціальності";
        });
        NavigateApplicationsCommand = new RelayCommand(() =>
        {
            CurrentView = ApplicationsVM;
            CurrentPageTitle = "Заяви";
        });
        NavigateRankingCommand = new RelayCommand(() =>
        {
            CurrentView = RankingVM;
            CurrentPageTitle = "Рейтинг";
        });
        NavigateStatisticsCommand = new RelayCommand(() =>
        {
            CurrentView = StatisticsVM;
            CurrentPageTitle = "Статистика";
        });
    }

    public void NavigateToApplicantDetails(int applicantId)
    {
        _ = ApplicantDetailsVM.LoadForApplicantAsync(applicantId);
        CurrentView = ApplicantDetailsVM;
        CurrentPageTitle = "Картка абітурієнта";
    }
}
