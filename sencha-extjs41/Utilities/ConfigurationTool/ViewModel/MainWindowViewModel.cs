using System.Collections.ObjectModel;
using ConfigurationTool.DataAccess;

namespace ConfigurationTool.ViewModel
{
  class MainWindowViewModel
  {
    readonly ProxyRepository _proxyRepository;

    ObservableCollection<ViewModelBase> _viewModels;

    public MainWindowViewModel()
    {
      _proxyRepository = new ProxyRepository();
      // create an instance of our viewmodel and add it to our collection
      ProxyParamsViewModel ViewModel = new ProxyParamsViewModel(_proxyRepository);
      this.ViewModels.Add(ViewModel);
    }

    public ObservableCollection<ViewModelBase> ViewModels
    {
      get
      {
        if (_viewModels == null)
        {
          _viewModels = new ObservableCollection<ViewModelBase>();
        }
        return _viewModels;
      }
    }
  }
}
