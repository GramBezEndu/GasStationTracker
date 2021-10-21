using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using static GasStationTracker.DependencyObjectHelper;

namespace GasStationTracker.Controls
{
    public enum PopupPlacement
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3,
    };
    /// <summary>
    /// Interaction logic for ViewAsPopup.xaml
    /// </summary>
    public partial class ViewAsPopup : UserControl, INotifyPropertyChanged
    {
        public List<PopupPlacement> PlacementMethods { get; set; } = new List<PopupPlacement>()
        {
            PopupPlacement.TopLeft,
            PopupPlacement.TopRight,
            PopupPlacement.BottomLeft,
            PopupPlacement.BottomRight,
        };

        public PopupPlacement CurrentPlacement
        {
            get => PlacementMethods[SelectedPlacementIndex];
            set
            {
                SelectedPlacementIndex = PlacementMethods.IndexOf(value);
            }
        }

        public int SelectedPlacementIndex 
        { 
            get => selectedPlacementIndex;
            set
            {
                if (value != selectedPlacementIndex && value >= 0 && value < PlacementMethods.Count)
                {
                    selectedPlacementIndex = value;
                    NotifyPropertyChanged();
                    if (popup != null)
                    {
                        //This forces placement callback call
                        popup.Placement = PlacementMode.Center;
                        popup.Placement = PlacementMode.Custom;
                    }
                }
            }
        }
        private Grid popupContent;

        private Window parentWindow;

        public string[] BindingListNames { get; set; } = new string[0];

        public Grid PopupContent
        {
            get => popupContent;
            set
            {
                popupContent = CloneGridViaXmlSerialization(value, BindingListNames);
                popup = new Popup();
                popup.DataContext = this.DataContext;
                popup.AllowsTransparency = true;
                popup.Child = popupContent;
                popup.Placement = PlacementMode.Custom;
                popup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(PlacePopup);
            }
        }

        private CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
        {
            CustomPopupPlacement[] positions = new CustomPopupPlacement[]
            {
                new CustomPopupPlacement(new Point(0, 0), PopupPrimaryAxis.Vertical),
                new CustomPopupPlacement(new Point(0, 0), PopupPrimaryAxis.Vertical),
            };

            int horizontalSafePixels = 8;
            int verticalSafePixels = 4;

            if (parentWindow != null)
            {
                switch (CurrentPlacement)
                {
                    case PopupPlacement.TopLeft:
                        positions[0] = new CustomPopupPlacement(new Point(0, 0), PopupPrimaryAxis.None);
                        break;
                    case PopupPlacement.TopRight:
                        positions[0] = new CustomPopupPlacement(new Point(WpfScreen.GetScreenFrom(parentWindow).WorkingArea.Width - PopupContent.ActualWidth - horizontalSafePixels, 0), PopupPrimaryAxis.None);
                        break;
                    case PopupPlacement.BottomLeft:
                        positions[0] = new CustomPopupPlacement(new Point(0, WpfScreen.GetScreenFrom(parentWindow).WorkingArea.Height - PopupContent.ActualHeight - verticalSafePixels), PopupPrimaryAxis.None);
                        break;
                    case PopupPlacement.BottomRight:
                        positions[0] = new CustomPopupPlacement(new Point(WpfScreen.GetScreenFrom(parentWindow).WorkingArea.Width - PopupContent.ActualWidth - horizontalSafePixels,
                            WpfScreen.GetScreenFrom(parentWindow).WorkingArea.Height - PopupContent.ActualHeight - verticalSafePixels), PopupPrimaryAxis.None);
                        break;
                }
            }
            return positions;
        }

        Popup popup;

        private int selectedPlacementIndex = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewAsPopup()
        {
            InitializeComponent();
            PlacementList.ItemsSource = PlacementMethods;
            this.ResetSize();
            Loaded += new RoutedEventHandler(UserControl_Loaded);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = Window.GetWindow(this);
        }

        private void TogglePopup(object sender, RoutedEventArgs e)
        {
            if (popup != null)
            {
                if (popup.IsOpen)
                {
                    popup.IsOpen = false;
                    ToggleButton.Content = "Show popup";
                }
                else
                {
                    popup.IsOpen = true;
                    ToggleButton.Content = "Hide popup";
                }
            }
        }

        public Grid CloneGridViaXmlSerialization(Grid grid, string[] propertyNames)
        {
            //TODO: Refactor
            List<BindingData> bindingList = new List<BindingData>();
            List<BindingBase> clonedBindings = new List<BindingBase>();
            GetBindingsDataRecursive(grid, bindingList);
            //foreach

            var sb = new StringBuilder();
            var writer = XmlWriter.Create(sb, new XmlWriterSettings
            {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
            });
            var mgr = new XamlDesignerSerializationManager(writer);

            // HERE BE MAGIC!!!
            mgr.XamlWriterMode = XamlWriterMode.Expression;
            // THERE WERE MAGIC!!!

            System.Windows.Markup.XamlWriter.Save(grid, mgr);
            StringReader stringReader = new StringReader(sb.ToString());
            XmlReader xmlReader = XmlReader.Create(stringReader);
            object clonedGrid = (object)XamlReader.Load(xmlReader);
            if (clonedGrid == null)
            {
                throw new ArgumentNullException("Grid could not be cloned via Xaml Serialization Stack.");
            }

            if (clonedGrid is Grid)
            {
                Grid newGrid = clonedGrid as Grid;
                for (int i = 0; i < propertyNames.Length; i++)
                {
                    string currentElementName = propertyNames[i];
                    BindingData bindingData = bindingList[i];
                    UIElement currentElement = (UIElement)newGrid.FindName(currentElementName);
                    if (currentElement is TextBlock)
                    {
                        TextBlock textBlock = currentElement as TextBlock;
                        textBlock.SetBinding(TextBlock.TextProperty, bindingData.Binding);
                    }
                }
                newGrid.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#565595");
                return newGrid;
            }

            else
            {
                throw new InvalidOperationException("Grid could not be cast.");
            }
        }

        public BindingBase CloneBindingViaXmlSerialization(BindingBase binding)
        {
            var sb = new StringBuilder();
            var writer = XmlWriter.Create(sb, new XmlWriterSettings
            {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
            });
            var mgr = new XamlDesignerSerializationManager(writer);

            // HERE BE MAGIC!!!
            mgr.XamlWriterMode = XamlWriterMode.Expression;
            // THERE WERE MAGIC!!!

            System.Windows.Markup.XamlWriter.Save(binding, mgr);
            StringReader stringReader = new StringReader(sb.ToString());
            XmlReader xmlReader = XmlReader.Create(stringReader);
            object newBinding = (object)XamlReader.Load(xmlReader);
            if (newBinding == null)
            {
                throw new ArgumentNullException("Binding could not be cloned via Xaml Serialization Stack.");
            }

            if (newBinding is Binding)
            {
                return (Binding)newBinding;
            }
            else if (newBinding is MultiBinding)
            {
                return (MultiBinding)newBinding;
            }
            else if (newBinding is PriorityBinding)
            {
                return (PriorityBinding)newBinding;
            }
            else
            {
                throw new InvalidOperationException("Binding could not be cast.");
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static void GetBindingsRecursive(DependencyObject dObj, List<BindingBase> bindingList)
        {
            bindingList.AddRange(DependencyObjectHelper.GetBindingObjects(dObj));

            int childrenCount = VisualTreeHelper.GetChildrenCount(dObj);
            if (childrenCount > 0)
            {
                for (int i = 0; i < childrenCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dObj, i);
                    GetBindingsRecursive(child, bindingList);
                }
            }
        }

        private void GetBindingsDataRecursive(DependencyObject dObj, List<BindingData> bindingList)
        {
            bindingList.AddRange(DependencyObjectHelper.GetBindingData(dObj));

            int childrenCount = VisualTreeHelper.GetChildrenCount(dObj);
            if (childrenCount > 0)
            {
                for (int i = 0; i < childrenCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dObj, i);
                    GetBindingsDataRecursive(child, bindingList);
                }
            }
        }
    }
}
