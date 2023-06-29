using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Lexplosion.WPF.NewInterface.Controls
{

    [TemplatePart(Name = PART_WRAP_NAME, Type = typeof(WrapPanel))]
    public class DigitCodeBox : FrameworkElement
    {
        private const string PART_WRAP_NAME = "PART_WrapPanel";

        private WrapPanel _fieldsPanel;

        private TextBox[] InputFields;


        #region Dependency Properties


        public static readonly DependencyProperty CodeSizeProperty
            = DependencyProperty.Register("CodeSize", typeof(uint), typeof(DigitCodeBox), new FrameworkPropertyMetadata(6));

        public uint CodeSize
        {
            get => (uint)GetValue(CodeSizeProperty);
            set => SetValue(CodeSizeProperty, value);
        }


        #endregion Dependency Properties



        #region Constructors 


        static DigitCodeBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UpperPlaceholderTextBox), new FrameworkPropertyMetadata(typeof(UpperPlaceholderTextBox)));
        }


        #endregion Constructors


        #region Public & Protected Properties


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }


        #endregion Public & Protected Properties


        #region Private Methods


        private void InitializeInputFields() 
        {
            InputFields = new TextBox[CodeSize];
            var template = new TextBox() 
            {
                
            };

            for (var i = 0; i < CodeSize; i++) 
            {
                InputFields[0] = template;
            }
        }


        #endregion Private Methods
    }
}
