using System;
using LearningReminder.Models;

namespace LearningReminder.ViewModels
{
    /// <summary>
    /// 里程碑行：界面上勾选后直接写回任务定义并落盘。
    /// </summary>
    public sealed class MilestoneRowViewModel : ObservableObject
    {
        private readonly MilestoneItem _item;
        private readonly Action _onChanged;

        /// <summary>构造里程碑行。</summary>
        public MilestoneRowViewModel(MilestoneItem item, Action onChanged)
        {
            _item = item;
            _onChanged = onChanged;
        }

        /// <summary>里程碑标识</summary>
        public string Id => _item.Id;

        /// <summary>小目标描述</summary>
        public string Title => _item.Title;

        /// <summary>是否已完成</summary>
        public bool IsDone
        {
            get => _item.Done;
            set
            {
                if (_item.Done == value)
                {
                    return;
                }

                _item.Done = value;
                _item.DoneAt = value ? DateTime.Now : null;
                OnPropertyChanged();
                _onChanged();
            }
        }
    }
}