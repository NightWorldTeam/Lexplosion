using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Controls
{
    public class DragDropBoard : ContentControl
    {
        public static readonly DependencyProperty AvailableFileExtensionsProperty
            = DependencyProperty.Register("AvailableFileExtensions", typeof(IEnumerable<string>), typeof(DragDropBoard),
                new FrameworkPropertyMetadata(defaultValue: new string[] { ".zip" }));

        public static readonly DependencyProperty ImportActionProperty
            = DependencyProperty.Register("ImportAction", typeof(Action<string[]>), typeof(DragDropBoard),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty AllowFolderProperty
            = DependencyProperty.Register("AllowFolder", typeof(bool), typeof(DragDropBoard),
            new FrameworkPropertyMetadata(false));

/*        public static readonly DependencyProperty CheckFolderContentProperty
            = DependencyProperty.Register("CheckFolderContent", typeof(bool), typeof(DragDropBoard),
            new FrameworkPropertyMetadata(false));*/

        public Action<IEnumerable<string>> ImportAction
        {
            get => (Action<IEnumerable<string>>)GetValue(ImportActionProperty);
            set => SetValue(ImportActionProperty, value);
        }

        public IEnumerable<string> AvailableFileExtensions
        {
            get => (IEnumerable<string>)GetValue(AvailableFileExtensionsProperty);
            set => SetValue(AvailableFileExtensionsProperty, value);
        }

        public bool AllowFolder
        {
            get => (bool)GetValue(AllowFolderProperty);
            set => SetValue(AllowFolderProperty, value);
        }

/*        public bool CheckFolderContent
        {
            get => (bool)GetValue(CheckFolderContentProperty);
            set => SetValue(CheckFolderContentProperty, value);
        }*/


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
            if (ImportAction == null)
                return;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var allowedFiles = new List<string>();
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            foreach (var file in files)
            {
                if (Directory.Exists(file) && AllowFolder)
                {
                    allowedFiles.Add(file);
                    continue;
                }

                if (!File.Exists(file))
                {
                    return;
                }

                foreach (var extension in AvailableFileExtensions)
                {
                    if (file.Contains(extension))
                    {
                        allowedFiles.Add(file);
                        break;
                    }
                }
            }


            if (allowedFiles.Count > 0)
                ImportAction.Invoke(allowedFiles);


            base.OnDrop(e);
        }


        #endregion Public & Protected Methods
    }
}
