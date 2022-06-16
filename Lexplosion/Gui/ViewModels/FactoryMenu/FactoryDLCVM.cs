using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui.ViewModels.FactoryMenu
{
    public class Mod
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Curseforge { get; set; }
        public bool IsEnable { get; set; }
        public bool IsUpdateAvaliable { get; set; }
        public BitmapImage Logo { get; set; }
    }

    public enum MarketDLCType 
    {
        Mods,
        Resourcepacks,
        World
    }

    public class FactoryDLCVM : VMBase
    {
        private MainViewModel _mainViewModel;

        public ObservableCollection<Mod> InstalledMods { get; set; }

        private RelayCommand _curseforgeCommand;
        private RelayCommand _updateCommand;
        private RelayCommand _deleteCommand;

        private RelayCommand _openMarket;

        #region Command

        public RelayCommand CurseforgeCommand
        {
            get => _curseforgeCommand ?? (new RelayCommand(obj =>
            {
                System.Diagnostics.Process.Start((string)obj);
            }));
        }

        public RelayCommand UpdateCommand
        {
            get => _updateCommand ?? (new RelayCommand(obj =>
            {

            }));
        }

        public RelayCommand DeleteCommand
        {
            get => _deleteCommand ?? (new RelayCommand(obj =>
            {

            }));
        }

        public RelayCommand OpenMarket 
        {
            get => _openMarket ?? (new RelayCommand(obj => 
            {
                var type = (MarketDLCType)obj;
                switch (type) 
                {
                    case MarketDLCType.Mods:
                        MainViewModel.NavigationStore.PrevViewModel = MainViewModel.NavigationStore.CurrentViewModel;
                        MainViewModel.NavigationStore.CurrentViewModel = new CurseforgeMarket.CurseforgeMarketViewModel(_mainViewModel);
                        break;
                    case MarketDLCType.Resourcepacks:
                        break;
                    case MarketDLCType.World:
                        break;
                }
            }));
        }
        #endregion

        public FactoryDLCVM(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            InstalledMods = new ObservableCollection<Mod>() 
            {
                new Mod
                {
                    Name = "Just Enough Items (JEI)",
                    Author = "mezz",
                    Version = "9.7.0.195",
                    Curseforge = "https://www.curseforge.com/minecraft/mc-mods/jei",
                    IsEnable = false,
                    IsUpdateAvaliable = false,
                    Logo = null,
                },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
                new Mod { Name = "ConnectedTexturesMod", Author = "tterrag1098", Version = "1.1.4+4", Curseforge = "https://www.curseforge.com/minecraft/mc-mods/ctm", IsEnable = false, IsUpdateAvaliable = false, Logo = null, },
            };
            Console.WriteLine(InstalledMods.Count);
        }
    }
}
