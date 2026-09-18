using System;
using Microsoft.Win32;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 开机自启管理（写入 HKCU 的 Run 键，无需管理员权限）。
    /// </summary>
    public static class AutoStartService
    {
        /// <summary>开机自启是否已开启。</summary>
        public static bool IsEnabled()
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(AppConstants.AutoRunRegistryPath);
                object? value = key?.GetValue(AppConstants.AutoRunValueName);
                return value != null;
            }
            catch (Exception ex)
            {
                FileLogger.Error("读取开机自启设置失败", ex);
                return false;
            }
        }

        /// <summary>写入或移除自启项。</summary>
        public static bool SetEnabled(bool enabled)
        {
            try
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                    AppConstants.AutoRunRegistryPath,
                    writable: true);
                if (key == null)
                {
                    FileLogger.Error("无法打开注册表 Run 键");
                    return false;
                }

                if (enabled)
                {
                    string? exePath = Environment.ProcessPath;
                    if (string.IsNullOrEmpty(exePath))
                    {
                        return false;
                    }

                    // 带参数启动，登录后直接驻留托盘，不弹出主窗口
                    key.SetValue(
                        AppConstants.AutoRunValueName,
                        "\"" + exePath + "\" " + AppStartupOptions.MinimizedArgument);
                }
                else
                {
                    key.DeleteValue(AppConstants.AutoRunValueName, throwOnMissingValue: false);
                }

                return true;
            }
            catch (Exception ex)
            {
                FileLogger.Error("写入开机自启设置失败", ex);
                return false;
            }
        }
    }
}