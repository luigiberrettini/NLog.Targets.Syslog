using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>Implementation of <see cref="INotifyPropertyChanged" /> to simplify config settings</summary>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary>Multicast event for property change notifications</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Sets a property invoking PropertyChanged event handlers</summary>
        protected bool SetProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (Equals(oldValue, newValue))
            {
                return false;
            }

            oldValue = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>Invokes the handlers of the PropertyChanged event</summary>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>Creates a collection changed event handler that manages the PropertyChanged event handler for colleciton items</summary>
        protected NotifyCollectionChangedEventHandler CollectionChangedFactory(PropertyChangedEventHandler onElemPropsChanged)
        {
            return (sender, eventArgs) =>
            {
                if (eventArgs.NewItems != null)
                    foreach (INotifyPropertyChanged item in eventArgs.NewItems)
                        item.PropertyChanged += onElemPropsChanged;

                if (eventArgs.OldItems != null)
                    foreach (INotifyPropertyChanged item in eventArgs.OldItems)
                        item.PropertyChanged -= onElemPropsChanged;
            };
        }
    }
}