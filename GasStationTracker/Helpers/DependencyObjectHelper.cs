using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup.Primitives;

namespace GasStationTracker
{
    public static class DependencyObjectHelper
    {
        public static List<BindingBase> GetBindingObjects(Object element)
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

        public static List<BindingData> GetBindingData(Object element)
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

        public static List<DependencyProperty> GetDependencyProperties(Object element)
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

        public static List<DependencyProperty> GetAttachedProperties(Object element)
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
            public DependencyProperty Dp { get; private set; }
            public BindingBase Binding { get; private set; }

            public BindingData(DependencyProperty _dp, BindingBase _binding)
            {
                Dp = _dp;
                Binding = _binding;
            }
        }
    }
}
