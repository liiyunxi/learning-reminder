using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LearningReminder.Resources;

namespace LearningReminder.Services
{
    /// <summary>
    /// 托盘图标宿主：封装 WinForms 的 NotifyIcon，向业务层提供简单的接口。
    /// </summary>
    public sealed class NotifyIconHost : IDisposable
    {
        private readonly List<ToolStripItem> _overviewItems = new List<ToolStripItem>();

        private bool _disposed;

        /// <summary>构造托盘图标与右键菜单。</summary>
        public NotifyIconHost()
        {
            TrayMenu = new ContextMenuStrip();

            ToolStripMenuItem openItem = new ToolStripMenuItem(AppStrings.TrayMenuOpen);
            openItem.Click += (sender, args) => OpenRequested?.Invoke(this, EventArgs.Empty);

            ToolStripMenuItem checkItem = new ToolStripMenuItem(AppStrings.TrayMenuCheckNow);
            checkItem.Click += (sender, args) => CheckAllRequested?.Invoke(this, EventArgs.Empty);

            AutoStartItem = new ToolStripMenuItem(AppStrings.TrayMenuAutoStart)
            {
                CheckOnClick = true
            };
            AutoStartItem.Click += (sender, args) => AutoStartToggled?.Invoke(this, AutoStartItem.Checked);

            ToolStripMenuItem exitItem = new ToolStripMenuItem(AppStrings.TrayMenuExit);
            exitItem.Click += (sender, args) => ExitRequested?.Invoke(this, EventArgs.Empty);

            TrayMenu.Items.Add(openItem);
            TrayMenu.Items.Add(checkItem);
            TrayMenu.Items.Add(new ToolStripSeparator());
            TrayMenu.Items.Add(AutoStartItem);
            TrayMenu.Items.Add(new ToolStripSeparator());
            TrayMenu.Items.Add(exitItem);

            Tray = new NotifyIcon
            {
                ContextMenuStrip = TrayMenu,
                // 必须先有图标，托盘区才会注册出可见图标（图标为空时等于没有托盘项）
                Icon = TrayIconFactory.IdleIcon,
                Text = AppConstants.AppName
            };
            Tray.MouseClick += OnMouseClick;
            Tray.BalloonTipClicked += (sender, args) => BalloonClicked?.Invoke(this, EventArgs.Empty);
            Tray.DoubleClick += (sender, args) => OpenRequested?.Invoke(this, EventArgs.Empty);
            Tray.Visible = true;
        }

        /// <summary>打开主界面</summary>
        public event EventHandler? OpenRequested;

        /// <summary>立即检查全部任务</summary>
        public event EventHandler? CheckAllRequested;

        /// <summary>切换开机自启</summary>
        public event EventHandler<bool>? AutoStartToggled;

        /// <summary>退出应用</summary>
        public event EventHandler? ExitRequested;

        /// <summary>点击了气泡</summary>
        public event EventHandler? BalloonClicked;

        /// <summary>托盘图标本体</summary>
        public NotifyIcon Tray { get; }

        /// <summary>右键菜单</summary>
        public ContextMenuStrip TrayMenu { get; }

        /// <summary>开机自启菜单项（用于同步勾选状态）</summary>
        public ToolStripMenuItem AutoStartItem { get; }

        /// <summary>更新托盘图标（图标由 TrayIconFactory 缓存复用，这里不负责释放）。</summary>
        public void SetIcon(Icon icon)
        {
            Tray.Icon = icon ?? TrayIconFactory.IdleIcon;
        }

        /// <summary>更新悬停提示。</summary>
        public void SetTooltip(string text)
        {
            // NotifyIcon 的提示文本上限为 63 个字符，超出会被系统截断
            Tray.Text = text.Length > 63 ? text.Substring(0, 63) : text;
        }

        /// <summary>显示气泡提示。</summary>
        public void ShowBalloon(string title, string text)
        {
            Tray.ShowBalloonTip(10000, title, text, ToolTipIcon.Info);
        }

        /// <summary>
        /// 更新菜单顶部的"今日状态总览"：一行摘要 + 各任务完成状态。
        /// 每次整段重建，任务数量级很小，开销可忽略。
        /// </summary>
        public void UpdateOverview(string summary, IReadOnlyList<string> taskLines)
        {
            foreach (ToolStripItem item in _overviewItems)
            {
                TrayMenu.Items.Remove(item);
                item.Dispose();
            }

            _overviewItems.Clear();

            int insertAt = 0;
            ToolStripMenuItem summaryItem = new ToolStripMenuItem(summary)
            {
                Enabled = false
            };
            TrayMenu.Items.Insert(insertAt++, summaryItem);
            _overviewItems.Add(summaryItem);

            foreach (string line in taskLines)
            {
                ToolStripMenuItem taskItem = new ToolStripMenuItem(line)
                {
                    Enabled = false
                };
                TrayMenu.Items.Insert(insertAt++, taskItem);
                _overviewItems.Add(taskItem);
            }

            ToolStripSeparator separator = new ToolStripSeparator();
            TrayMenu.Items.Insert(insertAt, separator);
            _overviewItems.Add(separator);
        }

        private void OnMouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                OpenRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Tray.Visible = false;
            Tray.Dispose();
            TrayMenu.Dispose();
        }
    }
}