# Setup & Build

This guide covers building the dataSonification Excel add-in from source and
configuring it to run against the Java engine.

## Prerequisites

- **Visual Studio Community 18.x ("Visual Studio 2026")** or newer, with the
  **.NET desktop development** workload and **.NET Framework 4.8** developer
  pack. (The project originated on VS 2019 16.x; 2019 is now end-of-life. The
  projects target .NET Framework 4.8 — do **not** let the IDE retarget them to
  a newer framework.)
- Microsoft Excel — 32-bit or 64-bit.
- The **dataSonification Java engine** (separate repo), runnable and listening
  on TCP port 2011.

## 1. Restore & build

1. Open `DataSonificationAddin.sln` in Visual Studio. Accept any one-time
   solution-retarget prompt, but keep the **target framework on .NET Framework
   4.8**.
2. Let NuGet restore packages (this brings in **ExcelDna.AddIn 1.7.0** and
   **System.Data.SQLite.Core 1.0.118.0**, including the native SQLite binaries).
3. Select the **Release | Any CPU** configuration and build.

Command-line equivalent (adjust the MSBuild path to your VS install):

```text
MSBuild.exe DataSonificationAddin.sln /p:Configuration=Release /p:Platform="Any CPU"
```

Because the projects are AnyCPU, the same managed assemblies load into both
32-bit and 64-bit Excel. Excel-DNA emits **both** XLLs from one build:

- `DataSonificationAddin.xll`   — for 32-bit Excel
- `DataSonificationAddin64.xll` — for 64-bit Excel

## 2. Assemble the runtime folder

Create a runtime folder and gather:

- The built `DataSonificationAddin*.xll` and `DataSonificationAddin.dna`.
- `SonifyStrategies.xml` (analyzer/arranger strategy table).
- `DataSonificationMonitor.exe` (watchdog).
- `dataSonification.db` and the `Samples/` folder — download
  **`dataSonification-runtime-samples.zip`** from the
  [dataSonification-java *Releases*](https://github.com/maestrovt/dataSonification-java/releases)
  and extract it here.
- The dataSonification Java engine (`dataSonification.jar`). You can either
  place it in the runtime folder with a bundled JRE at `jre\bin\java.exe`, or
  run it yourself from the Java repo with `ant run` before opening Excel.

## 3. Configure paths

You do **not** edit these paths by hand. When the add-in connects to the
engine, it derives the runtime locations from the folder the XLL is loaded
from and sends them to the engine, which override the engine's own defaults:

- `DB_NAME` / `INST_DB_NAME` → `dataSonification.db` in the XLL's folder
- `SAMPLE_DIR` → the `Samples` folder in the XLL's folder

So the only requirement is **layout**: keep `dataSonification.db` and the
extracted `Samples/` folder in the **same runtime folder as the XLL** (step 2).
Get the layout right and the add-in points the engine at them automatically.

(If you run the engine standalone, without Excel, these come from the engine's
own `settings/core.txt` instead — edit `SAMPLE_DIR` / `DB_NAME` there.)

The General MIDI soundbank (`soundbank-deluxe.gm`) is **not** shipped; it is
only needed for the MIDI-instrument playback path. See LICENSING.md.

## 4. Register the add-in in Excel

Pick the XLL matching your Excel bitness:

- **File → Options → Add-ins → Manage: Excel Add-ins → Go… → Browse…**, then
  select `DataSonificationAddin.xll` (32-bit Excel) or
  `DataSonificationAddin64.xll` (64-bit Excel).

## 5. Run

1. Start the Java engine (it must be listening on port 2011 — e.g. `ant run`
   in the Java repo).
2. Open a workbook and enter `=Sonify(name, arg0, arg1, arg2)` in a cell whose
   value updates (e.g. driven by a live data-feed add-in).
3. When the cell value changes, the add-in connects to the engine and you
   should hear the configured sonification.

## Troubleshooting

- Initialization errors are written to
  `%USERPROFILE%\Desktop\sonify_init_error.log`.
- If no audio plays, confirm the Java engine is running and reachable on port
  2011, and that `SAMPLE_DIR` points at a `Samples/` folder whose instrument
  subfolders match the `dataSonification.db` configuration.

## Notes

- There are no automated tests; `DataSonificationTest` is a console app for
  manual checks.
- The legacy VSTO add-in path was retired — the XLL is the only delivery
  mechanism.
