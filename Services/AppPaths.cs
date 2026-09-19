using System;
using System.IO;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 应用相关的本地路径。
    /// </summary>
    public static class AppPaths
    {
        /// <summary>数据目录：%AppData%\LearningReminder</summary>
        public static string DataDirectory
        {
            get
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    AppConstants.AppDataFolderName);
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        /// <summary>主数据文件路径</summary>
        public static string DataFile => Path.Combine(DataDirectory, AppConstants.DataFileName);

        /// <summary>备份目录：%AppData%\LearningReminder\backups</summary>
        public static string BackupDirectory
        {
            get
            {
                string dir = Path.Combine(DataDirectory, AppConstants.BackupFolderName);
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        /// <summary>错误日志文件路径</summary>
        public static string LogFile => Path.Combine(DataDirectory, AppConstants.LogFileName);
    }
}