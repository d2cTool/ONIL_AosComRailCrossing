using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PereezdSrv
{
    public class CustomObservableCollection<T> : ObservableCollection<T>
    {
        public CustomObservableCollection() : base() {}

        public CustomObservableCollection(IEnumerable<T> collection) : base(collection) { }

        public CustomObservableCollection(List<T> list) : base(list) { }

        public void Repopulate(IEnumerable<T> collection)
        {
            Items.Clear();
            foreach (var item in collection)
                Items.Add(item);

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
