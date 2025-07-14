using System.Runtime.InteropServices;

namespace SharpFramework.Platform.Windows
{
    internal static class DarkTitlebar
    {
        [DllImport("dwmapi.dll")]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool LockWindowUpdate(IntPtr hWndLock);

        [DllImport("user32.dll")]
        private static extern IntPtr BeginDeferWindowPos(int nNumWindows);

        [DllImport("user32.dll")]
        private static extern IntPtr DeferWindowPos(IntPtr hWinPosInfo, IntPtr hWnd,
            IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool EndDeferWindowPos(IntPtr hWinPosInfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
             int X, int Y, int cx, int cy, uint uFlags);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMAXIMIZED = 3;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        internal const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        internal const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        internal static void SetDarkModeTitleBar(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                _ = DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }
        }

        internal static void RefreshWindowLayout(IntPtr hwnd)
        {
            if (IsWindowVisible(hwnd) && !IsIconic(hwnd))
            {
                if (!GetWindowRect(hwnd, out RECT rect))
                    return;

                if (IsZoomed(hwnd))
                {
                    WINDOWPLACEMENT placement = WINDOWPLACEMENT.Default;
                    if (GetWindowPlacement(hwnd, ref placement))
                    {
                        RECT oldRect = placement.rcNormalPosition;
                        placement.rcNormalPosition = rect;
                        placement.rcNormalPosition.Right -= 1;
                        SetWindowPlacement(hwnd, ref placement);

                        LockWindowUpdate(hwnd);
                        ShowWindow(hwnd, SW_SHOWNORMAL);
                        ShowWindow(hwnd, SW_SHOWMAXIMIZED);
                        LockWindowUpdate(IntPtr.Zero);

                        placement.rcNormalPosition = oldRect;
                        SetWindowPlacement(hwnd, ref placement);
                    }
                }
                else
                {
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;

                    // 一時的に幅を -1 して再適用
                    SetWindowPos(hwnd, IntPtr.Zero, 0, 0, width - 1, height,
                        SWP_NOZORDER | SWP_NOMOVE | SWP_NOACTIVATE);

                    // 元の幅に戻す
                    SetWindowPos(hwnd, IntPtr.Zero, 0, 0, width, height,
                        SWP_NOZORDER | SWP_NOMOVE | SWP_NOACTIVATE);
                }
            }
        }


        internal static bool IsWindows10OrGreater(int build = -1)
        {
            Version version = Environment.OSVersion.Version;
            return version.Major >= 10 && version.Build >= build;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;

            public static WINDOWPLACEMENT Default
            {
                get
                {
                    WINDOWPLACEMENT result = new WINDOWPLACEMENT();
                    result.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    return result;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }
    }
}
