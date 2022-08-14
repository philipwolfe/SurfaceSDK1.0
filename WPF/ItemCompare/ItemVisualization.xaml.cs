using System.Windows;
using System.Windows.Controls;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;

namespace ItemCompare
{
    /// <summary>
    /// Interaction logic for ItemVisualization.xaml
    /// </summary>
    public partial class ItemVisualization : TagVisualization
    {
        private Items.ItemData itemData;

        public Items.ItemData ItemData
        {
            get { return itemData; }
            set { itemData = value; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemVisualization()
        {
            InitializeComponent();

            TagRemovedBehavior = TagRemovedBehavior.Wait;
            LostTagTimeout = double.PositiveInfinity;
        }

        /// <summary>
        /// Sets the item and properties to display.
        /// </summary>
        /// <param name="properties">An array of properties to display.</param>
        /// <param name="item">The item to display.</param>
        public void SetItem(
            Items.ItemProperty[] properties,
            Items.Item item)
        {
            // clear out any prior contents
            RowHost.Children.Clear();
            RowHost.RowDefinitions.Clear();

            // set up our row definitions
            for (int index = 0; index < properties.Length; ++index)
            {
                RowHost.RowDefinitions.Add(new RowDefinition());
            }

            // add our rows
            for (int index = 0; index < properties.Length; ++index)
            {
                InformationPanelRow row = new InformationPanelRow();
                Grid.SetRow(row, index);
                RowHost.Children.Add(row);
                row.HeadingLabel.Text = properties[index].Name;
                row.Cell.SetValue(item.Values[index]);
            }

            ItemNamePanel.Text = item.Name;
        }

        /// <summary>
        /// Determines whether this visualization matches the specified contact.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public override bool Matches(Contact contact)
        {
            // We match a given contact if the contact's tag value is present
            // in our item data.

            if (itemData == null)
            {
                return false;
            }

            // Has to be a byte tag
            if (contact.Tag.Type != TagType.Byte)
            {
                return false;
            }

            // Look for a match in our item data
            Items.Item matchingItem = itemData.Find(contact.Tag.Byte.Value);

            return matchingItem != null;
        }

        /// <summary>
        /// Refresh the item visualization properties when a tag is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnGotTag(RoutedEventArgs e)
        {
            TagVisualization tv = (TagVisualization)e.Source;

            TagData tag = tv.VisualizedTag;

            // Has to be a byte tag
            if (tag.Type == TagType.Byte)
            {
                // Look for a match in our item data
                Items.Item matchingItem = itemData.Find(tag.Byte.Value);

                if (matchingItem != null)
                {
                    SetItem(itemData.Properties, matchingItem);
                }
            }
        }      
    }
}
