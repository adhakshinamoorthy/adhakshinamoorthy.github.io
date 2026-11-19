// Builds 1200x630 PNG link-preview cards (Open Graph) for every topic and the home page.
// LinkedIn, X, Slack, Teams and WhatsApp do not show SVG preview images, so these are PNG.
//
//   npm install                               once, installs the two dev dependencies
//   node scripts/build-og-images.mjs          write assets/og/*.png (only changed files)
//   node scripts/build-og-images.mjs --check  fail if any card is missing or out of date
//
// Fonts are bundled in scripts/fonts (Inter, SIL Open Font License) so output is the
// same on every machine.
import { readFile, writeFile, mkdir } from 'node:fs/promises';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Resvg } from '@resvg/resvg-js';
import opentype from 'opentype.js';
import UPNG from 'upng-js';
import { escapeHtml as x } from '../assets/js/markup.js';

const WIDTH = 1200;
const HEIGHT = 630;
const PAD = 80;
const TEXT_WIDTH = WIDTH - PAD * 2;
const SITE_HOST = 'adhakshinamoorthy.github.io';
const AUTHOR_LINE = 'Dhakshinamoorthy A  ·  Technical Architect';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const fontDir = join(root, 'scripts', 'fonts');
const fontFiles = ['Inter-Regular.ttf', 'Inter-SemiBold.ttf', 'Inter-Bold.ttf'].map(f => join(fontDir, f));
const [regular, semibold, bold] = await Promise.all(fontFiles.map(async f => {
  const b = await readFile(f);
  return opentype.parse(b.buffer.slice(b.byteOffset, b.byteOffset + b.byteLength));
}));
const checkOnly = process.argv.includes('--check');

const technologies = JSON.parse(await readFile(join(root, 'data', 'technologies.json'), 'utf8'));
const samples = JSON.parse(await readFile(join(root, 'data', 'github-samples.json'), 'utf8'));

const cards = technologies.map(t => ({
  file: `${t.slug}.png`,
  color: t.color,
  label: t.category,
  title: t.name,
  description: t.shortDescription,
  meta: `${t.level}  ·  ${t.readingMinutes} min read`
}));
cards.push({
  file: 'home.png',
  color: '#6e5bd7',
  label: 'Learning portal',
  title: '.NET Atlas',
  description: `${technologies.length} practical guides and ${samples.length} runnable samples for modern .NET, Azure, and solution architecture.`,
  meta: 'Concepts  ·  Architecture  ·  Code'
});

const outDir = join(root, 'assets', 'og');
await mkdir(outDir, { recursive: true });
const stale = [];
for (const card of cards) {
  const png = render(card);
  const target = join(outDir, card.file);
  const current = await readFile(target).catch(() => null);
  if (current && Buffer.compare(current, png) === 0) continue;
  stale.push(card.file);
  if (!checkOnly) await writeFile(target, png);
}

if (checkOnly && stale.length) {
  console.error(`${stale.length} preview image(s) are missing or out of date. Run: node scripts/build-og-images.mjs`);
  stale.slice(0, 20).forEach(f => console.error(`- assets/og/${f}`));
  process.exit(1);
}
console.log(checkOnly
  ? `All ${cards.length} preview images are up to date.`
  : `Built ${cards.length} preview images in assets/og (${stale.length} changed).`);

// ---------- layout ----------

function width(font, text, size) {
  return font.getAdvanceWidth(text, size, { kerning: true });
}

// Greedy word wrap. If text needs more than maxLines, the last line ends with "…".
function wrap(font, text, size, maxWidth, maxLines) {
  const words = text.split(/\s+/);
  const lines = [];
  let line = '';
  for (const word of words) {
    const next = line ? `${line} ${word}` : word;
    if (width(font, next, size) <= maxWidth || !line) line = next;
    else { lines.push(line); line = word; }
  }
  if (line) lines.push(line);
  if (lines.length <= maxLines) return lines;
  const kept = lines.slice(0, maxLines);
  let last = kept[maxLines - 1];
  while (width(font, `${last}…`, size) > maxWidth && last.includes(' ')) last = last.slice(0, last.lastIndexOf(' '));
  kept[maxLines - 1] = `${last.replace(/[,.;:]$/, '')}…`;
  return kept;
}

// Largest title size (84 down to 52) that fits in two lines.
function fitTitle(text) {
  for (let size = 84; size >= 52; size -= 4) {
    const lines = wrap(bold, text, size, TEXT_WIDTH, 2);
    if (!lines.at(-1).endsWith('…')) return { size, lines };
  }
  return { size: 52, lines: wrap(bold, text, 52, TEXT_WIDTH, 2) };
}

function render(card) {
  const color = /^#[0-9a-f]{6}$/i.test(card.color) ? card.color : '#6e5bd7';
  const label = card.label.toUpperCase();
  const labelSize = 20;
  const labelWidth = width(semibold, label, labelSize) + 2 * label.length * 1.2 + 40; // letter-spacing 1.2
  const title = fitTitle(card.title);
  const descSize = 30;
  const descLineHeight = descSize * 1.4;
  const labelY = 160;
  const titleTop = labelY + 64;
  const titleLineHeight = title.size * 1.05;
  const titleLines = title.lines.map((l, i) =>
    `<text x="${PAD}" y="${titleTop + title.size * 0.92 + i * titleLineHeight}" font-family="Inter" font-weight="700" font-size="${title.size}" letter-spacing="-1.5" fill="#ffffff">${x(l)}</text>`).join('');
  const descTop = titleTop + titleLineHeight * title.lines.length + 24;
  // Use as many description lines (max 3) as fit above the footer divider at y=530.
  let maxDescLines = 0;
  while (maxDescLines < 3 && descTop + descSize + maxDescLines * descLineHeight + 8 <= 512) maxDescLines++;
  const desc = wrap(regular, card.description, descSize, TEXT_WIDTH - 40, Math.max(1, maxDescLines));
  const descLines = desc.map((l, i) =>
    `<text x="${PAD}" y="${descTop + descSize + i * descLineHeight}" font-family="Inter" font-weight="400" font-size="${descSize}" fill="#cbd5e1">${x(l)}</text>`).join('');

  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${WIDTH}" height="${HEIGHT}" viewBox="0 0 ${WIDTH} ${HEIGHT}">
  <defs>
    <linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop stop-color="#0f172a"/><stop offset="1" stop-color="#1e1b4b"/></linearGradient>
    <radialGradient id="glow" cx="0.5" cy="0.5" r="0.5"><stop stop-color="${color}" stop-opacity=".55"/><stop offset="1" stop-color="${color}" stop-opacity="0"/></radialGradient>
  </defs>
  <rect width="${WIDTH}" height="${HEIGHT}" fill="url(#bg)"/>
  <circle cx="1080" cy="90" r="360" fill="url(#glow)"/>
  <rect width="14" height="${HEIGHT}" fill="${color}"/>
  <rect x="${PAD}" y="64" width="60" height="60" rx="16" fill="#6e5bd7"/>
  <text x="${PAD + 30}" y="104" text-anchor="middle" font-family="Inter" font-weight="700" font-size="26" fill="#ffffff">.N</text>
  <text x="${PAD + 78}" y="105" font-family="Inter" font-weight="600" font-size="30" fill="#ffffff">.NET <tspan fill="#a89bff">Atlas</tspan></text>
  <text x="${WIDTH - PAD}" y="103" text-anchor="end" font-family="Inter" font-weight="400" font-size="22" fill="#94a3b8">${SITE_HOST}</text>
  <rect x="${PAD}" y="${labelY}" width="${labelWidth}" height="40" rx="20" fill="${color}" fill-opacity=".22" stroke="${color}" stroke-width="2"/>
  <text x="${PAD + 20}" y="${labelY + 27}" font-family="Inter" font-weight="600" font-size="${labelSize}" letter-spacing="1.2" fill="#e2e8f0">${x(label)}</text>
  ${titleLines}
  ${descLines}
  <rect x="${PAD}" y="530" width="${TEXT_WIDTH}" height="1" fill="#334155"/>
  <text x="${PAD}" y="574" font-family="Inter" font-weight="600" font-size="24" fill="#e2e8f0">${x(AUTHOR_LINE)}</text>
  <text x="${WIDTH - PAD}" y="574" text-anchor="end" font-family="Inter" font-weight="400" font-size="22" fill="#94a3b8">${x(card.meta)}</text>
</svg>`;

  const image = new Resvg(svg, {
    fitTo: { mode: 'width', value: WIDTH },
    font: { fontFiles, loadSystemFonts: false, defaultFontFamily: 'Inter' }
  }).render();
  // Reduce to a 256-colour palette: about 8x smaller with no visible change on these cards.
  return Buffer.from(UPNG.encode([image.pixels.buffer.slice(image.pixels.byteOffset, image.pixels.byteOffset + image.pixels.byteLength)], image.width, image.height, 256));
}
