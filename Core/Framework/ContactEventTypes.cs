//---------------------------------------------------------------------
// <copyright file="ContactEventTypes.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace CoreInteractionFramework
{
    /// <summary>
    /// The possible types of contact events.
    /// </summary>
    public enum ContactEventType
    {
        /// <summary>
        /// The contact was added to the Microsoft Surface unit.
        /// </summary>
        Added,

        /// <summary>
        /// The contact was removed from the Microsoft Surface unit.
        /// </summary>
        Removed,

        /// <summary>
        /// The contact was changed.
        /// </summary>
        Changed,

        /// <summary>
        /// The contact left the UI element.
        /// </summary>
        Leave,
        
        /// <summary>
        /// The contact entered a new UI element.
        /// </summary>
        Enter,
    }
}
