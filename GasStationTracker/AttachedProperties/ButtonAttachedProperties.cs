using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Shapes;

namespace GasStationTracker.AttachedProperties
{
    public class ButtonAttachedProperties
    {

        public static Shape GetShape(DependencyObject obj)
        {
            return (Shape)obj.GetValue(ShapeProperty);
        }

        public static void SetShape(DependencyObject obj, Shape value)
        {
            obj.SetValue(ShapeProperty, value);
        }

        public static readonly DependencyProperty ShapeProperty =
        DependencyProperty.RegisterAttached("Shape", typeof(Shape), typeof(ButtonAttachedProperties));

        private static void ShapePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //if (d is TextBox textbox && e.NewValue is bool mustAutoScroll && mustAutoScroll)
            //{
            //    textbox.TextChanged += (s, ee) => AutoScrollToEnd(s, ee, textbox);
            //}
        }

        //private static void AutoScrollToEnd(object sender, TextChangedEventArgs e, TextBox textbox)
        //{
        //    textbox.ScrollToEnd();
        //}
    }
}
