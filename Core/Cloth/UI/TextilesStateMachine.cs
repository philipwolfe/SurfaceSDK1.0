﻿//---------------------------------------------------------------------
// <copyright file="TextitlesStateMachine.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using Microsoft.Surface.Core;
using CoreInteractionFramework;
using Microsoft.Xna.Framework;

namespace Cloth.UI
{
    /// <summary>
    /// Provides a concrete instance of UIElementStateMachine to be encapsulated
    /// by the Textiles UIElement. 
    /// </summary>
    /// <remarks>
    /// The only functionality required here is to handle ContactDown and ContactUp to 
    /// capture and release contacts.  All other manipulations are handled in the 
    /// TextileManipulationComponent.
    /// </remarks>
    public class TextilesStateMachine : UIElementStateMachine
    {

        // The UIElement containing this statemachine.
        // Also contains the TextileManipulationComponent.
        private readonly Textiles textiles;

        /// <summary>
        /// Creates a TextilesStatemachine.
        /// </summary>
        /// <param name="controller">The UIController for this state machine.</param>
        /// <param name="textiles">The UIElement encapsulating this state machine.</param>
        public TextilesStateMachine(UIController controller, Textiles textiles)
            : base(controller, 0, 0)
        {
            this.textiles = textiles;
        }


        /// <summary>
        /// Handles the ContactDown event.
        /// </summary>
        /// <param name="contactEvent">The contact that hit element.</param>
        protected override void OnContactDown(ContactTargetEvent contactEvent)
        {
            if (textiles.ActiveContacts == null) 
            { 
                return; 
            }

            Contact contact = contactEvent.Contact;
            Controller.Capture(contact, this);

            Vector2 worldVector;
            if (textiles.ActiveContacts.TryGetValue(contact.Id, out worldVector))
            {
                 textiles.TextileComponent.ContactAdd(contact.Id, worldVector); 
            }

        }


        /// <summary>
        /// A contact was removed from the element.
        /// </summary>
        /// <param name="contactEvent">The contact that was removed.</param>
        protected override void OnContactUp(ContactTargetEvent contactEvent)
        {
            if (textiles.ActiveContacts == null)
            {
                return;
            }

            Contact contact = contactEvent.Contact;

            Vector2 worldVector;
            if (textiles.ActiveContacts.TryGetValue(contact.Id, out worldVector))
            {
                textiles.TextileComponent.ContactRemove(contact.Id, worldVector);
            }
 
            if (ContactsCaptured.Contains(contact.Id))
            {
                Controller.Release(contact);
            }
        }


        /// <summary>
        /// Handles the ContactChanged event.
        /// </summary>
        /// <param name="contactEvent">The Contact that changed.</param>
        protected override void OnContactChanged(ContactTargetEvent contactEvent)
        {
            // Suppress OnContactChanged events.
            // These will be handled in the textileComponent.
        }


        /// <summary>
        /// A contact has entered the element.
        /// </summary>
        /// <param name="contactEvent">The contact that entered.</param>
        protected override void OnContactEnter(ContactTargetEvent contactEvent)
        {
            // Suppress OnContactEnter.
        }

        /// <summary>
        /// A contact has left the element.
        /// </summary>
        /// <param name="contactEvent">The contact that left.</param>
        protected override void OnContactLeave(ContactTargetEvent contactEvent)
        {
            // Suppress OnContactLeave.
        }

    }


}
