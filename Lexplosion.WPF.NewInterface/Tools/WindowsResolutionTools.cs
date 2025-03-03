using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lexplosion.WPF.NewInterface.Core.Tools
{
    public readonly struct Resolution
    {
        public int Width { get; }
        public int Height { get; }
        public int DisplayFrequency { get; }
        public int Bits { get; }

        public Resolution(int width, int height, int displayFrequency, int bits)
        {
            Width = width;
            Height = height;
            DisplayFrequency = displayFrequency;
            Bits = bits;
        }

        /// <summary>
        /// Return string Width and Height
        /// </summary>
        /// <returns>Width and Height separated char 'x'</returns>
        public string GetWidthXHeight()
        {
            return Width + "x" + Height;
        }
    }

    public class WindowsResolutionTools
    {
        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(
              string deviceName, int modeNum, ref DEVMODE devMode);
        const int ENUM_CURRENT_SETTINGS = -1;

        const int ENUM_REGISTRY_SETTINGS = -2;

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public System.Windows.Forms.ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        public static IEnumerable<string> GetAvaliableResolutionsToString()
        {
            DEVMODE vDevMode = new DEVMODE();
            int i = 0;

            var hs = new HashSet<string>();

            while (EnumDisplaySettings(null, i, ref vDevMode))
            {
                //Console.WriteLine(vDevMode.dmPelsWidth + "x" + vDevMode.dmPelsHeight + " - " + vDevMode.dmBitsPerPel + ", " + vDevMode.dmDisplayFrequency);
                hs.Add(vDevMode.dmPelsWidth + "x" + vDevMode.dmPelsHeight);
                i++;
            }

            return hs;
        }
    }
}
