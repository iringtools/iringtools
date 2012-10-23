using System;
using System.ComponentModel;

namespace ConfigurationTool.ViewModel
{
  public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
  {
    protected ViewModelBase()
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChangedEventHandler handler = this.PropertyChanged;
      if (handler != null)
      {
        var e = new PropertyChangedEventArgs(propertyName);
        handler(this, e);
      }
    }

    public void Dispose()
    {
      this.OnDispose();
    }

    protected virtual void OnDispose()
    {
    }
  }
}
