//---------------------------------------------------------------------
// <copyright file="UIElementStateMachine.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Specifies the base class for all <strong>UIElementStateMachine</strong> classes such as 
    /// <strong><see cref="CoreInteractionFramework.ButtonStateMachine"/></strong>.  
    /// </summary>
    /// <remarks>
    /// <note type="caution"> The Core Interaction Framework and API use the 
    /// Model-View-Controller (MVC) design pattern. State machines 
    /// represents the Model component of the MVC design pattern. </note>
    /// </remarks>    
    public abstract class UIElementStateMachine : IInputElementStateMachine
    {
        private ReadOnlyContactCollectionCache contactsOver = new ReadOnlyContactCollectionCache();
        private UIController controller;
        private ReadOnlyContactCollectionCache contactsCaptured = new ReadOnlyContactCollectionCache();
        private object tag;
        private int numberOfPixelsInHorizontalAxis;
        private int numberOfPixelsInVerticalAxis;

        /// <summary>
        /// Manages a cached ContactCollection so that an editable version of the
        /// collection maybe accessed by internal components and a readonly version
        /// maybe returned to public callers.
        /// </summary>
        private class ReadOnlyContactCollectionCache
        {
            /// <summary>
            /// Used for caching. This should be used other then by 
            /// </summary>
            private List<Contact> actualContactCollection = new List<Contact>();
            internal bool IsStale = true;
            private ReadOnlyContactCollection cachedContactCollection;

            /// <summary>
            /// Gets the collection of contacts which are over this state machine.
            /// </summary>
            internal List<Contact> EditableContactCollection
            {
                get
                {
                    IsStale = true;
                    return actualContactCollection;
                }
            }

            /// <summary>
            /// Gets a cached version of the ReadOnlyContactCollection.
            /// </summary>
            internal ReadOnlyContactCollection CachedContactCollection
            {
                get
                {
                    if (IsStale)
                    {
                        IsStale = false;
                        cachedContactCollection = new ReadOnlyContactCollection(new ReadOnlyCollection<Contact>(actualContactCollection));
                    }

                    return cachedContactCollection;
                }
            }
        }

        /// <summary>
        /// Represents the number of pixels that this control occupies horizontally. 
        /// </summary>
        /// <returns>The horizontal dimension (width) of this control.</returns>
        /// <remarks>
        /// <para>
        /// Many Microsoft Surface controls require data about how much of a change has occurred in 
        /// physical screen space. The <strong>NumberOfPixelsInHorizontalAxis</strong> property provides mapping for this control from 
        /// normal space to screen space. For controls that occupy only 2-D screen spaces, 
        /// you can set this property as the height of the control, regardless of how it is 
        /// rotated in 2-D space. You should update this property when the control changes size.
        /// </para>
        /// <note type="caution"> If this control occupies 3-D space, set <strong>NumberOfPixelsInHorizontalAxis</strong> 
        /// to the number of pixels in screen space that the control projects into. You can update this 
        /// value as needed, but it is taken into account only when 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Update"/></strong>
        /// is called.</note>
        /// </remarks>
        public virtual int NumberOfPixelsInHorizontalAxis 
        {
            get
            {
                return numberOfPixelsInHorizontalAxis;
            }
            set
            {
                // Only update and notify if there has been a change.
                if (value == numberOfPixelsInHorizontalAxis)
                {
                    return;
                }

                numberOfPixelsInHorizontalAxis = value;

                OnNumberOfPixelsInHorizontalAxisChanged();
            }
        }

        /// <summary>
        /// Called when the number of pixels in the horizontal axis changes.
        /// </summary>
        protected virtual void OnNumberOfPixelsInHorizontalAxisChanged()
        {
            EventHandler temp = NumberOfPixelsInHorizontalAxisChanged;

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when the <strong><see cref="NumberOfPixelsInHorizontalAxis"/></strong> property is updated to a different 
        /// value.
        /// </summary>
        public event EventHandler NumberOfPixelsInHorizontalAxisChanged;

        /// <summary>
        /// Represents the number of pixels that this control occupies vertically. 
        /// </summary>
        /// <returns>The vertical dimension (height) of this control.</returns>
        /// <remarks>
        /// <para>
        /// Many Microsoft Surface controls require data about how much of a change has occurred in 
        /// physical screen space. The <strong>NumberOfPixelsInVerticalAxis</strong> property provides mapping for this control from 
        /// normal space to screen space. For controls that occupy only 2-D screen spaces, 
        /// you can set this property as the height of the control, regardless of how it is 
        /// rotated in 2-D space. You should update it when the control changes size.
        /// </para>
        /// <note type="caution"> If this control occupies 3-D space, set <strong>NumberOfPixelsInVerticalAxis</strong> 
        /// to the number of pixels in screen space that the control projects into. You can update 
        /// this value as needed, but it is taken into account only when 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Update"/></strong> 
        /// is called.</note>
        /// </remarks>
        public virtual int NumberOfPixelsInVerticalAxis
        {
            get
            {
                return numberOfPixelsInVerticalAxis;
            }
            set
            {
                // Only update and notify if there has been a change.
                if (value == numberOfPixelsInVerticalAxis)
                {
                    return;
                }

                numberOfPixelsInVerticalAxis = value;

                OnNumberOfPixelsInVerticalAxisChanged();
            }
        }

        /// <summary>
        /// Called when the number of pixels in the vertical axis changes.
        /// </summary>
        protected virtual void OnNumberOfPixelsInVerticalAxisChanged()
        {
            EventHandler temp = NumberOfPixelsInVerticalAxisChanged;

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Occurs when the 
        /// <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInVerticalAxis"/></strong>
        /// property is updated to a 
        /// different value.
        /// </summary>
        public event EventHandler NumberOfPixelsInVerticalAxisChanged;

        /// <summary>
        /// Occurs when a contact that is routed to this state machine changes.
        /// </summary>
        public event EventHandler<StateMachineContactEventArgs> ContactChanged;

        /// <summary>
        /// Occurs when a contact that is routed to this state machine goes down.
        /// </summary>
        public event EventHandler<StateMachineContactEventArgs> ContactDown;

        /// <summary>
        /// Occurs when a contact that is routed to this state machine enters the state machine.
        /// </summary>
        public event EventHandler<StateMachineContactEventArgs> ContactEnter;

        /// <summary>
        /// Occurs when a contact that is routed to this state machine leaves the state machine.
        /// </summary>
        public event EventHandler<StateMachineContactEventArgs> ContactLeave;

        /// <summary>
        /// Occurs when a contact that is routed to this state machine is removed from the state machine.
        /// </summary>
        public event EventHandler<StateMachineContactEventArgs> ContactUp;

        /// <summary>
        /// Occurs when the 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Capture"/></strong> 
        /// method is called for a contact and this state machine.
        /// </summary>
        public event EventHandler<StateMachineContactEventArgs> GotContactCapture;

        /// <summary>
        /// Occurs when the 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Release"/></strong>
        /// method is called for a contact that this state machine captured.
        /// </summary>
        public event EventHandler<StateMachineContactEventArgs> LostContactCapture;


        /// <summary>
        /// Gets the contacts that this state machine captured.
        /// </summary>
        /// <returns>
        /// A cached contact collection.
        /// </returns>
        public ReadOnlyContactCollection ContactsCaptured
        {
            get
            {
                return contactsCaptured.CachedContactCollection;
            }
        }

        /// <summary>
        /// Gets the contacts over this state machine.
        /// </summary>
        /// <returns>A collection of contacts over this state machine.</returns>
        public ReadOnlyContactCollection ContactsOver
        {
            get
            {
                return contactsOver.CachedContactCollection;
            }
        }

        /// <summary>
        /// Initializes the <strong><see cref="UIElementStateMachine"/></strong> objects.
        /// </summary>
        /// <param name="controller">The controller for this <strong>UIElementStateMachine</strong> object.</param>
        /// <param name="numberOfPixelsInHorizontalAxis">
        /// The number of pixels that this control occupies horizontally. 
        /// For more information, see 
        /// <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInHorizontalAxis">NumberOfPixelsInHorizontalAxis</see></strong>.
        /// </param>
        /// <param name="numberOfPixelsInVerticalAxis">
        /// The number of pixels that this control occupies vertically. 
        /// For more information, see 
        /// <strong><see cref="P:CoreInteractionFramework.UIElementStateMachine.NumberOfPixelsInVerticalAxis">NumberOfPixelsInVerticalAxis</see></strong>.
        /// </param>
        protected UIElementStateMachine(UIController controller, int numberOfPixelsInHorizontalAxis, int numberOfPixelsInVerticalAxis)
        {
            if (controller == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("controller");

            this.numberOfPixelsInHorizontalAxis = numberOfPixelsInHorizontalAxis;
            this.numberOfPixelsInVerticalAxis = numberOfPixelsInVerticalAxis;

            this.controller = controller;
            
            this.controller.ResetState += new EventHandler(OnResetState);
        }

        /// <summary>
        /// Overrides <strong>OnResetState</strong> to handle the 
        /// <strong><see cref="E:CoreInteractionFramework.UIController.ResetState"/></strong> event.
        /// Performs actions necessary to reset the state machine state at the beginning
        /// of each update cycle.
        /// </summary>
        /// <param name="sender">The controller that raised the 
        /// <strong>UIController.ResetState</strong> event.</param>
        /// <param name="e">Empty.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Security", 
            "CA2109:ReviewVisibleEventHandlers",
            Justification=@"This method is meant to be overridden by subclasses to support event routing.
                            The event handler code doesn't do anything that makes it dangerous or exploitable
                            as no permissions are being asserted in the code.
                            Hence the concerns of this security rule do not apply.")]
        protected virtual void OnResetState(object sender, EventArgs e) {}

        /// <summary>
        /// Gets or sets the <strong><see cref="CoreInteractionFramework.UIController"/></strong>
        /// object for this state machine.
        /// </summary>
        /// <returns>The <strong>UIController</strong> object for this state machine.</returns>
        public virtual UIController Controller
        {
            get
            {
                return controller;
            }
            set
            {
                if (value == null && controller == null)
                {
                    return;
                }

                if (value == null && controller != null)
                {
                    // Release any contact still captured by this control.
                    foreach (Contact contact in this.ContactsCaptured)
                    {
                        controller.Release(contact);
                    }

                    controller.ResetState -= new EventHandler(OnResetState);
                }
                else if (value != controller)
                {
                    if (controller != null)
                    {
                        // Release any contact still captured by this control.
                        foreach (Contact contact in this.ContactsCaptured)
                        {
                            controller.Release(contact);
                        }

                        controller.ResetState -= new EventHandler(OnResetState);
                    }

                    value.ResetState += new EventHandler(OnResetState);
                }
                
                controller = value;
            }
        }

        /// <summary>
        /// Gets or sets a data storage object for this state machine.
        /// </summary>
        /// <returns>The tag data object.</returns>
        public virtual object Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = value;
            }
        }

        /// <summary>
        /// Gets the type that implements 
        /// <strong><see cref="CoreInteractionFramework.IHitTestDetails"/></strong> for this state machine. 
        /// </summary>
        /// <remarks>If <strong>TypeOfHitTestDetails</strong> returns null, the second parameter should be null 
        /// when your application calls 
        /// the <strong><see cref="M:CoreInteractionFramework.HitTestResult.SetCapturedHitTestInformation"/></strong>
        /// or <strong><see cref="M:CoreInteractionFramework.HitTestResult.SetUncapturedHitTestInformation"/></strong> 
        /// methods, when the first parameter is this type of state machine.
        /// </remarks>
        /// <returns>The type that implements <strong>IHitTestDetails</strong> for this state machine.</returns>
        public virtual Type TypeOfHitTestDetails
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Called when a contact that is routed to this state machine changed.
        /// </summary>
        /// <param name="contactEvent">The container object for the contact that changes.</param>
        protected virtual void OnContactChanged(ContactTargetEvent contactEvent)
        {
            if (contactEvent == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contact");

            if (!contactsOver.CachedContactCollection.Contains(contactEvent.Contact.Id) && !contactsCaptured.CachedContactCollection.Contains(contactEvent.Contact.Id))
                throw SurfaceCoreFrameworkExceptions.ContactIsNotInCollection(contactEvent.Contact, contactsOver.CachedContactCollection);

            Debug.Assert(contactsOver.EditableContactCollection.Contains(contactEvent.Contact.Id) || contactsCaptured.EditableContactCollection.Contains(contactEvent.Contact.Id), "Contact was not in the CachedContactCollection, but is in the EditableContactCollection.");

            EventHandler<StateMachineContactEventArgs> temp = ContactChanged;

            if (temp != null)
            {
                temp(this, new StateMachineContactEventArgs(contactEvent.Contact, this));
            }
        }

        /// <summary>
        /// Called when a contact that is routed to this state machine goes down.
        /// </summary>
        /// <param name="contactEvent">The container object for the contact that is down.</param>
        protected virtual void OnContactDown(ContactTargetEvent contactEvent)
        {
            if (contactEvent.Contact == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contact");

            EventHandler<StateMachineContactEventArgs> temp = ContactDown;

            if (temp != null)
            {
                temp(this, new StateMachineContactEventArgs(contactEvent.Contact, this));
            }
        }

        /// <summary>
        /// Called when a contact that is routed to this state machine enters the state machine.
        /// </summary>
        /// <param name="contactEvent">The container for the contact that entered this state 
        /// machine.</param>
        protected virtual void OnContactEnter(ContactTargetEvent contactEvent)
        {
            if (contactEvent.Contact == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contact");

            if (contactsOver.CachedContactCollection.Contains(contactEvent.Contact.Id))
                throw SurfaceCoreFrameworkExceptions.ContactIsAlreadyInCollection(contactEvent.Contact, contactsOver.CachedContactCollection);

            Debug.Assert(!contactsOver.EditableContactCollection.Contains(contactEvent.Contact.Id), "Contact was not in the CachedContactCollection, but is in the EditableContactCollection.");

            this.contactsOver.EditableContactCollection.Add(contactEvent.Contact);

            EventHandler<StateMachineContactEventArgs> temp = ContactEnter;

            if (temp != null)
            {
                temp(this, new StateMachineContactEventArgs(contactEvent.Contact, this));
            }
        }

        /// <summary>
        /// Called when a contact that is routed to this state machine leaves the state machine.
        /// </summary>
        /// <param name="contactEvent">The container object for the departed contact.</param>
        protected virtual void OnContactLeave(ContactTargetEvent contactEvent)
        {
            if (contactEvent.Contact == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contact");
            
            if (contactsOver.EditableContactCollection.Contains(contactEvent.Contact.Id))
                this.contactsOver.EditableContactCollection.Remove(contactEvent.Contact.Id);

            EventHandler<StateMachineContactEventArgs> temp = ContactLeave;

            if (temp != null)
            {
                temp(this, new StateMachineContactEventArgs(contactEvent.Contact, this));
            }
        }

        /// <summary>
        /// Called when a contact that is routed to this state machine is removed.
        /// </summary>
        /// <param name="contactEvent">The container for the removed contact.</param>
        protected virtual void OnContactUp(ContactTargetEvent contactEvent)
        {
            if (contactEvent.Contact == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contact");

            if (!contactsOver.CachedContactCollection.Contains(contactEvent.Contact.Id) && !contactsCaptured.CachedContactCollection.Contains(contactEvent.Contact.Id))
                throw SurfaceCoreFrameworkExceptions.ContactIsNotInCollection(contactEvent.Contact, contactsOver.CachedContactCollection);

            Debug.Assert(contactsOver.EditableContactCollection.Contains(contactEvent.Contact.Id) || contactsCaptured.EditableContactCollection.Contains(contactEvent.Contact.Id), "Contact was not in the CachedContactCollection, but is in the EditableContactCollection.");

            if (contactsOver.EditableContactCollection.Contains(contactEvent.Contact.Id))
                this.contactsOver.EditableContactCollection.Remove(contactEvent.Contact.Id);

            EventHandler<StateMachineContactEventArgs> temp = ContactUp;

            if (temp != null)
            {
                temp(this, new StateMachineContactEventArgs(contactEvent.Contact, this));
            }

            if (contactsCaptured.CachedContactCollection.Contains(contactEvent.Contact.Id))
            {
                Controller.Release(contactEvent.Contact);
            }
        }

        /// <summary>
        /// Called when the 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Capture"/></strong> method is called for a contact and this 
        /// state machine.
        /// </summary>
        /// <param name="contact">The container for the contact that is captured.</param>
        protected virtual void OnGotContactCapture(Contact contact)
        {
            if (contact == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contact");

            if (contactsCaptured.CachedContactCollection.Contains(contact.Id))
                throw SurfaceCoreFrameworkExceptions.ContactAlreadyCapturedException(contact);

            Debug.Assert(!contactsCaptured.EditableContactCollection.Contains(contact.Id), "Contact was not in the CachedContactCollection, but is in the EditableContactCollection.");

            contactsCaptured.EditableContactCollection.Add(contact);

            EventHandler<StateMachineContactEventArgs> temp = GotContactCapture;

            // Use a temporary delegate for thread-safety.
            if (temp != null)
            {
                temp(this, new StateMachineContactEventArgs(contact, this));
            }
        }

        /// <summary>
        /// Called when the 
        /// <strong><see cref="M:CoreInteractionFramework.UIController.Release"/></strong>
        ///  method is called for a contact that this state machine captured.
        /// </summary>
        /// <param name="contact">The contact that is released.</param>
        protected virtual void OnLostContactCapture(Contact contact)
        {
            if (contact == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contact");

            if (!contactsCaptured.CachedContactCollection.Contains(contact.Id))
                throw SurfaceCoreFrameworkExceptions.CantReleaseNonCapturedContact(contact);

            Debug.Assert(contactsCaptured.EditableContactCollection.Contains(contact.Id), "Contact was not captured in the CachedContactCollection, but is in the EditableContactCollection.");

            contactsCaptured.EditableContactCollection.Remove(contact.Id);

            EventHandler<StateMachineContactEventArgs> temp = LostContactCapture;

            if (temp != null)
            {
                temp(this, new StateMachineContactEventArgs(contact, this));
            }
        }

        /// <summary>
        /// Called when all of the contact events have been routed to this state machine.
        /// </summary>
        /// <param name="orderContacts">An ordered list of contacts for each contact event.</param>
        protected virtual void OnUpdated(System.Collections.Generic.Queue<ContactTargetEvent> orderContacts)
        {
            foreach (ContactTargetEvent cte in orderContacts)
            {
                switch (cte.EventType)
                {
                    case ContactEventType.Added:
                        OnContactDown(cte);
                        break;
                    case ContactEventType.Removed:
                        OnContactUp(cte);
                        break;
                    case ContactEventType.Changed:
                        OnContactChanged(cte);
                        break;
                    case ContactEventType.Enter:
                        OnContactEnter(cte);
                        break;
                    case ContactEventType.Leave:
                        OnContactLeave(cte);
                        break;
                    default:
                        Debug.Fail("ContactEventType is unknown at time of UIElementStateMachine OnUpdate");
                        break;
                }
            }
        }

        #region IInputElementStateMachine Members

        //Review: Does this SuppressMessage make sense? Why does it warn about this method, but not the next two?
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void IInputElementStateMachine.Update(Queue<ContactTargetEvent> contacts)
        {
            OnUpdated(contacts);
        }

        void IInputElementStateMachine.OnGotContactCapture(Contact contact)
        {
            OnGotContactCapture(contact);
        }

        void IInputElementStateMachine.OnLostContactCapture(Contact contact)
        {
            OnLostContactCapture(contact);
        }

        #endregion
    }
}
