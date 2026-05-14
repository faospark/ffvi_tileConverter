# FFVI Old Tile Tool

FFVI Old Tile Tool is an application for viewing, exporting, importing, and managing section graphics / tiles stored in Final Fantasy VI Old Ver map bin files. This is based of Maki's Original FFVI Tile Tool. This version attempts to lessen the friction in editing the tiles of the game and introduces a variety of quality of life improvements in the work flow. 

## Key Change from Previous Versions
This version can now properly reinsert modified section 1 (Previously called Chunk 1) 


## What The Tool Does

The app loads map bin files and displays two image sections:
- Section 1 image
- Section 2 image

You can then:
- export either section to PNG or BMP
- import an 8-bit indexed image into either section
- inspect palette locations and colors used by each section
- run folder-level workflows like mass export, backup, and file isolation

## Key Features

### Core Section Editing
- Section image preview for both sections in the same window
- Export image for Section 1 and Section 2
- Import image for Section 1 and Section 2
- Strict indexed color workflow for reliable palette-based editing

### Palette Info Dialog
- Clickable palette info buttons for Section 1 and Section 2
- Custom dialog shows:
  - full, scrollable list of palette offsets
  - 256-color visual palette preview grid
- Dialog supports dark theme integration

### Dark Mode
- App-wide dark mode toggle
- Themed forms, controls, menus, and dialog surfaces
- Dark title bar support where Windows allows it
- Scrollbar theme hooks applied for dark-friendly behavior on supported controls/OS rendering paths

### File Navigation And Convenience
- Recent directories menu with persisted history
- File list context actions:
  - Reveal in File Explorer
  - Gzip this file
  - Isolate selected file(s)
- Multi-select in file list for isolation workflows

### Filters
Built-in map category filters:
- Off
- Snow Tiles
- Grass Tiles
- Magitek Tiles

Also includes isolation action for filtered results.

### Safety And Workflow Tools
- First-run backup reminder
- Create Backup menu action
- Isolate selected files to controlled output folders
- Optional decompression flow when map bin.gz files are detected

### Mass Export
- Folder-driven mass export of map files
- Export format prompt (PNG or BMP)
- Caution prompt and progress dialog
- Cancellation support and summary reporting

## File Format And Data Layout

The editor works with map bin files that contain two section image data areas and section palettes.

High-level behavior in the app:
- Section 1 uses a fixed-size 512x512 indexed image region at the front
- Section 2 uses a variable-height 512-wide indexed image region near the end
- Palettes use BGRA-like storage with inverted alpha byte behavior

Palette write strategy:
- Section 1: update all matched Section 1 palette copies found in valid scan range
- Section 2: update at its dedicated section palette offset

## Import Rules

### Section 1 Import
- Accepted files: PNG or BMP
- Must be 8-bit indexed
- Must be exactly 512x512

### Section 2 Import
- Accepted files: PNG or BMP
- Must be 8-bit indexed
- Must be 512 pixels wide
- Height is variable according to section data usage

## Build And Run

## Requirements
- Windows
- .NET Framework target used by the project
- Visual Studio or compatible MSBuild environment

## Build
From repository root:

```powershell
dotnet build FFVI_tileTool.sln -v minimal
```

## Run
Use the generated executable:

```powershell
FFVI_tileTool\bin\Debug\FFVI_tileTool.exe
```

Note about dotnet run:
- this project is a Windows GUI WinExe target
- launching via dotnet run may not behave like a standard console project depending on environment
- use the built exe for the most reliable launch behavior

## Command-Line Behavior
- No arguments: launches GUI
- Invalid or unsupported path argument: falls back to GUI
- Legacy map\\convertedTiles import argument path: uses legacy import path handling

## Repository Structure

- FFVI_tileTool.sln
- FFVI_tileTool/
  - Form1.cs
  - Form1.Designer.cs
  - Program.cs
  - AboutForm.cs
  - PaletteInfoDialog.cs
  - project and resource files

## Additional Enhancements Added During Development

This repository includes iterative quality-of-life and reliability improvements beyond the original basic editor flow, including:
- terminology cleanup from chunk naming to section naming in app UI/workflow
- integrated palette info buttons directly in section I/O controls
- improved dark mode coverage for dialogs and title bars
- recent directory persistence and improved browse workflow
- robust file isolation workflow with destination choices
- backup recommendation flow and backup generation
- better handling around map bin.gz decompression paths
- safer launch fallback behavior for unsupported command-line usage

The project is focused on practical modding workflows:
- open a folder of map files quickly
- inspect section images side-by-side
- import edited indexed images back into the bin safely
- track and review palette placement
- batch export map images
- keep source data safe with backup and isolation tools

## Known Limitations

- Legacy CLI export path in Program.cs remains incomplete by design and is bypassed for unsupported arg flows
- Scrollbar theming depth depends on Windows version/control renderer support
- Editing assumes indexed-palette workflows; non-indexed source images are not accepted for section import

## License

MIT License. See LICENSE.
