using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;

namespace PhotoPaint
{
    /// <summary>
    /// Interaction logic for Window1.xaml.
    /// </summary>

    public partial class Window1
    {
        /// <summary>
        /// The most recent stroke (used for undo operations).
        /// </summary>
        private Stroke mostRecentStroke;

        /// <summary>
        /// A mapping of each stroke to list of point timings in milliseconds.
        /// </summary>
        private readonly Dictionary<Stroke, List<long>> recordedStrokes = new Dictionary<Stroke, List<long>>();

        /// <summary>
        /// A copy of the recordedStrokes used for playback.
        /// Strokes are removed from this Dictionary as they are played back.
        /// </summary>
        private Dictionary<Stroke, List<long>> recordedStrokesCopy;

        /// <summary>
        /// Keeps track of strokes as they are played back.
        /// Maps the new stroke (copy) being constructed during playback to the original stroke.
        /// </summary>
        private readonly Dictionary<Stroke, Stroke> newStrokes = new Dictionary<Stroke, Stroke>();

        /// <summary>
        /// Whether or not new strokes are being recorded.
        /// </summary>
        private bool isRecording;

        /// <summary>
        /// Whether or not strokes are being played back.
        /// </summary>
        private bool isPlaying;

        /// <summary>
        /// Whether or not the playback or recording is paused.
        /// </summary>
        private bool isPaused;

        /// <summary>
        /// True when end of media has been reached.
        /// </summary>
        private bool mediaEnded;

        /// <summary>
        /// Interval timer used to drive recorded stroke playback.
        /// </summary>
        private DispatcherTimer timer;

        /// <summary>
        /// Stopwatch used to time strokes for playback when no video is present.
        /// </summary>
        private Stopwatch stopwatch;

        /// <summary>
        /// Duration of playback loop (in seconds) when no video is present.
        /// </summary>
        private const int duration = 12;
        
        /// <summary>
        /// Tracks a stroke being created by the mouse.
        /// </summary>
        private Stroke currentMouseStroke;

        /// <summary>
        /// Key used to track strokes in a contact's user data.
        /// </summary>
        private readonly object strokeKey = new object();

        // Cached button images
        private readonly Image eraseButtonImage;
        private readonly Image drawButtonImage;


        /// <summary>
        /// Gets or sets the value of isPaused.
        /// </summary>
        internal bool IsPaused
        {
            get { return isPaused; }
            set
            {
                isPaused = value;
                if (isPaused)
                {
                    PauseButton.Background = Brushes.DarkGray;
                }
                else
                {
                    PauseButton.Background = Brushes.Transparent;
                }
            }
        }

        #region Initalization

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Window1()
        {
            InitializeComponent();

            eraseButtonImage = LoadImageFromPath("Resources\\Erase.png");
            drawButtonImage = LoadImageFromPath("Resources\\Draw.png");


            // Add handlers for Application activation events
            AddActivationHandlers();
        }

        /// <summary>
        /// Occurs when the window is about to close.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // Remove handlers for Application activation events
            RemoveActivationHandlers();
        }

        /// <summary>
        /// Called when the application is finished initalizing
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Load images and video from the public folders.
            // These are default OS folders that will always be in these locations
            // unless the user has deliberately moved them.
            string publicFoldersPath = Environment.GetEnvironmentVariable("public");

            SetMoviePadContent(publicFoldersPath + @"\Videos\Sample Videos");
            SetPhotoPadContent(publicFoldersPath + @"\Pictures\Sample Pictures");
        }

        /// <summary>
        /// Sets the content for the MovieCanvas.
        /// </summary>
        /// <param name="publicVideosPath">Path to public videos.</param>
        private void SetMoviePadContent(string publicVideosPath)
        {
            if (Directory.Exists(publicVideosPath))
            {
                // Use this video if it exists.
                string targetVideoPath = publicVideosPath + @"\Bear.wmv";
                if (File.Exists(targetVideoPath))
                {
                    // Target movie exists, use it.
                    Movie.Source = new Uri(targetVideoPath);
                }
                else
                {
                    // Target movie does not exist, use the first WMV found in the directory.
                    string[] files = Directory.GetFiles(publicVideosPath, "*.wmv");
                    if (files.Length > 0)
                    {
                        Movie.Source = new Uri(files[0]);
                    }
                }
            }

            if (Movie.Source == null)
            {
                // If there aren't any movies at all, use a blank canvas.
                MovieCanvas.Background = Brushes.White;
                stopwatch = new Stopwatch();
            }

            timer = new DispatcherTimer();
            timer.Tick += OnTick;

            // Play the movie on startup.
            PlayMovie();
           
            // Initial button state.
            isPlaying = true;
            PlayButton.Background = Brushes.DarkGray;
            isRecording = false;
            RecordButton.Background = Brushes.Transparent;
            IsPaused = false;
            EnableButtons(true);
        }

        /// <summary>
        /// Sets the content for the PhotoCanvas.
        /// </summary>
        /// <param name="publicPhotosPath">Path to public photos.</param>
        private void SetPhotoPadContent(string publicPhotosPath)
        {

            if (Directory.Exists(publicPhotosPath))
            {
                // Use this images if it exists.
                string targetPhotoPath = publicPhotosPath + @"\Humpback Whale.jpg";
                if (File.Exists(targetPhotoPath))
                {
                    // Target image exists, use it
                    Photo.Source = new BitmapImage(new Uri(targetPhotoPath));
                }
                else
                {
                    // Target image does not exist, use the first JPG found in the directory.
                    string[] files = Directory.GetFiles(publicPhotosPath, "*.jpg");
                    if (files.Length > 0)
                    {
                        Photo.Source = new BitmapImage(new Uri(files[0]));
                    }
                }
            }

            if (Photo.Source == null)
            {
                // If there aren't any images at all, use a blank canvas.
                PostcardCanvas.Background = Brushes.White;
                
            }
        }

        #endregion Initalization

        #region InkCanvasEvents

        /// <summary>
        /// Toggles the edit mode of a SurfaceInkCanvas between EraseByPoint and Ink.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void EditModeClicked(object sender, RoutedEventArgs args)
        {
            SurfaceButton button = (SurfaceButton)sender;
            if (DrawingPadCanvas.EditingMode == SurfaceInkEditingMode.Ink)
            {
                DrawingPadCanvas.EditingMode = SurfaceInkEditingMode.EraseByPoint;

                // Load the new button image
                button.Content = eraseButtonImage;
            }
            else
            {
                DrawingPadCanvas.EditingMode = SurfaceInkEditingMode.Ink;

                // Load the new button image
                button.Content = DrawButtonImage;
            }
        }

        #endregion InkCanvasEvents

        #region Drawing Pad Specific Code

        /// <summary>
        /// Handles the OnStrokeCollected event for SurfaceInkCanvas.
        /// </summary>
        /// <param name="sender">The SurfaceInkCanvas that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs args)
        {
            mostRecentStroke = args.Stroke;
        }

        /// <summary>
        /// Handles the click event for the undo button.
        /// </summary>
        /// <param name="sender">The button that raised the event.</param>
        /// <param name="e">The arguments for the event.</param>
        void UndoClicked(object sender, RoutedEventArgs e)
        {
            if (mostRecentStroke != null)
            {
                DrawingPadCanvas.Strokes.Remove(mostRecentStroke);
                mostRecentStroke = null;
            }
        }

        /// <summary>
        /// Handles the tap event for the current color indicator.
        /// </summary>
        /// <param name="sender">The color wheel.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnCurrentColorTap(object sender, ContactEventArgs args)
        {
            // Replace the current color botton with the color wheel
            ColorWheel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the tap event for the color wheel.
        /// </summary>
        /// <param name="sender">The color wheel.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnColorWheelContactTap (object sender, ContactEventArgs args)
        {
            // Set the color on the CurrentColor indicator and on the SurfaceInkCanvas
            Color color = GetPixelColor(args.Contact);
            
            // Black means the user touched the transparent part of the wheel. In that 
            // case, leave the color set to its current value
            if (color != Colors.Black)
            {
                DrawingPadCanvas.DefaultDrawingAttributes.Color = color;
                CurrentColor.Fill = new SolidColorBrush(color);
            }

            // Set editing mode to ink
            DrawingPadCanvas.EditingMode = SurfaceInkEditingMode.Ink;

            // Update the button image
            EditModeButton.Content = DrawButtonImage;

            // Replace the color wheel with the current color button
            ColorWheel.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Handles the ContactDownEvent for the color wheel and the current color indicator.
        /// </summary>
        /// <param name="sender">The color wheel that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnColorSelectionPreviewContactDown(object sender, ContactEventArgs args)
        {
            // Capture the contact and handle the event 
            IInputElement element = sender as IInputElement;
            if (element != null && Contacts.CaptureContact(args.Contact, element))
            {
                args.Handled = true;
            }
        }

        /// <summary>
        /// Gets the color of a specific pixel.
        /// </summary>
        /// <param name="pt">The point from which to get a color.</param>
        /// <returns>The color of the point.</returns>
        private System.Windows.Media.Color GetPixelColor(Contact contact)
        {
            // Translate the point according to whatever transforms are on the color wheel.
            Point rawPoint = contact.GetPosition(ColorWheel);
            Point transformedPoint = ColorWheel.RenderTransform.Transform(rawPoint);

            // The point is outside the color wheel. Return black.
            if (transformedPoint.X < 0 || transformedPoint.X >= ColorWheel.Source.Width ||
                transformedPoint.Y < 0 || transformedPoint.Y >= ColorWheel.Source.Height)
            {
                return Colors.Black;
            }

            // The point is inside the color wheel. Find the color at the point.
            CroppedBitmap cb = new CroppedBitmap(ColorWheel.Source as BitmapSource, new Int32Rect((int)transformedPoint.X, (int)transformedPoint.Y, 1, 1));
            byte[] pixels = new byte[4];
            cb.CopyPixels(pixels, 4, 0);
            return Color.FromRgb(pixels[2], pixels[1], pixels[0]);
        }

        #endregion Drawing Pad Specific Code

        #region Photo Pad Specific Code

        /// <summary>
        /// Handles the click event for the clear button.
        /// </summary>
        /// <param name="sender">The button that reaised the event.</param>
        /// <param name="e">The arguments for the event.</param>
        void ClearClicked(object sender, RoutedEventArgs e)
        {
            PostcardCanvas.Strokes.Clear();
        }

        /// <summary>
        /// Handles the click event for the Move/Draw mode button on the PhotoPad.
        /// </summary>
        /// <param name="sender">The button that raised the event.</param>
        /// <param name="e">The arguments for the event.</param>
        void InkCanvasOnOffChanged(object sender, RoutedEventArgs e)
        {
            SurfaceButton button = (SurfaceButton) sender;
            if (PostcardCanvas.IsHitTestVisible)
            {
                // Prevent the SurfaceinkCanvas from processing contacts
                PostcardCanvas.IsHitTestVisible = false;

                // Load the new button image
                button.Content = MoveButtonImage;
            }
            else
            {
                // Let the SurfaceinkCanvas start processing contacts
                PostcardCanvas.IsHitTestVisible = true;

                // Load the new button image
                button.Content = drawButtonImage;
            }
        }

        #endregion Photo Pad Specific Code

        #region Movie Pad Specific Code

        #region Contact and Mouse Event Handlers

        // Remarks:
        // 
        // We use the preview events because the normal events are consumed by
        // the MovieCanvas.
        //
        // These events are used to create a stroke that looks like the stroke 
        // being added to the MovieCanvas.  The actual stroke is not available
        // until it is completed.  We use these events to record the stroke and
        // timestamp data so we can manully recreate the stroke during playback.


        /// <summary>
        /// Handles the PreviewContactDown event for the MovieCanvas.
        /// Begins recording a new stroke.
        /// </summary>
        /// <param name="sender">The SurfaceInkCanvas that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnMovieCanvasPreviewContactDown(object sender, ContactEventArgs args)
        {
            // If isRecord is set to true, record the stroke for playback later.
            if (isRecording && MovieCanvas.CaptureContact(args.Contact))
            {
                SurfaceInkCanvas canvas = (SurfaceInkCanvas)sender;
                System.Windows.Point point = args.Contact.GetPosition(canvas);
                StylusPointCollection points = new StylusPointCollection();
                points.Add(new StylusPoint(point.X, point.Y));

                // Cannot create a stroke without at least one point in the StylusPointCollection
                Stroke stroke = new Stroke(points);
                stroke.DrawingAttributes = canvas.DefaultDrawingAttributes.Clone();
                if (MovieCanvas.UsesContactShape)
                {
                    stroke.DrawingAttributes.StylusTip = StylusTip.Ellipse;
                    Ellipse contactEllipse = args.Contact.GetEllipse(MovieCanvas);
                    stroke.DrawingAttributes.Height = contactEllipse.Height;
                    stroke.DrawingAttributes.Width = contactEllipse.Width;

                    Matrix rotate = Matrix.Identity;
                    rotate.Rotate(args.Contact.GetOrientation(MovieCanvas));
                    stroke.DrawingAttributes.StylusTipTransform = rotate;
                }

                recordedStrokes.Add(stroke, new List<long>());
                recordedStrokes[stroke].Add(CurrentPosition());

                // Associate the new stroke with this contact
                args.Contact.SetUserData(strokeKey, stroke);
            }
        }

        /// <summary>
        /// Handles PreviewContactChanged event and LostContactCapture events for the MovieCanvas.
        /// Adds a point to the stroke associated with the contact.
        /// </summary>
        /// <param name="sender">The SurfaceInkCanvas that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnPreviewContactChangedOrLostContactCaptured(object sender, ContactEventArgs args)
        {
            if (isRecording)
            {
                // Retrieve the stroke assocatiated with this contact
                Stroke stroke = args.Contact.GetUserData(strokeKey) as Stroke;
                if (stroke != null)
                {
                    SurfaceInkCanvas canvas = (SurfaceInkCanvas) sender;
                    System.Windows.Point position = args.Contact.GetPosition(canvas);
                    AddPointToStroke(stroke, position.X, position.Y);
                }
            }
        }

        /// <summary>
        /// Handles the PreviewLeftMouseDown event for the MovieCanvas.
        /// Begins a new currentMouseStroke.
        /// </summary>
        /// <param name="sender">The SurfaceInkCanvas that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnMovieCanvasPreviewLeftMouseDown(object sender, MouseButtonEventArgs args)
        {
            if (isRecording)
            {
                SurfaceInkCanvas canvas = (SurfaceInkCanvas)sender;

                System.Windows.Point point = args.MouseDevice.GetPosition(MovieCanvas);
                StylusPointCollection points = new StylusPointCollection();
                points.Add(new StylusPoint(point.X, point.Y));

                currentMouseStroke = new Stroke(points);
                currentMouseStroke.DrawingAttributes = canvas.DefaultDrawingAttributes.Clone();

                recordedStrokes.Add(currentMouseStroke, new List<long>());
                recordedStrokes[currentMouseStroke].Add(CurrentPosition());
            }
        }

        /// <summary>
        /// Handles the PreviewMouseMove event for the MovieCanvas.
        /// Adds point to currentMouseStroke and record timing data.
        /// </summary>
        /// <param name="sender">The SurfaceInkCanvas that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnMovieCanvasPreviewMouseMove(object sender, MouseEventArgs args)
        {
            if (isRecording && currentMouseStroke != null)
            {
                System.Windows.Point position = args.MouseDevice.GetPosition(MovieCanvas);
                AddPointToStroke(currentMouseStroke, position.X, position.Y);
            }
        }

        /// <summary>
        /// Handles the PreviewLeftMouseUp event for the MovieCanvas.
        /// Ends the currentMouseStroke.
        /// </summary>
        /// <param name="sender">The SurfaceInkCanvas that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnMovieCanvasPreviewLeftMouseUp(object sender, MouseButtonEventArgs args)
        {
            if (isRecording && currentMouseStroke != null)
            {
                System.Windows.Point position = args.MouseDevice.GetPosition(MovieCanvas);
                AddPointToStroke(currentMouseStroke, position.X, position.Y);
            }
            currentMouseStroke = null;
        }

        /// <summary>
        /// Adds a point to a Stroke and records the time the point was added.
        /// </summary>
        /// <param name="stroke">The stroke to modify</param>
        /// <param name="positionX">The X coordinate of the point to add</param>
        /// <param name="positionY">the Y coordinate of the point to add</param>
        private void AddPointToStroke(Stroke stroke, double positionX, double positionY)
        {
            stroke.StylusPoints.Add(new StylusPoint(positionX, positionY));
            if (recordedStrokes.ContainsKey(stroke))
            {
                recordedStrokes[stroke].Add(CurrentPosition());
            }
        }

        #endregion

        #region Button Event Handlers

        /// <summary>
        /// Handles the click event for the record button.
        /// Resumes the current recording, or starts a new recording from the 
        /// beginning of our media.
        /// </summary>
        /// <param name="sender">The button that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnRecordButtonPressed(object sender, RoutedEventArgs args)
        {
            if (IsPaused && isRecording)
            {
                // Just Keep doing what we were doing.
                PlayMovie();
                return;
            }

            // Stop stroke playback
            isPlaying = false;
            PlayButton.Background = Brushes.Transparent;

            // Begin new recording
            MovieCanvas.Strokes.Clear();
            recordedStrokes.Clear();
            isRecording = true;
            RecordButton.Background = Brushes.DarkGray;

            // Allow user to draw on the SurfaceInkCanvas.
            MovieCanvas.IsHitTestVisible = true;

            RestartMovie();
        }


        /// <summary>
        /// Handles the click event for the play button.
        /// Resumes the current playback, or start a new playback from the
        /// beginning of our media.
        /// </summary>
        /// <param name="sender">The button that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnPlayButtonPressed(object sender, RoutedEventArgs args)
        {
            if (IsPaused && isPlaying)
            {
                // Just Keep doing what we were doing.
                PlayMovie();
                return;
            }

            // Stop recording
            isRecording = false;
            RecordButton.Background = Brushes.Transparent;
            MovieCanvas.IsHitTestVisible = false;

            ResetPlayback();

            isPlaying = true;
            PlayButton.Background = Brushes.DarkGray;

            RestartMovie();
        }

        /// <summary>
        /// Handles the click event for the pause button.
        /// Resumes the current recording or playback.  If at end of media,
        /// then start a new playback.
        /// </summary>
        /// <param name="sender">The button that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnPauseButtonPressed(object sender, RoutedEventArgs args)
        {
            if (IsPaused)
            {
                if (mediaEnded)
                {
                    // If at the end media, treat hitting pause like hitting play.
                    OnPlayButtonPressed(sender, args);
                }
                else
                {
                    PlayMovie();
                }
            }
            else
            {
                PauseMovie();
            }
        }

        #endregion

        /// <summary>
        /// Handles the MediaEnded event for the MovieCanvas video.
        /// </summary>
        /// <param name="sender">The MediaElement that raised the event.</param>
        /// <param name="args">The arguments for the event.</param>
        private void OnMediaEnded(object sender, RoutedEventArgs args)
        {
            mediaEnded = true;
            Reset();
        }

        /// <summary>
        /// Resets all buttons on MovieCanvas to initial state.
        /// </summary>
        private void Reset()
        {
            MovieCanvas.IsHitTestVisible = false;

            PauseMovie();
            PauseButton.Opacity = 1.0;

            isPlaying = false;
            PlayButton.Background = Brushes.Transparent;
            PlayButton.Opacity = 1.0;

            isRecording = false;
            RecordButton.Background = Brushes.Transparent;
            RecordButton.Opacity = 1.0;

            EnableButtons(true);
        }

        /// <summary>
        /// Handles the Tick event from the DispatcherTimer.
        /// On each tick determines what strokes need to be added to the canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTick(object sender, EventArgs e)
        {
            timer.Stop();
            try
            {
                if (IsPaused) { return; }

                // If there is no media, determine if the record/playback loop should end.
                if (stopwatch != null && stopwatch.Elapsed.Seconds >= duration)
                {
                    OnMediaEnded(null, null);
                    return;
                }

                if (recordedStrokesCopy == null || recordedStrokesCopy.Count < 1)
                {
                    // There are no strokes to play back.
                    return;
                }

                // Determine what strokes/points needed to be added to the MovieCanvas.
                if (isPlaying && !IsPaused)
                {
                    // Get the current time in the playback loop.
                    long  elapsed = CurrentPosition();

                    foreach (Stroke copy in newStrokes.Keys)
                    {
                        Stroke original = newStrokes[copy];

                        if (!recordedStrokesCopy.ContainsKey(original)) 
                        {
                            // The complete stroke has already been added to the canvas.
                            continue; 
                        }

                        List<long> dueTime = recordedStrokesCopy[original];

                        // See if the stroke is already added.
                        if (MovieCanvas.Strokes.Contains(copy))
                        {
                            // Stroke is already added. Add a new point to the stroke.
                            int added = copy.StylusPoints.Count;
                            if (added < original.StylusPoints.Count)
                            {
                                // There are more points to add.
                                if (dueTime.Count > added && elapsed >= dueTime[added])
                                {
                                    // It's time to add this point.
                                    StylusPoint point = original.StylusPoints[copy.StylusPoints.Count];
                                    copy.StylusPoints.Add(point);
                                    if (copy.StylusPoints.Count == original.StylusPoints.Count)
                                    {
                                        // We've added all the points for this stroke.
                                        recordedStrokesCopy.Remove(original);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Stroke has not been added yet. See if it's time to add it.
                            // We already added the first point when we created the strokes.
                            if  (dueTime.Count > 0 && elapsed >= dueTime[0])
                            {
                                MovieCanvas.Strokes.Add(copy);
                            }
                        }
                    }
                }
            }
            finally
            {
                timer.Start();
            }

        }

        /// <summary>
        /// Resets MoviePad state to begin stroke playback.
        /// </summary>
        private void ResetPlayback()
        {
            PauseMovie();
            ResetPosition();;
            MovieCanvas.Strokes.Clear();

            // Make a copy of the recorded strokes dictionary so its contents can be modified 
            // without changing the original dictionary
            recordedStrokesCopy = new Dictionary<Stroke, List<long>>(recordedStrokes);
               
            // Make a dictionary of all the strokes the user recorded mapped to copies of those strokes.
            //
            // These copies will:
            //   - Only have one point in their strokes collection
            //   - Have the same DrawingAttributes as their mapped stroke
            //   - Be added to the SurfaceInkCanvaas instead of the original stroke
            //   - Have additional points added as the movie plays
            newStrokes.Clear();

            // Copy the recorded strokes into a new distionary, but only copy the first point 
            // of each stroke so the stroke can grow as the movie plays.
            foreach (Stroke original in recordedStrokesCopy.Keys)
            {
                StylusPointCollection points = new StylusPointCollection();
                points.Add(original.StylusPoints[0]);
                Stroke copy = new Stroke(points);
                copy.DrawingAttributes = original.DrawingAttributes;
                newStrokes.Add(copy, original);
            }
        }
  

        /// <summary>
        /// Returns the current position in the playback loop.
        /// </summary>
        /// <returns></returns>
        private long CurrentPosition()
        {
            if (stopwatch != null)
            {
                return stopwatch.ElapsedMilliseconds;
            }
            else
            {
                return (long) Movie.Position.TotalMilliseconds;
            }
        }

        /// <summary>
        /// Resets the current position to the start of the playback loop.
        /// </summary>
        private void ResetPosition()
        {
            if (stopwatch != null)
            {
                stopwatch.Reset();
            }
            else
            {
                Movie.Position = TimeSpan.Zero;         
            }        
        }

        /// <summary>
        /// Starts the playback from the beginning.
        /// </summary>
        private void RestartMovie()
        {
            ResetPosition();
            PlayMovie();
            mediaEnded = false;
        }


        /// <summary>
        /// Resumes the playback from current position.
        /// </summary>
        /// <returns></returns>
        private void PlayMovie()
        {
            timer.Start();

            if (stopwatch != null)
            {
                stopwatch.Start();
            }
            else
            {
                Movie.Play();
            }

            IsPaused = false;
        }

        /// <summary>
        /// Pauses the media playback.
        /// </summary>
        private void PauseMovie()
        {
            if (IsPaused) return;
            timer.Stop();

            if (stopwatch != null)
            {
                stopwatch.Stop();
            }
            else
            {
                Movie.Pause();
            }

            IsPaused = true;
        }

        /// <summary>
        /// Stops the media playback and resets the MoviePad.
        /// </summary>
        private void StopMovie()
        {
            timer.Stop();
            if (stopwatch != null)
            {
                stopwatch.Stop();
            }
            else
            {
                Movie.Stop();
            }
            Reset();
        }

        /// <summary>
        /// Enables or Diables MoviePad buttons according to the isEnabled parameter.
        /// </summary>
        /// <param name="isEnabled">Boolean indicating if the buttons are enabled or not.</param>
        private void EnableButtons(bool isEnabled)
        {
            PlayButton.IsEnabled = isEnabled;
            RecordButton.IsEnabled = isEnabled;
            PauseButton.IsEnabled = isEnabled;
        }
        
        #endregion Movie Pad Specific Code

        /// <summary>
        /// Loads an image.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static Image LoadImageFromPath(string path)
        {
            ImageSourceConverter converter = new ImageSourceConverter();
            Image image = new Image();
            image.Source = (ImageSource)converter.ConvertFromString(path);
            return image;
        }

        /// <summary>
        /// Adds handlers for Application activation events.
        /// </summary>
        private void AddActivationHandlers()
        {
            // Subscribe to surface application activation events
            ApplicationLauncher.ApplicationActivated += OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed += OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated += OnApplicationDeactivated;
        }

        /// <summary>
        /// Removes handlers for Application activation events.
        /// </summary>
        private void RemoveActivationHandlers()
        {
            // Unsubscribe from surface application activation events
            ApplicationLauncher.ApplicationActivated -= OnApplicationActivated;
            ApplicationLauncher.ApplicationPreviewed -= OnApplicationPreviewed;
            ApplicationLauncher.ApplicationDeactivated -= OnApplicationDeactivated;
        }

        /// <summary>
        /// This is called when application has been activated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationActivated(object sender, EventArgs e)
        {
            // Enable audio for our movie.
            Movie.IsMuted = false;
        }

        /// <summary>
        /// This is called when application is in preview mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationPreviewed(object sender, EventArgs e)
        {
            // Disable audio for our movie.
            Movie.IsMuted = true;
        }

        /// <summary>
        ///  This is called when application has been deactivated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
            // If our movie is currently playing, stop it.
            StopMovie();
        }

    }

}
