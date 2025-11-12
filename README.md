Shutdown Overlay

Shutdown Overlay is a lightweight Windows utility designed to help parents manage computer time responsibly.
It displays a friendly, semi-transparent countdown message reminding the user that the computer will shut down soon.
The overlay remains visible but unobtrusive, allowing the user to finish what they’re doing.
When the timer reaches zero, the system shuts down silently.

This app is ideal for encouraging healthy screen-time limits — for example, letting kids wrap up games or homework before bedtime without sudden shutdowns or constant reminders.

Features

30-minute countdown timer (customizable)

Always-visible overlay that moves to random screen positions every 10 seconds

Smooth color cycling through neon shades for visibility

Semi-transparent and click-through — doesn’t interrupt other activity

System-tray icon with "Cancel shutdown (Exit)" option

Silent, automatic shutdown when the timer reaches zero

Requirements

Windows 10 or Windows 11

No installation required (self-contained executable)

Installation

Download or clone this repository.

Open a command prompt in the project folder and run:

dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=true --self-contained=true

The ready-to-run program will be created at:

ShutdownOverlay\bin\Release\net8.0-windows\win-x64\publish\ShutdownOverlay.exe

Copy ShutdownOverlay.exe anywhere convenient (for example, C:\Tools\ShutdownOverlay).

Usage

Double-click ShutdownOverlay.exe to start the timer.

A semi-transparent box appears near the top of the screen displaying:

Machine will shutdown soon
30:00

The timer counts down every second, changing color and moving occasionally.

To cancel, right-click the tray icon and select "Cancel shutdown (Exit)".

When the timer reaches zero, the computer shuts down quietly.

Scheduling Automatic Shutdown at Night

To have the program run automatically at a specific time (for example, every night at 9:30 PM):

Step 1: Open Task Scheduler

Press Win + R, type taskschd.msc, and press Enter.

Step 2: Create a New Task

In the right pane, click "Create Task..."

General tab:

Name it something like "Shutdown Overlay Timer"

Check "Run whether user is logged on or not"

Optionally check "Run with highest privileges"

Triggers tab:

Click "New..."

Set "Begin the task" to "On a schedule"

Choose "Daily" and set the "Start time" (for example, 9:30 PM)

Click OK

Actions tab:

Click "New..."

Set "Action" to "Start a program"

Browse to your ShutdownOverlay.exe

Click OK

Conditions tab (optional):

Uncheck "Start the task only if the computer is on AC power" if you want it to run on laptops too.

Settings tab:

Check "Allow task to be run on demand" and "Run task as soon as possible after a scheduled start is missed."

Step 3: Save and Test

Click OK to save the task.

Enter your administrator password if prompted.

To test, right-click the new task and select "Run" — the overlay should appear and start counting down.

Customization

Setting: Countdown duration
Location: _remaining = TimeSpan.FromMinutes(30);
Example: Change 30 to desired minutes

Setting: Box width
Location: BOX_WIDTH = 800;
Example: Adjust overlay width

Setting: Opacity
Location: Opacity = 0.5;
Example: Use values between 0.1 and 1.0

Setting: Movement interval
Location: _moveTimer.Interval = 10_000;
Example: Set in milliseconds

Setting: Colors
Location: _cycle[] array
Example: Customize color sequence

Example Use

"I built this tool to help my son wind down at night — it lets him finish what he’s doing while gently reminding him that the computer will soon shut down automatically.
It keeps bedtime stress-free and consistent."

Notes

The app issues a standard Windows shutdown (shutdown /s /t 0), so unsaved work will be lost.

To change the shutdown behavior (for example, log off or hibernate), modify the command in TryShutdown().

For best reliability, keep the .exe somewhere permanent (not on the desktop) before scheduling.
