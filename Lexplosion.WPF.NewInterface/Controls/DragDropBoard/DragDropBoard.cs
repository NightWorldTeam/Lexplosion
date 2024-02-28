using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class DragDropBoard : ContentControl
    {
        public static readonly DependencyProperty ImportActionProperty
            = DependencyProperty.Register("ImportAction", typeof(Action<string[]>), typeof(DragDropBoard),
                new FrameworkPropertyMetadata(null));

        public Action<string[]> ImportAction
        {
            get => (Action<string[]>)GetValue(ImportActionProperty);
            set => SetValue(ImportActionProperty, value);
        }


        #region Contructors


        static DragDropBoard()
        {
            AllowDropProperty.OverrideMetadata(typeof(DragDropBoard), new FrameworkPropertyMetadata(true));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragDropBoard), new FrameworkPropertyMetadata(typeof(DragDropBoard)));
        }

        public DragDropBoard()
        {
            
        }


        #endregion Constructors


        #region Public & Protected Methods


        protected override void OnDrop(DragEventArgs e)
        {
            if (ImportAction != null)
            {
                var allowedFiles = new List<string>();

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = e.Data.GetData(DataFormats.FileDrop) as string[];

                    foreach (var file in files)
                    {
                        if (file.Contains(".zip"))
                        {
                            allowedFiles.Add(file);
                        }
                    }
                }

                ImportAction.Invoke(allowedFiles.ToArray());
            }

            base.OnDrop(e);
        }


        #endregion Public & Protected Methods
    }
}
