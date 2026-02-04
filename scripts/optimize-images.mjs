import sharp from 'sharp';
import fs from 'fs/promises';
import path from 'path';

const SOURCE_DIR = './Kg.Velocity.UI/wwwroot/images/destinations';
const BACKUP_DIR = './Kg.Velocity.UI/wwwroot/images/destinations-original';
const TARGET_WIDTH = 600;
const JPEG_QUALITY = 85;

async function optimizeImages() {
  console.log('Image Optimization Script');
  console.log('=========================\n');

  // Create backup directory
  try {
    await fs.mkdir(BACKUP_DIR, { recursive: true });
    console.log(`Created backup directory: ${BACKUP_DIR}\n`);
  } catch (err) {
    if (err.code !== 'EEXIST') throw err;
  }

  // Get all jpg files
  const files = (await fs.readdir(SOURCE_DIR))
    .filter(f => f.toLowerCase().endsWith('.jpg'));

  console.log(`Found ${files.length} images to optimize\n`);

  let totalOriginalSize = 0;
  let totalOptimizedSize = 0;

  for (const file of files) {
    const sourcePath = path.join(SOURCE_DIR, file);
    const backupPath = path.join(BACKUP_DIR, file);

    // Get original stats
    const originalStats = await fs.stat(sourcePath);
    const originalSize = originalStats.size;
    totalOriginalSize += originalSize;

    // Backup original
    await fs.copyFile(sourcePath, backupPath);

    // Get original dimensions
    const metadata = await sharp(sourcePath).metadata();

    // Optimize: resize if wider than target, always recompress
    let pipeline = sharp(sourcePath);

    if (metadata.width > TARGET_WIDTH) {
      pipeline = pipeline.resize(TARGET_WIDTH, null, {
        withoutEnlargement: true,
        fit: 'inside'
      });
    }

    const optimizedBuffer = await pipeline
      .jpeg({ quality: JPEG_QUALITY, mozjpeg: true })
      .toBuffer();

    // Write optimized image
    await fs.writeFile(sourcePath, optimizedBuffer);

    const optimizedSize = optimizedBuffer.length;
    totalOptimizedSize += optimizedSize;

    const reduction = ((1 - optimizedSize / originalSize) * 100).toFixed(1);
    const originalMB = (originalSize / 1024 / 1024).toFixed(2);
    const optimizedKB = (optimizedSize / 1024).toFixed(0);

    console.log(`${file}`);
    console.log(`  ${metadata.width}x${metadata.height} -> ${Math.min(metadata.width, TARGET_WIDTH)}px wide`);
    console.log(`  ${originalMB} MB -> ${optimizedKB} KB (${reduction}% smaller)\n`);
  }

  const totalOriginalMB = (totalOriginalSize / 1024 / 1024).toFixed(2);
  const totalOptimizedMB = (totalOptimizedSize / 1024 / 1024).toFixed(2);
  const totalReduction = ((1 - totalOptimizedSize / totalOriginalSize) * 100).toFixed(1);

  console.log('=========================');
  console.log('Summary');
  console.log('=========================');
  console.log(`Total original:  ${totalOriginalMB} MB`);
  console.log(`Total optimized: ${totalOptimizedMB} MB`);
  console.log(`Reduction:       ${totalReduction}%`);
  console.log(`\nOriginals backed up to: ${BACKUP_DIR}`);
}

optimizeImages().catch(console.error);
