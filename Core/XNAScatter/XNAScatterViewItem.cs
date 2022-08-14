//---------------------------------------------------------------------
// <copyright file="XnaScatterViewItem.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using CoreInteractionFramework;
using Microsoft.Surface.Core.Manipulations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XnaScatter
{
    /// <summary>
    /// The class that represents a catterViewItem.
    /// </summary>
    /// <remarks>
    /// XnaScatterViewItem inherits from UIElementStateMachine, but does not fully implement
    /// a standard controller/view/model design pattern like most CoreInteractionFramework 
    /// controls. The purpose here is to demonstrate how to use the contact capture and 
    /// routing mechanisms in CIF without using the other aspects of the  framework. For 
    /// examples that implement a full state machine seperate from the control visual, see 
    /// the button and scroll viewer controls in the Cloth sample.
    /// </remarks>
    public class XnaScatterViewItem : UIElementStateMachine, IContactableObject, IDisposable
    {
        private string textureSourceFile;
        private Texture2D content;
        private Vector2 transformedCenter;
        private float scaleFactor = 0.5f;
        private float minScaleFactor = 0.25f;
        private float maxScaleFactor = 2.0f;
        private float zoomFactor = 1.0f;
        private float orientation;

        private readonly XnaScatterView parent;

        public XnaScatterView Parent
        {
            get { return parent; }
        }

        private Affine2DManipulationProcessor manipulationProcessor;
        private Affine2DInertiaProcessor inertiaProcessor;
        private bool manipulating;
        private bool extrapolating;
        private List<Manipulator> currentManipulators = new List<Manipulator>();
        private List<Manipulator> removedManipulators = new List<Manipulator>();

        private bool canTranslate = true;
        private bool canTranslateFlick = true;
        private bool canRotate = true;
        private bool canRotateFlick = true;
        private bool canScale = true;
        private bool canScaleFlick = true;
        private Matrix transform = Matrix.Identity;

        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;

        #region IDisposable

        private bool disposed;

        public bool IsDisposed
        {
            get {return disposed;}
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose of managed resources.
                    content.Dispose();
                }

                // Clean up unmanaged resources.

                disposed = true;
            }
        }

        ~XnaScatterViewItem()
        {
            Dispose(false);
        }

        #endregion

        #region Layout and Hit Testing Properties
        //==========================================================//
        /// <summary>
        /// The screen transform of all of the objects. Contacts need to be hit test based on this
        /// transformation. The center of this is also updated when this property is updated.
        /// </summary>
        public Matrix Transform
        {
            get
            {
                return transform;
            }
            set
            {
                Vector2 center = Center;

                transform = value;

                // Transform the center into the screen coordinate system.
                transformedCenter = Vector2.Transform(center, Transform);
            }
        }

        //==========================================================//
        /// <summary>
        /// The transformed center of the object. All layout is based off of this property.
        /// </summary>
        public Vector2 TransformedCenter
        {
            get
            {
                return transformedCenter;
            }
            set
            {
                transformedCenter = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// The center of the object. All layout is based off of this property.
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return Vector2.Transform(transformedCenter, Matrix.Invert(Transform));
            }
            set
            {
                // Transform the center into the screen coordinate system.
                transformedCenter = Vector2.Transform(value, Transform);
            }
        }

        //==========================================================//
        /// <summary>
        /// The scaled hight of the object. Does not reflect changes to
        /// height based on orientation.
        /// </summary>
        public float Height
        {
            get
            {
                return (float)content.Height * zoomFactor * scaleFactor;
            }
        }

        //==========================================================//
        /// <summary>
        /// The scaled width of the object. Does not reflect changes to
        /// width based on orientation.
        /// </summary>
        public float Width
        {
            get
            {
                return (float)content.Width * zoomFactor * scaleFactor;
            }
        }

        //==========================================================//
        /// <summary>
        /// The scale of the object based on the parent's scale.
        /// </summary>
        public float ZoomFactor
        {
            get
            {
                return zoomFactor;
            }
            set
            {
                zoomFactor = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// The drawing origin for the item. Drawing origin is loosely equivilant
        /// to WPF's transform origin, but it is specified in pixels from the top left,
        /// not as a value between 0 and 1. When XNA draws, it uses the DrawingOrigin
        /// before it applies scale or orientation, so use _unmodified_ values here.
        /// </summary>
        private Vector2 DrawingOrigin
        {
            get
            {
                return new Vector2(content.Width / 2, content.Height / 2);
            }
        }

        //==========================================================//
        /// <summary>
        /// The bounding rectangle for the item if its orientation were set to 0.
        /// </summary>
        private Rectangle AxisAlignedBoundingRectangle
        {
            get { return new Rectangle((int)(transformedCenter.X - (Width / 2)), (int)(transformedCenter.Y - (Height / 2)), (int)Width, (int)Height); }
        }

        #endregion

        #region Manipulation Properties

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be moved via direct manipulation.
        /// </summary>
        public bool CanTranslate
        {
            get
            {
                return canTranslate;
            }
            set
            {
                canTranslate = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be moved via flick.
        /// </summary>
        public bool CanTranslateFlick
        {
            get
            {
                return canTranslateFlick;
            }
            set
            {
                canTranslateFlick = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be rotated via direct manipulation.
        /// </summary>
        public bool CanRotate
        {
            get
            {
                return canRotate;
            }
            set
            {
                canRotate = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be rotated via flick.
        /// </summary>
        public bool CanRotateFlick
        {
            get
            {
                return canRotateFlick;
            }
            set
            {
                canRotateFlick = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be scaled via direct manipulation.
        /// </summary>
        public bool CanScale
        {
            get
            {
                return canScale;
            }
            set
            {
                canScale = value;
            }
        }

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be scaled via flick.
        /// </summary>
        public bool CanScaleFlick
        {
            get
            {
                return canScaleFlick;
            }
            set
            {
                canScaleFlick = value;
            }
        }

        #endregion

        //==========================================================//
        /// <summary>
        /// Gets the type of HitTestDetails used to store the results of a hit test against this class.
        /// </summary>
        public override Type TypeOfHitTestDetails
        {
            get
            {
                return typeof(XnaScatterHitTestDetails);
            }
        }

        #region Initalization

        //==========================================================//
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="passedColor">The color of the item.</param>
        /// <param name="passedParent">The item's parent XnaScatterView.</param>
        public XnaScatterViewItem(UIController controller, string contentImage, XnaScatterView passedParent)
            :base(controller, 1, 1 )
        {
            textureSourceFile = contentImage;
            parent = passedParent;
        }

        //==========================================================//
        /// <summary>
        /// Creates manipulation and inertis processors that support only this item's allowed manipulations.
        /// </summary>
        private void SetAllowedManipulations()
        {
            Affine2DManipulations supportedManipulations =
                ((canTranslate || canTranslateFlick) ? Affine2DManipulations.TranslateX | Affine2DManipulations.TranslateY : Affine2DManipulations.None) |
                ((canScale || canScaleFlick) ? Affine2DManipulations.Scale : Affine2DManipulations.None) |
                ((canRotate || canRotateFlick) ? Affine2DManipulations.Rotate : Affine2DManipulations.None);

            manipulationProcessor = new Affine2DManipulationProcessor(supportedManipulations);
            manipulationProcessor.Affine2DManipulationStarted += OnAffine2DManipulationStarted;
            manipulationProcessor.Affine2DManipulationDelta += OnAffine2DDelta;
            manipulationProcessor.Affine2DManipulationCompleted += OnAffine2DManipulationCompleted;

            inertiaProcessor = new Affine2DInertiaProcessor();
            inertiaProcessor.Affine2DInertiaCompleted += OnAffine2DInertiaCompleted;
            inertiaProcessor.Affine2DInertiaDelta += OnAffine2DDelta;
        }

        //==========================================================//
        /// <summary>
        /// Load the image that is used to draw the XNAScatterViewItem.
        /// </summary>
        /// <param name="device">The graphics device that is being for the application.</param>
        /// <param name="contentPath">The path to the content directory.</param>
        public void LoadContent(GraphicsDevice device, String contentPath)
        {
            content = Texture2D.FromFile(device, contentPath + textureSourceFile);
        }

        #endregion

        #region Manipulation and Inertia Processor Events

        //==========================================================//
        /// <summary>
        /// Event handler for the manipulation processor's delta event. 
        /// Occurs whenever the first time that the manipulation processor processes a 
        /// group of manipulators.
        /// </summary>
        /// <param name="sender">The manipulation processor that raised the event.</param>
        /// <param name="e">The event args for the event.</param>
        private void OnAffine2DManipulationStarted(object sender, Affine2DOperationStartedEventArgs e)
        {
            Debug.Assert(!extrapolating);
            manipulating = true;

            if (canRotate || canRotateFlick)
            {
                manipulationProcessor.PivotX = transformedCenter.X;
                manipulationProcessor.PivotY = transformedCenter.Y;
                manipulationProcessor.PivotRadius = Math.Max(Width, Height) / 2.0f;
            }
        }

        //==========================================================//
        /// <summary>
        /// Event handler for the manipulation and inertia processor's delta events. 
        /// Occurs whenever the manipulation or inertia processors processes or extrapolate 
        /// manipulator data.
        /// </summary>
        /// <param name="sender">The manipulation or inertia processor that raised the event.</param>
        /// <param name="e">The event args for the event.</param>
        private void OnAffine2DDelta(object sender, Affine2DOperationDeltaEventArgs e)
        {
            Debug.Assert(manipulating && sender is Affine2DManipulationProcessor ||
                extrapolating && sender is Affine2DInertiaProcessor);

            Vector2 manipulationOrigin = new Vector2(e.ManipulationOriginX, e.ManipulationOriginY);
            Vector2 manipulationDelta = new Vector2(e.DeltaX, e.DeltaY);
            Vector2 previousOrigin = new Vector2(manipulationOrigin.X - manipulationDelta.X, manipulationOrigin.Y - manipulationDelta.Y);
            float restrictedOrientation = RestrictOrientation(e.RotationDelta);
            float restrictedScale = RestrictScale(e.ScaleDelta);

            // Adjust the position of the item based on change in rotation
            if (restrictedOrientation != 0.0f)
            {
                Vector2 manipulationOffset = transformedCenter - previousOrigin;
                Vector2 rotatedOffset = GeometryHelper.RotatePointVector(manipulationOffset, restrictedOrientation);
                Vector2 compensation = rotatedOffset - manipulationOffset;
                transformedCenter += compensation;
            }

            // Adjust the position of the item based on change in scale
            if (restrictedScale != 1.0f)
            {
                Vector2 manipulationOffset = manipulationOrigin - transformedCenter;
                Vector2 scaledOffset = manipulationOffset * restrictedScale;
                Vector2 compensation = manipulationOffset - scaledOffset;
                transformedCenter += compensation;
            }

            // Rotate the item if it is allowed
            if (canRotate || canRotateFlick)
            {
                orientation += restrictedOrientation;
            }

            // Scale the item if it is allowed
            if (canScale || canScaleFlick)
            {
                scaleFactor *= restrictedScale;
            }

            // Translate the item if it is allowed
            if (canTranslate || canTranslateFlick)
            {
                transformedCenter += new Vector2(e.DeltaX, e.DeltaY);
            }

            RestrictCenter();

            if (canRotate || canRotateFlick)
            {
                manipulationProcessor.PivotX = transformedCenter.X;
                manipulationProcessor.PivotY = transformedCenter.Y;
                manipulationProcessor.PivotRadius = Math.Max(Width, Height) / 2.0f;
            }
        }

        //==========================================================//
        /// <summary>
        /// Event handler for the manipulation processor's completed event. 
        /// Occurs whenever the manipulation processor processes manipulator 
        /// data where all remaining contacts have been removed.
        /// Check final deltas and start the inertia processor if they are high enough.
        /// </summary>
        /// <param name="sender">The manipulation processor that raised the event.</param>
        /// <param name="e">The event args for the event.</param>
        private void OnAffine2DManipulationCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
            // manipulation completed
            manipulating = false;

            // Get the inital inertia values
            Vector2 initialVelocity = new Vector2(e.VelocityX, e.VelocityY);
            float angularVelocity = e.AngularVelocity;
            float expansionVelocity = e.ExpansionVelocity;

            // Calculate the deceleration rates

            // 4 inches/second squared (4 inches * 96 pixels per inch / (1000 per millisecond)^2)
            const float deceleration = 4.0f * 96.0f / (1000.0f * 1000.0f);
            const float expansionDeceleration = 4.0f * 96.0f / (1000.0f * 1000.0f);

            // 90 degrees/second squared, specified in radians (180 * pi / (1000 per miliseconds)^2)
            const float angularDeceleration = 90.0f / 180.0f * (float)Math.PI / (1000.0f * 1000.0f);

            // Rotate around the center of the item
            inertiaProcessor.InitialOriginX = TransformedCenter.X;
            inertiaProcessor.InitialOriginY = TransformedCenter.Y;

            // set initial velocity if translate flicks are allowed
            if (canTranslateFlick)
            {
                inertiaProcessor.InitialVelocityX = initialVelocity.X;
                inertiaProcessor.InitialVelocityY = initialVelocity.Y;
                inertiaProcessor.DesiredDeceleration = deceleration;
            }
            else
            {
                inertiaProcessor.InitialVelocityX = 0.0f;
                inertiaProcessor.InitialVelocityY = 0.0f;
                inertiaProcessor.DesiredDeceleration = 0.0f;
            }


            // set expansion velocity if scale flicks are allowed
            if (!IsZero(expansionVelocity) && canScaleFlick)
            {
                inertiaProcessor.InitialExpansionVelocity = expansionVelocity;
                inertiaProcessor.InitialRadius = (AxisAlignedBoundingRectangle.Width / 2 + AxisAlignedBoundingRectangle.Height / 2) / 2;
                inertiaProcessor.DesiredExpansionDeceleration = expansionDeceleration;
            }
            else
            {
                inertiaProcessor.InitialExpansionVelocity = 0.0f;
                inertiaProcessor.InitialRadius = 1.0f;
                inertiaProcessor.DesiredExpansionDeceleration = 0.0f;
            }


            // set angular velocity if rotation flicks are allowed
            if (!IsZero(angularVelocity) && canRotateFlick)
            {
                inertiaProcessor.InitialAngularVelocity = angularVelocity;
                inertiaProcessor.DesiredAngularDeceleration = angularDeceleration;
            }
            else
            {
                inertiaProcessor.InitialAngularVelocity = 0.0f;
                inertiaProcessor.DesiredAngularDeceleration = 0.0f;
            }

            // Use parent's edges as the outermost bounds for InertiaProcessor.
            inertiaProcessor.LeftBoundary = parent.Left;
            inertiaProcessor.RightBoundary = parent.Right;
            inertiaProcessor.TopBoundary = parent.Top;
            inertiaProcessor.BottomBoundary = parent.Bottom;

            extrapolating = true;
        }

        //==========================================================//
        /// <summary>
        /// Event handler for the inertia processor's complete event.
        /// Occurs whenever the item comes to rest after being flicked.
        /// </summary>
        /// <param name="sender">The inertia processor that raised the event.</param>
        /// <param name="e">The event args for the event.</param>
        private void OnAffine2DInertiaCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
            extrapolating = false;
        }

        #endregion

        #region Contact Tracking

        //==========================================================//
        /// <summary>
        /// Handles contact down events from the UIController.
        /// </summary>
        /// <param name="contactEvent">Event data.</param>
        protected override void OnContactDown(ContactTargetEvent contactEvent)
        {
            base.OnContactDown(contactEvent);

            // Capture the contact
            Controller.Capture(contactEvent.Contact, this);

            // Raise the Activated event if this is the first contact
            if (ContactsCaptured.Count == 1)
            {
                OnActivated();
            }

            // Transform the contact into the screen coordinate system.
            Vector2 temp = Vector2.Transform(new Vector2(contactEvent.Contact.CenterX, contactEvent.Contact.CenterY), Transform);
            currentManipulators.Add(new Manipulator(contactEvent.Contact.Id, temp.X, temp.Y));
        }

        //==========================================================//
        /// <summary>
        /// Handles contact changed events from the UIController.
        /// </summary>
        /// <param name="contactEvent">Event data.</param>
        protected override void OnContactChanged(ContactTargetEvent contactEvent)
        {
            // Transform the contact into the screen coordinate system.
            Vector2 temp = Vector2.Transform(new Vector2(contactEvent.Contact.CenterX, contactEvent.Contact.CenterY), Transform);
            // Remove manipulator if the Id already exists in the list. Can't update Manipulator
            // in-place as X and Y are read-only.
            // Using an anonymous predicate delegate to locate matching ManipulatorIds. 
            currentManipulators.RemoveAll(delegate(Manipulator m)
                    { return m.ManipulatorId == contactEvent.Contact.Id; });
            // Add it to the currentManipulators list with updated values.
            currentManipulators.Add(new Manipulator(contactEvent.Contact.Id, temp.X, temp.Y));
        }

        //==========================================================//
        /// <summary>
        /// Handles contact up events from the UIController.
        /// </summary>
        /// <param name="contactEvent">Event data.</param>
        protected override void OnContactUp(ContactTargetEvent contactEvent)
        {
            // Release the contact
            Controller.Release(contactEvent.Contact);
            base.OnContactUp(contactEvent);

            // A manipulator can't be current and removed at the same time.
            // Remove it from the current list before adding to removed list.
            currentManipulators.RemoveAll(delegate(Manipulator m)
                    { return m.ManipulatorId == contactEvent.Contact.Id; });
            // Add the contact to the removed manipulators list
            removedManipulators.Add(new Manipulator(contactEvent.Contact.Id, contactEvent.Contact.CenterX, contactEvent.Contact.CenterY));
            
            // Raise the Deactivated event if this is the last contact
            if (ContactsCaptured.Count < 1)
            {
                OnDeactivated();
            }
        }

        //==========================================================//
        /// <summary>
        /// Let the manipulation processor process contact information.
        /// </summary>
        public void ProcessContacts()
        {
            if (manipulationProcessor == null || inertiaProcessor == null)
            {
                SetAllowedManipulations();
            }

            if (extrapolating)
            {
                if (currentManipulators.Count == 0)
                {
                    // process inertia
                    inertiaProcessor.Process();
                }
                else
                {
                    // stop inertia
                    inertiaProcessor.Complete();
                    manipulationProcessor.ProcessManipulators(currentManipulators, removedManipulators);
                }
            }

            // update the manipulation
            else
            {
                manipulationProcessor.ProcessManipulators(currentManipulators, removedManipulators);
            }

            currentManipulators.Clear();
            removedManipulators.Clear();
        }

        //==========================================================//
        /// <summary>
        /// Raise the Activated event.
        /// </summary>
        protected virtual void OnActivated()
        {
            if (Activated != null)
            {
                Activated(this, new EventArgs());
            }
        }

        //==========================================================//
        /// <summary>
        /// Raise the Deactivated event.
        /// </summary>
        protected virtual void OnDeactivated()
        {
            if (Deactivated != null)
            {
                Deactivated(this, new EventArgs());
            }
        }

        #endregion

        //==========================================================//
        /// <summary>
        /// Draw the XNAScatterViewItem to the screen.
        /// </summary>
        /// <param name="batch">The SpriteBatch to which content will be drawn.</param>
        /// <itemZOrder>The z order to use when drawing the item.</itemZOrder>
        public void Draw(SpriteBatch batch, float itemZOrder)
        {
            batch.Draw(content, transformedCenter, null, Color.White, orientation, DrawingOrigin, zoomFactor * scaleFactor, SpriteEffects.None, itemZOrder);
        }

        //==========================================================//
        /// <summary>
        /// Checks center against boundaries and moved center inside 
        /// of boundaries if it is found to be out of bounds.
        /// </summary>
        private void RestrictCenter()
        {
            float leftEdge, rightEdge, topEdge, bottomEdge;
            GetBoundingRect(out leftEdge, out rightEdge, out topEdge, out bottomEdge);

            if (transformedCenter.X < leftEdge)
            {
                transformedCenter.X = leftEdge;
            }
            else if (transformedCenter.X > rightEdge)
            {
                transformedCenter.X = rightEdge;
            }

            if (transformedCenter.Y < topEdge)
            {
                transformedCenter.Y = topEdge;
            }
            else if (transformedCenter.Y > bottomEdge)
            {
                transformedCenter.Y = bottomEdge;
            }
        }

        /// <summary>
        /// To ensure that the item doesn't go off completely into a corner from where the user
        /// can't bring it back, we restrict the item's center within a deflated bounding box.
        /// </summary>
        /// <param name="leftBoundingEdge">Left edge of the bounding rect.</param>
        /// <param name="rightBoundingEdge">Right edge of the bounding rect.</param>
        /// <param name="topBoundingEdge">Top edge of the bounding rect.</param>
        /// <param name="bottomBoundingEdge">Bottom edge of the bounding rect.</param>
        private void GetBoundingRect(out float leftBoundingEdge, out float rightBoundingEdge, out float topBoundingEdge, out float bottomBoundingEdge)
        {
            leftBoundingEdge = parent.Left + parent.BoundaryThreshold;
            rightBoundingEdge = parent.Right - parent.BoundaryThreshold;
            topBoundingEdge = parent.Top + parent.BoundaryThreshold;
            bottomBoundingEdge = parent.Bottom - parent.BoundaryThreshold;
        }

        //==========================================================//
        /// <summary>
        /// Determines if the item contains a point (represented by a vector).
        /// </summary>
        /// <param name="point">The vector that is being checked against the item.</param>
        /// <returns>True if the item contains the vector, false otherwise.</returns>
        public bool Contains(Vector2 point)
        {
            // Create an axis aligned bounding rect and rotate the vector so its values are relative to the new rectangle
            Vector2 rotated = GeometryHelper.RotatePointVectorAroundOrigin(transformedCenter, point, -orientation);
            Rectangle bounds = AxisAlignedBoundingRectangle;

            // see if the adjusted vector is in the axis aligned rectangle
            return GeometryHelper.RectangleContainsPointVector(bounds, rotated);
        }

        //==========================================================//
        /// <summary>
        /// Restrict scale to prevent the scatter view from being scaled past its maximum or minimum scale values.
        /// </summary>
        /// <param name="scaleDelta">The proposed change in scale.</param>
        /// <returns>The restricted change in scale</returns>
        private float RestrictScale(float scaleDelta)
        {
            float modifiedScale = scaleDelta * scaleFactor;

            if (modifiedScale > maxScaleFactor)
            {
                return maxScaleFactor / scaleFactor;
            }

            if (modifiedScale < minScaleFactor)
            {
                return minScaleFactor / scaleFactor;
            }

            return scaleDelta;
        }

        //==========================================================//
        /// <summary>
        /// Restrict rotation to keep values between 0 and 360 degrees.
        /// </summary>
        /// <param name="desiredRotation">The proposed change in rotation.</param>
        /// <returns>The restricted change in rotation.</returns>
        private static float RestrictOrientation(float desiredRotation)
        {
            float constrainedRotation = desiredRotation % 360.0f;

            return constrainedRotation;
        }

        //==========================================================//
        /// <summary>
        /// Determines whether or not a float value is effectively zero.
        /// </summary>
        /// <param name="value">The float that is being tested.</param>
        /// <returns>True if the float is within float.Epsilon of zero, false otherwise.</returns>
        private static bool IsZero(float value)
        {
            return Math.Abs(value) < float.Epsilon;
        }
    }
}
