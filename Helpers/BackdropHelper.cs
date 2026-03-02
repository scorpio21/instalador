using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Instalador.Helpers
{
    public static class BackdropHelper
    {
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMWA_MICA_EFFECT = 1029;

        private const int WCA_ACCENT_POLICY = 19;

        private enum TipoBackdrop
        {
            Auto = 0,
            Ninguno = 1,
            Mica = 2,
            Acrylic = 3,
            Tabbed = 4
        }

        private enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public static void AplicarEfecto(Window ventana)
        {
            if (ventana == null) return;

            try
            {
                var handle = new WindowInteropHelper(ventana).Handle;
                if (handle == IntPtr.Zero) return;

                if (EsWindows11())
                {
                    if (TryAplicarMica(handle)) return;
                }

                TryAplicarAcrylic(handle);
            }
            catch
            {
            }
        }

        private static bool TryAplicarMica(IntPtr handle)
        {
            try
            {
                int enable = 1;
                _ = DwmSetWindowAttribute(handle, DWMWA_MICA_EFFECT, ref enable, Marshal.SizeOf<int>());

                int backdrop = (int)TipoBackdrop.Mica;
                int r = DwmSetWindowAttribute(handle, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, Marshal.SizeOf<int>());
                return r == 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryAplicarAcrylic(IntPtr handle)
        {
            try
            {
                var accent = new AccentPolicy
                {
                    AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                    AccentFlags = 2,
                    // Formato: 0xAABBGGRR (alpha, blue, green, red)
                    GradientColor = unchecked((int)0xAA202020)
                };

                int size = Marshal.SizeOf(accent);
                IntPtr ptr = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(accent, ptr, fDeleteOld: false);

                    var data = new WindowCompositionAttributeData
                    {
                        Attribute = WCA_ACCENT_POLICY,
                        SizeOfData = size,
                        Data = ptr
                    };

                    int result = SetWindowCompositionAttribute(handle, ref data);
                    return result != 0;
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool EsWindows11()
        {
            try
            {
                var v = Environment.OSVersion.Version;
                // Windows 11 = build 22000+
                return v.Major >= 10 && v.Build >= 22000;
            }
            catch
            {
                return false;
            }
        }
    }
}
