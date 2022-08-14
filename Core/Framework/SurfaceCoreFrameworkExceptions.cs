//---------------------------------------------------------------------
// <copyright file="SurfaceCoreFrameworkExceptions.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System;
using System.Globalization;
using Microsoft.Surface.Core;

namespace CoreInteractionFramework
{
    internal static class SurfaceCoreFrameworkExceptions
    {
        #region UIController exceptions

        internal static Exception ContactAlreadyCapturedException(Contact contact)
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ContactAlreadyCapturedException, contact.Id));
        }

        internal static Exception CantReleaseNonCapturedContact(Contact contact)
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.CantReleaseNonCapturedContactException, contact.Id));
        }

        #endregion

        #region UIElementStateMachine exceptions

        internal static Exception ContactIsNotInCollection(Contact contact, ReadOnlyContactCollection collection)
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ContactIsNotInCollectionException, contact.Id, collection));
        }

        internal static Exception ContactIsAlreadyInCollection(Contact contact, ReadOnlyContactCollection collection)
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ContactIsAlreadyInCollectionException, contact.Id, collection));
        }

        #endregion

        #region State Machine Exceptions

        internal static Exception HitTestDetailsMustBeTypeof(Type hitTestDetailsType, Type stateMachineType, CoreInteractionFramework.IHitTestDetails hitTestDetails)
        {
            //The IHitTestDetails supplied: {0} were not of type {1} as is required by {2}.
            return new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.HitTestDetailsMustBeTypeofException, hitTestDetails, hitTestDetailsType, stateMachineType));  
        }

        #endregion

        internal static Exception UpdateCannotBeCalledDuringUpdate()
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.UpdateCannotBeCalledDuringUpdate));
        }

        internal static Exception MaximumQueueSizeReached(int maxQueueSize)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.MaximumQueueSizeReached, maxQueueSize));
        }

        internal static Exception StateMachineMustBeOfType(Type providedType, Type requiredType)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.StateMachineMustBeOfType, providedType, requiredType));
        }

        internal static Exception CalledCapturedHitTestInformationForReleasedElement()
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.CalledCapturedHitTestInformationForReleasedElement));
        }

        internal static Exception CalledReleaseHitTestInformationForCapturedElement()
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.CalledReleaseHitTestInformationForCapturedElement));
        }

        internal static Exception ControllerSetToADifferentControllerException(CoreInteractionFramework.IInputElementStateMachine IInputElementStateMachine)
        {
            return new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ControllerSetToADifferentController, IInputElementStateMachine));
        }

        internal static Exception ArgumentNullException(string argument)
        {
            return new ArgumentNullException(argument);
        }

        internal static Exception ArgumentOutOfRangeException(string argument)
        {
            return new ArgumentOutOfRangeException(argument);
        }

        internal static Exception InvalidOrientationArgumentException(string argument, Orientation orientation)
        {
            return new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ControllerSetToADifferentController, orientation, argument));
        }

        internal static Exception ItemIsAlreadyInCollection()
        {
            return new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ItemIsAlreadyInCollection));
        }

        internal static Exception ItemIsNotInListBoxItemsCollection(string collectionName)
        {
            return new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ItemNotInCollection, collectionName));
        }
    }
}
