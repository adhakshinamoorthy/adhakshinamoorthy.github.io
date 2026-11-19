// Builds the static site from data/*.json. No dependencies: needs only Node 20+.
//
//   node scripts/build-pages.mjs           write topic pages, index JSON, sitemap, home counts
//   node scripts/build-pages.mjs --check   fail if any generated file is out of date (use in CI)
//
// Every topic page ships with its full article in the HTML, so search engines, link
// previews, and readers without JavaScript see the real content.
import { readFile, writeFile, mkdir } from 'node:fs/promises';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { escapeHtml as e, footerMarkup, indexFields, sidebarMarkup } from '../assets/js/markup.js';
import { renderArticle, renderToc } from './lib/article-markup.mjs';

const BASE_URL = (process.env.BASE_URL || 'https://adhakshinamoorthy.github.io').replace(/\/$/, '');
const ASSET_VERSION = '20261119';
const AUTHOR = { name: 'Dhakshinamoorthy A', url: `${BASE_URL}/about.html` };
const AUTHOR_SAME_AS = ['https://www.linkedin.com/in/adhakshinamoorthy', 'https://github.com/adhakshinamoorthy'];

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const checkOnly = process.argv.includes('--check');
const technologies = JSON.parse(await readFile(join(root, 'data', 'technologies.json'), 'utf8'));
const samples = JSON.parse(await readFile(join(root, 'data', 'github-samples.json'), 'utf8'));
const buildYear = Math.max(...technologies.map(t => Number(t.updated.slice(0, 4))));

const outputs = new Map();

for (const topic of technologies) {
  outputs.set(join('technologies', `${topic.slug}.html`), topicPage(topic));
}

const index = technologies.map(topic => Object.fromEntries(indexFields.filter(f => f in topic).map(f => [f, topic[f]])));
outputs.set(join('data', 'technologies-index.json'), JSON.stringify(index) + '\n');
outputs.set('sitemap.xml', sitemap());

const about = await readFile(join(root, 'about.html'), 'utf8');
outputs.set('about.html', about.replace(/\?v=\d+/g, `?v=${ASSET_VERSION}`));

const home = await readFile(join(root, 'index.html'), 'utf8');
outputs.set('index.html', home
  .replace(/(<dt id="technology-guide-count">)\d+(<\/dt>)/, `$1${technologies.length}$2`)
  .replace(/(<dt id="sample-count">)\d+(<\/dt>)/, `$1${samples.length}$2`)
  .replace(/\?v=\d+/g, `?v=${ASSET_VERSION}`));

const stale = [];
for (const [relativePath, content] of outputs) {
  const target = join(root, relativePath);
  const current = await readFile(target, 'utf8').catch(() => null);
  if (current === content) continue;
  stale.push(relativePath);
  if (!checkOnly) {
    await mkdir(dirname(target), { recursive: true });
    await writeFile(target, content, 'utf8');
  }
}

if (checkOnly && stale.length) {
  console.error(`${stale.length} generated file(s) are out of date. Run: node scripts/build-pages.mjs`);
  stale.slice(0, 20).forEach(file => console.error(`- ${file}`));
  process.exit(1);
}
console.log(checkOnly
  ? `All ${outputs.size} generated files are up to date.`
  : `Built ${technologies.length} topic pages, technologies-index.json, sitemap.xml, index.html (${stale.length} changed).`);

function topicPage(topic) {
  const url = `${BASE_URL}/technologies/${topic.slug}.html`;
  const title = `${topic.name} — .NET Atlas`;
  const article = renderArticle(topic, technologies, samples);
  const jsonLd = {
    '@context': 'https://schema.org',
    '@type': 'TechArticle',
    headline: topic.name,
    description: topic.shortDescription,
    url,
    dateModified: topic.updated,
    proficiencyLevel: topic.level,
    timeRequired: `PT${topic.readingMinutes}M`,
    keywords: (topic.keywords || []).join(', '),
    author: { '@type': 'Person', '@id': `${AUTHOR.url}#person`, ...AUTHOR, sameAs: AUTHOR_SAME_AS },
    isPartOf: { '@type': 'WebSite', name: '.NET Atlas', url: `${BASE_URL}/` }
  };
  // `<` is escaped so content can never close the script tag early.
  const jsonLdText = JSON.stringify(jsonLd).replaceAll('<', '\\u003c');

  return `<!doctype html>
<html lang="en" style="--topic-color:${e(topic.color)}">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta name="description" content="${e(topic.shortDescription)}">
  <meta name="author" content="${e(AUTHOR.name)}">
  <meta name="theme-color" content="#f7f9fc" media="(prefers-color-scheme: light)">
  <meta name="theme-color" content="#111827" media="(prefers-color-scheme: dark)">
  <meta property="og:type" content="article">
  <meta property="og:title" content="${e(title)}">
  <meta property="og:description" content="${e(topic.shortDescription)}">
  <meta property="og:image" content="${BASE_URL}/assets/og/${topic.slug}.png">
  <meta property="og:image:type" content="image/png">
  <meta property="og:image:width" content="1200">
  <meta property="og:image:height" content="630">
  <meta property="og:image:alt" content="${e(`${topic.name}: ${topic.category} guide on .NET Atlas`)}">
  <meta property="og:site_name" content=".NET Atlas">
  <meta property="og:url" content="${url}">
  <meta property="article:modified_time" content="${e(topic.updated)}">
  <meta name="twitter:card" content="summary_large_image">
  <meta name="twitter:title" content="${e(title)}">
  <meta name="twitter:description" content="${e(topic.shortDescription)}">
  <meta name="twitter:image" content="${BASE_URL}/assets/og/${topic.slug}.png">
  <title>${e(title)}</title>
  <link rel="canonical" href="${url}">
  <link rel="icon" href="../assets/icons/favicon.svg" type="image/svg+xml">
  <link rel="stylesheet" href="../assets/css/styles.css?v=${ASSET_VERSION}">
  <script type="application/ld+json">${jsonLdText}</script>
  <script>
    (() => {
      const saved = localStorage.getItem('dotnet-atlas-theme');
      const theme = saved || (matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
      document.documentElement.dataset.theme = theme;
    })();
  </script>
  <noscript><style>.reveal{opacity:1;transform:none}</style></noscript>
</head>
<body data-page="topic" data-topic="${e(topic.slug)}" data-base="../">
  <a class="skip-link" href="#main-content">Skip to main content</a>
  <div id="site-navbar"></div>
  <div class="site-shell">
    <aside id="site-sidebar" class="sidebar" aria-label="Technology navigation" data-prerendered>${sidebarMarkup('../', technologies, topic.slug)}</aside>
    <main id="main-content" class="main-content" tabindex="-1">
      <div class="article-main">
        <article id="topic-article" class="article">${article}</article>
        <aside id="article-toc" class="toc" aria-label="Table of contents">${renderToc(article)}</aside>
      </div>
    </main>
  </div>
  <div id="site-footer">${footerMarkup('../', buildYear)}</div>
  <button id="back-to-top" class="back-to-top" type="button" aria-label="Back to top" title="Back to top">&uarr;</button>
  <script type="module" src="../assets/js/topic.js?v=${ASSET_VERSION}"></script>
</body>
</html>
`;
}

function sitemap() {
  const latest = technologies.map(t => t.updated).sort().at(-1);
  const entries = [
    `  <url><loc>${BASE_URL}/</loc><lastmod>${latest}</lastmod></url>`,
    `  <url><loc>${BASE_URL}/about.html</loc><lastmod>${latest}</lastmod></url>`,
    ...technologies.map(t => `  <url><loc>${BASE_URL}/technologies/${t.slug}.html</loc><lastmod>${t.updated}</lastmod></url>`)
  ];
  return `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
${entries.join('\n')}
</urlset>
`;
}
