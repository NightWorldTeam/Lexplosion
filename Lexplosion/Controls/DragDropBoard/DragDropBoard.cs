using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Controls
{
    public class DragDropBoard : ContentControl
    {
        public static readonly DependencyProperty UploadedFilesProperty
            = DependencyProperty.Register("UploadedFiles", typeof(string[]), typeof(DragDropBoard));

        public string[] UploadedFiles 
        {
            get => (string[])GetValue(UploadedFilesProperty);
            set => SetValue(UploadedFilesProperty, value);
        }

        static DragDropBoard() 
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragDropBoard), new FrameworkPropertyMetadata(typeof(DragDropBoard)));
            AllowDropProperty.OverrideMetadata(typeof(DragDropBoard), new FrameworkPropertyMetadata(true));  
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                UploadedFiles = files;
            }
            base.OnDrop(e);
        }
    }
}
