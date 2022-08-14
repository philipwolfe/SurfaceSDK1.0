//---------------------------------------------------------------------
// <copyright file="ContactTargetEvent.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Represents a specific event that has occurred for a contact. 
    /// </summary>
    /// <remarks>
    /// <para>The <strong><see cref="CoreInteractionFramework.ContactEventType"/></strong>
    /// enumeration defines the following contact events:
    /// <strong>Added</strong>, <strong>Changed</strong>, <strong>Removed</strong>, 
    /// <strong>Enter</strong>, and <strong>Leave</strong>. <strong>ContactTargetEvent</strong> 
    /// objects have state set to <strong>Leave</strong> or <strong>Enter</strong>,
    /// depending on <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> route 
    /// destination and the capture state.</para>
    /// <note type="caution"> Contacts that are processed on 
    /// <strong>Added</strong>, <strong>Changed</strong>, <strong>Removed</strong>, 
    /// <strong>Enter</strong>, and <strong>Leave</strong>
    /// events are immediately sent to the state machine, so the contacts in a 
    /// frame update are split into separate update calls for the state machines. This action is 
    /// important because a state machine often changes the capture state of a contact 
    /// when it receives one of these four events.  
    /// </note>
    /// </remarks>
    public class ContactTargetEvent
    {
        private Contact contact;
        private ContactEventType eventType;
        private IHitTestDetails hitTestDetails;

        /// <summary>
        /// Parameterized class constructor that creates a ContactTargetEvent.
        /// </summary>
        /// <param name="eventType">Event types include contact Added, Changed, Removed, 
        /// Enter and Leave.</param>
        /// <param name="contact">The subject contact of this ContactTargetEvent</param>
        internal ContactTargetEvent(ContactEventType eventType, Contact contact)
        {
            this.EventType = eventType;
            this.contact = contact;
        }
        
        /// <summary>
        /// Gets a value that represents the <strong>Contact</strong> source 
        /// of this event.
        /// </summary>
        public Contact Contact
        {
            get
            {
                return contact;
            }
            internal set
            {
                contact = value;
            }
        }

        /// <summary>
        /// Gets a value that represents the <strong><see cref="CoreInteractionFramework.ContactEventType"/></strong> 
        /// that is associated with this <strong>ContactTargetEvent</strong> event.  
        /// </summary>
        /// <remarks>The possible values include <strong>Added</strong>, <strong>Changed</strong>, <strong>Removed</strong>, 
        /// <strong>Enter</strong>, and <strong>Leave</strong>.</remarks>
        public ContactEventType EventType
        {
            get
            {
                return eventType;
            }
            internal set
            {
                eventType = value;
            }
        }

        /// <summary>
        /// Gets a value that represents details about a hit test for 
        /// certain state machines.  
        /// </summary>
        /// <remarks>For more information
        /// about which state machines require hit test details, see <strong><see cref="CoreInteractionFramework.IHitTestDetails">IHitTestDetails</see></strong>.
        /// </remarks>
        public IHitTestDetails HitTestDetails
        {
            get { return hitTestDetails; }
            internal set { hitTestDetails = value; }
        }
    }
}
