#region Using directives
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Resources;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;


#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("CoreInteractionFramework")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]


// These attributes are for security settings on our shared library. Changes
// here may result in a need to go through an additional security review.
// ****************************************************************************
// To prevent the accidental introduction of any security critical code,
// we mark the entire assembly as security transparent. This prevents elevation
// of privileges on the call stack.
[assembly: SecurityTransparent]
// ****************************************************************************


//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

[assembly: NeutralResourcesLanguage("en",
           UltimateResourceFallbackLocation.MainAssembly)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]


#if ENABLE_TEST_HOOKS
// The following assemblies are used to test this assembly, and thus need access to
// internal properties, methods, and events.
[assembly: InternalsVisibleTo("Microsoft.Surface.Core.UnitTest, PublicKey=00240000048000009400000006020000002400005253413100040000010001008f8ba2293e9ad29c4a5b7c5c4ed1313c15a649f2898de19e9bb58233f5376c520c7619a5eb4d2f0be7f09679d262035d8b6831b2d3603505dca1aa5ce30a2ca22fb1457360137089e7bb344e8d9120264dbdcebf7891a1dd63ec34b2c98d707e3c5749ebbc3ef547a1f9b4459b69a6e835ded950ac9248cdab2128e1d76cc6cf")]
#endif
