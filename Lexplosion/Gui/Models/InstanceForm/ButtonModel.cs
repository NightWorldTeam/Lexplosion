using System.Windows.Media;

namespace Lexplosion.Gui.Models.InstanceForm
{
    public struct Tip
    {
        public string Text { get; set; }
        public int Offset { get; set; }
    }

    public enum UpperButtonFunc : byte
    {
        Download,
        Play,
        ProgressBar,
        Close
    }

    public enum LowerButtonFunc : byte
    {
        AddToLibrary,
        DeleteFromLibrary,
        OpenFolder,
        CancelDownload,
        Update,
        OpenWebsite,
        RemoveInstance,
        Export,
        OpenDLCPage
    }

    public sealed class MultiButtonProperties
    {
        /**
         * Pathes (Images on button)
         */
        //public static readonly Geometry GeometryDownloadIcon = Geometry.Parse("M 14 11 V 14 H 2 V 11 H 0 V 14 C 0 15.1 0.9 16 2 16 H 14 C 15.1 16 16 15.1 16 14 V 11 H 14 Z M 13 7 L 11.59 5.59 L 9 8.17 V 0 H 7 V 8.17 L 4.41 5.59 L 3 7 L 8 12 L 13 7 Z");
        //public static readonly Geometry GeometryPauseIcon = Geometry.Parse("M0.666992 0.833374H3.16699V9.16671H0.666992V0.833374ZM4.83366 0.833374H7.33366V9.16671H4.83366V0.833374Z");
        
        /// lower
        public static readonly Geometry GeometryLibraryAdd = Geometry.Parse("M 32.5 40 V 31.5 H 24 V 28.5 H 32.5 V 20 H 35.5 V 28.5 H 44 V 31.5 H 35.5 V 40 Z M 6 31.5 V 28.5 H 21 V 31.5 Z M 6 23.25 V 20.25 H 29.5 V 23.25 Z M 6 15 V 12 H 29.5 V 15 Z");
        
        public static readonly Geometry GeometryLibraryDelete = Geometry.Parse("M 6 31.5 V 28.5 H 21 V 31.5 Z M 6 23.25 V 20.25 H 29.5 V 23.25 Z M 6 15 V 12 H 29.5 V 15 Z M 28.55 44 L 26.4 41.85 L 32.1 36.2 L 26.4 30.55 L 28.55 28.4 L 34.2 34.1 L 39.85 28.4 L 42 30.55 L 36.3 36.2 L 42 41.85 L 39.85 44 L 34.2 38.3 Z");

        public static readonly Geometry GeometryCancelIcon = Geometry.Parse("M 16.5 33.6 L 24 26.1 L 31.5 33.6 L 33.6 31.5 L 26.1 24 L 33.6 16.5 L 31.5 14.4 L 24 21.9 L 16.5 14.4 L 14.4 16.5 L 21.9 24 L 14.4 31.5 Z M 24 44 Q 19.75 44 16.1 42.475 Q 12.45 40.95 9.75 38.25 Q 7.05 35.55 5.525 31.9 Q 4 28.25 4 24 Q 4 19.8 5.525 16.15 Q 7.05 12.5 9.75 9.8 Q 12.45 7.1 16.1 5.55 Q 19.75 4 24 4 Q 28.2 4 31.85 5.55 Q 35.5 7.1 38.2 9.8 Q 40.9 12.5 42.45 16.15 Q 44 19.8 44 24 Q 44 28.25 42.45 31.9 Q 40.9 35.55 38.2 38.25 Q 35.5 40.95 31.85 42.475 Q 28.2 44 24 44 Z M 24 24 Q 24 24 24 24 Q 24 24 24 24 Q 24 24 24 24 Q 24 24 24 24 Q 24 24 24 24 Q 24 24 24 24 Q 24 24 24 24 Q 24 24 24 24 Z M 24 41 Q 31 41 36 36 Q 41 31 41 24 Q 41 17 36 12 Q 31 7 24 7 Q 17 7 12 12 Q 7 17 7 24 Q 7 31 12 36 Q 17 41 24 41 Z");
    }
}