using Lexplosion.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lexplosion.Logic.Management
{
    public class Player : VMBase
    {
        public string Nickname { get; } = "Player";
        public byte[] Skin { get; } = null;
        public string ProfileUrl { get; } = null;

        private bool _isKicked;
        public bool IsKicked 
        { 
            get => _isKicked; private set 
            {
                _isKicked = value;
                OnPropertyChanged();
            } 
        }

        public RelayCommand AccessChangeAction 
        { 
            get => new RelayCommand(obj => 
            {
                AccessChange();
            });
        }

        public Player()
        {

        }

        private void AccessChange() 
        {
            if (IsKicked) 
                Unkick();
            else 
                Kick();
        }

        private void Kick()
        {
            IsKicked = true;
        }

        private void Unkick()
        {
            IsKicked= false;
        }
    }
}
