# Destination Card Images

Guidelines for sourcing and preparing images for the destination picker cards.

## Specifications

| Property | Value | Notes |
|----------|-------|-------|
| **Dimensions** | 400 x 200 px | 2x the card size (200x100 visible) for retina/high-DPI |
| **Aspect ratio** | 2:1 | Landscape orientation |
| **Format** | WebP (preferred) or JPEG | WebP is ~30% smaller at same quality |
| **File size** | < 30 KB each | Keeps app bundle light |
| **Color profile** | sRGB | Standard web color space |

## Composition Tips

- **Focal point**: Center the main subject - cards crop to center
- **Contrast**: Ensure the subject stands out from the background
- **No text overlays**: The card adds its own text (name, tagline, distance)
- **Dark/space themes work well**: Cards have dark placeholder backgrounds

## File Naming

Use lowercase destination name, matching the `Name` property:

```
the-moon.webp
mercury.webp
the-sun.webp
mars.webp
jupiter.webp
saturn.webp
pluto.webp
proxima-centauri.webp
polaris-north-star.webp
betelgeuse.webp
horsehead-nebula.webp
crab-nebula.webp
pillars-of-creation.webp
milky-way-center.webp
andromeda-galaxy.webp
```

## Location

Place images in:
```
Kg.Velocity.UI/wwwroot/images/destinations/
```

## Sources

- NASA Image Gallery: https://images.nasa.gov/
- Hubble Gallery: https://hubblesite.org/images/gallery
- ESA/Hubble: https://esahubble.org/images/

Most NASA/ESA images are public domain or CC-licensed for use.

## Converting to WebP

**Using Squoosh (web-based):**
1. Go to https://squoosh.app/
2. Drop image, select WebP format
3. Adjust quality slider until < 30 KB
4. Download

**Using ImageMagick (CLI):**
```bash
magick input.jpg -resize 400x200^ -gravity center -extent 400x200 -quality 80 output.webp
```
