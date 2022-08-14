using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;


namespace DataVisualizer
{
    /// <summary>
    /// This UserControl is designed to display diagrams of contacts, demonstrating
    /// their available properties.
    /// </summary>
    public partial class ContactDataVisualizer 
    {
        /// <summary>
        /// The diagrams Dictionary is used to keep track of ContactDiagrams and the Contact
        /// each diagram is associated with.
        /// </summary>
        private Dictionary<Contact, ContactDiagram> diagrams;

        // The Application Launcher orientation at the time the application was launched. 
        private readonly UserOrientation orientation;

        /// <summary>
        /// Constructor
        /// </summary>
        public ContactDataVisualizer()
        {
            diagrams = new Dictionary<Contact, ContactDiagram>();
            
            InitializeComponent();
            UpdateStatistics();

            orientation = ApplicationLauncher.Orientation;

            //Event handlers for ContactDown, ContactChanged and LostContactCapture are
            //added in ContactDataVisualizer.xaml.
        }

        /// <summary>
        /// This handler is called when a Contact is first recognized.
        /// </summary>
        /// <param name="sender">the element raising the ContactDownEvent</param>
        /// <param name="args">information about this ContactDownEvent</param>
        private void OnContactDown(object sender, ContactEventArgs args)
        {
            // Add a diagram to the main window to represent the new contact
            AddDiagram(args.Contact);

            // Fill the diagram with information about the contact and display it
            UpdateDiagram(args.Contact);

            //Acquire Contact capture so ActiveArea will receive all events for this Contact.
            //The LostContactCapture event is used here for notification that this Contact has been
            //completely removed. LostContactCapture is raised after the ContactUp event.
            //When ContactUp is raised, the Contact will still be counted when calling Contacts.GetContactsOver.
            //When LostContactCapture is raised, the Contact will not be counted when calling Contacts.GetContactsOver.
            //Because this application uses Contacts.GetContactsOver to update the statistics, it is more
            //appropriate to use LostContactCapture instead of ContactUp.
            args.Contact.Capture(ActiveArea);
            UpdateStatistics();
        }

        /// <summary>
        /// This handler is called when any of a Contact's properties have changed.
        /// </summary>
        /// <param name="sender">the element raising the ContactChangedEvent</param>
        /// <param name="args">information about this ContactChangedEvent</param>
        private void OnContactChanged(object sender, ContactEventArgs args)
        {
            UpdateDiagram(args.Contact);
        }

        /// <summary>
        /// This handler is called when Contact capture is lost.
        /// </summary>
        /// <param name="sender">the element raising the LostContactCapture</param>
        /// <param name="args">information about this LostContactCapture event</param>
        private void OnLostContactCapture(object sender, ContactEventArgs args)
        {
            RemoveDiagram(args.Contact);
            UpdateStatistics();
        }

        /// <summary>
        /// Called when any of the checkboxes to show/hide contact info elements is changed
        /// </summary>
        /// <param name="sender">the element raising the checked/unchecked event</param>
        /// <param name="e">information about this checked/unchecked event</param>
        void DisplayOptionsChanged(object sender, RoutedEventArgs e)
        {
            foreach(Contact contact in diagrams.Keys)
            {
                UpdateDiagram(contact);
            }
        }

        /// <summary>
        /// Create a ContactDiagram and add it to the diagrams Dictionary and the DiagramContainerGrid.
        /// </summary>
        /// <param name="contact">the contact to diagram</param>
        private void AddDiagram(Contact contact)
        {
            ContactDiagram diagram = new ContactDiagram(this.orientation);
            diagrams.Add(contact, diagram);
            DiagramContainerGrid.Children.Add(diagram);
        }

        /// <summary>
        /// Update a ContactDiagram.
        /// </summary>
        /// <param name="contact"></param>
        private void UpdateDiagram(Contact contact)
        {
            ContactDiagram diagram;
            if (diagrams.TryGetValue(contact, out diagram))
            {
                diagram.Update(DiagramContainerGrid, 
                               contact, 
                               (bool)ShowBoundingRectCheckBox.IsChecked, 
                               (bool)ShowContactInfoCheckBox.IsChecked);
            }
        }

        /// <summary>
        /// Remove a ContactDiagram from the diagrams Dictionary and the DiagramContainerGrid.
        /// </summary>
        /// <param name="contact"></param>
        private void RemoveDiagram(Contact contact)
        {
            ContactDiagram diagram;
            if (diagrams.TryGetValue(contact, out diagram))
            {
                diagrams.Remove(contact);
                DiagramContainerGrid.Children.Remove(diagram);
            }
        }

        /// <summary>
        /// Updates the text description of contacts over the ActiveArea Rectangle.
        /// </summary>
        private void UpdateStatistics()
        {
            //Get all of the contacts over the ActiveArea Rectangle.
            //The ActiveArea Rectangle is defined in XAML.
            ReadOnlyContactCollection contactsOver = Contacts.GetContactsOver(ActiveArea);

            int totalContacts = contactsOver.Count;
            int blobCount = 0;
            int tagCount = 0;
            int fingerCount = 0;
            foreach(Contact contact in contactsOver)
            {
                if(contact.IsTagRecognized)
                {
                    tagCount++;
                }
                else if(contact.IsFingerRecognized)
                {
                    fingerCount++;
                }
                else
                {
                    blobCount++;
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Total Fingers {0}", fingerCount));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Total Blobs: {0}", blobCount));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Total Tags: {0}", tagCount));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "Total Contacts: {0}", totalContacts));
            Statistics.Text = sb.ToString();
        }
    }
}