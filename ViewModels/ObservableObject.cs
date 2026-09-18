using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LearningReminder.ViewModels
{
    /// <summary>
    /// 支持属性变更通知的基类（轻量实现，不引入 MVVM 框架）。
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>触发属性变更通知。</summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>赋值并在变化时通知界面。</summary>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}