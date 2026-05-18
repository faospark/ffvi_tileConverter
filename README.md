# FFVI Old Tile Tool

FFVI Old Tile Tool is an application for viewing, exporting, importing, and managing section graphics / tiles stored in Final Fantasy VI Old Ver map bin files. This is based of Maki's Original FFVI Tile Tool. This version attempts to lessen the friction in editing the tiles of the game and introduces a variety of quality of life improvements in the work flow.

## Key Change from Previous Versions and Major Limitation

This build finally fixes the reinsertion of Section 1 (formerly Chunk 1) into BIN files cutting a pretty significant time in building and less prone to errors compared to manual hex editing. In previous versions, re‑injecting Chunk 1 would corrupt or garble the file.

The indexed editing workflow itself hasn’t changed, but there’s still a hard limitation: you can’t just shuffle the color palette in Photoshop and expect it to work. The executable relies on a strict index order for colors, and if that arrangement isn’t preserved, the output breaks — almost like a film negative.

The only real way around this is to implement a custom texture support system, similar to Special K, that bypasses the palette indexing entirely and removes this restriction.

## What's New

- Correct reinsertion for Section 1 and Section 2 images.
- Automatically detects mirrored palette repetitions per BIN file and updates them during import.
- General UI improvements, including a proper app icon, hidden console window on launch, and additional Settings and About tabs.
- Modern file management using Windows Open/Select Folder dialogs instead of the legacy Browse Folder dialog.
- Remembers your last working directory and keeps a history list.
- Added mass export and mass import.
- Added dark mode.
- Added .bmp support for export
- Added context menus for the file list and image previews.
- Added multi-file selection in the file list pane.
- Added Gzip support (including multi-file selection workflows).
- Added isolation tools to separate tiles you want to edit.
- Added known tile filters.
- Updated app terminology from "chunk" naming to "section" naming across the UI and workflow.
- Added a palette info button to inspect palette repetition and quickly check whether section palettes match.
- Improved image preview orientation for easier viewing while preserving upside-down saved image data.
- Added preview transparency support to make it easier to identify pixels using the dedicated transparent color `050505`.
- Added a first-run backup reminder.
- Added a Create Backup menu action.
- Added isolation of selected files into controlled output folders.
- Added an optional decompression flow when `map bin.gz` files are detected.
- Added safer launch fallback behavior for unsupported command-line usage.

### Core Section Editing

- Section image preview for both sections in the same window
- Export image for Section 1 and Section 2
- Import image for Section 1 and Section 2
- Strict indexed color workflow for reliable palette-based editing

### Recommended Tools For Tile Editing

- Aseprite
- MtPaint
- Photoshop

### Filters

Built-in map category filters:
- Off
- Snow Tiles
- Grass Tiles
- Magitek Tiles

Also includes isolation action for filtered results.


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

## Known Limitations

- Legacy CLI export path in Program.cs remains incomplete by design and is bypassed for unsupported arg flows
- Scrollbar theming depth depends on Windows version/control renderer support
- Editing assumes indexed-palette workflows; non-indexed source images are not accepted for section import

## License

MIT License. See LICENSE.
