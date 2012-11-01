using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using ProxyConfig.ViewModel;

namespace ProxyConfig
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      MainWindow window = new MainWindow();
      var viewModel = new MainWindowViewModel();
      window.DataContext = viewModel;
      window.Show();
    }
  }
}
