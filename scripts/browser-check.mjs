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
  check(
    await page.locator('.tech-blob').count() === await page.locator('.sidebar-link').count(),
    'Home page technology blobs do not match the sidebar topics'
  );
  check(
    Number(await page.locator('#technology-guide-count').innerText()) === await page.locator('.sidebar-link').count(),
    'Home page technology guide total does not match the sidebar topics'
  );
  check(await page.locator('#featured-samples .github-card').count() === 3, 'Home page does not render three featured samples');
  check(await page.locator('#hero-title').isVisible(), 'Hero heading is not visible');
  check(await page.locator('.hero-actions .button-primary').isVisible(), 'Primary hero action is not visible');
  check(await page.locator('.sidebar-disclosure').count() > 0, 'Sidebar category disclosures are missing');
  check(await page.locator('.sidebar-disclosure[open]').count() === 0, 'Sidebar categories are not collapsed by default');
  await page.locator('.sidebar-label').first().click();
  check(await page.locator('.sidebar-disclosure').first().getAttribute('open') !== null, 'Sidebar category did not expand');
  check(await page.locator('.sidebar-list').first().isVisible(), 'Expanded sidebar topics are not visible');
  await page.locator('.sidebar-label').first().click();
  check(await page.locator('.sidebar-disclosure').first().getAttribute('open') === null, 'Sidebar category did not collapse');

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

  await page.goto(`${baseUrl}/technologies/dotnet.html`, { waitUntil: 'networkidle' });
  check((await page.locator('h1').innerText()) === '.NET', 'Direct topic link did not render .NET');
  check(await page.locator('.topic-status-complete').count() === 1, '.NET is not marked as a complete guide');
  check(await page.locator('.article-section').count() === 16, '.NET topic does not render all sixteen content sections');
  check(await page.locator('.toc-link').count() === 16, '.NET table of contents is incomplete');
  check(await page.locator('.concept-card').count() === 8, '.NET key concepts are incomplete');
  check((await page.locator('#decision-guide h2').innerText()) === 'When to use .NET', '.NET decision guide title is not topic-specific');
  check(await page.locator('.step-list li').count() === 7, '.NET implementation walkthrough is incomplete');
  check(await page.locator('.troubleshooting-card').count() === 5, '.NET troubleshooting guidance is incomplete');
  check(await page.locator('.checklist li').count() === 7, '.NET production checklist is incomplete');
  check(await page.locator('.faq-item').count() === 6, '.NET interview FAQ is incomplete');
  check(await page.locator('#github-sample .sample-commands code').count() === 2, '.NET sample run and test commands are missing');
  check((await page.locator('#github-sample .github-link').getAttribute('href')).includes('/samples/dotnet-platform-baseline'), '.NET sample links to the wrong GitHub path');

  await page.goto(`${baseUrl}/technologies/csharp.html`, { waitUntil: 'networkidle' });
  check((await page.locator('h1').innerText()) === 'C#', 'Direct topic link did not render C#');
  check(await page.locator('.topic-status-complete').count() === 1, 'C# is not marked as a complete guide');
  check(await page.locator('.article-section').count() === 16, 'C# topic does not render all sixteen content sections');
  check(await page.locator('.toc-link').count() === 16, 'C# table of contents is incomplete');
  check(await page.locator('.concept-card').count() === 8, 'C# key concepts are incomplete');
  check((await page.locator('#decision-guide h2').innerText()) === 'When to use C#', 'C# decision guide title is not topic-specific');
  check(await page.locator('.step-list li').count() === 7, 'C# implementation walkthrough is incomplete');
  check(await page.locator('.troubleshooting-card').count() === 5, 'C# troubleshooting guidance is incomplete');
  check(await page.locator('.checklist li').count() === 7, 'C# production checklist is incomplete');
  check(await page.locator('.faq-item').count() === 7, 'C# interview FAQ is incomplete');
  check(await page.locator('#github-sample .sample-commands code').count() === 2, 'C# sample run and test commands are missing');
  check((await page.locator('#github-sample .github-link').getAttribute('href')).includes('/samples/csharp-language-workbench'), 'C# sample links to the wrong GitHub path');
  await page.screenshot({ path: join(artifactDirectory, 'csharp-desktop-light-top.png'), fullPage: false });

  await page.goto(`${baseUrl}/technologies/dependency-injection.html`, { waitUntil: 'networkidle' });
  check((await page.locator('h1').innerText()) === 'Dependency Injection', 'Direct topic link did not render Dependency Injection');
  check(await page.locator('.topic-status-complete').count() === 1, 'Dependency Injection is not marked as a complete guide');
  check(await page.locator('.article-section').count() === 16, 'Dependency Injection topic does not render all sixteen content sections');
  check(await page.locator('.toc-link').count() === 16, 'Dependency Injection table of contents is incomplete');
  check(await page.locator('.concept-card').count() === 10, 'Dependency Injection key concepts are incomplete');
  check((await page.locator('#decision-guide h2').innerText()) === 'When to use Dependency Injection', 'Dependency Injection decision guide title is not topic-specific');
  check(await page.locator('.step-list li').count() === 7, 'Dependency Injection implementation walkthrough is incomplete');
  check(await page.locator('.troubleshooting-card').count() === 6, 'Dependency Injection troubleshooting guidance is incomplete');
  check(await page.locator('.checklist li').count() === 8, 'Dependency Injection production checklist is incomplete');
  check(await page.locator('.faq-item').count() === 7, 'Dependency Injection interview FAQ is incomplete');
  check(await page.locator('#github-sample .sample-commands code').count() === 2, 'Dependency Injection sample run and test commands are missing');
  check((await page.locator('#github-sample .github-link').getAttribute('href')).includes('/samples/dependency-injection-lifetimes'), 'Dependency Injection sample links to the wrong GitHub path');
  await page.screenshot({ path: join(artifactDirectory, 'dependency-injection-desktop-light-top.png'), fullPage: false });

  await page.goto(`${baseUrl}/technologies/source-generators.html`, { waitUntil: 'networkidle' });
  check((await page.locator('h1').innerText()) === 'Source Generators', 'Direct topic link did not render Source Generators');
  check(await page.locator('.topic-status-complete').count() === 1, 'Source Generators is not marked as a complete guide');
  check(await page.locator('.article-section').count() === 16, 'Source Generators topic does not render all sixteen content sections');
  check(await page.locator('.toc-link').count() === 16, 'Source Generators table of contents is incomplete');
  check(await page.locator('.concept-card').count() === 10, 'Source Generators key concepts are incomplete');
  check((await page.locator('#decision-guide h2').innerText()) === 'When to use Source Generators', 'Source Generators decision guide title is not topic-specific');
  check(await page.locator('.step-list li').count() === 7, 'Source Generators implementation walkthrough is incomplete');
  check(await page.locator('.troubleshooting-card').count() === 6, 'Source Generators troubleshooting guidance is incomplete');
  check(await page.locator('.checklist li').count() === 8, 'Source Generators production checklist is incomplete');
  check(await page.locator('.faq-item').count() === 7, 'Source Generators interview FAQ is incomplete');
  check(await page.locator('#github-sample .sample-commands code').count() === 2, 'Source Generators sample run and test commands are missing');
  check((await page.locator('#github-sample .github-link').getAttribute('href')).includes('/samples/source-generators-telemetry'), 'Source Generators sample links to the wrong GitHub path');
  await page.screenshot({ path: join(artifactDirectory, 'source-generators-desktop-light-top.png'), fullPage: false });

  await page.goto(`${baseUrl}/technologies/aspnet-core.html`, { waitUntil: 'networkidle' });
  check((await page.locator('h1').innerText()) === 'ASP.NET Core', 'Direct topic link did not render ASP.NET Core');
  check(await page.locator('.sidebar-disclosure[open]').count() === 1, 'Topic page does not expand exactly one sidebar category');
  check(
    await page.locator('.sidebar-disclosure[open] .sidebar-link[aria-current="page"]').count() === 1,
    'Expanded sidebar category does not contain the active topic'
  );
  check(await page.locator('.topic-status-complete').count() === 1, 'ASP.NET Core is not marked as a complete guide');
  check(await page.locator('.article-section').count() === 16, 'Gold-standard topic does not render all sixteen content sections');
  check(await page.locator('.toc-link').count() === 16, 'Gold-standard table of contents is incomplete');
  check(await page.locator('.concept-card').count() === 6, 'Gold-standard topic key concepts are incomplete');
  check(await page.locator('.decision-panel').count() === 2, 'When-to-use decision guidance is incomplete');
  check(await page.locator('.step-list li').count() === 7, 'Implementation walkthrough is incomplete');
  check(await page.locator('.troubleshooting-card').count() === 4, 'Troubleshooting guidance is incomplete');
  check(await page.locator('.checklist li').count() === 6, 'Production checklist is incomplete');
  check(await page.locator('.diagram-node').count() >= 3, 'Architecture diagram flow is incomplete');
  check(await page.locator('.callout').count() === 3, 'Topic callout panels are incomplete');
  check(await page.locator('.faq-item').count() >= 5, 'Topic interview FAQ is incomplete');
  check((await page.locator('#github-sample h2').innerText()) === 'Dedicated GitHub sample', 'Topic does not identify its dedicated sample');
  check(await page.locator('#github-sample .sample-commands code').count() === 2, 'Dedicated sample run and test commands are missing');
  check((await page.locator('#github-sample .github-link').getAttribute('href')).includes('/samples/aspnet-core-api'), 'Dedicated sample links to the wrong GitHub path');
  check(await page.locator('.article-nav a').count() === 2, 'Previous/next navigation is incomplete');
  check(await page.locator('.code-block .tok-keyword').count() > 0, 'C# syntax highlighting is missing');

  await page.goto(`${baseUrl}/technologies/blazor.html`, { waitUntil: 'networkidle' });
  check((await page.locator('h1').innerText()) === 'Blazor', 'Direct topic link did not render Blazor');
  check(await page.locator('.topic-status-complete').count() === 1, 'Blazor is not marked as a complete guide');
  check(await page.locator('.article-section').count() === 16, 'Blazor topic does not render all sixteen content sections');
  check(await page.locator('.toc-link').count() === 16, 'Blazor table of contents is incomplete');
  check(await page.locator('.concept-card').count() === 10, 'Blazor key concepts are incomplete');
  check((await page.locator('#decision-guide h2').innerText()) === 'When to use Blazor', 'Blazor decision guide title is not topic-specific');
  check(await page.locator('.step-list li').count() === 7, 'Blazor implementation walkthrough is incomplete');
  check(await page.locator('.troubleshooting-card').count() === 6, 'Blazor troubleshooting guidance is incomplete');
  check(await page.locator('.checklist li').count() === 8, 'Blazor production checklist is incomplete');
  check(await page.locator('.faq-item').count() === 7, 'Blazor interview FAQ is incomplete');
  check(await page.locator('#github-sample .sample-commands code').count() === 2, 'Blazor sample run and test commands are missing');
  check((await page.locator('#github-sample .github-link').getAttribute('href')).includes('/samples/blazor-interactive-catalog'), 'Blazor sample links to the wrong GitHub path');
  await page.screenshot({ path: join(artifactDirectory, 'blazor-desktop-light-top.png'), fullPage: false });

  await page.goto(`${baseUrl}/technologies/minimal-apis.html`, { waitUntil: 'networkidle' });
  check((await page.locator('h1').innerText()) === 'Minimal APIs', 'Direct topic link did not render Minimal APIs');
  check(await page.locator('.topic-status-complete').count() === 1, 'Minimal APIs is not marked as a complete guide');
  check(await page.locator('.article-section').count() === 16, 'Minimal APIs topic does not render all sixteen content sections');
  check(await page.locator('.toc-link').count() === 16, 'Minimal APIs table of contents is incomplete');
  check(await page.locator('.concept-card').count() === 10, 'Minimal APIs key concepts are incomplete');
  check((await page.locator('#decision-guide h2').innerText()) === 'When to use Minimal APIs', 'Minimal APIs decision guide title is not topic-specific');
  check(await page.locator('.step-list li').count() === 7, 'Minimal APIs implementation walkthrough is incomplete');
  check(await page.locator('.troubleshooting-card').count() === 6, 'Minimal APIs troubleshooting guidance is incomplete');
  check(await page.locator('.checklist li').count() === 8, 'Minimal APIs production checklist is incomplete');
  check(await page.locator('.faq-item').count() === 7, 'Minimal APIs interview FAQ is incomplete');
  check(await page.locator('#github-sample .sample-commands code').count() === 2, 'Minimal APIs sample run and test commands are missing');
  check((await page.locator('#github-sample .github-link').getAttribute('href')).includes('/samples/minimal-apis-orders'), 'Minimal APIs sample links to the wrong GitHub path');
  await page.screenshot({ path: join(artifactDirectory, 'minimal-apis-desktop-light-top.png'), fullPage: false });

  const securityBatch = [
    ['authentication-authorization', 'Authentication & Authorization', 8, 6, 8, 7, '/samples/authentication-authorization-documents'],
    ['api-security-owasp', 'API Security & OWASP', 8, 6, 8, 7, '/samples/api-security-owasp-orders'],
    ['secrets-management', 'Secrets Management', 8, 6, 8, 7, '/samples/secrets-rotation']
  ];
  for (const [slug, heading, concepts, troubleshooting, checklist, faq, samplePath] of securityBatch) {
    await page.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await page.locator('h1').innerText()) === heading, `${heading} rendered the wrong heading`);
    check(await page.locator('.topic-status-complete').count() === 1, `${heading} is not marked complete`);
    check(await page.locator('.article-section').count() === 16, `${heading} does not render all content sections`);
    check(await page.locator('.toc-link').count() === 16, `${heading} table of contents is incomplete`);
    check(await page.locator('.concept-card').count() === concepts, `${heading} key concepts are incomplete`);
    check(await page.locator('.step-list li').count() === 7, `${heading} walkthrough is incomplete`);
    check(await page.locator('.troubleshooting-card').count() === troubleshooting, `${heading} troubleshooting is incomplete`);
    check(await page.locator('.checklist li').count() === checklist, `${heading} checklist is incomplete`);
    check(await page.locator('.faq-item').count() === faq, `${heading} interview FAQ is incomplete`);
    check((await page.locator('#github-sample .github-link').getAttribute('href')).includes(samplePath), `${heading} sample link is wrong`);
  }
  await page.screenshot({ path: join(artifactDirectory, 'security-batch-desktop-light-top.png'), fullPage: false });

  const httpContractsBatch = [
    ['api-design-best-practices', 'API Design Best Practices', 8, 6, 8, 6, '/samples/api-design-orders'],
    ['openapi-scalar', 'OpenAPI & Scalar', 8, 6, 8, 6, '/samples/openapi-scalar-catalog'],
    ['webhook-patterns', 'Webhook Patterns', 8, 6, 8, 6, '/samples/webhook-durable-inbox']
  ];
  for (const [slug, heading, concepts, troubleshooting, checklist, faq, samplePath] of httpContractsBatch) {
    await page.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await page.locator('h1').innerText()) === heading, `${heading} rendered the wrong heading`);
    check(await page.locator('.topic-status-complete').count() === 1, `${heading} is not marked complete`);
    check(await page.locator('.article-section').count() === 16, `${heading} does not render all content sections`);
    check(await page.locator('.toc-link').count() === 16, `${heading} table of contents is incomplete`);
    check(await page.locator('.concept-card').count() === concepts, `${heading} key concepts are incomplete`);
    check(await page.locator('.step-list li').count() === 7, `${heading} walkthrough is incomplete`);
    check(await page.locator('.troubleshooting-card').count() === troubleshooting, `${heading} troubleshooting is incomplete`);
    check(await page.locator('.checklist li').count() === checklist, `${heading} checklist is incomplete`);
    check(await page.locator('.faq-item').count() === faq, `${heading} interview FAQ is incomplete`);
    check(await page.locator('#github-sample .sample-commands code').count() === 2, `${heading} sample commands are missing`);
    check((await page.locator('#github-sample .github-link').getAttribute('href')).includes(samplePath), `${heading} sample link is wrong`);
    const topicMetrics = await page.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(topicMetrics.scrollWidth <= topicMetrics.width + 1, `${heading} has desktop horizontal overflow`);
  }
  await page.screenshot({ path: join(artifactDirectory, 'http-contracts-batch-desktop-light-top.png'), fullPage: false });

  const realtimeRpcBatch = [
    ['grpc', 'gRPC', 8, 6, 8, 7, '/samples/grpc-inventory'],
    ['signalr', 'SignalR', 8, 6, 8, 7, '/samples/signalr-operations-room'],
    ['graphql', 'GraphQL in .NET', 8, 6, 8, 7, '/samples/graphql-inventory-catalog']
  ];
  for (const [slug, heading, concepts, troubleshooting, checklist, faq, samplePath] of realtimeRpcBatch) {
    await page.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await page.locator('h1').innerText()) === heading, `${heading} rendered the wrong heading`);
    check(await page.locator('.topic-status-complete').count() === 1, `${heading} is not marked complete`);
    check(await page.locator('.article-section').count() === 16, `${heading} does not render all content sections`);
    check(await page.locator('.toc-link').count() === 16, `${heading} table of contents is incomplete`);
    check(await page.locator('.concept-card').count() === concepts, `${heading} key concepts are incomplete`);
    check(await page.locator('.step-list li').count() === 7, `${heading} walkthrough is incomplete`);
    check(await page.locator('.troubleshooting-card').count() === troubleshooting, `${heading} troubleshooting is incomplete`);
    check(await page.locator('.checklist li').count() === checklist, `${heading} checklist is incomplete`);
    check(await page.locator('.faq-item').count() === faq, `${heading} interview FAQ is incomplete`);
    check(await page.locator('#github-sample .sample-commands code').count() === 2, `${heading} sample commands are missing`);
    check((await page.locator('#github-sample .github-link').getAttribute('href')).includes(samplePath), `${heading} sample link is wrong`);
    const metrics = await page.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(metrics.scrollWidth <= metrics.width + 1, `${heading} has desktop horizontal overflow`);
  }
  await page.screenshot({ path: join(artifactDirectory, 'realtime-rpc-batch-desktop-light-top.png'), fullPage: false });

  const dataCachingBatch = [
    ['redis-distributed-caching', 'Redis & Distributed Caching', 8, 6, 8, 7, '/samples/redis-hybrid-products'],
    ['multi-tenancy-patterns', 'Multi-Tenancy Patterns', 8, 6, 8, 7, '/samples/multi-tenant-invoices'],
    ['performance', 'Performance', 8, 6, 8, 8, '/samples/performance-telemetry-parser']
  ];
  for (const [slug, heading, concepts, troubleshooting, checklist, faq, samplePath] of dataCachingBatch) {
    await page.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await page.locator('h1').innerText()) === heading, `${heading} rendered the wrong heading`);
    check(await page.locator('.topic-status-complete').count() === 1, `${heading} is not marked complete`);
    check(await page.locator('.article-section').count() === 16, `${heading} does not render all content sections`);
    check(await page.locator('.toc-link').count() === 16, `${heading} table of contents is incomplete`);
    check(await page.locator('.concept-card').count() === concepts, `${heading} key concepts are incomplete`);
    check(await page.locator('.step-list li').count() === 7, `${heading} walkthrough is incomplete`);
    check(await page.locator('.troubleshooting-card').count() === troubleshooting, `${heading} troubleshooting is incomplete`);
    check(await page.locator('.checklist li').count() === checklist, `${heading} checklist is incomplete`);
    check(await page.locator('.faq-item').count() === faq, `${heading} interview FAQ is incomplete`);
    check(await page.locator('#github-sample .sample-commands code').count() === 2, `${heading} sample commands are missing`);
    check((await page.locator('#github-sample .github-link').getAttribute('href')).includes(samplePath), `${heading} sample link is wrong`);
    const metrics = await page.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(metrics.scrollWidth <= metrics.width + 1, `${heading} has desktop horizontal overflow`);
  }
  await page.screenshot({ path: join(artifactDirectory, 'data-caching-batch-desktop-light-top.png'), fullPage: false });

  const applicationStructureBatch = [
    ['clean-architecture', 'Clean Architecture', 8, 6, 8, 7, '/samples/clean-architecture-orders'],
    ['vertical-slice-architecture', 'Vertical Slice Architecture', 8, 6, 8, 7, '/samples/vertical-slice-support'],
    ['modular-monolith', 'Modular Monolith', 8, 6, 8, 7, '/samples/modular-monolith-storefront']
  ];
  for (const [slug, heading, concepts, troubleshooting, checklist, faq, samplePath] of applicationStructureBatch) {
    await page.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await page.locator('h1').innerText()) === heading, `${heading} rendered the wrong heading`);
    check(await page.locator('.topic-status-complete').count() === 1, `${heading} is not marked complete`);
    check(await page.locator('.article-section').count() === 16, `${heading} does not render all content sections`);
    check(await page.locator('.toc-link').count() === 16, `${heading} table of contents is incomplete`);
    check(await page.locator('.concept-card').count() === concepts, `${heading} key concepts are incomplete`);
    check(await page.locator('.step-list li').count() === 7, `${heading} walkthrough is incomplete`);
    check(await page.locator('.troubleshooting-card').count() === troubleshooting, `${heading} troubleshooting is incomplete`);
    check(await page.locator('.checklist li').count() === checklist, `${heading} checklist is incomplete`);
    check(await page.locator('.faq-item').count() === faq, `${heading} interview FAQ is incomplete`);
    check(await page.locator('#github-sample .sample-commands code').count() === 2, `${heading} sample commands are missing`);
    check((await page.locator('#github-sample .github-link').getAttribute('href')).includes(samplePath), `${heading} sample link is wrong`);
    const metrics = await page.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(metrics.scrollWidth <= metrics.width + 1, `${heading} has desktop horizontal overflow`);
  }
  await page.screenshot({ path: join(artifactDirectory, 'application-structure-batch-desktop-light-top.png'), fullPage: false });

  const domainDesignBatch = [
    ['domain-driven-design', 'Domain-Driven Design', 8, 6, 8, 7, '/samples/ddd-subscriptions'],
    ['cqrs-mediatr', 'CQRS & MediatR', 8, 6, 8, 7, '/samples/cqrs-orders-mediator'],
    ['design-patterns', 'Design Patterns', 8, 6, 8, 7, '/samples/design-patterns-pricing']
  ];
  for (const [slug, heading, concepts, troubleshooting, checklist, faq, samplePath] of domainDesignBatch) {
    await page.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await page.locator('h1').innerText()) === heading, `${heading} rendered the wrong heading`);
    check(await page.locator('.topic-status-complete').count() === 1, `${heading} is not marked complete`);
    check(await page.locator('.article-section').count() === 16, `${heading} does not render all content sections`);
    check(await page.locator('.toc-link').count() === 16, `${heading} table of contents is incomplete`);
    check(await page.locator('.concept-card').count() === concepts, `${heading} key concepts are incomplete`);
    check(await page.locator('.step-list li').count() === 7, `${heading} walkthrough is incomplete`);
    check(await page.locator('.troubleshooting-card').count() === troubleshooting, `${heading} troubleshooting is incomplete`);
    check(await page.locator('.checklist li').count() === checklist, `${heading} checklist is incomplete`);
    check(await page.locator('.faq-item').count() === faq, `${heading} interview FAQ is incomplete`);
    check(await page.locator('#github-sample .sample-commands code').count() === 2, `${heading} sample commands are missing`);
    check((await page.locator('#github-sample .github-link').getAttribute('href')).includes(samplePath), `${heading} sample link is wrong`);
    const metrics = await page.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(metrics.scrollWidth <= metrics.width + 1, `${heading} has desktop horizontal overflow`);
  }
  await page.screenshot({ path: join(artifactDirectory, 'domain-design-batch-desktop-light-top.png'), fullPage: false });

  const messagingFoundationsBatch = [
    ['event-driven-messaging', 'Event-Driven Messaging', 8, 6, 8, 7, '/samples/event-driven-orders'],
    ['rabbitmq', 'RabbitMQ', 8, 6, 8, 7, '/samples/rabbitmq-routing-lab'],
    ['apache-kafka', 'Apache Kafka', 8, 6, 8, 7, '/samples/kafka-partition-lab']
  ];
  for (const [slug, heading, concepts, troubleshooting, checklist, faq, samplePath] of messagingFoundationsBatch) {
    await page.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await page.locator('h1').innerText()) === heading, `${heading} rendered the wrong heading`);
    check(await page.locator('.topic-status-complete').count() === 1, `${heading} is not marked complete`);
    check(await page.locator('.article-section').count() === 16, `${heading} does not render all content sections`);
    check(await page.locator('.toc-link').count() === 16, `${heading} table of contents is incomplete`);
    check(await page.locator('.concept-card').count() === concepts, `${heading} key concepts are incomplete`);
    check(await page.locator('.step-list li').count() === 7, `${heading} walkthrough is incomplete`);
    check(await page.locator('.troubleshooting-card').count() === troubleshooting, `${heading} troubleshooting is incomplete`);
    check(await page.locator('.checklist li').count() === checklist, `${heading} checklist is incomplete`);
    check(await page.locator('.faq-item').count() === faq, `${heading} interview FAQ is incomplete`);
    check(await page.locator('#github-sample .sample-commands code').count() === 2, `${heading} sample commands are missing`);
    check((await page.locator('#github-sample .github-link').getAttribute('href')).includes(samplePath), `${heading} sample link is wrong`);
    const metrics = await page.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(metrics.scrollWidth <= metrics.width + 1, `${heading} has desktop horizontal overflow`);
  }
  await page.screenshot({ path: join(artifactDirectory, 'messaging-foundations-batch-desktop-light-top.png'), fullPage: false });

  const distributedWorkflowsBatch = [
    ['saga-pattern', 'Saga Pattern', 8, 6, 8, 7, '/samples/saga-order-workflow'],
    ['event-sourcing', 'Event Sourcing', 8, 6, 8, 7, '/samples/event-sourced-account'],
    ['background-services', 'Background & Hosted Services', 8, 6, 8, 7, '/samples/hosted-work-queue']
  ];
  for (const [slug, heading, concepts, troubleshooting, checklist, faq, samplePath] of distributedWorkflowsBatch) {
    await page.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await page.locator('h1').innerText()) === heading, `${heading} rendered the wrong heading`);
    check(await page.locator('.topic-status-complete').count() === 1, `${heading} is not marked complete`);
    check(await page.locator('.article-section').count() === 16, `${heading} does not render all content sections`);
    check(await page.locator('.toc-link').count() === 16, `${heading} table of contents is incomplete`);
    check(await page.locator('.concept-card').count() === concepts, `${heading} key concepts are incomplete`);
    check(await page.locator('.step-list li').count() === 7, `${heading} walkthrough is incomplete`);
    check(await page.locator('.troubleshooting-card').count() === troubleshooting, `${heading} troubleshooting is incomplete`);
    check(await page.locator('.checklist li').count() === checklist, `${heading} checklist is incomplete`);
    check(await page.locator('.faq-item').count() === faq, `${heading} interview FAQ is incomplete`);
    check(await page.locator('#github-sample .sample-commands code').count() === 2, `${heading} sample commands are missing`);
    check((await page.locator('#github-sample .github-link').getAttribute('href')).includes(samplePath), `${heading} sample link is wrong`);
    const metrics = await page.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(metrics.scrollWidth <= metrics.width + 1, `${heading} has desktop horizontal overflow`);
  }
  await page.screenshot({ path: join(artifactDirectory, 'distributed-workflows-batch-desktop-light-top.png'), fullPage: false });

  const reliabilityBatch = [
    ['resilience-rate-limiting', 'Resilience & Rate Limiting', 8, 6, 8, 7, '/samples/resilience-admission-lab'],
    ['health-checks', 'Health Checks', 8, 6, 8, 7, '/samples/health-readiness-lab'],
    ['observability', 'Observability & OpenTelemetry', 8, 6, 8, 7, '/samples/otel-correlation-lab']
  ];
  for (const [slug, heading, concepts, troubleshooting, checklist, faq, samplePath] of reliabilityBatch) {
    await page.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await page.locator('h1').innerText()) === heading, `${heading} rendered the wrong heading`);
    check(await page.locator('.topic-status-complete').count() === 1, `${heading} is not marked complete`);
    check(await page.locator('.article-section').count() === 16, `${heading} does not render all content sections`);
    check(await page.locator('.toc-link').count() === 16, `${heading} table of contents is incomplete`);
    check(await page.locator('.concept-card').count() === concepts, `${heading} key concepts are incomplete`);
    check(await page.locator('.step-list li').count() === 7, `${heading} walkthrough is incomplete`);
    check(await page.locator('.troubleshooting-card').count() === troubleshooting, `${heading} troubleshooting is incomplete`);
    check(await page.locator('.checklist li').count() === checklist, `${heading} checklist is incomplete`);
    check(await page.locator('.faq-item').count() === faq, `${heading} interview FAQ is incomplete`);
    check(await page.locator('#github-sample .sample-commands code').count() === 2, `${heading} sample commands are missing`);
    check((await page.locator('#github-sample .github-link').getAttribute('href')).includes(samplePath), `${heading} sample link is wrong`);
    const metrics = await page.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(metrics.scrollWidth <= metrics.width + 1, `${heading} has desktop horizontal overflow`);
  }
  await page.screenshot({ path: join(artifactDirectory, 'reliability-batch-desktop-light-top.png'), fullPage: false });

  await page.goto(`${baseUrl}/technologies/entity-framework-core.html`, { waitUntil: 'networkidle' });
  check((await page.locator('h1').innerText()) === 'Entity Framework Core', 'Direct topic link did not render Entity Framework Core');
  check(await page.locator('.topic-status-complete').count() === 1, 'Entity Framework Core is not marked as a complete guide');
  check(await page.locator('.article-section').count() === 16, 'EF Core topic does not render all sixteen content sections');
  check(await page.locator('.toc-link').count() === 16, 'EF Core table of contents is incomplete');
  check(await page.locator('.concept-card').count() === 8, 'EF Core key concepts are incomplete');
  check(await page.locator('.step-list li').count() === 7, 'EF Core implementation walkthrough is incomplete');
  check(await page.locator('.troubleshooting-card').count() === 5, 'EF Core troubleshooting guidance is incomplete');
  check(await page.locator('.checklist li').count() === 7, 'EF Core production checklist is incomplete');
  check(await page.locator('.faq-item').count() === 6, 'EF Core interview FAQ is incomplete');
  check(await page.locator('#github-sample .sample-commands code').count() === 2, 'EF Core sample run and test commands are missing');
  check((await page.locator('#github-sample .github-link').getAttribute('href')).includes('/samples/ef-core-order-management'), 'EF Core sample links to the wrong GitHub path');

  await page.goto(`${baseUrl}/technologies/dapper.html`, { waitUntil: 'networkidle' });
  check((await page.locator('h1').innerText()) === 'Dapper', 'Direct topic link did not render Dapper');
  check(await page.locator('.topic-status-complete').count() === 1, 'Dapper is not marked as a complete guide');
  check(await page.locator('.article-section').count() === 16, 'Dapper topic does not render all sixteen content sections');
  check(await page.locator('.toc-link').count() === 16, 'Dapper table of contents is incomplete');
  check(await page.locator('.concept-card').count() === 8, 'Dapper key concepts are incomplete');
  check((await page.locator('#decision-guide h2').innerText()) === 'When to use Dapper', 'Dapper decision guide title is not topic-specific');
  check(await page.locator('.step-list li').count() === 7, 'Dapper implementation walkthrough is incomplete');
  check(await page.locator('.troubleshooting-card').count() === 5, 'Dapper troubleshooting guidance is incomplete');
  check(await page.locator('.checklist li').count() === 7, 'Dapper production checklist is incomplete');
  check(await page.locator('.faq-item').count() === 6, 'Dapper interview FAQ is incomplete');
  check(await page.locator('#github-sample .sample-commands code').count() === 2, 'Dapper sample run and test commands are missing');
  check((await page.locator('#github-sample .github-link').getAttribute('href')).includes('/samples/dapper-order-reporting'), 'Dapper sample links to the wrong GitHub path');

  const sidebarOrder = await page.locator('.sidebar-link').evaluateAll(links =>
    links.map(link => new URL(link.href).pathname.split('/').at(-1))
  );
  for (let index = 0; index < sidebarOrder.length; index += 1) {
    const current = sidebarOrder[index];
    const expectedPrevious = sidebarOrder[(index - 1 + sidebarOrder.length) % sidebarOrder.length];
    const expectedNext = sidebarOrder[(index + 1) % sidebarOrder.length];
    await page.goto(`${baseUrl}/technologies/${current}`, { waitUntil: 'networkidle' });
    const adjacentLinks = await page.locator('.article-nav a').evaluateAll(links =>
      links.map(link => new URL(link.href).pathname.split('/').at(-1))
    );
    check(
      adjacentLinks[0] === expectedPrevious && adjacentLinks[1] === expectedNext,
      `${current} navigation does not match sidebar order`
    );
  }

  const wafCategory = page.locator('.sidebar-group[aria-label="Azure Well-Architected"] .sidebar-link');
  check(await wafCategory.count() === 6, 'Azure Well-Architected category does not contain six guides');
  check(
    JSON.stringify((await wafCategory.allTextContents()).map(text => text.trim())) === JSON.stringify([
      'Azure Well-Architected Framework',
      'WAF: Reliability',
      'WAF: Security',
      'WAF: Cost Optimization',
      'WAF: Operational Excellence',
      'WAF: Performance Efficiency'
    ]),
    'Azure Well-Architected guides are not in the expected learning order'
  );

  const architecturePractice = page.locator('.sidebar-group[aria-label="Architecture Practice"] .sidebar-link');
  check(await architecturePractice.count() === 7, 'Architecture Practice category does not contain seven guides');
  check(
    JSON.stringify((await architecturePractice.allTextContents()).map(text => text.trim())) === JSON.stringify([
      'Solution Architecture Fundamentals',
      'Architecture Decision Records',
      'API Design Best Practices',
      'Multi-Tenancy Patterns',
      'Saga Pattern',
      'Legacy Modernization',
      'Backend for Frontend'
    ]),
    'Architecture Practice guides are not in the expected learning order'
  );

  const cloudPlatforms = page.locator('.sidebar-group[aria-label="Cloud Platforms"] .sidebar-link');
  check(await cloudPlatforms.count() === 9, 'Cloud Platforms category does not contain nine guides');
  check(
    JSON.stringify((await cloudPlatforms.allTextContents()).map(text => text.trim())) === JSON.stringify([
      'Azure Resource Manager',
      'Azure API Management',
      'Azure Functions & Serverless',
      'Azure Logic Apps',
      'Azure Event Hubs',
      'Azure Data Factory & ETL',
      'Cloud Adoption Framework',
      'AWS for .NET',
      'Azure Container Apps'
    ]),
    'Cloud Platforms guides are not in the expected learning order'
  );

  const expectedBatchTopics = [
    'Event Sourcing',
    'Azure Container Apps',
    'Redis & Distributed Caching',
    'Architecture Testing',
    'Testcontainers for .NET',
    'Webhook Patterns',
    'Infrastructure as Code (Bicep)',
    'Health Checks',
    'YARP — Reverse Proxy',
    'GraphQL in .NET',
    'Azure Resource Manager',
    'Apache Kafka',
    'Azure Logic Apps',
    'Azure Event Hubs',
    'RabbitMQ'
  ];
  const sidebarLabels = (await page.locator('.sidebar-link').allTextContents()).map(text => text.trim());
  check(
    expectedBatchTopics.every(topic => sidebarLabels.includes(topic)),
    'One or more supplied batch topics are missing from the sidebar'
  );

  const reviewedBatch = [
    ['azure-resource-manager', 'Azure Resource Manager', 2],
    ['apache-kafka', 'Apache Kafka', 2],
    ['azure-logic-apps', 'Azure Logic Apps', 2],
    ['azure-event-hubs', 'Azure Event Hubs', 3],
    ['rabbitmq', 'RabbitMQ', 3]
  ];
  for (const [topicSlug, expectedHeading, expectedResources] of reviewedBatch) {
    await page.goto(`${baseUrl}/technologies/${topicSlug}.html`, { waitUntil: 'networkidle' });
    check((await page.locator('h1').innerText()) === expectedHeading, `${topicSlug} rendered the wrong heading`);
    check(
      await page.locator('.official-resource-links a').count() === expectedResources,
      `${topicSlug} does not show all reviewed official resources`
    );
    check(
      await page.locator('.official-resource-links a').evaluateAll(links =>
        links.every(link => link.href.startsWith('https://') && link.target === '_blank' && link.rel.includes('noreferrer'))
      ),
      `${topicSlug} official resources are not safe external links`
    );
  }

  await page.goto(`${baseUrl}/technologies/azure-well-architected.html`, { waitUntil: 'networkidle' });
  check(await page.locator('.official-resource-links a').count() === 3, 'WAF overview does not show all official resources');
  check(
    await page.locator('.official-resource-links a').evaluateAll(links =>
      links.every(link => link.href.startsWith('https://learn.microsoft.com/') && link.target === '_blank' && link.rel.includes('noreferrer'))
    ),
    'WAF official resources are not safe external Microsoft links'
  );

  await page.goto(`${baseUrl}/technologies/aspnet-core.html`, { waitUntil: 'networkidle' });

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
  await mobilePage.locator('.sidebar-label').first().tap();
  check(await mobilePage.locator('.sidebar-list').first().isVisible(), 'Mobile sidebar category did not expand');
  await Promise.all([
    mobilePage.waitForURL('**/technologies/dotnet.html'),
    mobilePage.locator('.sidebar-link').first().tap()
  ]);
  await mobilePage.waitForLoadState('networkidle');
  check(mobilePage.url().includes('/technologies/dotnet.html'), `Mobile sidebar route was unexpected: ${mobilePage.url()}`);
  check((await mobilePage.locator('h1').innerText()) === '.NET', 'Mobile topic page did not render');
  await mobilePage.goto(`${baseUrl}/technologies/csharp.html`, { waitUntil: 'networkidle' });
  check((await mobilePage.locator('h1').innerText()) === 'C#', 'Mobile C# topic page did not render');
  const csharpMobileMetrics = await mobilePage.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
  check(csharpMobileMetrics.scrollWidth <= csharpMobileMetrics.width + 1, `Mobile C# horizontal overflow: ${csharpMobileMetrics.scrollWidth}px`);
  await mobilePage.screenshot({ path: join(artifactDirectory, 'csharp-mobile-dark-top.png'), fullPage: false });
  await mobilePage.goto(`${baseUrl}/technologies/dependency-injection.html`, { waitUntil: 'networkidle' });
  check((await mobilePage.locator('h1').innerText()) === 'Dependency Injection', 'Mobile Dependency Injection topic page did not render');
  const dependencyInjectionMobileMetrics = await mobilePage.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
  check(dependencyInjectionMobileMetrics.scrollWidth <= dependencyInjectionMobileMetrics.width + 1, `Mobile Dependency Injection horizontal overflow: ${dependencyInjectionMobileMetrics.scrollWidth}px`);
  await mobilePage.screenshot({ path: join(artifactDirectory, 'dependency-injection-mobile-dark-top.png'), fullPage: false });
  await mobilePage.goto(`${baseUrl}/technologies/source-generators.html`, { waitUntil: 'networkidle' });
  check((await mobilePage.locator('h1').innerText()) === 'Source Generators', 'Mobile Source Generators topic page did not render');
  const sourceGeneratorsMobileMetrics = await mobilePage.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
  check(sourceGeneratorsMobileMetrics.scrollWidth <= sourceGeneratorsMobileMetrics.width + 1, `Mobile Source Generators horizontal overflow: ${sourceGeneratorsMobileMetrics.scrollWidth}px`);
  await mobilePage.screenshot({ path: join(artifactDirectory, 'source-generators-mobile-dark-top.png'), fullPage: false });
  await mobilePage.goto(`${baseUrl}/technologies/blazor.html`, { waitUntil: 'networkidle' });
  check((await mobilePage.locator('h1').innerText()) === 'Blazor', 'Mobile Blazor topic page did not render');
  const blazorMobileMetrics = await mobilePage.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
  check(blazorMobileMetrics.scrollWidth <= blazorMobileMetrics.width + 1, `Mobile Blazor horizontal overflow: ${blazorMobileMetrics.scrollWidth}px`);
  await mobilePage.screenshot({ path: join(artifactDirectory, 'blazor-mobile-dark-top.png'), fullPage: false });
  await mobilePage.goto(`${baseUrl}/technologies/minimal-apis.html`, { waitUntil: 'networkidle' });
  check((await mobilePage.locator('h1').innerText()) === 'Minimal APIs', 'Mobile Minimal APIs topic page did not render');
  const minimalApisMobileMetrics = await mobilePage.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
  check(minimalApisMobileMetrics.scrollWidth <= minimalApisMobileMetrics.width + 1, `Mobile Minimal APIs horizontal overflow: ${minimalApisMobileMetrics.scrollWidth}px`);
  await mobilePage.screenshot({ path: join(artifactDirectory, 'minimal-apis-mobile-dark-top.png'), fullPage: false });
  for (const [slug, heading] of [
    ['authentication-authorization', 'Authentication & Authorization'],
    ['api-security-owasp', 'API Security & OWASP'],
    ['secrets-management', 'Secrets Management']
  ]) {
    await mobilePage.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await mobilePage.locator('h1').innerText()) === heading, `Mobile ${heading} topic did not render`);
    const metrics = await mobilePage.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(metrics.scrollWidth <= metrics.width + 1, `Mobile ${heading} horizontal overflow: ${metrics.scrollWidth}px`);
  }
  await mobilePage.screenshot({ path: join(artifactDirectory, 'security-batch-mobile-dark-top.png'), fullPage: false });
  for (const [slug, heading] of [
    ['api-design-best-practices', 'API Design Best Practices'],
    ['openapi-scalar', 'OpenAPI & Scalar'],
    ['webhook-patterns', 'Webhook Patterns']
  ]) {
    await mobilePage.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await mobilePage.locator('h1').innerText()) === heading, `Mobile ${heading} topic did not render`);
    const metrics = await mobilePage.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(metrics.scrollWidth <= metrics.width + 1, `Mobile ${heading} horizontal overflow: ${metrics.scrollWidth}px`);
  }
  await mobilePage.screenshot({ path: join(artifactDirectory, 'http-contracts-batch-mobile-dark-top.png'), fullPage: false });
  for (const [slug, heading] of [
    ['grpc', 'gRPC'],
    ['signalr', 'SignalR'],
    ['graphql', 'GraphQL in .NET']
  ]) {
    await mobilePage.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await mobilePage.locator('h1').innerText()) === heading, `Mobile ${heading} topic did not render`);
    const metrics = await mobilePage.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(metrics.scrollWidth <= metrics.width + 1, `Mobile ${heading} horizontal overflow: ${metrics.scrollWidth}px`);
  }
  await mobilePage.screenshot({ path: join(artifactDirectory, 'realtime-rpc-batch-mobile-dark-top.png'), fullPage: false });
  for (const [slug, heading] of [
    ['redis-distributed-caching', 'Redis & Distributed Caching'],
    ['multi-tenancy-patterns', 'Multi-Tenancy Patterns'],
    ['performance', 'Performance']
  ]) {
    await mobilePage.goto(`${baseUrl}/technologies/${slug}.html`, { waitUntil: 'networkidle' });
    check((await mobilePage.locator('h1').innerText()) === heading, `Mobile ${heading} topic did not render`);
    const metrics = await mobilePage.evaluate(() => ({ width: innerWidth, scrollWidth: document.documentElement.scrollWidth }));
    check(metrics.scrollWidth <= metrics.width + 1, `Mobile ${heading} horizontal overflow: ${metrics.scrollWidth}px`);
  }
  await mobilePage.screenshot({ path: join(artifactDirectory, 'data-caching-batch-mobile-dark-top.png'), fullPage: false });
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
