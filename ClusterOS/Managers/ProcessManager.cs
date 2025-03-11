using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Dispatching;
using ClusterOS.InteropPro;

namespace ClusterOS.Managers
{
    public class ProcessManager
    {
        private static Dictionary<string, int> activeProcesses = new Dictionary<string, int>();
        private bool isDockOnTop = true;
        private bool isDockVisible = true;
        private Microsoft.UI.Windowing.AppWindow appWindow;

        public void MinimizeRestoreApplication(string extractedProcessName)
        {
            if (activeProcesses.TryGetValue(extractedProcessName, out int pid))
            {
                try
                {
                    Process process = Process.GetProcessById(pid);
                    IntPtr hWnd = NativeMethods.GetMainWindowHandle(process);
                    if (hWnd != IntPtr.Zero)
                    {
                        if (NativeMethods.IsIconic(hWnd))
                            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
                        else
                            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_MINIMIZE);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG] Error minimizing/restoring application: {ex.Message}");
                }
            }
        }

        public void CloseApplication(string extractedProcessName)
        {
            if (activeProcesses.TryGetValue(extractedProcessName, out int pid))
            {
                try
                {
                    Process process = Process.GetProcessById(pid);
                    if (!process.CloseMainWindow())
                        process.Kill();
                    activeProcesses.Remove(extractedProcessName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG] Error closing application: {ex.Message}");
                }
            }
        }

        public void OpenNewInstance(string path)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG] Error opening new instance: {ex.Message}");
            }
        }

        public string ExtractProcessName(string path)
        {
            return !string.IsNullOrEmpty(path)
                ? System.IO.Path.GetFileNameWithoutExtension(path).ToLower()
                : string.Empty;
        }

        public async void DockHandleApplicationLaunch(Border border, string path, ProgressRing progressRing, Ellipse indicator)
        {
            try
            {
                string extractedProcessName = ExtractProcessName(path);
                var browserProcesses = new Dictionary<string, string>
        {
            { "chrome", "chrome" },
            { "msedge", "msedge" },
            { "firefox", "firefox" },
            { "opera", "opera" },
            { "brave", "brave" },
            { "zen", "zen" }
        };

                string expectedProcessName = browserProcesses
                    .Where(b => extractedProcessName.Contains(b.Key))
                    .Select(b => b.Value)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(expectedProcessName))
                    expectedProcessName = extractedProcessName;

                if (activeProcesses.TryGetValue(extractedProcessName, out int savedPid))
                {
                    try
                    {
                        Process savedProcess = Process.GetProcessById(savedPid);
                        IntPtr savedWindowHandle = NativeMethods.GetMainWindowHandle(savedProcess);
                        if (savedWindowHandle != IntPtr.Zero && NativeMethods.IsWindowVisible(savedWindowHandle))
                        {
                            if (NativeMethods.IsIconic(savedWindowHandle))
                                NativeMethods.ShowWindow(savedWindowHandle, NativeMethods.SW_RESTORE);
                            NativeMethods.SetForegroundWindow(savedWindowHandle);
                            DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                                indicator.Fill = new SolidColorBrush(Microsoft.UI.Colors.Green));
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        activeProcesses.Remove(extractedProcessName);
                    }
                }

                Process[] processes = Process.GetProcessesByName(expectedProcessName);
                foreach (var process in processes)
                {
                    try
                    {
                        IntPtr hWnd = NativeMethods.GetMainWindowHandle(process);
                        if (hWnd != IntPtr.Zero && NativeMethods.IsWindowVisible(hWnd))
                        {
                            if (NativeMethods.IsIconic(hWnd))
                            {
                                NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
                                if (process.ProcessName.ToLower() == "firefox")
                                {
                                    await Task.Delay(200);
                                    if (NativeMethods.IsIconic(hWnd))
                                        NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
                                }
                            }
                            NativeMethods.SetForegroundWindow(hWnd);
                            activeProcesses[extractedProcessName] = process.Id;
                            DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                                indicator.Fill = new SolidColorBrush(Microsoft.UI.Colors.Green));
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DEBUG] Process error: {process.ProcessName}: {ex.Message}");
                    }
                }

                // Ativa a animação de carregamento
                progressRing.Visibility = Visibility.Visible;
                progressRing.IsActive = true;

                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                };

                // Captura o dispatcher da thread da UI
                var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

                var processStarted = Process.Start(startInfo);
                await Task.Delay(1000);

                if (processStarted != null)
                {
                    processes = Process.GetProcessesByName(expectedProcessName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            IntPtr hWnd = NativeMethods.GetMainWindowHandle(process);
                            if (hWnd != IntPtr.Zero && NativeMethods.IsWindowVisible(hWnd))
                            {
                                if (NativeMethods.IsIconic(hWnd))
                                {
                                    NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
                                    if (process.ProcessName.ToLower() == "firefox")
                                    {
                                        await Task.Delay(200);
                                        if (NativeMethods.IsIconic(hWnd))
                                            NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE);
                                    }
                                }
                                NativeMethods.SetForegroundWindow(hWnd);
                                activeProcesses[extractedProcessName] = process.Id;
                                dispatcherQueue.TryEnqueue(() =>
                                    indicator.Fill = new SolidColorBrush(Microsoft.UI.Colors.Green));
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[DEBUG] Process error: {process.ProcessName}: {ex.Message}");
                        }
                    }

                    // Finaliza a animação de carregamento
                    dispatcherQueue.TryEnqueue(() =>
                    {
                        progressRing.Visibility = Visibility.Collapsed;
                        progressRing.IsActive = false;
                    });

                    Task.Run(() =>
                    {
                        processStarted.WaitForExit();
                        activeProcesses.Remove(extractedProcessName);
                        dispatcherQueue.TryEnqueue(() =>
                            indicator.Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent));
                    });
                }
                else
                {
                    // Caso o processo não seja iniciado, desativa a animação
                    dispatcherQueue.TryEnqueue(() =>
                    {
                        progressRing.Visibility = Visibility.Collapsed;
                        progressRing.IsActive = false;
                    });
                }
            }
            catch (Exception)
            {
                DispatcherQueue.GetForCurrentThread().TryEnqueue(async () =>
                {
                    indicator.Fill = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    await Task.Delay(2000);
                    indicator.Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                });
            }
        }

        public void DockToggleZOrder()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(Window.Current);
            if (isDockOnTop)
            {
                NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_BOTTOM, 0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
                if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                    presenter.IsAlwaysOnTop = false;
            }
            else
            {
                NativeMethods.SetWindowPos(hwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                    NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
                if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                    presenter.IsAlwaysOnTop = true;
            }
            isDockOnTop = !isDockOnTop;
        }

        public void DockToggleVisibility()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(Window.Current);
            if (isDockVisible)
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_HIDE);
            else
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOW);
            isDockVisible = !isDockVisible;
        }
    }
}
