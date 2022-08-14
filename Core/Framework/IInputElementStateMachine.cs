//---------------------------------------------------------------------
// <copyright file="IInputElementStateMachine.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Defines an interface for working with state machines that update their state based on the 
    /// <strong>Update</strong>
    /// method being called from some outside controller.
    /// </summary>
    public interface IInputElementStateMachine
    {
        /// <summary>
        /// Changes the internal state based on the specified list of contact events.
        /// </summary>
        /// <param name="contacts">The list of contacts used to update the state for this state
        /// machine.</param>
        void Update(Queue<ContactTargetEvent> contacts);

        /// <summary>
        /// Called when this state machine captures a new contact.
        /// </summary>
        /// <param name="contact">A contact that this state machine captures.</param>
        void OnGotContactCapture(Contact contact);

        /// <summary>
        /// Called when this state machine release a currently captured contact.
        /// </summary>
        /// <param name="contact">The contact that is no longer captured.</param>
        void OnLostContactCapture(Contact contact);

        /// <summary>
        /// Provides type information about a hit test.
        /// </summary>
        Type TypeOfHitTestDetails { get; }

        /// <summary>
        /// Gets or sets the controller to use with this state machine.
        /// </summary>
        UIController Controller { get; set; }

        /// <summary>
        /// Gets or sets the tag that is associated with this state machine.
        /// </summary>
        object Tag { get; set; }
    }
}
