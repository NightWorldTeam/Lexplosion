namespace Lexplosion.Gui
{
    public interface ISubmenu
    {
        public delegate void NavigationToMenuCallBack();
        public event NavigationToMenuCallBack NavigationToMainMenu;
    }
}
