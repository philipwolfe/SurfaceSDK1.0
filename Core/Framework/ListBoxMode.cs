//---------------------------------------------------------------------
// <copyright file="ListBoxMode.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace CoreInteractionFramework
{
    /// <summary>
    /// The mode of the list box.
    /// </summary>
    public enum ListBoxMode
    {
        /// <summary>
        /// Items are selected when a contact goes up over an item.
        /// </summary>
        Selection,

        /// <summary>
        /// Scrolling occur and no items are selected.
        /// </summary>
        Scrolling,
    }
}
