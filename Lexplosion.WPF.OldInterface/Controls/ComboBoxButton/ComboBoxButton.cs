using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.Controls
{
    public class ComboBoxButton : ComboBox, ICommandSource
    {
        #region DependencyProperties





        #endregion DependencyProperties


        #region Commands


        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        public IInputElement CommandTarget { get; set; }


        #endregion Commands


        #region Constructors


        static ComboBoxButton() 
        {
            
        }

        public ComboBoxButton()
        {
            
        }


        #endregion Constructors


        #region Public Methods




        #endregion Public Methods
    }
}
