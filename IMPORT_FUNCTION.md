# FFVI Tile Tool - Import Function

This document explains how the app imports image data into `map*.bin` files.

## Supported Input Formats

- `.png`
- `.bmp`

Both must be 8-bit indexed images (`PixelFormat.Format8bppIndexed`).

## Import Chunk 1

Trigger: "Import image" button for chunk 1.

Workflow:

1. User selects a `.png` or `.bmp` file.
2. App validates image format is 8bpp indexed.
3. App validates dimensions are exactly `512 x 512`.
4. App builds a 1024-byte palette block from the image palette.
5. App reads indexed pixel bytes (`512 * 512`).
6. App creates a combined buffer:
	- first 1024 bytes: palette
	- remaining bytes: chunk 1 image indices
7. App writes this buffer to the currently selected `map*.bin` file at offset `0`.
8. App also mirrors the same chunk 1 palette to additional palette bank offsets when present:
	- `0x40400` to `0x44000` in steps of `0x400`
9. App saves and reloads the preview.

## Import Chunk 2

Trigger: "Import image" button for chunk 2.

Workflow:

1. User selects a `.png` or `.bmp` file.
2. App validates image format is 8bpp indexed.
3. App validates width is exactly `512` (height can vary).
4. App builds a 1024-byte palette block.
5. App reads indexed pixel bytes (`512 * height`).
6. App creates a combined buffer:
	- first 1024 bytes: palette
	- remaining bytes: chunk 2 image indices
7. App writes this buffer near the end of the selected `map*.bin` at:
	- `fileLength - 0x80400`
8. App saves and reloads the preview.

## Palette Conversion Details

Palette entries are stored in this order per color:

- `B`
- `G`
- `R`
- inverted alpha (`255 - A`)

When importing an edited PNG, the app performs a **complete replacement**:

- **Pixel data**: The edited PNG's pixel indices are written directly to the bin (no remapping)
- **Palette data**: The edited PNG's palette replaces the bin's palette completely

The edited PNG becomes the source of truth. All pixel indices and palette entries from the PNG are copied as-is to the bin, preserving any edits to both color order and pixel structure.

So importing updates both:

- indexed pixel data (from the edited PNG)
- palette data used by that chunk

## Safety Note

For chunk 2, there is currently no strict size guard for extremely large images beyond the width check. This means a very large height could overwrite unintended regions in the target binary.
