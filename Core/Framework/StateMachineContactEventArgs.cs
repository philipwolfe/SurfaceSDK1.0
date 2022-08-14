//---------------------------------------------------------------------
// <copyright file="StateMachineContactEventArgs.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>Represents details for events that relate to contacts on an 
    /// <strong><see cref="T:CoreInteractionFramework.IInputElementStateMachine">IInputElementStateMachine</see></strong> object.</summary>
    public class StateMachineContactEventArgs : ContactEventArgs
    {
        /// <summary>
        /// The state machine that raised this event.
        /// </summary>
        public IInputElementStateMachine StateMachine { get; private set; }

        internal StateMachineContactEventArgs(Contact contact, IInputElementStateMachine stateMachine) : base(contact)
        {
            StateMachine = stateMachine;
        }
    }
}
