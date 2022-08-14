//---------------------------------------------------------------------
// <copyright file="ClickMode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace CoreInteractionFramework
{
    /// <summary>
    /// Identifies the contact state that determines 
    /// when a click occurs. The default mode is <strong>Release</strong>.
    /// </summary>
    public enum ClickMode
    {
        /// <summary>
        /// A click occurs when a contact is added and released.
        /// </summary>
        Release,

        /// <summary>
        /// A click occurs when the button is pressed.
        /// </summary>
        Press,

        /// <summary>
        /// A click occurs when a contact enters or is added to the button.
        /// </summary>
        Hover,
    }
}
