# Destination Card Images

Guidelines for sourcing and preparing images for the destination picker cards.

## Specifications

| Property | Value | Notes |
|----------|-------|-------|
| **Dimensions** | 400 x 200 px | 2x the card size (200x100 visible) for retina/high-DPI |
| **Aspect ratio** | 2:1 | Landscape orientation |
| **Format** | JPEG | Standard format, widely supported |
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
the-moon.jpg
mercury.jpg
the-sun.jpg
mars.jpg
jupiter.jpg
saturn.jpg
pluto.jpg
proxima-centauri.jpg
polaris-north-star.jpg
betelgeuse.jpg
horsehead-nebula.jpg
crab-nebula.jpg
pillars-of-creation.jpg
milky-way-center.jpg
andromeda-galaxy.jpg
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

## Resizing Images

**Using Squoosh (web-based):**
1. Go to https://squoosh.app/
2. Drop image, resize to 400x200
3. Adjust quality slider until < 30 KB
4. Download

**Using ImageMagick (CLI):**
```bash
magick input.jpg -resize 400x200^ -gravity center -extent 400x200 -quality 80 output.jpg
```
