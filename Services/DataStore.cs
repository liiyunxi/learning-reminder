using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LearningReminder.Models;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 数据仓库：负责 JSON 落盘/读取（可选 DPAPI 加密）、每天自动备份、导入导出、
    /// 当天记录维护与过期记录清理。
    /// 全部调用都在 UI 线程（调度器与界面均运行在 UI 线程），因此内部不加锁。
    /// </summary>
    public sealed class DataStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            // 允许中文原样输出，便于直接查看数据文件
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly Lazy<DataStore> LazyInstance =
            new Lazy<DataStore>(() => new DataStore());

        private string _currentDateKey = string.Empty;
        private bool _loaded;

        private DataStore()
        {
            SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <summary>全局唯一实例</summary>
        public static DataStore Instance => LazyInstance.Value;

        /// <summary>内存中的数据</summary>
        public AppData Data { get; private set; } = new AppData();

        /// <summary>数据发生变化（用于界面刷新）</summary>
        public event EventHandler? Changed;

        /// <summary>跨天重置完成</summary>
        public event EventHandler? DayRolledOver;

        /// <summary>从磁盘加载数据，文件缺失或损坏时使用空数据。</summary>
        public void Load()
        {
            try
            {
                if (File.Exists(AppPaths.DataFile))
                {
                    byte[] raw = File.ReadAllBytes(AppPaths.DataFile);
                    string json = DecodeContent(raw);
                    AppData? loaded = JsonSerializer.Deserialize<AppData>(json, SerializerOptions);
                    if (loaded != null)
                    {
                        Data = loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Error("读取数据文件失败，已备份并使用空数据", ex);
                BackupBrokenFile();
                Data = new AppData();
            }

            Normalize();
            PruneHistory();
            EnsureToday();
            _loaded = true;
        }

        /// <summary>原子写入磁盘（先写临时文件再替换，避免中途断电导致文件损坏）。</summary>
        public void Save()
        {
            // 未完成加载时绝不落盘：避免二次启动的进程用空数据覆盖已有文件
            if (!_loaded)
            {
                return;
            }

            try
            {
                string json = JsonSerializer.Serialize(Data, SerializerOptions);
                byte[] payload = Data.Settings.EncryptData
                    ? ProtectedData.Protect(Encoding.UTF8.GetBytes(json), null, DataProtectionScope.CurrentUser)
                    : Encoding.UTF8.GetBytes(json);

                string tempFile = AppPaths.DataFile + ".tmp";
                File.WriteAllBytes(tempFile, payload);

                if (File.Exists(AppPaths.DataFile))
                {
                    File.Replace(tempFile, AppPaths.DataFile, null);
                }
                else
                {
                    File.Move(tempFile, AppPaths.DataFile);
                }
            }
            catch (Exception ex)
            {
                FileLogger.Error("保存数据文件失败", ex);
            }
        }

        /// <summary>保存并通知界面刷新。</summary>
        public void SaveAndNotify()
        {
            Save();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>今天的日期键</summary>
        public string TodayKey => DateTime.Now.ToString(AppConstants.DateFormat);

        /// <summary>取今天的记录，必要时创建（含跨天重置通知）。</summary>
        public DailyRecord EnsureToday()
        {
            string today = TodayKey;
            DailyRecord? record = FindRecord(today);

            if (record == null)
            {
                record = new DailyRecord { Date = today };
                Data.Records.Add(record);
                PruneHistory();
            }

            if (_currentDateKey != today)
            {
                bool isFirstCall = string.IsNullOrEmpty(_currentDateKey);
                _currentDateKey = today;
                if (!isFirstCall)
                {
                    // 跨天：通知调度器为新的一天重新排期
                    DayRolledOver?.Invoke(this, EventArgs.Empty);
                }
            }

            return record;
        }

        /// <summary>今天的记录（已确保存在）。</summary>
        public DailyRecord Today => EnsureToday();

        /// <summary>按日期键查找记录。</summary>
        public DailyRecord? FindRecord(string dateKey)
        {
            foreach (DailyRecord record in Data.Records)
            {
                if (record.Date == dateKey)
                {
                    return record;
                }
            }

            return null;
        }

        /// <summary>按标识查找任务。</summary>
        public LearningTask? FindTask(string taskId)
        {
            foreach (LearningTask task in Data.Tasks)
            {
                if (task.Id == taskId)
                {
                    return task;
                }
            }

            return null;
        }

        /// <summary>删除任务（历史记录保留）。</summary>
        public void RemoveTask(string taskId)
        {
            Data.Tasks.RemoveAll(task => task.Id == taskId);
            SaveAndNotify();
        }

        /// <summary>创建当天备份（同一天只建一份；数据文件不存在时跳过）。</summary>
        public void CreateDailyBackup(DateTime now)
        {
            try
            {
                if (!File.Exists(AppPaths.DataFile))
                {
                    return;
                }

                string target = Path.Combine(
                    AppPaths.BackupDirectory,
                    AppConstants.BackupFilePrefix + now.ToString("yyyyMMdd") + AppConstants.BackupFileSuffix);
                if (!File.Exists(target))
                {
                    File.Copy(AppPaths.DataFile, target);
                }

                PruneBackups();
            }
            catch (Exception ex)
            {
                FileLogger.Error("创建数据备份失败", ex);
            }
        }

        /// <summary>时间戳备份（文件名带时分秒）：导入前与手动备份共用，不会覆盖当天自动备份。</summary>
        public void CreateTimestampedBackup(DateTime now)
        {
            try
            {
                if (!File.Exists(AppPaths.DataFile))
                {
                    return;
                }

                string target = Path.Combine(
                    AppPaths.BackupDirectory,
                    AppConstants.BackupFilePrefix + now.ToString("yyyyMMdd-HHmmss") + AppConstants.BackupFileSuffix);
                File.Copy(AppPaths.DataFile, target);
                PruneBackups();
            }
            catch (Exception ex)
            {
                FileLogger.Error("创建备份失败", ex);
            }
        }

        /// <summary>导出为明文 JSON（便于迁移与人工查看）。</summary>
        public void ExportTo(string path)
        {
            string json = JsonSerializer.Serialize(Data, SerializerOptions);
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        /// <summary>从 JSON 文件导入并替换当前数据（导入前自动备份现有数据）。</summary>
        public void ImportFrom(string path)
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            AppData? loaded = JsonSerializer.Deserialize<AppData>(json, SerializerOptions);
            if (loaded == null)
            {
                throw new InvalidDataException("导入文件解析结果为空");
            }

            CreateTimestampedBackup(DateTime.Now);
            Data = loaded;
            Normalize();
            // 重新判定跨天，避免沿用旧日期键
            _currentDateKey = string.Empty;
            PruneHistory();
            EnsureToday();
            SaveAndNotify();
        }

        /// <summary>解析数据文件内容：明文 JSON 或 DPAPI 加密内容。</summary>
        private static string DecodeContent(byte[] raw)
        {
            // 明文 JSON 以 { 开头（允许 BOM 与空白）
            string text = Encoding.UTF8.GetString(raw);
            string trimmed = text.TrimStart('\uFEFF', ' ', '\t', '\r', '\n');
            if (trimmed.StartsWith("{", StringComparison.Ordinal))
            {
                return text;
            }

            // 非明文：按当前 Windows 账户解密（DPAPI）
            byte[] plain = ProtectedData.Unprotect(raw, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plain);
        }

        /// <summary>把可能为空的集合补齐，保持后续逻辑免判空。</summary>
        private void Normalize()
        {
            Data.Settings ??= new AppSettings();
            Data.Tasks ??= new List<LearningTask>();
            Data.Records ??= new List<DailyRecord>();
            Data.Templates ??= new List<TaskTemplate>();
        }

        /// <summary>保留最近若干天的记录，避免数据文件无限增长。</summary>
        private void PruneHistory()
        {
            DateTime keepFrom = DateTime.Today.AddDays(-AppConstants.HistoryKeepDays);
            Data.Records.RemoveAll(record =>
            {
                DateTime parsed;
                return !DateTime.TryParse(record.Date, out parsed) || parsed < keepFrom;
            });

            // 新的日期排在前面，历史界面直接顺序读取
            Data.Records.Sort((left, right) => string.CompareOrdinal(right.Date, left.Date));
        }

        /// <summary>备份数量超过上限时删除最旧的备份。</summary>
        private static void PruneBackups()
        {
            DirectoryInfo directory = new DirectoryInfo(AppPaths.BackupDirectory);
            FileInfo[] files = directory.GetFiles(
                AppConstants.BackupFilePrefix + "*" + AppConstants.BackupFileSuffix);
            if (files.Length <= AppConstants.BackupKeepCount)
            {
                return;
            }

            Array.Sort(files, (left, right) => string.CompareOrdinal(left.Name, right.Name));
            int removeCount = files.Length - AppConstants.BackupKeepCount;
            for (int index = 0; index < removeCount; index++)
            {
                try
                {
                    files[index].Delete();
                }
                catch (Exception ex)
                {
                    FileLogger.Error("清理旧备份失败", ex);
                }
            }
        }

        /// <summary>数据文件损坏时备份，便于人工排查。</summary>
        private void BackupBrokenFile()
        {
            try
            {
                string broken = AppPaths.DataFile + ".broken";
                if (File.Exists(broken))
                {
                    File.Delete(broken);
                }

                File.Move(AppPaths.DataFile, broken);
            }
            catch (Exception ex)
            {
                FileLogger.Error("备份损坏的数据文件失败", ex);
            }
        }
    }
}