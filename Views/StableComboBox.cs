using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Autokolcsonzo.Views;

public sealed class StableComboBox : ComboBox
{
    protected override DependencyObject GetContainerForItemOverride()
    {
        var item = new StableItem();
        item.SetResourceReference(StyleProperty, typeof(ComboBoxItem));
        return item;
    }

    private sealed class StableItem : ComboBoxItem
    {
        public StableItem()
        {
            RequestBringIntoView += (_, e) =>
            {
                if (focusingOnHover) e.Handled = true;
            };
        }

        private bool focusingOnHover;

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            // WPF focuses hovered items and requests that clipped items scroll into view.
            focusingOnHover = true;
            try { base.OnMouseEnter(e); }
            finally { focusingOnHover = false; }
        }
    }
}
