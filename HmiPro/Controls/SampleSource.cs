using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmiPro.Controls {
    public class SampleSource : BindableBase {
        Random rnd = new Random();
        SampleItem currentItem;

        public ObservableCollection<SampleItem> Items { get; set; }

        public SampleItem CurrentItem {
            get { return currentItem; }
            set {
                currentItem = value;
                RaisePropertyChanged("CurrentItem");
            }
        }
        public SampleSource(int count) {
            Items = new ObservableCollection<SampleItem>();
            InitItems(count);
        }
        void InitItems(int count) {
            Random rnd = new Random();
            for (int i = 0; i < count; i++) {
                SampleItem item = new SampleItem() { Name = "item" + i, Value = rnd.Next(-50, 50) };
                Items.Add(item);
            }
        }
        public void RemoveRandom() {
            if (Items.Count == 0)
                return;
            Items.RemoveAt(rnd.Next(0, this.Items.Count));
        }
        public void RemoveAt(int index) {
            if (Items.Count > index)
                Items.RemoveAt(index);
        }
        public void Add() {
            int i = new Random(DateTime.Now.Millisecond).Next(1, 100);
            SampleItem item = new SampleItem() { Name = "item" + i, Value = i };
            Items.Add(item);
        }
    }

    public class SampleItem : BindableBase {
        string name;
        int amount;

        public string Name {
            get { return name; }
            set {
                name = value;
                RaisePropertyChanged("Name");
            }
        }
        public int Value {
            get { return amount; }
            set {
                amount = value;
                RaisePropertyChanged("Value");
            }
        }
    }

    public class BindableBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string fieldName) {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(fieldName));
        }
    }
}
