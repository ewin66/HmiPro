using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Navigation;

namespace HmiPro.Controls.Panels {
    public class FilterUnselectionBehavior : Behavior<TileBar> {
        bool selectFilterEnable = true;

        public static readonly DependencyProperty SelectedFilterProperty =
            DependencyProperty.Register("SelectedFilter", typeof(FilterCriteriaControl.FilterItem), typeof(FilterUnselectionBehavior),
                new PropertyMetadata(null, (d, e) => ((FilterUnselectionBehavior)d).OnSelectedFilterChanged()));
        static readonly DependencyProperty TileBarItemInternalProperty =
            DependencyProperty.Register("TilebarItemInternal", typeof(FilterCriteriaControl.FilterItem), typeof(FilterUnselectionBehavior),
                new PropertyMetadata(null, (d, e) => ((FilterUnselectionBehavior)d).OnTileBarItemInternalChanged()));

        public FilterCriteriaControl.FilterItem SelectedFilter {
            get { return (FilterCriteriaControl.FilterItem)GetValue(SelectedFilterProperty); }
            set { SetValue(SelectedFilterProperty, value); }
        }
        FilterCriteriaControl.FilterItem TileBarItemInternal {
            get { return (FilterCriteriaControl.FilterItem)GetValue(TileBarItemInternalProperty); }
            set { SetValue(TileBarItemInternalProperty, value); }
        }

        void OnSelectedFilterChanged() {
            if (AssociatedObject == null || AssociatedObject.ItemsSource == null || SelectedFilter == TileBarItemInternal) return;
            if (SelectedFilter == null) {
                SelectTileBarItem(null);
                return;
            }
            foreach (var item in AssociatedObject.ItemsSource)
                if (item == SelectedFilter) {
                    SelectTileBarItem(SelectedFilter);
                    return;
                }
            SelectTileBarItem(null);
        }
        void OnTileBarItemInternalChanged() {
            if (selectFilterEnable)
                SelectedFilter = TileBarItemInternal;
        }

        protected override void OnAttached() {
            base.OnAttached();
            BindingOperations.SetBinding(this, FilterUnselectionBehavior.TileBarItemInternalProperty, new Binding("SelectedItem") { Source = AssociatedObject, Mode = BindingMode.OneWay });
            OnSelectedFilterChanged();
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            BindingOperations.ClearBinding(this, FilterUnselectionBehavior.TileBarItemInternalProperty);
        }

        void SelectTileBarItem(FilterCriteriaControl.FilterItem item) {
            selectFilterEnable = false;
            AssociatedObject.SelectedItem = item;
            selectFilterEnable = true;
        }
    }
}
