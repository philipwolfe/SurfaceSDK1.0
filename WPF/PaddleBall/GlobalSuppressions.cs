// Supress FxCop messages for violations that occur in generated code
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Scope = "member", Target = "PaddleBall.Window1.System.Windows.Markup.IComponentConnector.Connect(System.Int32,System.Object):System.Void", Justification = "Casting done internally by XAML")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Scope = "member", Target = "PaddleBall.PlayingArea.System.Windows.Markup.IComponentConnector.Connect(System.Int32,System.Object):System.Void", Justification = "Casting done internally by XAML")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Scope = "member", Target = "Paddleball.PlayingArea.#System.Windows.Markup.IComponentConnector.Connect(System.Int32,System.Object)", Justification = "Casting done internally by XAML")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Scope = "member", Target = "Paddleball.Window1.#System.Windows.Markup.IComponentConnector.Connect(System.Int32,System.Object)", Justification = "Casting done internally by XAML")]

// Code called from only XAML (such as event handlers) is viewed as uncalled by FxCop, even though it is actually called.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "PaddleBall.Window1.OnGameOver(System.Object,System.EventArgs):System.Void", Justification="Referenced by XAML")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "PaddleBall.Window1.winningScoreSlider", Justification = "Referenced by XAML")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Paddleball.Window1.#OnGameOver(System.Object,System.EventArgs)", Justification = "Referenced by XAML")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "Paddleball.Window1.#winningScoreSlider", Justification = "Referenced by XAML")]

