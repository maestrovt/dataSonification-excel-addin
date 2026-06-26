# dataSonification — Excel Add-in

An Excel add-in that turns live spreadsheet data into sound. It exposes a
worksheet function:

```
=Sonify(name, arg0, arg1, arg2)
```

When a `Sonify` cell's data changes (for example, a live market quote pulled in
by a data-feed plugin), the add-in sends the update over a local TCP connection
to the **dataSonification Java engine**, which renders the audio. This lets you
monitor many data series by ear instead of watching the screen.

See <http://www.datasonification.com/> for background on data sonification.

## How it fits together

```text
Excel  ──=Sonify()──►  this add-in (XLL)  ──TCP :2011──►  dataSonification Java engine  ──►  audio
```

- **This repository** — the C# add-in, delivered as an [Excel-DNA](https://excel-dna.net/)
  XLL, plus the SQLite configuration database (`data/dataSonification.db`).
- **[dataSonification-java](https://github.com/maestrovt/dataSonification-java)**
  — the audio engine. Run it first (it listens on port 2011), then open your
  workbook.
- **Audio samples** — the runtime sample set is distributed as a GitHub Release
  asset on the **[dataSonification-java](https://github.com/maestrovt/dataSonification-java/releases)**
  repository: **`dataSonification-runtime-samples.zip`** (under *Releases*). It
  contains the `Samples/` folders and a runtime copy of `dataSonification.db`.

## Sonification schemes

The engine supports four schemes, from simplest to richest: movement (up/down),
movement-plus-reference, target proximity, and transaction-size trills. See the
Java engine's README for details.

## Requirements

- **Visual Studio Community 18.x ("2026")** (or newer), with **.NET Framework
  4.8** targeting.
- Microsoft Excel (32-bit or 64-bit — the add-in is AnyCPU and emits both an
  x86 and an x64 XLL).
- The dataSonification Java engine running on port 2011.

## Quick start

1. Build this solution in Visual Studio (Release | Any CPU). See
   [SETUP.md](SETUP.md) for full details.
2. Download `dataSonification-runtime-samples.zip` from the
   [dataSonification-java Releases](https://github.com/maestrovt/dataSonification-java/releases)
   and extract it into your runtime folder so that `Samples/` and
   `dataSonification.db` sit beside the XLL. The add-in locates them
   automatically — no path to edit.
3. Start the Java engine (`ant run` in the dataSonification-java repo).
4. Register the XLL matching your Excel bitness, open a workbook, and enter a
   `=Sonify(...)` formula on a cell whose value updates.

## Build & deploy

Full instructions — toolchain, NuGet restore, output layout, XLL registration,
and runtime configuration — are in **[SETUP.md](SETUP.md)**.

## Architecture

- **DataSonificationFunctions** — the XLL entry point; exposes `=Sonify(...)`
  via Excel-DNA `[ExcelFunction]` attributes.
- **DataSonificationLib** — TCP client (`SonifyClient`) to the Java engine, and
  the strategy table loaded from `SonifyStrategies.xml`.
- **DataSonificationConfiguration** — thin SQLite wrapper over
  `dataSonification.db` (tickers, instruments, analyzers, arrangers).
- **DataSonificationMonitor** — a small watchdog process started alongside the
  engine.

## License

Project source code: [MIT](LICENSE). Third-party NuGet packages and the audio
sample sets carry their own licenses — see [LICENSING.md](LICENSING.md). The
**ACB (Acoustic Branding)** sample set in the runtime release is included with
grateful acknowledgment to its creators.
