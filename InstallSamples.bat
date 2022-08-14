@ECHO Off
pushd "%~dp0"

ECHO.
ECHO Building and installing Surface SDK samples.
PAUSE
ECHO.

::-------------------------------------------------------------------------------
:: Try to copy a file to a protected directory to see if the script is being run 
:: as admin. If not, elevate or warn the user and quit. 
:: - Make sure we dont leave the file on the hard drive!

copy CreateShellXML.vbs "%ProgramFiles%" > NUL
IF NOT EXIST "%ProgramFiles%\CreateShellXML.vbs" goto NO_RIGHTS
del "%ProgramFiles%\CreateShellXML.vbs"

::-------------------------------------------------------------------------------
:: See if the script is being run on a real table or a PC that will use the simulator

FOR /F "eol=H tokens=2*" %%A IN ('REG QUERY HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Surface\v1.0 /v IsLogicalSurfaceUnit') DO SET LogicalTableValue=%%B
SET InstallingOnTable=false
IF %LogicalTableValue%==0x1 SET InstallingOnTable=true 


::-------------------------------------------------------------------------------
:: Check to see if the samples are in a location that TableUser cannot access
:: Only perform this check on actual tables, not on PCs

IF NOT %InstallingOnTable%==true GOTO USER_HAS_PERMISSIONS

FOR /F %%A IN ('cacls .') DO (
  CheckUserPermissions.VBS %%A
  IF ERRORLEVEL 1 GOTO USER_HAS_PERMISSIONS
)

ECHO.
ECHO TABLE_USER does not have permission to access this directory. 
ECHO If you install the samples from this directory, you will not be
ECHO able to access them from user mode.
ECHO.
ECHO Continue?
CHOICE

IF ERRORLEVEL 2 GOTO :USER_HAS_NO_PERMISSIONS

:USER_HAS_PERMISSIONS
::-------------------------------------------------------------------------------
:: Check existence MSBuild.exe

ECHO Checking for MSBuild.
SET MsBuildPath="%windir%\Microsoft.NET\Framework\v3.5\MSBuild.exe"
SET LogFile=build.log.txt
SET MSBuildParameters=/p:Configuration=Release /noconsolelogger /fl /fileLoggerParameters:LogFile=%LogFile%;Append /nologo
IF NOT EXIST %MsBuildPath% GOTO CANNOT_FIND_MSBUILD

::-------------------------------------------------------------------------------
:: Build the projects

:: Track any errors when building
SET BuildErrors=""
IF EXIST %LogFile% del %LogFile%

:ActivateApplication

ECHO.
ECHO Building Activate Application...
%MsBuildPath% Shell\ActivateApplication\ActivateApplication.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto ActivateApplicationError
ECHO Generating Activate Application XML...
CreateShellXML.vbs Shell\ActivateApplication\InstalledAppInfo\ActivateApplication.xml "%CD%\Shell" ActivateApplication
ECHO Installing Activate Application to Surface Shell... 
ECHO.
goto AttractApplication

:ActivateApplicationError
SET BuildErrors=true
ECHO Error building Activate Application. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:AttractApplication

ECHO Building Attract Application...
%MsBuildPath% Shell\AttractApplication\AttractApplication.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto AttractApplicationError
ECHO Generating Attract Application XML...
CreateShellXML.vbs Shell\AttractApplication\InstalledAppInfo\AttractApplication.xml "%CD%\Shell" AttractApplication
ECHO Installing Attract Application to Surface Shell...
ECHO.
goto Cloth

:AttractApplicationError
SET BuildErrors=true
ECHO Error building Attract Application. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:Cloth

ECHO Building Cloth...
%MsBuildPath% Core\Cloth\Cloth.sln %MSBuildParameters%
IF ERRORLEVEL 1 goto ClothError
ECHO Generating Controls Box XML...
CreateShellXML.vbs Core\Cloth\InstalledAppInfo\Cloth.xml "%CD%\Core" Cloth
ECHO Installing Cloth to Surface Shell...
ECHO.
goto ControlsBox

:ClothError
SET BuildErrors=true
ECHO Error building Cloth. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:ControlsBox

ECHO Building Controls Box...
%MsBuildPath% WPF\ControlsBox\ControlsBox.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto ControlsBoxError
ECHO Generating Controls Box XML...
CreateShellXML.vbs WPF\ControlsBox\InstalledAppInfo\ControlsBox.xml "%CD%\WPF" ControlsBox
CreateShellXML.vbs WPF\ControlsBox\InstalledAppInfo\es-ES\ControlsBox.xml "%CD%\WPF" ControlsBox es-ES
ECHO Installing Controls Box to Surface Shell...
ECHO.
goto DataVisualizer

:ControlsBoxError
SET BuildErrors=true
ECHO Error building Controls Box. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:DataVisualizer

ECHO Building Data Visualizer...
%MsBuildPath% WPF\DataVisualizer\DataVisualizer.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto DataVisualizerError
ECHO Generating Data Visualizer XML...
CreateShellXML.vbs WPF\DataVisualizer\InstalledAppInfo\DataVisualizer.xml "%CD%\WPF" DataVisualizer
ECHO Installing Data Visualizer to Surface Shell...
ECHO.
goto FingerFountain

:DataVisualizerError
SET BuildErrors=true
ECHO Error building Data Visualizer. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:FingerFountain

ECHO Building Finger Fountain...
%MsBuildPath% Core\FingerFountain\FingerFountain.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto FingerFountainError
ECHO Generating Finger Fountain XML...
CreateShellXML.vbs Core\FingerFountain\InstalledAppInfo\FingerFountain.xml "%CD%\Core" FingerFountain
ECHO Installing Finger Fountain to Surface Shell...
ECHO.
goto FractalBrowser

:FingerFountainError
SET BuildErrors=true
ECHO Error building Finger Fountain. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:FractalBrowser

ECHO Building Fractal Browser...
%MsBuildPath% WPF\FractalBrowser\FractalBrowser.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto FractalBrowserError
ECHO Generating Fractal Browser XML...
CreateShellXML.vbs WPF\FractalBrowser\InstalledAppInfo\FractalBrowser.xml "%CD%\WPF" FractalBrowser
ECHO Installing Fractal Browser to Surface Shell...
ECHO.
goto GrandPiano

:FractalBrowserError
SET BuildErrors=true
ECHO Error building Fractal Browser. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:GrandPiano

ECHO Building Grand Piano...
%MsBuildPath% WPF\GrandPiano\GrandPiano.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto GrandPianoError
ECHO Generating Grand Piano XML...
CreateShellXML.vbs WPF\GrandPiano\InstalledAppInfo\GrandPiano.xml "%CD%\WPF" GrandPiano
ECHO Installing Grand Piano to Surface Shell...
ECHO.
goto ActivateApplication

:GrandPianoError
SET BuildErrors=true
ECHO Error building Grand Piano. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:ActivateApplication

ECHO Building Ignore Orientation...
%MsBuildPath% Shell\ActivateApplication\IgnoreOrientation\IgnoreOrientation.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto ActivateApplicationError
ECHO Generating Ignore Orientation XML...
CreateShellXML.vbs Shell\ActivateApplication\IgnoreOrientation\InstalledAppInfo\IgnoreOrientation.xml "%CD%\Shell" IgnoreOrientation
ECHO Installing Ignore Orientation to Surface Shell...
ECHO.
goto ItemCompare

:ActivateApplicationError
SET BuildErrors=true
ECHO Error building Activate Application. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:ItemCompare

ECHO Building ItemCompare...
%MsBuildPath% WPF\ItemCompare\ItemCompare.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto ItemCompareError
ECHO Inserting ItemCompare registry keys...
REG IMPORT WPF\ItemCompare\ItemCompare.reg
ECHO Generating ItemCompare XML...
CreateShellXML.vbs WPF\ItemCompare\InstalledAppInfo\ItemCompare.xml "%CD%\WPF" ItemCompare
ECHO Installing ItemCompare to Surface Shell...
ECHO.
goto Notifications

:ItemCompareError
SET BuildErrors=true
ECHO Error building ItemCompare. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:Notifications

ECHO Building Notifications...
%MsBuildPath% Shell\Notifications\Notifications.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto NotificationsError
ECHO Generating Notifications XML...
CreateShellXML.vbs Shell\Notifications\InstalledAppInfo\Notifications.xml "%CD%\Shell" Notifications
ECHO Installing Notifications to Surface Shell...
ECHO.
goto PaddleBall

:NotificationsError
SET BuildErrors=true
ECHO Error building Notifications. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:PaddleBall

ECHO Building Paddle Ball...
%MsBuildPath% WPF\PaddleBall\PaddleBall.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto PaddleBallError
ECHO Generating Paddle Ball XML...
CreateShellXML.vbs WPF\PaddleBall\InstalledAppInfo\PaddleBall.xml "%CD%\WPF" PaddleBall
ECHO Installing Paddle Ball to Surface Shell...
ECHO.
goto PhotoPaint

:PaddleBallError
SET BuildErrors=true
ECHO Error building Paddle Ball. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:PhotoPaint

ECHO Building Photo Paint...
%MsBuildPath% WPF\PhotoPaint\PhotoPaint.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto PhotoPaintError
ECHO Generating Photo Paint XML...
CreateShellXML.vbs WPF\PhotoPaint\InstalledAppInfo\PhotoPaint.xml "%CD%\WPF" PhotoPaint
ECHO Installing Photo Paint to Surface Shell...
ECHO.
goto RawImageVisualizer

:PhotoPaintError
SET BuildErrors=true
ECHO Error building Photo Paint. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:RawImageVisualizer

ECHO Building Raw Image Visualizer...
%MsBuildPath% Core\RawImageVisualizer\RawImageVisualizer.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto RawImageVisualizerError
ECHO Generating Raw Image Visualizer XML...
CreateShellXML.vbs Core\RawImageVisualizer\InstalledAppInfo\RawImageVisualizer.xml "%CD%\Core" RawImageVisualizer
ECHO Installing Raw Image Visualizer to Surface Shell...
ECHO.
goto ScatterPuzzle

:RawImageVisualizerError
SET BuildErrors=true
ECHO Error building Raw Image Visualizer. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:ScatterPuzzle

ECHO Building Scatter Puzzle...
%MsBuildPath% WPF\ScatterPuzzle\ScatterPuzzle.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto ScatterPuzzleError
ECHO Generating Scatter Puzzle XML...
CreateShellXML.vbs WPF\ScatterPuzzle\InstalledAppInfo\ScatterPuzzle.xml "%CD%\WPF" ScatterPuzzle
ECHO Installing Scatter Puzzle to Surface Shell...
ECHO.
goto ShoppingCart

:ScatterPuzzleError
SET BuildErrors=true
ECHO Error building Scatter Puzzle. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:ShoppingCart

ECHO Building Shopping Cart...
%MsBuildPath% WPF\ShoppingCart\ShoppingCart.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto ShoppingCartError
ECHO Generating Shopping Cart XML...
CreateShellXML.vbs WPF\ShoppingCart\InstalledAppInfo\ShoppingCart.xml "%CD%\WPF" ShoppingCart
ECHO Installing Shopping Cart to Surface Shell...
ECHO.
goto TagVisualizerEvents

:ShoppingCartError
SET BuildErrors=true
ECHO Error building Shopping Cart. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:TagVisualizerEvents

ECHO Building TagVisualizerEvents...
%MsBuildPath% WPF\TagVisualizerEvents\TagVisualizerEvents.csproj %MSBuildParameters%
IF ERRORLEVEL 1 goto TagVisualizerEventsError
ECHO Generating TagVisualizerEvents XML...
CreateShellXML.vbs WPF\TagVisualizerEvents\InstalledAppInfo\TagVisualizerEvents.xml "%CD%\WPF" TagVisualizerEvents
ECHO Installing TagVisualizerEvents to Surface Shell...
ECHO.
goto XNAScatter

:TagVisualizerEventsError
SET BuildErrors=true
ECHO Error building TagVisualizerEvents. Please build sample in Visual Studio and rerun this batch file.
ECHO.

:XNAScatter

ECHO Building XNA Scatter...
%MsBuildPath% Core\XNAScatter\XNAScatter.sln %MSBuildParameters%
IF ERRORLEVEL 1 goto XNAScatterError
ECHO Generating XNA Scatter XML...
CreateShellXML.vbs Core\XNAScatter\InstalledAppInfo\XNAScatter.xml "%CD%\Core" XNAScatter
ECHO Installing XNA Scatter to Surface Shell...
ECHO.
goto COMPLETE

:XNAScatterError
SET BuildErrors=true
ECHO Error building XNA Scatter. Please build sample in Visual Studio and rerun this batch file.
ECHO.

goto COMPLETE


:NO_RIGHTS
ECHO.
ECHO Please rerun this batch file as administrator. 
PAUSE
ECHO.  
GOTO EOF

:USER_HAS_NO_PERMISSIONS
ECHO.
ECHO Please extract samples to a public directory and rerun this script.
PAUSE
ECHO.  
GOTO EOF

:CANNOT_FIND_MSBUILD
ECHO.
ECHO Cannot find MSBuild.exe to build samples.
ECHO Please ensure that version 3.5 of the .Net Framework is installed.
PAUSE
ECHO.
GOTO EOF

:COMPLETE
ECHO.
IF %BuildErrors%==true ECHO Sample projects installed, but some samples failed to build. Please build samples in Visual Studio to fix build errors
IF NOT %BuildErrors%==true ECHO Successfully installed all SDK samples.
IF %InstallingOnTable%==true ECHO Please start the Surface Shell to browse sample applications from the Application Launcher.
IF NOT %InstallingOnTable%==true ECHO Please run the Surface Simulator to browse sample applications from the Surface Shell Application launcher.
PAUSE
ECHO.  

:EOF
popd
