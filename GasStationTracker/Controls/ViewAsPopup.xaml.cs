using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace GasStationTracker.Controls
{
    /// <summary>
    /// Interaction logic for ViewAsPopup.xaml
    /// </summary>
    public partial class ViewAsPopup : UserControl, INotifyPropertyChanged
    {
        public List<string> PlacementMethods { get; set; } = new List<string>()
        {
            "Top Left",
            "Top Right",
            "Bottom Left",
            "Bottom Right",
        };

        public string CurrentPlacement
        {
            get => PlacementMethods[SelectedPlacementIndex];
        }

        public int SelectedPlacementIndex 
        { 
            get => selectedPlacementIndex;
            set
            {
                if (value != selectedPlacementIndex)
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

        public Grid PopupContent
        {
            get => popupContent;
            set
            {
                popupContent = CloneViaXamlSerialization(value);
                popup = new Popup();
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
                    case "Top Left":
                        positions[0] = new CustomPopupPlacement(new Point(0, 0), PopupPrimaryAxis.None);
                        break;
                    case "Top Right":
                        positions[0] = new CustomPopupPlacement(new Point(WpfScreen.GetScreenFrom(parentWindow).WorkingArea.Width - PopupContent.ActualWidth - horizontalSafePixels, 0), PopupPrimaryAxis.None);
                        break;
                    case "Bottom Left":
                        positions[0] = new CustomPopupPlacement(new Point(0, WpfScreen.GetScreenFrom(parentWindow).WorkingArea.Height - PopupContent.ActualHeight - verticalSafePixels), PopupPrimaryAxis.None);
                        break;
                    case "Bottom Right":
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

        public static Grid CloneViaXamlSerialization(Grid grid)
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
                newGrid.Background = Brushes.LightGray;
                return newGrid;
            }

            else
            {
                throw new InvalidOperationException("Grid could not be cast.");
            }
        }

        private void PlacementList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //PlacementExpander.IsExpanded = false;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
