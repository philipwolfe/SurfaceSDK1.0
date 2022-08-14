using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Surface.Core;
using Microsoft.Surface.Core.Manipulations;

namespace TextileManipulation
{
    internal interface IContactableObject
    {
        void AddContact(int contactId, Vector2 position);
        void UpdateContact(int contactId, Vector2 position);
        void RemoveContact(int contactId, Vector2 position);
        void ProcessContacts();

        Vector2 Center { get; set; }
        float Scale { get; set; }
        float Orientation { get; set; }

        bool CanTranslate { get; set; }
        bool CanTranslateFlick { get; set; }
        bool CanRotate { get; set; }
        bool CanRotateFlick { get; set; }
        bool CanScale { get; set; }
        bool CanScaleFlick { get; set; }
    }

    internal struct BoundingRect
    {
        private readonly Vector2 center;
        private readonly Vector2 size;

        public BoundingRect(Vector2 center, Vector2 size)
        {
            this.center = center;
            this.size = size;
        }

        public float Left
        {
            get { return center.X - (size.X / 2.0f); }
        }
        public float Top
        {
            get { return center.Y - (size.Y / 2.0f); }
        }
        public float Right
        {
            get { return center.X + (size.X / 2.0f); }
        }
        public float Bottom
        {
            get { return center.Y + (size.Y / 2.0f); }
        }
    }

    internal static class Contacts
    {
        private static Dictionary<int, IContactableObject> CapturedContactsByContact = new Dictionary<int, IContactableObject>();

        private static Dictionary<IContactableObject, List<int>> CapturedContactsByContactableObject = new Dictionary<IContactableObject, List<int>>();

        public static void CaptureContact(int contactId, IContactableObject contactable)
        {
            // First add the pair to the dictionary indexed by ID
            CapturedContactsByContact.Add(contactId, contactable);

            // Now add the pair to the dictionary indexed by contactable object.
            if (CapturedContactsByContactableObject.ContainsKey(contactable))
            {
                // If the contact already exists in the dictionary, add this contact to the object's list
                CapturedContactsByContactableObject[contactable].Add(contactId);
            }
            else
            {
                // If the contact is not in the dictionary yet, make a new entry and put the contact
                // in the object's list
                List<int> list = new List<int>();
                list.Add(contactId);
                CapturedContactsByContactableObject.Add(contactable, list);
            }
        }

        public static void ReleaseContactCapture(int contactId)
        {
            // Before the id is removed from the dictionary indexed by id, get the value 
            // so it can be used later to modify the dictionary indexed by object
            IContactableObject contactable = null;
            if (CapturedContactsByContact.TryGetValue(contactId, out contactable))
            {
                // Got the object, now remove the id from the dictionary
                CapturedContactsByContact.Remove(contactId);

                if (CapturedContactsByContactableObject.ContainsKey(contactable))
                {
                    CapturedContactsByContactableObject[contactable].Remove(contactId);
                }
            }

        }
    }

    internal static class Timer
    {
        private static Stopwatch stopwatch = new Stopwatch();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static Timer()
        {
            Start();
        }

        public static long EllapsedTime
        {
            get
            {
                return stopwatch.ElapsedTicks;
            }
        }

        public static void Start()
        {
            stopwatch.Start();
        }
    }

    internal class ManipulationAdapter : IContactableObject
    {
        private Vector2 center;
        private float scale = 1.0f;
        private float minScaleFactor = 0.45f;
        private float maxScaleFactor = 2.6f;
        private float orientation;

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

        private readonly Vector2 sizeOrigin;
        private readonly BoundingRect boundingRect;

        private Matrix transformMatrix;

        public ManipulationAdapter(Vector2 sizeOrigin, BoundingRect boundingRect)
        {
            this.sizeOrigin = sizeOrigin;
            this.boundingRect = boundingRect;

            SetConvertMatrix();
        }

        #region Layout and Hit Testing Properties

        private void SetConvertMatrix()
        {
            transformMatrix = Matrix.CreateScale(scale) *
                        Matrix.CreateRotationZ(orientation) *
                        Matrix.CreateTranslation(center.X, center.Y, 0);
        }

        public Vector2 Transform(Vector2 position)
        {
            return Vector2.Transform(position, transformMatrix);
        }

        //==========================================================//
        /// <summary>
        /// The center of the object. All layout is based off of this property
        /// </summary>
        public Vector2 Center
        {
            get
            {
                return center;
            }
            set
            {
                center = value;
                SetConvertMatrix(); 
            }
        }

        public float Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                SetConvertMatrix(); 
            }
        }

        public float Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                orientation = value;
                SetConvertMatrix(); 
            }
        }

        private Vector2 ActualSize
        {
            get { return sizeOrigin * Scale; }
        }

        #endregion

        #region Manipulation Properties

        //==========================================================//
        /// <summary>
        /// Gets or sets whether or not the item can be moved via direct manipulation
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
        /// Gets or sets whether or not the item can be moved via flick
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
        /// Gets or sets whether or not the item can be rotated via direct manipulation
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
        /// Gets or sets whether or not the item can be rotated via flick
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
        /// Gets or sets whether or not the item can be scaled via direct manipulation
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
        /// Gets or sets whether or not the item can be scaled via flick
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

        #region Initalization

        //==========================================================//
        /// <summary>
        /// Creates manipulation and inertis processors that support only this item's allowed manipulations
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

        #endregion

        #region Manipulation and Inertia Processor Events

        //==========================================================//
        /// <summary>
        /// Event handler for the manipulation processor's delta event. 
        /// Occurs whenever the first time that the manipulation processor processes a 
        /// group of manipulators
        /// </summary>
        /// <param name="sender">The manipulation processor that raised the event</param>
        /// <param name="e">The event args for the event</param>
        private void OnAffine2DManipulationStarted(object sender, Affine2DOperationStartedEventArgs e)
        {
            if (extrapolating)
            {
                StopInertia();
            }
            manipulating = true;

            if (canRotate || canRotateFlick)
            {
                manipulationProcessor.PivotX = center.X;
                manipulationProcessor.PivotY = center.Y;
                manipulationProcessor.PivotRadius = Math.Max(ActualSize.X, ActualSize.Y) / 2.0f;
            }
        }

        //==========================================================//
        /// <summary>
        /// Event handler for the manipulation and inertia processor's delta events. 
        /// Occurs whenever the manipulation or inertia processors processes or extrapolate 
        /// manipulator data
        /// </summary>
        /// <param name="sender">The manipulation or inertia processor that raised the event</param>
        /// <param name="e">The event args for the event</param>
        private void OnAffine2DDelta(object sender, Affine2DOperationDeltaEventArgs e)
        {
            Debug.Assert(manipulating && sender is Affine2DManipulationProcessor ||
                extrapolating && sender is Affine2DInertiaProcessor);

            Vector2 manipulationOrigin = new Vector2(e.ManipulationOriginX, e.ManipulationOriginY);
            Vector2 manipulationDelta = new Vector2(e.DeltaX, e.DeltaY);
            Vector2 previousOrigin = new Vector2(manipulationOrigin.X - e.DeltaX, manipulationOrigin.Y - e.DeltaY);
            float restrictedOrientation = RestrictOrientation(e.RotationDelta);
            float restrictedScale = RestrictScale(e.ScaleDelta);

            // Adjust the position of the item based on change in rotation
            if (restrictedOrientation != 0.0f)
            {
                Vector2 manipulationOffset = center - previousOrigin;
                Vector2 rotatedOffset = RotatePointVector(manipulationOffset, restrictedOrientation);
                Vector2 compensation = rotatedOffset - manipulationOffset;
                center += compensation;
            }

            // Adjust the position of the item based on change in scale
            if (restrictedScale != 1.0f)
            {
                Vector2 manipulationOffset = manipulationOrigin - center;
                Vector2 scaledOffset = manipulationOffset * restrictedScale;
                Vector2 compensation = manipulationOffset - scaledOffset;
                center += compensation;
            }

            // Rotate the item if it is allowed
            if (canRotate || canRotateFlick)
            {
                orientation += restrictedOrientation;
            }

            // Scale the item if it is allowed
            if (canScale || canScaleFlick)
            {
                scale *= restrictedScale;
            }

            // Translate the item if it is allowed
            if (canTranslate || canTranslateFlick)
            {
                center += new Vector2(e.DeltaX, e.DeltaY);
            }

            RestrictCenter();

            if (canRotate || canRotateFlick)
            {
                manipulationProcessor.PivotX = center.X;
                manipulationProcessor.PivotY = center.Y;
                manipulationProcessor.PivotRadius = Math.Max(ActualSize.X, ActualSize.Y) / 2.0f;
            }

            SetConvertMatrix();
        }

        //==========================================================//
        /// <summary>
        /// Event handler for the manipulation processor's completed event. 
        /// Occurs whenever the manipulation processor processes manipulator 
        /// data where all remaining contacts have been removed.
        /// Check final deltas and start the inertia processor if they are high enough
        /// </summary>
        /// <param name="sender">The manipulation processor that raised the event</param>
        /// <param name="e">The event args for the event</param>
        private void OnAffine2DManipulationCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
            // manipulation completed
            manipulating = false;

            // Get the inital inertia values
            Vector2 initialVelocity = new Vector2(e.VelocityX, e.VelocityY);
            float angularVelocity = e.AngularVelocity;
            float expansionVelocity = e.ExpansionVelocity;

            // Calculate the deceleration rates

            // 4 inches/second squared (4 inches * 96 pixels per inch / (10000000 ticks per second)^2)
            const float deceleration = 4.0f * 96.0f / (10000000.0f * 10000000.0f);
            const float expansionDeceleration = 4.0f * 96.0f / (10000000.0f * 10000000.0f);

            // 90 degrees/second squared, specified in radians (1/2 * pi / (1000 miliseconds per second)^2)
            const float angularDeceleration = 90.0f / 180.0f * (float)Math.PI / (10000000.0f * 10000000.0f);

            // Start inertia
            inertiaProcessor.InitialOriginX = e.ManipulationOriginX;
            inertiaProcessor.InitialOriginY = e.ManipulationOriginY;
            inertiaProcessor.InitialTimestamp = Timer.EllapsedTime;

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
                inertiaProcessor.InitialRadius = (ActualSize.X / 2f + ActualSize.Y / 2f) / 2f;
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


            // Set the boundaries in which manipulations can occur
            Vector2 manipulationOffset = Vector2.Subtract(new Vector2(e.ManipulationOriginX, e.ManipulationOriginY), center);
            inertiaProcessor.LeftBoundary = boundingRect.Left + manipulationOffset.X;
            inertiaProcessor.RightBoundary = boundingRect.Right + manipulationOffset.X;
            inertiaProcessor.TopBoundary = boundingRect.Top + manipulationOffset.Y;
            inertiaProcessor.BottomBoundary = boundingRect.Bottom + manipulationOffset.Y;

            extrapolating = true;

        }

        //==========================================================//
        /// <summary>
        /// Event handler for the inertia processor's complete event. 
        /// Occurs whenever the item comes to rest after being flicked
        /// </summary>
        /// <param name="sender">The inertia processor that raised the event</param>
        /// <param name="e">The event args for the event</param>
        private void OnAffine2DInertiaCompleted(object sender, Affine2DOperationCompletedEventArgs e)
        {
            extrapolating = false;
        }

        #endregion

        #region Contact Tracking

        public void AddContact(int contactId, Vector2 position)
        {
            Contacts.CaptureContact(contactId, this);
            currentManipulators.Add(new Manipulator(contactId, position.X, position.Y));
        }

        public void UpdateContact(int contactId, Vector2 position)
        {
            // Remove manipulator if the Id already exists in the list. Can't update Manipulator
            // in-place as X and Y are read-only.
            // Using an anonymous predicate delegate to locate matching ManipulatorIds. 
            currentManipulators.RemoveAll(delegate(Manipulator m)
                    { return m.ManipulatorId == contactId; });
            currentManipulators.Add(new Manipulator(contactId, position.X, position.Y));
        }

        public void RemoveContact(int contactId, Vector2 position)
        {
            // A manipulator can't be current and removed at the same time.
            // Remove it from the current list before adding to removed list.
            currentManipulators.RemoveAll(delegate(Manipulator m)
                    { return m.ManipulatorId == contactId; });
            removedManipulators.Add(new Manipulator(contactId, position.X, position.Y));
            Contacts.ReleaseContactCapture(contactId);
        }

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
                    try
                    {
                        // process inertia
                        inertiaProcessor.Process(Timer.EllapsedTime);
                    }
                    catch (InvalidOperationException)
                    {
                        // TODO : something is wrong here. Fix it.
                        Debug.WriteLine("ProcessContacts(): InvalidOperationExeception caught.");
                    }
                }
                else
                {
                    StopInertia();
                    manipulationProcessor.ProcessManipulators(Timer.EllapsedTime, currentManipulators, removedManipulators);
                }
            }

            // update the manipulation
            else
            {
                manipulationProcessor.ProcessManipulators(Timer.EllapsedTime, currentManipulators, removedManipulators);
            }

            currentManipulators.Clear();
            removedManipulators.Clear();
        }

        private void StopInertia()
        {
            try
            {
                // stop inertia
                inertiaProcessor.Complete(Timer.EllapsedTime);
            }
            catch (InvalidOperationException)
            {
                // TODO : something is wrong here. Fix it. 
                Debug.WriteLine("StopInertia(): InvalidOperationExeception caught.");
            }
        }

        #endregion

        //==========================================================//
        /// <summary>
        /// Checks center against boundaries and moved center inside 
        /// of boundaries if it is found to be out of bounds
        /// </summary>
        private void RestrictCenter()
        {
            if (center.X < boundingRect.Left)
                center.X = boundingRect.Left;
            else if (center.X > boundingRect.Right)
                center.X = boundingRect.Right;

            if (center.Y < boundingRect.Top)
                center.Y = boundingRect.Top;
            else if (center.Y > boundingRect.Bottom)
                center.Y = boundingRect.Bottom;
        }

        //==========================================================//
        /// <summary>
        /// Restrict scale to prevent the scatter view from being scaled past its maximum or minimum scale values
        /// </summary>
        /// <param name="scaleDelta">The proposed change in scale</param>
        /// <returns>The restricted change in scale</returns>
        private float RestrictScale(float scaleDelta)
        {
            float modifiedScale = scaleDelta * scale;

            if (modifiedScale > maxScaleFactor)
            {
                return maxScaleFactor / scale;
            }

            if (modifiedScale < minScaleFactor)
            {
                return minScaleFactor / scale;
            }

            return scaleDelta;
        }

        //==========================================================//
        /// <summary>
        /// Restrict rotation to keep values between 0 and 360 degrees
        /// </summary>
        /// <param name="desiredRotation">The proposed change in rotation</param>
        /// <returns>The restricted change in rotation</returns>
        private static float RestrictOrientation(float desiredRotation)
        {
            float constrainedRotation = desiredRotation % 360.0f;

            return constrainedRotation;
        }

        //==========================================================//
        /// <summary>
        /// Determines whether or not a float value is effectively zero
        /// </summary>
        /// <param name="value">The float that is being tested</param>
        /// <returns>True if the float is within float.Epsilon of zero, false otherwise</returns>
        private static bool IsZero(float value)
        {
            return Math.Abs(value) < float.Epsilon;
        }

        public static Vector2 RotatePointVector(Vector2 rotateMe, float radians)
        {
            return new Vector2((float)(rotateMe.X * Math.Cos(radians) - rotateMe.Y * Math.Sin(radians)),
                               (float)(rotateMe.X * Math.Sin(radians) + rotateMe.Y * Math.Cos(radians)));
        }
    }
}
