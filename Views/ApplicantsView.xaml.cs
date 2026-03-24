using System.Windows.Controls;
using System.Windows.Input;
using AdmissionSystem.ViewModels;

namespace AdmissionSystem.Views;

public partial class ApplicantsView : UserControl
{
    public ApplicantsView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ApplicantsViewModel vm && vm.SelectedApplicant != null)
        {
            // Find MainViewModel and navigate to details
            var mainWindow = System.Windows.Application.Current.MainWindow;
            if (mainWindow?.DataContext is MainViewModel mainVm)
                mainVm.NavigateToApplicantDetails(vm.SelectedApplicant.Id);
        }
    }
}
