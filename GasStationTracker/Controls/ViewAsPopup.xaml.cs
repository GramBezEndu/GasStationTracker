using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class ViewAsPopup : UserControl
    {
        public List<string> PlacementMethods { get; set; } = new List<string>()
        {
            "Top Left",
            "Top Right",
            "Bottom Left",
            "Bottom Right",
        };

        public string CurrentPlacement { get; set; } = "Top Left";

        private Grid popupContent;

        public Grid PopupContent 
        {
            get => popupContent;
            set
            {
                popupContent = CloneViaXamlSerialization(value);
                popup = new Popup();
                popup.Child = popupContent;
                popup.Placement = PlacementMode.Custom;
                popup.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(PlacePopup);
            }
        }

        private CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
        {
            CustomPopupPlacement placement1 = new CustomPopupPlacement(new Point(0, 0), PopupPrimaryAxis.Vertical);

            CustomPopupPlacement placement2 = new CustomPopupPlacement(new Point(0, 300), PopupPrimaryAxis.Horizontal);

            CustomPopupPlacement[] ttplaces = new CustomPopupPlacement[] { placement1, placement2 };
            return ttplaces;
        }

        Popup popup;

        public ViewAsPopup()
        {
            InitializeComponent();
            PlacementList.ItemsSource = PlacementMethods;
            this.ResetSize();
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
    }
}
