namespace GasStationTracker
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup.Primitives;

    public static class DependencyObjectHelper
    {
        public static List<BindingBase> GetBindingObjects(object element)
        {
            List<BindingBase> bindings = new List<BindingBase>();
            List<DependencyProperty> dpList = new List<DependencyProperty>();
            dpList.AddRange(GetDependencyProperties(element));
            dpList.AddRange(GetAttachedProperties(element));

            foreach (DependencyProperty dp in dpList)
            {
                BindingBase b = BindingOperations.GetBindingBase(element as DependencyObject, dp);
                if (b != null)
                {
                    bindings.Add(b);
                }
            }

            return bindings;
        }

        public static List<BindingData> GetBindingData(object element)
        {
            List<BindingData> data = new List<BindingData>();
            //List<BindingBase> bindings = new List<BindingBase>();
            List<DependencyProperty> dpList = new List<DependencyProperty>();
            dpList.AddRange(GetDependencyProperties(element));
            dpList.AddRange(GetAttachedProperties(element));

            foreach (DependencyProperty dp in dpList)
            {
                BindingBase b = BindingOperations.GetBindingBase(element as DependencyObject, dp);
                if (b != null)
                {
                    //bindings.Add(b);
                    data.Add(new BindingData(dp, b));
                }
            }

            return data;
        }

        public static List<DependencyProperty> GetDependencyProperties(object element)
        {
            List<DependencyProperty> properties = new List<DependencyProperty>();
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);
            if (markupObject != null)
            {
                foreach (MarkupProperty mp in markupObject.Properties)
                {
                    if (mp.DependencyProperty != null)
                    {
                        properties.Add(mp.DependencyProperty);
                    }
                }
            }

            return properties;
        }

        public static List<DependencyProperty> GetAttachedProperties(object element)
        {
            List<DependencyProperty> attachedProperties = new List<DependencyProperty>();
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);
            if (markupObject != null)
            {
                foreach (MarkupProperty mp in markupObject.Properties)
                {
                    if (mp.IsAttached)
                    {
                        attachedProperties.Add(mp.DependencyProperty);
                    }
                }
            }

            return attachedProperties;
        }

        public class BindingData
        {
            public BindingData(DependencyProperty dp, BindingBase binding)
            {
                DependecyProperty = dp;
                Binding = binding;
            }

            public DependencyProperty DependecyProperty { get; private set; }

            public BindingBase Binding { get; private set; }
        }
    }
}
