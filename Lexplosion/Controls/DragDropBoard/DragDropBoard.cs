using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Controls
{
    public class DragDropBoard : ContentControl
    {
        public static readonly DependencyProperty ImportActionProperty
            = DependencyProperty.Register(
                "ImportAction",
                typeof(Action<string[]>),
                typeof(DragDropBoard),
                new PropertyMetadata(null));

        public Action<string[]> ImportAction
        {
            get => (Action<string[]>)GetValue(ImportActionProperty);
            set => SetValue(ImportActionProperty, value);
        }

        static DragDropBoard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragDropBoard), new FrameworkPropertyMetadata(typeof(DragDropBoard)));
            AllowDropProperty.OverrideMetadata(typeof(DragDropBoard), new FrameworkPropertyMetadata(true));
        }

        protected override void OnDrop(DragEventArgs e)
        {
            //Console.WriteLine("----------Method OnDrop Started----------");
            if (this.ImportAction != null)
            {
                var allowedFiles = new List<string>();

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    foreach (var file in files)
                    {
                        //Console.WriteLine(file + " <-- Allowed file? --> " + file.Contains(".zip"));
                        if (file.Contains(".zip"))
                        {
                            allowedFiles.Add(file);
                        }
                    }
                }

                ImportAction.Invoke(allowedFiles.ToArray());
            }
            base.OnDrop(e);

            //Console.WriteLine("----------Method OnDrop finished----------");
        }
    }
}
