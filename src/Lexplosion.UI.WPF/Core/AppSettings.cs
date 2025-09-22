using Lexplosion.UI.WPF.Core.Services;
using Lexplosion.UI.WPF.Mvvm.Models.MainContent.Content.GeneralSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lexplosion.UI.WPF.Core
{
    public class AppSettings
    {
        public AppColorThemeService ThemeService { get; set; }
        public LanguageService LanguageService { get; set; }

        public AppSettings()
        {
            ThemeService = new();
            LanguageService = new();
        }
    }

    public class LanguageService 
    {
        public void Add() { }

        public void Remove() { }

        public void AddAndSelect() { }

        public void Select() { }
    }
}
