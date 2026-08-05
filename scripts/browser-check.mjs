import { createRequire } from 'node:module';
import { mkdir } from 'node:fs/promises';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const moduleRoot = process.env.CODEX_NODE_MODULES;
if (!moduleRoot) throw new Error('Set CODEX_NODE_MODULES to a node_modules folder containing Playwright.');
const require = createRequire(join(moduleRoot, 'package.json'));
const { chromium } = require('playwright');
const baseUrl = process.env.PORTAL_URL || 'http://127.0.0.1:8080';
const executablePath = process.env.CHROME_PATH;
const projectRoot = join(dirname(fileURLToPath(import.meta.url)), '..');
const artifactDirectory = join(projectRoot, 'artifacts');
await mkdir(artifactDirectory, { recursive: true });

const browser = await chromium.launch({ headless: true, executablePath });
const issues = [];

function check(condition, message) {
  if (!condition) issues.push(message);
}

async function watchPage(page, label) {
  page.on('console', message => {
    if (message.type() === 'error') issues.push(`${label} console: ${message.text()}`);
  });
  page.on('pageerror', error => issues.push(`${label} page error: ${error.message}`));
}

try {
  const desktop = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    colorScheme: 'light',
    permissions: ['clipboard-read', 'clipboard-write']
  });
  const page = await desktop.newPage();
  await watchPage(page, 'desktop');
  await page.goto(`${baseUrl}/`, { waitUntil: 'networkidle' });

  check(await page.title() === '.NET Atlas — Learn the modern .NET ecosystem', 'Unexpected home page title');
  check(await page.locator('.tech-blob').count() === 17, 'Home page does not render 17 technology blobs');
  check(await page.locator('#featured-samples .github-card').count() === 3, 'Home page does not render three featured samples');
  check(await page.locator('#hero-title').isVisible(), 'Hero heading is not visible');
  check(await page.locator('.hero-actions .button-primary').isVisible(), 'Primary hero action is not visible');

  const desktopMetrics = await page.evaluate(() => ({
    width: innerWidth,
    scrollWidth: document.documentElement.scrollWidth,
    heroBottom: document.querySelector('.hero').getBoundingClientRect().bottom,
    navBottom: document.querySelector('.topbar').getBoundingClientRect().bottom
  }));
  check(desktopMetrics.scrollWidth <= desktopMetrics.width + 1, `Desktop horizontal overflow: ${desktopMetrics.scrollWidth}px`);
  check(desktopMetrics.navBottom > 50, 'Desktop top navigation is clipped');

  await page.locator('#global-search').fill('authentication');
  check(await page.locator('#search-popover .search-result').count() >= 1, 'Global search returns no authentication result');
  check((await page.locator('#search-popover').innerText()).includes('Authentication'), 'Global search returned the wrong content');
  await page.locator('#global-search').fill('definitely-not-a-topic');
  check(await page.locator('#search-popover .search-empty').isVisible(), 'Zero-result search state is missing');
  await page.locator('#global-search').fill('');

  const initialTheme = await page.locator('html').getAttribute('data-theme');
  await page.locator('#theme-toggle').click();
  const changedTheme = await page.locator('html').getAttribute('data-theme');
  check(initialTheme !== changedTheme, 'Theme toggle did not change the theme');
  await page.reload({ waitUntil: 'networkidle' });
  check(await page.locator('html').getAttribute('data-theme') === changedTheme, 'Theme choice was not persisted');
  await page.locator('#theme-toggle').click();
  await page.evaluate(() => localStorage.removeItem('dotnet-atlas-theme'));
  await page.reload({ waitUntil: 'networkidle' });

  await page.screenshot({ path: join(artifactDirectory, 'home-desktop-light.png'), fullPage: false });

  await page.goto(`${baseUrl}/technologies/aspnet-core.html`, { waitUntil: 'networkidle' });
  check((await page.locator('h1').innerText()) === 'ASP.NET Core', 'Direct topic link did not render ASP.NET Core');
  check(await page.locator('.article-section').count() === 7, 'Topic does not render all seven content sections');
  check(await page.locator('.concept-card').count() === 4, 'Topic key concepts are incomplete');
  check(await page.locator('.diagram-node').count() >= 3, 'Architecture diagram flow is incomplete');
  check(await page.locator('.callout').count() === 3, 'Topic callout panels are incomplete');
  check(await page.locator('.faq-item').count() >= 3, 'Topic interview FAQ is incomplete');
  check(await page.locator('.article-nav a').count() === 2, 'Previous/next navigation is incomplete');
  check(await page.locator('.code-block .tok-keyword').count() > 0, 'C# syntax highlighting is missing');

  await page.locator('.copy-button').click();
  check(/Copied|Select & copy/.test(await page.locator('.copy-button').innerText()), 'Copy button did not provide feedback');
  await page.locator('.faq-item').first().locator('summary').click();
  check(await page.locator('.faq-item').first().getAttribute('open') !== null, 'FAQ accordion did not open');
  await page.locator('.toc-link[href="#best-practices"]').click();
  await page.waitForTimeout(250);
  check((await page.locator('#best-practices').boundingBox()).y < 300, 'Table of contents did not navigate to best practices');
  await page.screenshot({ path: join(artifactDirectory, 'topic-desktop-light.png'), fullPage: false });
  await page.goto(`${baseUrl}/technologies/aspnet-core.html`, { waitUntil: 'networkidle' });
  await page.screenshot({ path: join(artifactDirectory, 'topic-desktop-light-top.png'), fullPage: false });

  await page.emulateMedia({ media: 'print' });
  check(await page.locator('.topbar').evaluate(element => getComputedStyle(element).display) === 'none', 'Print styles do not hide navigation');
  await page.emulateMedia({ media: 'screen', reducedMotion: 'reduce' });
  check(await page.evaluate(() => matchMedia('(prefers-reduced-motion: reduce)').matches), 'Reduced motion preference was not applied');
  await desktop.close();

  const mobile = await browser.newContext({
    viewport: { width: 390, height: 844 },
    isMobile: true,
    hasTouch: true,
    colorScheme: 'dark'
  });
  const mobilePage = await mobile.newPage();
  await watchPage(mobilePage, 'mobile');
  await mobilePage.goto(`${baseUrl}/`, { waitUntil: 'networkidle' });
  check(await mobilePage.locator('html').getAttribute('data-theme') === 'dark', 'System dark theme was not detected');
  const mobileMetrics = await mobilePage.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
  check(mobileMetrics.scrollWidth <= mobileMetrics.width + 1, `Mobile horizontal overflow: ${mobileMetrics.scrollWidth}px`);
  check(await mobilePage.locator('.menu-button').isVisible(), 'Mobile menu button is not visible');
  await mobilePage.locator('.menu-button').tap();
  check(await mobilePage.locator('#site-sidebar').evaluate(element => element.classList.contains('is-open')), 'Mobile sidebar did not open');
  check(await mobilePage.locator('.sidebar-overlay').isVisible(), 'Mobile sidebar overlay is missing');
  await Promise.all([
    mobilePage.waitForURL('**/technologies/dotnet.html'),
    mobilePage.locator('.sidebar-link').first().tap()
  ]);
  await mobilePage.waitForLoadState('networkidle');
  check(mobilePage.url().includes('/technologies/dotnet.html'), `Mobile sidebar route was unexpected: ${mobilePage.url()}`);
  check((await mobilePage.locator('h1').innerText()) === '.NET', 'Mobile topic page did not render');
  await mobilePage.goto(`${baseUrl}/`, { waitUntil: 'networkidle' });
  await mobilePage.screenshot({ path: join(artifactDirectory, 'home-mobile-dark.png'), fullPage: false });
  await mobile.close();
} finally {
  await browser.close();
}

if (issues.length) {
  console.error(`Browser verification failed with ${issues.length} issue(s):`);
  issues.forEach(issue => console.error(`- ${issue}`));
  process.exit(1);
}

console.log('Browser verification passed: desktop, mobile, light/dark, search, navigation, article components, print, reduced motion, and console checks.');
