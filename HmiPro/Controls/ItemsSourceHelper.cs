using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.LayoutControl;

namespace HmiPro.Controls {
    public class ItemsSourceHelper : Behavior<LayoutGroup> {
        public static readonly DependencyProperty ItemTemplateSelectorProperty =
           DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(ItemsSourceHelper), new PropertyMetadata());
        public static readonly DependencyProperty ItemTemplateProperty =
          DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ItemsSourceHelper), new PropertyMetadata());
        public static readonly DependencyProperty ItemsSourceProperty =
           DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ItemsSourceHelper), new PropertyMetadata((d, e) => ((ItemsSourceHelper)d).OnItemsSourceChanged(e)));

        public DataTemplateSelector ItemTemplateSelector {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }
        public DataTemplate ItemTemplate {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public IEnumerable ItemsSource {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        protected LayoutGroup Group { get { return AssociatedObject; } }
        protected UIElementCollection Children { get { return Group.Children; } }

        protected override void OnAttached() {
            base.OnAttached();
            RearrangeChildren();
        }

        protected virtual void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is INotifyCollectionChanged)
                ((INotifyCollectionChanged)e.NewValue).CollectionChanged -= OnItemsSourceCollectionChanged;
            if (e.NewValue is INotifyCollectionChanged)
                ((INotifyCollectionChanged)e.NewValue).CollectionChanged += OnItemsSourceCollectionChanged;
            if (Group != null)
                RearrangeChildren();
        }
        protected virtual void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (Group == null)
                return;
            if (e.Action == NotifyCollectionChangedAction.Reset)
                RearrangeChildren();
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    AddItem(item);
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    RemoveItem(item as SampleItem);
        }
        protected virtual void RearrangeChildren() {
            Children.Clear();
            if (ItemsSource != null)
                foreach (var item in ItemsSource)
                    AddItem(item);
        }
        protected virtual void RemoveItem(object item) {
            var layoutItem = Children.OfType<LayoutItem>().FirstOrDefault(x => x.DataContext.Equals(item));
            if (layoutItem != null)
                Children.Remove(layoutItem);
        }
        protected virtual void AddItem(object item) {
            var layoutItem = new LayoutItem { DataContext = item };
            if (item is SampleItem)
                LayoutControl.SetTabHeader(layoutItem, ((SampleItem)item).Name);
            var content = new ContentControl { Content = item };
            content.SetBinding(ContentControl.ContentTemplateProperty, new Binding("ItemTemplate") { Mode = BindingMode.TwoWay, Source = this });
            content.SetBinding(ContentControl.ContentTemplateSelectorProperty, new Binding("ItemTemplateSelector") { Mode = BindingMode.TwoWay, Source = this });
            layoutItem.Content = content;
            Children.Add(layoutItem);
        }
    }
}
