import { readFile, access, readdir } from 'node:fs/promises';
import { join, dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const projectRoot = join(dirname(fileURLToPath(import.meta.url)), '..');
const technologies = JSON.parse(await readFile(join(projectRoot, 'data', 'technologies.json'), 'utf8'));
const samples = JSON.parse(await readFile(join(projectRoot, 'data', 'github-samples.json'), 'utf8'));
const errors = [];

const requiredTopicFields = [
  'slug', 'name', 'category', 'color', 'overview', 'detail', 'concepts', 'architecture',
  'code', 'bestPractices', 'interviewQuestions', 'sampleId'
];
const slugs = new Set();
const sampleIds = new Set(samples.map(sample => sample.id));

for (const topic of technologies) {
  for (const field of requiredTopicFields) {
    if (!topic[field] || (Array.isArray(topic[field]) && topic[field].length === 0)) {
      errors.push(`${topic.slug || 'unknown topic'} is missing ${field}`);
    }
  }
  if (slugs.has(topic.slug)) errors.push(`Duplicate slug: ${topic.slug}`);
  slugs.add(topic.slug);
  if (!sampleIds.has(topic.sampleId)) errors.push(`${topic.slug} references unknown sample ${topic.sampleId}`);
  if (topic.concepts?.length < 4) errors.push(`${topic.slug} needs at least four key concepts`);
  if (topic.bestPractices?.length < 4) errors.push(`${topic.slug} needs at least four best practices`);
  if (topic.interviewQuestions?.length < 3) errors.push(`${topic.slug} needs at least three interview questions`);
  if (topic.architecture?.nodes?.length < 3) errors.push(`${topic.slug} needs an architecture flow`);
  for (const resource of topic.resources || []) {
    if (!resource.label || !/^https:\/\//.test(resource.url || '')) errors.push(`${topic.slug} has an invalid official resource`);
  }
  try { await access(join(projectRoot, 'technologies', `${topic.slug}.html`)); }
  catch { errors.push(`Missing generated page for ${topic.slug}`); }
}

const expectedFiles = [
  'index.html', 'README.md', 'robots.txt', 'sitemap.xml', '.nojekyll',
  'assets/css/styles.css', 'assets/js/components.js', 'assets/js/home.js',
  'assets/js/topic.js', 'assets/icons/favicon.svg', 'assets/images/og-card.svg',
  'components/templates.html', 'data/technologies.json', 'data/github-samples.json'
];
for (const relativePath of expectedFiles) {
  try { await access(join(projectRoot, relativePath)); }
  catch { errors.push(`Missing required file: ${relativePath}`); }
}

const generated = (await readdir(join(projectRoot, 'technologies'))).filter(file => file.endsWith('.html'));
if (generated.length !== technologies.length) errors.push(`Expected ${technologies.length} topic pages, found ${generated.length}`);

const sitemap = await readFile(join(projectRoot, 'sitemap.xml'), 'utf8');
for (const topic of technologies) {
  if (!sitemap.includes(`/technologies/${topic.slug}.html`)) errors.push(`Sitemap is missing ${topic.slug}`);
}

const htmlFiles = [join(projectRoot, 'index.html'), ...generated.map(file => join(projectRoot, 'technologies', file))];
for (const htmlFile of htmlFiles) {
  const html = await readFile(htmlFile, 'utf8');
  for (const match of html.matchAll(/(?:href|src)="([^"]+)"/g)) {
    const reference = match[1];
    if (/^(?:https?:|mailto:|data:|#)/.test(reference)) continue;
    const cleanReference = reference.split('#')[0].split('?')[0];
    if (!cleanReference) continue;
    const target = resolve(dirname(htmlFile), cleanReference);
    try { await access(target); }
    catch { errors.push(`${htmlFile.slice(projectRoot.length + 1)} has broken reference: ${reference}`); }
  }
}

if (errors.length) {
  console.error(`Verification failed with ${errors.length} issue(s):`);
  errors.forEach(error => console.error(`- ${error}`));
  process.exit(1);
}

console.log(`Verified ${technologies.length} technology guides, ${samples.length} samples, ${generated.length} generated pages, required assets, sitemap entries, and internal page references.`);
