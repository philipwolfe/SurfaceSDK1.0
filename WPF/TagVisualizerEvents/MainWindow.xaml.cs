using System;
using System.Windows;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;

namespace TagVisualizerEventsSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SurfaceWindow
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Set up the TagVisualizer so that it will match all tags.
            // Set Source to null so that tags will create "empty"
            // visualizations (no visible UI, but the position gets tracked
            // the same as if a visible UI were present).
            TagVisualizationDefinition definition = new MatchEverythingDefinition();
            definition.Source = new Uri("GlowVisualization.xaml", UriKind.Relative);
            definition.LostTagTimeout = 500;
            Visualizer.Definitions.Add(definition);

            // The value of TagVisualizerEvents.Mode is set to Auto
            // in MainWindow.xaml, so it's not necessary to do any special tracking
            // or updating when the ScatterViewItems move. If the mode were set to
            // Manual, however, it would be necessary to uncomment the following
            // two lines in order to ensure that updates are made properly when
            // the ScatterViewItems move.
            //Checkerboard.ScatterManipulationDelta += OnScatterDelta;
            //Square.ScatterManipulationDelta += OnScatterDelta;
        }
        
        /// <summary>
        /// Here when a ScatterViewItem moves.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScatterDelta(object sender, ScatterManipulationDeltaEventArgs e)
        {
            // The ScatterViewItem moved, so synchronize visualizations appropriately.
            // Note that doing so is unnecessary when TagVisualizerEvents.Mode is set
            // to Auto.
            TagVisualizerEvents.Synchronize();

            // It's not necessary to check "is auto-update active?" and only call
            // Synchronize() if it isn't, because the Synchronize() method is smart
            // enough not to do redundant checking when auto-synchronize is on.
        }
        
        /// <summary>
        /// Here when a visualization enters another object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVisualizationEnter(object sender, TagVisualizationEnterLeaveEventArgs e)
        {
            GlowVisualization glow = (GlowVisualization)e.Visualization;
            glow.Enter();
        }

        /// <summary>
        /// Here when a visualization leaves another object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVisualizationLeave(object sender, TagVisualizationEnterLeaveEventArgs e)
        {
            GlowVisualization glow = (GlowVisualization)e.Visualization;
            glow.Leave();
        }

        /// <summary>
        /// A TagVisualizationDefinition that matches all tags.
        /// </summary>
        private class MatchEverythingDefinition : TagVisualizationDefinition
        {
            protected override bool Matches(TagData tag)
            {
                return true;
            }

            protected override Freezable CreateInstanceCore()
            {
                return new MatchEverythingDefinition();
            }
        }
    }
}