//---------------------------------------------------------------------
// <copyright file="UIController.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Represents the UI controller. 
    /// </summary>
    /// <remarks>A <strong>UIController</strong> object retrieves contacts from the ordered 
    /// contact events buffer, hit tests the contacts, and routes contacts that were hit tested 
    /// successfully or were captured to the correct state machine.  </remarks>
    public class UIController
    {
        // Used to determine if update is currently being called.
        private bool isUpdating;

        // The maximum size of the orderedContactEventsBackbuffer before an exception is thrown.
        private const int MaximumQueueSize = 200000; // About 52 contact events per frame at 60 FPS for 1 minute.

        // An ordered list of contactEvents that is updated by OnFrameReceived 
        internal System.Collections.Generic.Queue<ContactTargetEvent> orderedContactEvents = new Queue<ContactTargetEvent>();
        internal System.Collections.Generic.Queue<ContactTargetEvent> orderedContactEventsBackbuffer = new Queue<ContactTargetEvent>();

        // The list of contacts that will be routed to UI elements. 
        internal System.Collections.Generic.Queue<System.Collections.Generic.Dictionary<ContactTargetEvent, IInputElementStateMachine>> contactEventsToRoute = new Queue<Dictionary<ContactTargetEvent, IInputElementStateMachine>>();

        internal System.Collections.Generic.Dictionary<ContactTargetEvent, IInputElementStateMachine> packagedContacts = new Dictionary<ContactTargetEvent, IInputElementStateMachine>();
        internal Dictionary<ContactTargetEvent, IInputElementStateMachine> unpackagedContacts = new Dictionary<ContactTargetEvent, IInputElementStateMachine>();


        // A paired list of contacts to perform hitTesting on.  The IInputElementStateMachine parameter
        // will be null when passed to the HitTestCallback delegate such that
        // the client hitTesting will fill out the IInputElementStateMachine.
        private System.Collections.Generic.Dictionary<ContactTargetEvent, IInputElementStateMachine> hitTestingContacts = new Dictionary<ContactTargetEvent, IInputElementStateMachine>();

        // A dictionary of contacts (contact ID) with are captured and the IInputElementStateMachine
        private System.Collections.Generic.Dictionary<int, IInputElementStateMachine> capturedContacts = new Dictionary<int, IInputElementStateMachine>();

        // A dictionary of contacts (contact ID) with are captured and the IInputElementStateMachine
        private System.Collections.Generic.Dictionary<int, IInputElementStateMachine> contactsOver = new Dictionary<int, IInputElementStateMachine>();

        /// <summary>
        /// A lock to synchronize access to the swapLock.
        /// </summary>
        private readonly object swapLock = new object();

        private readonly ContactTarget contactTarget;
        private readonly HitTestCallback hitTestCallback;

        /// <summary>
        /// Occurs in <strong>Update</strong> before any contacts are processed. 
        /// </summary>
        /// <remarks>The <strong>ResetState</strong> event gives
        /// listeners a chance to reset any state before <strong>Update</strong> is processed.</remarks>
        public event EventHandler ResetState;

        /// <summary>
        /// Raises the <strong><see cref="ResetState"/></strong> event to enable listeners to 
        /// reset their state before any contacts are processed.
        /// </summary>
        private void OnResetState()
        {
            EventHandler temp = ResetState;

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Creates a <strong><see cref="UIController"/></strong> instance with the specified parameters.
        /// </summary>
        /// <param name="contactTarget">A contact target to use for collecting contacts.</param>
        /// <param name="hitTestCallback">A delegate that is used to do hit testing.</param>
        public UIController(ContactTarget contactTarget, HitTestCallback hitTestCallback)
        {
            if (contactTarget == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contactTarget");

            if (hitTestCallback == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("hitTestCallback");

            this.contactTarget = contactTarget;
            this.hitTestCallback = hitTestCallback;

            this.contactTarget.ContactAdded += new EventHandler<ContactEventArgs>(OnContactAdded);
            this.contactTarget.ContactChanged += new EventHandler<ContactEventArgs>(OnContactChanged);
            this.contactTarget.ContactRemoved += new EventHandler<ContactEventArgs>(OnContactRemoved);
        }

        /// <summary>
        /// Gives exclusive access to events that are raised for a particular contact.  
        /// </summary>
        /// <remarks>All events raised on the specified contact are routed to the element that passed
        /// to the <strong>Capture</strong> method. The 
        /// <strong><see cref="M:CoreInteractionFramework.HitTestCallback"/></strong> delegate is not called while
        /// a contact is captured.
        /// </remarks>
        /// <param name="contact">The contact to capture.</param>
        /// <param name="element">The element to route the captured contact's event to.</param>
        public void Capture(Contact contact, IInputElementStateMachine element)
        {
            if (contact == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contact");

            if (element == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("element");

            if (this.capturedContacts.ContainsKey(contact.Id))
                throw SurfaceCoreFrameworkExceptions.ContactAlreadyCapturedException(contact);


            this.capturedContacts.Add(contact.Id, element);

            element.OnGotContactCapture(contact);
        }

        /// <summary>
        /// Gets the 
        /// <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> object
        /// that has captured the contact.  
        /// </summary>
        /// <param name="contact">The contact to check if it is captured by a 
        /// <strong>IInputElementStateMachine</strong> object.
        /// </param>
        /// <returns>The <strong>IInputElementStateMachine</strong> object that the contact was captured on as 
        /// <strong>IInputElementStateMachine</strong>. Returns null if the contact is not captured.</returns>
        public IInputElementStateMachine GetCapturingElement(Contact contact)
        {
            if (contact == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contact");

            if (this.capturedContacts.ContainsKey(contact.Id))
            {

                IInputElementStateMachine statemachine;

                this.capturedContacts.TryGetValue(contact.Id, out statemachine);

                return statemachine;
            }


            return null;
        }

        /// <summary>
        /// Releases a captured contact. 
        /// </summary>
        /// <remarks>The <strong>Release</strong> method causes hit testing to be
        /// performed for the specified contact.</remarks>
        /// <param name="contact">The contact to release.</param>
        public void Release(Contact contact)
        {
            if (contact == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contact");

            IInputElementStateMachine element = null;

            // Attempt to get the statemachine from the collection with the contact.Id as a key.
            if (this.capturedContacts.TryGetValue(contact.Id, out element))
            {
                if (!this.capturedContacts.Remove(contact.Id))
                {
                    Debug.Fail("capturedContacts should have been tested for containing this contact.");

                    throw SurfaceCoreFrameworkExceptions.CantReleaseNonCapturedContact(contact);
                }

                element.OnLostContactCapture(contact);
            }
            else
            {
                // Couldn't get a statemachine out so this Contact isn't captured.
                throw SurfaceCoreFrameworkExceptions.CantReleaseNonCapturedContact(contact);
            }
        }

        /// <summary>
        /// Specifies whether the contact hit tested to the capturing element.
        /// </summary>
        /// <param name="contact">The contact to test.</param>
        /// <returns><strong>true</strong> if hit tested to the captured element; otherwise, <strong>false</strong>.</returns>
        public bool DoesHitTestMatchCapture(Contact contact)
        {
            if (contact == null)
                throw SurfaceCoreFrameworkExceptions.ArgumentNullException("contact");

            IInputElementStateMachine capturedModel = null, overModel = null;

            if (capturedContacts.TryGetValue(contact.Id, out capturedModel))
            {
                if (contactsOver.TryGetValue(contact.Id, out overModel))
                {
                    if (overModel == capturedModel)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Processes input that the <strong>ContactTarget</strong> receives and dispatches that input for 
        /// hit testing and 
        /// <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> updates.
        /// </summary>
        public void Update()
        {
            if (isUpdating)
                throw SurfaceCoreFrameworkExceptions.UpdateCannotBeCalledDuringUpdate();

            try
            {
                isUpdating = true;

                OnResetState();
                Swap();
                DispatchHitTesting();
                RoutePackagedContacts();
                ClearOrderedContactEvents();
            }
            finally
            {
                isUpdating = false;
            }
        }

        /// <summary>
        /// Swaps the <strong>orderedContactEvents</strong> and the <strong>orderedContactEventsBackbuffer</strong>.  This is thread-safe.
        /// </summary>
        private void Swap()
        {
            // Only lock long enough to swap the collections.
            lock (swapLock)
            {
                Queue<ContactTargetEvent> newFrontBuffer = this.orderedContactEventsBackbuffer;
                orderedContactEventsBackbuffer = this.orderedContactEvents;
                this.orderedContactEvents = newFrontBuffer;
            }
        }

        /// <summary>
        /// Causes each Contact in orderedContactEvents to be routed 
        /// to the IInputElementStateMachine associated with the Contact in Contacts.
        /// </summary>
        private void RoutePackagedContacts()
        {
            Dictionary<IInputElementStateMachine, Queue<ContactTargetEvent>> modelsToRouteContactsTo = new Dictionary<IInputElementStateMachine, Queue<ContactTargetEvent>>();

            foreach (KeyValuePair<ContactTargetEvent, IInputElementStateMachine> contactTargetWithModels in packagedContacts)
            {
                Debug.Assert(contactTargetWithModels.Value != null, "The model is null and so the contact can't be routed");

                if (modelsToRouteContactsTo.ContainsKey(contactTargetWithModels.Value))
                {
                    Queue<ContactTargetEvent> modelQueue;

                    if (modelsToRouteContactsTo.TryGetValue(contactTargetWithModels.Value, out modelQueue))
                    {
                        modelQueue.Enqueue(contactTargetWithModels.Key);
                    }
                    else
                    {
                        Debug.Fail("Unable to get the value from modelsToRouteContactsTo even though the key was found with the ContainsKey method.");
                    }
                }
                else
                {
                    Queue<ContactTargetEvent> modelQueue = new Queue<ContactTargetEvent>();

                    modelQueue.Enqueue(contactTargetWithModels.Key);

                    modelsToRouteContactsTo.Add(contactTargetWithModels.Value, modelQueue);
                }
            }

            foreach (KeyValuePair<IInputElementStateMachine, Queue<ContactTargetEvent>> models in modelsToRouteContactsTo)
            {
                models.Key.Update(models.Value);
            }
        }

        /// <summary>
        /// Calls HitTestCallBack passing a dictionary (Dictionary&lt;Contact, IInputElementStateMachine\&gt;) which contains a single 
        /// contact for each contact id that was in orderedContactEvents.  Captured contacts are removed from
        /// this dictionary when it is constructed and placed in contactsToRoute so that they aren't hit tested. 
        /// When the call to HitTestCallback returns the pairs (KeyValuePair&lt;Contact, IInputElementStateMachine&gt;) which contain 
        /// non-null IInputElementStateMachine are added to contactsToRoute.
        /// </summary>
        private void DispatchHitTesting()
        {
            PackageCapturedContacts();
            HitTestNonCapturedContacts();
        }

        /// <summary>
        /// Packages contacts which are captured and places them into the packagedContact dictionary so they are 
        /// ready to be routed to the IInputElementStateMachine.       
        /// </summary>
        private void PackageCapturedContacts()
        {
            // Don't waste time going through the queue of orderedContacts if no Contacts are captured.
            if (this.capturedContacts.Count == 0)
            {
                // We need to make sure that unpackedContacts is filled out.
                foreach (ContactTargetEvent unpackagedContactEvent in orderedContactEvents)
                {
                    unpackagedContacts.Add(unpackagedContactEvent, null);
                }

                return;
            }

            foreach (ContactTargetEvent cte in orderedContactEvents)
            {
                // Check if contacts arriving on the orderedContactEvents queue are in teh capturedContacts queue.
                if (this.capturedContacts.ContainsKey(cte.Contact.Id))
                {
                    IInputElementStateMachine model;

                    if (this.capturedContacts.TryGetValue(cte.Contact.Id, out model))
                    {
                        Debug.Assert(model != null, "A captured Contact has a null IInputElementStateMachine.");

                        if (model.Controller == this)
                        {
                            this.packagedContacts.Add(cte, model);
                        }
                        else
                        {
                            Debug.Fail("This shouldn't happen because setting the Controller of the IInputElementStateMachine should have released the contact from capture.");
                        }
                    }
                    else
                    {
                        // When we build the SDK in debug this won't matter, but in release we want to handle this gracefully.
                        unpackagedContacts.Add(cte, null);

                        Debug.Fail("Unable to retrieve IInputElementStateMachine from capturedContacts.");
                    }
                }
                else
                {
                    unpackagedContacts.Add(cte, null);
                }
            }

            Debug.Assert(unpackagedContacts.Count + packagedContacts.Count == orderedContactEvents.Count, "The count of ContactTargetEvents in orderedContactEvents wasn't equal to the sum of packaged and unpackaged ContactTargetEvents");
        }

        /// <summary>
        /// Calls the users HitTestCallback delegate passing the unpackaged contacts.
        /// </summary>
        private void HitTestNonCapturedContacts()
        {
            // Don't try to do hit testing if there are no contacts to test.
            if (this.unpackagedContacts.Count == 0 && this.packagedContacts.Count == 0)
            {
                return;
            }

            ReadOnlyHitTestResultCollection uncapturedHitTestResults = new ReadOnlyHitTestResultCollection(this.unpackagedContacts);
            ReadOnlyHitTestResultCollection capturedHitTestResults = new ReadOnlyHitTestResultCollection(this.packagedContacts);

            // We want to remove the packed contacts such that all contacts must be re-added.
            this.packagedContacts.Clear();

            Debug.Assert(hitTestCallback != null, "The HitTestCallBack is null");

            // Call the user HitTestCallback delegate.
            this.hitTestCallback(uncapturedHitTestResults, capturedHitTestResults);

            // Check the capturedContactResults to see if they were hit Tested successfully
            foreach (HitTestResult capturedHitTestResult in capturedHitTestResults)
            {
                PostHitTestResult(capturedHitTestResult);
            }

            foreach (HitTestResult hitTestResult in uncapturedHitTestResults)
            {
                PostHitTestResult(hitTestResult);
            }
        }

        private void PostHitTestResult(HitTestResult hitTestResult)
        {
            if (hitTestResult.StateMachine == null)
            {   
                // If the model is null then the hit testing didn't find an IInputElementStateMachine
                // This means it's not over any models.
                HitTestFail(hitTestResult);
            }
            else
            { 
                HitTestSuccess(hitTestResult);
            }
        }

        private void HitTestFail(HitTestResult hitTestResult)
        {
            IInputElementStateMachine stateMachine;

            // Check to see if this contact is captured to an IInputElementStateMachine.
            if (capturedContacts.TryGetValue(hitTestResult.Contact.Id, out stateMachine))
            {
                if (hitTestResult.ContactTargetEvent.EventType == ContactEventType.Removed)
                {
                    // Send the up notification to the old statemachine.
                    packagedContacts.Add(hitTestResult.ContactTargetEvent, stateMachine);

                    // Send leave notification to the old statemachine.
                    ContactTargetEvent leaveEvent = new ContactTargetEvent(ContactEventType.Leave, hitTestResult.Contact);
                    packagedContacts.Add(leaveEvent, stateMachine);
                }
                else
                {
                    // Send the leave notification to the old statemachine.
                    packagedContacts.Add(hitTestResult.ContactTargetEvent, stateMachine);
                }

                // If this contact was over it shouldn't be any longer.
                if (contactsOver.ContainsKey(hitTestResult.Contact.Id))
                {
                    contactsOver.Remove(hitTestResult.Contact.Id);
                }

            }
            else
            {
                // Is this contact over an IInputElementStateMachine currently?
                if (contactsOver.TryGetValue(hitTestResult.Contact.Id, out stateMachine))
                {
                    // The contact just moved off the edge of the IInputElementStateMachine.
                    if (hitTestResult.ContactTargetEvent.EventType == ContactEventType.Changed)
                    {
                        hitTestResult.ContactTargetEvent.EventType = ContactEventType.Leave;
                    }

                    // Send the notification to the old statemachine.
                    packagedContacts.Add(hitTestResult.ContactTargetEvent, stateMachine);

                    contactsOver.Remove(hitTestResult.Contact.Id);
                }
            }

            // The contact isn't captured, isn't in the contacts over 
            // and it didn't hit anything so there is no where to route it.
        }

        private void HitTestSuccess(HitTestResult hitTestResult)
        {

            if (hitTestResult.StateMachine.Controller != this)
            {
                throw SurfaceCoreFrameworkExceptions.ControllerSetToADifferentControllerException(hitTestResult.StateMachine);
            }

            // Check if the ContactId is in the contactsOver collection.
            IInputElementStateMachine overStateMachine;
            if (contactsOver.TryGetValue(hitTestResult.Contact.Id, out overStateMachine))
            {
                // Then check if the hitTestResult is over the sane statemachine
                if (hitTestResult.StateMachine == overStateMachine)
                {
                    // Just because the contactOver collection hasn't change doesn't mean this event is being
                    // sent to the correct statemachine.  Check if this should be sent to the captured statemachine.
                    IInputElementStateMachine capturedStateMachine;
                    if (capturedContacts.TryGetValue(hitTestResult.Contact.Id, out capturedStateMachine))
                    {
                        if (hitTestResult.ContactTargetEvent.EventType == ContactEventType.Removed)
                        {
                            // Send the up notification to the old statemachine.
                            packagedContacts.Add(hitTestResult.ContactTargetEvent, capturedStateMachine);

                            // Send leave notification to the old statemachine.
                            ContactTargetEvent leaveEvent = new ContactTargetEvent(ContactEventType.Leave,
                                                                                   hitTestResult.Contact);
                            packagedContacts.Add(leaveEvent, capturedStateMachine);                                              
                        }
                        else
                        {
                            packagedContacts.Add(hitTestResult.ContactTargetEvent, capturedStateMachine);
                        }  

                    }
                    else
                    {
                        // Contact is not currently captured.
                        if (hitTestResult.ContactTargetEvent.EventType == ContactEventType.Removed)
                        {
                            // Send the up notification to the old statemachine.
                            packagedContacts.Add(hitTestResult.ContactTargetEvent, hitTestResult.StateMachine);

                            // Send leave notification to the old statemachine.
                            ContactTargetEvent leaveEvent = new ContactTargetEvent(ContactEventType.Leave,
                                                                                   hitTestResult.Contact);

                            packagedContacts.Add(leaveEvent, hitTestResult.StateMachine);
                        }
                        else
                        {
                            packagedContacts.Add(hitTestResult.ContactTargetEvent, hitTestResult.StateMachine);
                        }
                    }

                    // No calls to RoutePackagedContacts() and packagedContacts.Clear() 
                    // When hitTestResult.StateMachine and overStateMachine are the same.

                }
                else
                {
                    // It's over a different statemachine.

                    // Remove the old IInputElementStateMachine from the contactsOver collection.
                    contactsOver.Remove(hitTestResult.Contact.Id);

                    // Add the new IInputElementStateMachine to the contactsOver collection
                    contactsOver.Add(hitTestResult.Contact.Id, hitTestResult.StateMachine);

                    IInputElementStateMachine capturedStateMachine;
                    if (capturedContacts.TryGetValue(hitTestResult.Contact.Id, out capturedStateMachine))
                    {
                        // Contact is captured, but over a different statemachine.
                        // If the contact is captured then don't send enter leave events.

                        // Route this event to the capturedStateMachine.
                        packagedContacts.Add(hitTestResult.ContactTargetEvent, capturedStateMachine);
                    }
                    else
                    {
                        // Contact is not captured over a new statemachine.

                        // It's not over the same statemachine, so we need to add a leave ContactTargetEvent to tell 
                        // the statemachine its leaving. 
                        ContactTargetEvent leaveEvent = new ContactTargetEvent(ContactEventType.Leave,
                                                                               hitTestResult.Contact);

                        // We need to add the leave event or it will not get routed.
                        packagedContacts.Add(leaveEvent, overStateMachine);

                        // Then change the EventType to Enter so that the new statemachine
                        // will know a Contact just entered.
                        hitTestResult.ContactTargetEvent.EventType = ContactEventType.Enter;
                        packagedContacts.Add(hitTestResult.ContactTargetEvent, hitTestResult.StateMachine);

                    }

                    // Route contacts and remove the added ones anytime a contact Enter, Add, Remove or Leaves.
                    RoutePackagedContacts();
                    packagedContacts.Clear();
                }
            }
            else  
            {
                // Not in contactsOver.

                // This contact is just coming over a statemachine either change or add.

                // Check to see if this contact is captured to an IInputElementStateMachine.
                IInputElementStateMachine capturedStateMachine = null;
                if (capturedContacts.TryGetValue(hitTestResult.Contact.Id, out capturedStateMachine))
                {
                    // ContactsOver should reflect which element this contact is over, not which it's captured too.
                    contactsOver.Add(hitTestResult.Contact.Id, hitTestResult.StateMachine);

                    // We should send this event to the element that captured it.
                    packagedContacts.Add(hitTestResult.ContactTargetEvent, capturedStateMachine);
                }
                else
                {
                    // Not captured.

                    // We want to send an Enter event instead of a changed.
                    if (hitTestResult.ContactTargetEvent.EventType == ContactEventType.Changed)
                    {
                        hitTestResult.ContactTargetEvent.EventType = ContactEventType.Enter;
                    }

                    if (hitTestResult.ContactTargetEvent.EventType == ContactEventType.Added)
                    {
                        ContactTargetEvent enterEvent = new ContactTargetEvent(ContactEventType.Enter, hitTestResult.Contact);
                        packagedContacts.Add(enterEvent, hitTestResult.StateMachine);
                    }

                    // This contact is now over this IInputElementStateMachine.
                    switch (hitTestResult.ContactTargetEvent.EventType)
                    {
                        case ContactEventType.Enter:
                        case ContactEventType.Added:
                            contactsOver.Add(hitTestResult.Contact.Id, hitTestResult.StateMachine);
                            break;
                        case ContactEventType.Removed:
                        case ContactEventType.Leave:
                            Debug.Fail("If we get an removed or leave we missed adding it to an IInputElementStateMachine somewhere.");
                            break;
                    }

                    packagedContacts.Add(hitTestResult.ContactTargetEvent, hitTestResult.StateMachine);
                }
                // Route contacts and remove the added ones anytime a contact Enter, Add, Remove or Leaves.
                RoutePackagedContacts();
                packagedContacts.Clear();

            }
        }

        /// <summary>
        /// This method clears out all the collections that were used during this update loop.
        /// </summary>
        private void ClearOrderedContactEvents()
        {
            orderedContactEvents.Clear();
            contactEventsToRoute.Clear();
            packagedContacts.Clear();
            unpackagedContacts.Clear();
            hitTestingContacts.Clear();
        }

        /// <summary>
        /// Handles ContactAdded events from the ContactTarget
        /// </summary>
        private void OnContactAdded(object sender, ContactEventArgs e)
        {
            ContactTargetEvent cte = new ContactTargetEvent(ContactEventType.Added, e.Contact);

            lock (swapLock)
            {
                this.orderedContactEventsBackbuffer.Enqueue(cte);

                if (this.orderedContactEventsBackbuffer.Count > MaximumQueueSize)
                {
                    throw SurfaceCoreFrameworkExceptions.MaximumQueueSizeReached(MaximumQueueSize);
                }
            }
        }

        /// <summary>
        /// Handles ContactRemoved events from the ContactTarget
        /// </summary>
        private void OnContactRemoved(object sender, ContactEventArgs e)
        {
            ContactTargetEvent cte = new ContactTargetEvent(ContactEventType.Removed, e.Contact);

            lock (swapLock)
            {
                this.orderedContactEventsBackbuffer.Enqueue(cte);

                if (this.orderedContactEventsBackbuffer.Count > MaximumQueueSize)
                {
                    throw SurfaceCoreFrameworkExceptions.MaximumQueueSizeReached(MaximumQueueSize);
                }
            }
        }

        /// <summary>
        /// Handles ContactChanged events from the ContactTarget
        /// </summary>
        private void OnContactChanged(object sender, ContactEventArgs e)
        {
            ContactTargetEvent cte = new ContactTargetEvent(ContactEventType.Changed, e.Contact);

            lock (swapLock)
            {
                this.orderedContactEventsBackbuffer.Enqueue(cte);

                if (this.orderedContactEventsBackbuffer.Count > MaximumQueueSize)
                {
                    throw SurfaceCoreFrameworkExceptions.MaximumQueueSizeReached(MaximumQueueSize);
                }
            }
        }
    }
}
