//---------------------------------------------------------------------
// <copyright file="ReadOnlyContactCollectionCacheUtilities.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System.Collections.Generic;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    static class ReadOnlyContactCollectionCacheUtilities
    {
        internal static bool Contains(this List<Contact> contacts, int id)
        {
            foreach (Contact contact in contacts)
            {
                if (contact.Id == id)
                    return true;
            }

            return false;
        }

        internal static void Remove(this List<Contact> contacts, int id)
        {
            for (int i = 0; i < contacts.Count; i++)
			{
                Contact contact = contacts[i];

                if (contact.Id == id)
                {
                    contacts.RemoveAt(i);

                    return;
                }
            }
        }
    }
}
