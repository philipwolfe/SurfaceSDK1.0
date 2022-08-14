//---------------------------------------------------------------------
// <copyright file="HitTestCallback.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace CoreInteractionFramework
{
    /// <summary>
    /// Defines a delegate for the method that is called to determine if contacts collide with UI elements.  
    /// </summary>
    /// <remarks>The parameters include two collections:
    /// <list type="bullet">
    /// <item>The first parameter is a 
    /// <strong>ReadOnlyHitTestResultCollection</strong> collection of elements that contain 
    /// contacts that are not 
    /// captured by an <strong><see cref="CoreInteractionFramework.IInputElementStateMachine"/></strong> state machine. 
    /// The delegate implementation should hit 
    /// test each contact element of the <strong>ReadOnlyHitTestResultCollection</strong> collection and assign the 
    /// <strong>IInputElementStateMachine</strong> instance that the contact hit. If the contact 
    /// did not hit anything, the <strong>IInputElementStateMachine</strong> instance should be set to null.</item>
    /// <item>The second parameter is a <strong>ReadOnlyHitTestResultCollection</strong> collection of elements 
    /// that contain contacts that an <strong>IInputElementStateMachine</strong> captured. The <strong>Contact</strong> property 
    /// of the <strong><see cref="CoreInteractionFramework.HitTestResult"/></strong> is initialized with the 
    /// <strong>IInputElementStateMachine</strong> instance that
    /// captured it. The delegate implementation should test each contact to determine if 
    /// it hit. Those contacts that did not hit should be set to null.</item>
    /// </list>
    /// 
    /// 
    /// 
    /// </remarks>
    /// <param name="uncapturedContactEventsToHitTest">A paired list of which contacts hit 
    /// which <strong>IInputElementStateMachine</strong> instances.</param>
    /// <param name="capturedContactEventsToHitTest">A paired list of which captured contacts 
    /// hit which <strong>IInputElementStateMachine</strong> instances.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public delegate void HitTestCallback(ReadOnlyHitTestResultCollection uncapturedContactEventsToHitTest, ReadOnlyHitTestResultCollection capturedContactEventsToHitTest);
}
