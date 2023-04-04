namespace Lexplosion.Common
{
    public interface ISubmenu
    {
        public delegate void NavigationToMenuCallBack();
        public event NavigationToMenuCallBack NavigationToMainMenu;
    }
}
