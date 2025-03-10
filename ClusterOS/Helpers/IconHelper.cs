using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace ClusterOS.Helpers
{
    public static class IconHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        private const int SHIL_JUMBO = 0x4; // 256x256
        private const int SHIL_EXTRALARGE = 0x2; // 48x48
        private const int SHIL_LARGE = 0x0; // 32x32

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_SYSICONINDEX = 0x000004000;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        private interface IImageList
        {
            int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);
            int ReplaceIcon(int i, IntPtr hicon, ref int pi);
            int SetOverlayImage(int iImage, int iOverlay);
            int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);
            int AddMasked(IntPtr hbmImage, int crMask, ref int pi);
            int Draw(ref IMAGELISTDRAWPARAMS pimldp);
            int Remove(int i);
            int GetIcon(int i, int flags, ref IntPtr picon);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IMAGELISTDRAWPARAMS
        {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;
            public int yBitmap;
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        [DllImport("shell32.dll", EntryPoint = "#727")]
        private static extern int SHGetImageList(int iImageList, ref Guid riid, out IImageList ppv);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private static Guid IID_IImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

        public static BitmapImage GetIconUsingShellImageList(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            SHFILEINFO shfi = new SHFILEINFO();

            IntPtr hSuccess = SHGetFileInfo(
                filePath,
                FILE_ATTRIBUTE_NORMAL,
                ref shfi,
                (uint)Marshal.SizeOf(shfi),
                SHGFI_SYSICONINDEX | SHGFI_USEFILEATTRIBUTES
            );

            if (hSuccess == IntPtr.Zero)
                return null;

            IImageList imageList;
            int hr = SHGetImageList(SHIL_JUMBO, ref IID_IImageList, out imageList);

            if (hr != 0)
            {
                hr = SHGetImageList(SHIL_EXTRALARGE, ref IID_IImageList, out imageList);
                if (hr != 0)
                    return null;
            }

            IntPtr hIcon = IntPtr.Zero;
            imageList.GetIcon(shfi.iIcon, 0, ref hIcon);

            if (hIcon == IntPtr.Zero)
                return null;

            try
            {
                using (Icon icon = Icon.FromHandle(hIcon))
                {
                    int iconSize = Math.Max(icon.Width, 256);

                    using (Bitmap bitmap = new Bitmap(iconSize, iconSize, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.Clear(Color.Transparent);
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                            int x = (iconSize - icon.Width) / 2;
                            int y = (iconSize - icon.Height) / 2;

                            g.DrawIcon(icon, x, y);

                            using (MemoryStream stream = new MemoryStream())
                            {
                                bitmap.Save(stream, ImageFormat.Png);
                                stream.Position = 0;

                                BitmapImage bitmapImage = new BitmapImage();
                                bitmapImage.SetSource(stream.AsRandomAccessStream());
                                return bitmapImage;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (hIcon != IntPtr.Zero)
                    DestroyIcon(hIcon);
            }
        }
    }
}