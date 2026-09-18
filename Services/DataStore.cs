using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LearningReminder.Models;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 数据仓库：负责 JSON 落盘/读取、当天记录维护、过期记录清理。
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
                    string json = File.ReadAllText(AppPaths.DataFile);
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

            Data.Settings ??= new AppSettings();
            Data.Tasks ??= new List<LearningTask>();
            Data.Records ??= new List<DailyRecord>();

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
                string tempFile = AppPaths.DataFile + ".tmp";
                File.WriteAllText(tempFile, json);

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

        /// <summary>删除任务（同时清理与今天相关的排期状态，历史记录保留）。</summary>
        public void RemoveTask(string taskId)
        {
            Data.Tasks.RemoveAll(task => task.Id == taskId);
            SaveAndNotify();
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