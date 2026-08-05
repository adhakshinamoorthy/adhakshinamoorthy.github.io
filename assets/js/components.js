let templatesReady;

const escapeHtml = (value = '') => String(value)
  .replaceAll('&', '&amp;')
  .replaceAll('<', '&lt;')
  .replaceAll('>', '&gt;')
  .replaceAll('"', '&quot;')
  .replaceAll("'", '&#039;');

export async function loadTemplates(base = '') {
  if (!templatesReady) {
    templatesReady = fetch(`${base}components/templates.html`)
      .then((response) => {
        if (!response.ok) throw new Error(`Could not load component templates (${response.status})`);
        return response.text();
      })
      .then((markup) => {
        const host = document.createElement('div');
        host.id = 'component-templates';
        host.hidden = true;
        host.innerHTML = markup;
        document.body.append(host);
      });
  }
  return templatesReady;
}

function cloneTemplate(id) {
  const template = document.getElementById(id);
  if (!template) throw new Error(`Missing component template: ${id}`);
  return template.content.firstElementChild.cloneNode(true);
}

export function technologyCard(technology, base = '') {
  const card = cloneTemplate('technology-card-template');
  card.style.setProperty('--topic-color', technology.color);
  card.querySelector('.technology-card-icon').textContent = technology.symbol;
  card.querySelector('.card-kicker').textContent = technology.category;
  card.querySelector('h3').textContent = technology.name;
  card.querySelector('p').textContent = technology.shortDescription;
  card.querySelector('a').href = `${base}technologies/${technology.slug}.html`;
  return card;
}

export function githubCard(sample) {
  const card = cloneTemplate('github-card-template');
  card.querySelector('h3').textContent = sample.name;
  card.querySelector('.repo-description').textContent = sample.description;
  card.querySelector('.tech-pills').replaceChildren(...sample.technologies.map(label => pill(label)));
  card.querySelector('.tag-list').replaceChildren(...sample.tags.map(label => pill(label)));
  card.querySelector('.github-link').href = sample.githubUrl;
  const demo = card.querySelector('.demo-link');
  if (sample.liveDemoUrl) demo.href = sample.liveDemoUrl;
  else demo.remove();
  return card;
}

function pill(text) {
  const element = document.createElement('span');
  element.textContent = text;
  return element;
}

export function callout(type, text) {
  const panel = cloneTemplate('callout-template');
  panel.classList.add(`callout-${type}`);
  panel.querySelector('.callout-label').textContent = type;
  panel.querySelector('p').textContent = text;
  return panel;
}

export function faqItem(question) {
  const item = cloneTemplate('faq-template');
  item.querySelector('.faq-question').textContent = question.question;
  const answer = document.createElement('p');
  answer.textContent = question.answer;
  item.querySelector('.faq-answer').append(answer);
  return item;
}

export function codeBlock(example) {
  const block = cloneTemplate('code-block-template');
  block.querySelector('.code-language').textContent = example.label || example.language;
  const code = block.querySelector('code');
  code.className = `language-${example.language}`;
  code.innerHTML = highlight(example.code, example.language);
  const button = block.querySelector('.copy-button');
  button.addEventListener('click', async () => {
    try {
      await navigator.clipboard.writeText(example.code);
      button.textContent = '✓ Copied';
      setTimeout(() => { button.innerHTML = '<span aria-hidden="true">▣</span> Copy'; }, 1600);
    } catch {
      button.textContent = 'Select & copy';
      const selection = getSelection();
      const range = document.createRange();
      range.selectNodeContents(code);
      selection.removeAllRanges();
      selection.addRange(range);
    }
  });
  return block;
}

function highlight(source, language) {
  const keywords = new Set([
    'abstract','as','async','await','base','bool','break','case','catch','class','const','continue','default','delegate','do','else','enum','false','finally','for','foreach','from','get','if','in','init','interface','internal','is','lock','namespace','new','not','null','object','operator','or','out','override','params','private','protected','public','readonly','record','required','return','sealed','set','static','string','struct','switch','this','throw','true','try','using','var','virtual','void','when','where','while','with','yield'
  ]);
  const types = new Set(['Task','CancellationToken','WebApplication','IServiceCollection','DbContext','DbSet','HttpClient','Results','ActionResult','Guid','DateTime','TimeSpan','List','Dictionary','IEnumerable']);
  if (!['csharp', 'cs'].includes(language)) return escapeHtml(source);

  const pattern = /(\/\/[^\n]*|\/\*[\s\S]*?\*\/|@?"(?:""|\\.|[^"\\])*"|'(?:\\.|[^'\\])'|\b\d+(?:\.\d+)?\b|\b[A-Za-z_]\w*\b)/g;
  let cursor = 0;
  let output = '';
  for (const match of source.matchAll(pattern)) {
    output += escapeHtml(source.slice(cursor, match.index));
    const token = match[0];
    let kind = '';
    if (token.startsWith('//') || token.startsWith('/*')) kind = 'comment';
    else if (token.startsWith('"') || token.startsWith('@"') || token.startsWith("'")) kind = 'string';
    else if (/^\d/.test(token)) kind = 'number';
    else if (keywords.has(token)) kind = 'keyword';
    else if (types.has(token) || /^[A-Z][A-Za-z0-9_]*$/.test(token)) kind = 'type';
    output += kind ? `<span class="tok-${kind}">${escapeHtml(token)}</span>` : escapeHtml(token);
    cursor = match.index + token.length;
  }
  return output + escapeHtml(source.slice(cursor));
}

export function renderNavbar(base, technologies, activeSlug = '') {
  const host = document.getElementById('site-navbar');
  host.innerHTML = `
    <header class="topbar">
      <div class="topbar-inner">
        <button class="icon-button menu-button" id="menu-toggle" type="button" aria-label="Open technology navigation" aria-expanded="false"><span class="menu-lines" aria-hidden="true"></span></button>
        <a class="brand" href="${base}index.html" aria-label=".NET Atlas home"><span class="brand-mark">.N</span><span class="brand-word">.NET <em>Atlas</em></span></a>
        <nav class="top-links" aria-label="Primary navigation">
          <a class="top-link" href="${base}index.html#featured-title">Explore</a>
          <a class="top-link" href="${base}index.html#roadmap">Roadmap</a>
          <a class="top-link" href="${base}index.html#samples-title">Samples</a>
        </nav>
        <span class="nav-spacer"></span>
        <div class="nav-search">
          <span class="search-icon" aria-hidden="true"></span>
          <label class="sr-only" for="global-search">Search all topics</label>
          <input id="global-search" type="search" placeholder="Search topics…" autocomplete="off" aria-controls="search-popover" aria-expanded="false">
          <kbd>/</kbd>
          <div id="search-popover" class="search-popover" role="listbox" aria-label="Search results"></div>
        </div>
        <button class="icon-button" id="theme-toggle" type="button" aria-label="Switch color theme" title="Switch color theme"><span class="theme-icon" aria-hidden="true"></span></button>
      </div>
    </header>`;
  bindThemeToggle();
  bindGlobalSearch(base, technologies);
  bindMobileMenu();
}

export function renderSidebar(base, technologies, activeSlug = '') {
  const host = document.getElementById('site-sidebar');
  const groups = technologies.reduce((result, item) => {
    if (!result.has(item.category)) result.set(item.category, []);
    result.get(item.category).push(item);
    return result;
  }, new Map());
  host.innerHTML = [...groups].map(([category, items]) => `
    <nav class="sidebar-group" aria-label="${escapeHtml(category)}">
      <span class="sidebar-label">${escapeHtml(category)}</span>
      <ul class="sidebar-list">
        ${items.map(item => `<li><a class="sidebar-link" href="${base}technologies/${item.slug}.html" ${item.slug === activeSlug ? 'aria-current="page"' : ''}><span class="sidebar-dot" style="--topic-color:${item.color}"></span>${escapeHtml(item.name)}</a></li>`).join('')}
      </ul>
    </nav>`).join('');
  const overlay = document.createElement('div');
  overlay.className = 'sidebar-overlay';
  overlay.id = 'sidebar-overlay';
  document.body.append(overlay);
  overlay.addEventListener('click', closeMobileMenu);
  host.addEventListener('click', event => {
    if (event.target.closest('a')) closeMobileMenu();
  });
}

function bindThemeToggle() {
  const button = document.getElementById('theme-toggle');
  const systemTheme = matchMedia('(prefers-color-scheme: dark)');
  const refreshLabel = () => {
    const next = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark';
    button.setAttribute('aria-label', `Switch to ${next} mode`);
    button.title = `Switch to ${next} mode`;
  };
  refreshLabel();
  button.addEventListener('click', () => {
    const next = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark';
    document.documentElement.dataset.theme = next;
    localStorage.setItem('dotnet-atlas-theme', next);
    refreshLabel();
  });
  systemTheme.addEventListener('change', event => {
    if (localStorage.getItem('dotnet-atlas-theme')) return;
    document.documentElement.dataset.theme = event.matches ? 'dark' : 'light';
    refreshLabel();
  });
}

function bindGlobalSearch(base, technologies) {
  const input = document.getElementById('global-search');
  const popover = document.getElementById('search-popover');
  const update = () => {
    const query = input.value.trim().toLowerCase();
    if (!query) return close();
    const matches = technologies.filter(item => searchable(item).includes(query)).slice(0, 7);
    popover.innerHTML = matches.length
      ? matches.map(item => searchResultMarkup(item, base)).join('')
      : '<p class="search-empty">No topics found. Try a broader term.</p>';
    popover.classList.add('is-open');
    input.setAttribute('aria-expanded', 'true');
  };
  const close = () => {
    popover.classList.remove('is-open');
    input.setAttribute('aria-expanded', 'false');
  };
  input.addEventListener('input', update);
  input.addEventListener('keydown', event => {
    if (event.key === 'Escape') { input.value = ''; close(); input.blur(); }
    if (event.key === 'ArrowDown') { event.preventDefault(); popover.querySelector('a')?.focus(); }
  });
  popover.addEventListener('keydown', event => {
    if (event.key === 'Escape') { close(); input.focus(); }
  });
  document.addEventListener('click', event => {
    if (!event.target.closest('.nav-search')) close();
  });
  document.addEventListener('keydown', event => {
    if (event.key === '/' && !/INPUT|TEXTAREA|SELECT/.test(document.activeElement.tagName)) {
      event.preventDefault(); input.focus();
    }
  });
}

export function searchable(item) {
  return [item.name, item.category, item.shortDescription, ...(item.keywords || [])].join(' ').toLowerCase();
}

export function searchResultMarkup(item, base = '') {
  return `<a class="search-result" role="option" href="${base}technologies/${item.slug}.html"><span class="search-result-icon" style="background:${item.color}">${escapeHtml(item.symbol)}</span><span><strong>${escapeHtml(item.name)}</strong><small>${escapeHtml(item.category)}</small></span></a>`;
}

function bindMobileMenu() {
  document.getElementById('menu-toggle').addEventListener('click', () => {
    const open = !document.getElementById('site-sidebar').classList.contains('is-open');
    document.getElementById('site-sidebar').classList.toggle('is-open', open);
    document.getElementById('sidebar-overlay')?.classList.toggle('is-open', open);
    document.getElementById('menu-toggle').setAttribute('aria-expanded', String(open));
    document.body.style.overflow = open ? 'hidden' : '';
  });
}

function closeMobileMenu() {
  document.getElementById('site-sidebar')?.classList.remove('is-open');
  document.getElementById('sidebar-overlay')?.classList.remove('is-open');
  document.getElementById('menu-toggle')?.setAttribute('aria-expanded', 'false');
  document.body.style.overflow = '';
}

export function renderFooter(base = '') {
  document.getElementById('site-footer').innerHTML = `
    <footer class="site-footer">
      <div class="footer-inner">
        <div class="footer-brand">
          <a class="brand" href="${base}index.html"><span class="brand-mark">.N</span><span>.NET <em>Atlas</em></span></a>
          <p>A practical map of the modern .NET ecosystem, built for curious developers and architecture-minded teams.</p>
        </div>
        <div><strong class="footer-heading">Explore</strong><div class="footer-links"><a href="${base}technologies/dotnet.html">Start with .NET</a><a href="${base}index.html#roadmap">Learning roadmap</a><a href="${base}sitemap.xml">Sitemap</a></div></div>
        <div><strong class="footer-heading">Connect</strong><div class="footer-links"><a href="https://github.com/adhakshinamoorthy" target="_blank" rel="noreferrer">GitHub profile ↗</a><a href="https://www.linkedin.com/in/adhakshinamoorthy" target="_blank" rel="noreferrer">LinkedIn ↗</a></div></div>
      </div>
      <div class="footer-bottom"><p>© ${new Date().getFullYear()} Dhakshinamoorthy A. Built for learning.</p><p>HTML · CSS · JavaScript · GitHub Pages</p></div>
    </footer>`;
}

export function bindSharedInteractions() {
  const backToTop = document.getElementById('back-to-top');
  const updateBackToTop = () => backToTop?.classList.toggle('is-visible', scrollY > 650);
  addEventListener('scroll', updateBackToTop, { passive: true });
  updateBackToTop();
  backToTop?.addEventListener('click', () => scrollTo({ top: 0, behavior: 'smooth' }));

  const observer = new IntersectionObserver(entries => {
    entries.forEach(entry => {
      if (entry.isIntersecting) { entry.target.classList.add('is-visible'); observer.unobserve(entry.target); }
    });
  }, { threshold: .08, rootMargin: '0px 0px -30px' });
  document.querySelectorAll('.reveal').forEach(element => observer.observe(element));
  document.querySelectorAll('img:not([loading])').forEach(image => { image.loading = 'lazy'; });
}

export async function getPortalData(base = '') {
  const [technologyResponse, sampleResponse] = await Promise.all([
    fetch(`${base}data/technologies.json`),
    fetch(`${base}data/github-samples.json`)
  ]);
  if (!technologyResponse.ok || !sampleResponse.ok) throw new Error('Portal data could not be loaded. Serve the site through HTTP instead of opening files directly.');
  return { technologies: await technologyResponse.json(), samples: await sampleResponse.json() };
}

export function renderError(error) {
  const main = document.getElementById('main-content');
  if (main) main.innerHTML = `<div class="error-panel"><h1>We could not load this page</h1><p>${escapeHtml(error.message)}</p><p>For local preview, start a web server as described in the README.</p></div>`;
  console.error(error);
}

export { escapeHtml };
