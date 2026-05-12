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
8. App saves and reloads the preview.

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

In addition, import now enforces the game transparency key color:

- key color is `05 05 05` (hex `050505`)
- importer ensures this color exists in the output palette
- any source pixel that is transparent (palette alpha `< 128`) or already `050505` is remapped to the key-color palette index

So importing updates both:

- indexed pixel data
- palette data used by that chunk

## Safety Note

For chunk 2, there is currently no strict size guard for extremely large images beyond the width check. This means a very large height could overwrite unintended regions in the target binary.
