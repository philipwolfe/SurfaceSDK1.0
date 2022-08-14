//---------------------------------------------------------------------
// <copyright file="ScrollBarPart.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreInteractionFramework
{
    /// <summary>
    /// Defines the parts of a <strong><see cref="CoreInteractionFramework.ScrollBarStateMachine"/></strong> object.
    /// </summary>
    public enum ScrollBarPart
    {
        /// <summary>
        /// No part.
        /// </summary>
        None,

        /// <summary>
        /// The thumb of the <strong>ScrollBarStateMachine</strong> object.
        /// </summary>
        Thumb,

        /// <summary>
        /// The track of the <strong>ScrollBarStateMachine</strong> object.
        /// </summary>
        Track
    };
}
