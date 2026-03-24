using System.Windows.Controls;
using AdmissionSystem.ViewModels;

namespace AdmissionSystem.Views;

public partial class ApplicantDetailsView : UserControl
{
    public ApplicantDetailsView()
    {
        InitializeComponent();
    }

    private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var mainWindow = System.Windows.Application.Current.MainWindow;
        if (mainWindow?.DataContext is MainViewModel mainVm)
        {
            mainVm.CurrentView = mainVm.ApplicantsVM;
            mainVm.CurrentPageTitle = "Абітурієнти";
        }
    }
}
