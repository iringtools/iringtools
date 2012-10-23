using Ninject.Modules;

namespace org.iringtools.nhibernate
{
  public class NHibernateModule : NinjectModule
  {
    public override void Load()
    {
      Bind<NHibernateSettings>().ToSelf().InSingletonScope();
    }
  }
}
