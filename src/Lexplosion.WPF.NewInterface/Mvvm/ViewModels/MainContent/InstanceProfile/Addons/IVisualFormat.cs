namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.MainContent.InstanceProfile.Addons
{
    public interface IVisualFormat<T>
    {
        T CurrentFormat { get; }
        void ChangeVisualFormat(T format);
    }
}
