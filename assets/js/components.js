import {
  escapeHtml,
  footerMarkup,
  getTechnologyNavigationOrder,
  groupTechnologiesByCategory,
  highlight,
  sidebarMarkup
} from './markup.js?v=20261119';

let templatesReady;

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
  const status = technology.contentStatus === 'complete' ? 'Complete' : technology.contentStatus === 'in-progress' ? 'In progress' : 'Outline';
  card.querySelector('.card-kicker').textContent = `${technology.category} · ${status}`;
  card.querySelector('h3').textContent = technology.name;
  card.querySelector('p').textContent = technology.shortDescription;
  card.querySelector('a').href = `${base}technologies/${technology.slug}.html`;
  return card;
}

export function githubCard(sample) {
  const card = cloneTemplate('github-card-template');
  if (sample.status === 'complete') card.querySelector('.repo-visibility').textContent = 'Verified sample';
  card.querySelector('h3').textContent = sample.name;
  card.querySelector('.repo-description').textContent = sample.description;
  card.querySelector('.tech-pills').replaceChildren(...sample.technologies.map(label => pill(label)));
  card.querySelector('.tag-list').replaceChildren(...sample.tags.map(label => pill(label)));
  card.querySelector('.github-link').href = sample.githubUrl;
  if (sample.runCommand || sample.testCommand) {
    const commands = document.createElement('dl');
    commands.className = 'sample-commands';
    if (sample.runCommand) commands.append(commandRow('Run', sample.runCommand));
    if (sample.testCommand) commands.append(commandRow('Test', sample.testCommand));
    card.querySelector('.repo-actions').before(commands);
  }
  const demo = card.querySelector('.demo-link');
  if (sample.liveDemoUrl) demo.href = sample.liveDemoUrl;
  else demo.remove();
  return card;
}

function commandRow(label, command) {
  const wrapper = document.createElement('div');
  const term = document.createElement('dt'); term.textContent = label;
  const value = document.createElement('dd');
  const code = document.createElement('code'); code.textContent = command;
  value.append(code);
  wrapper.append(term, value);
  return wrapper;
}

function pill(text) {
  const element = document.createElement('span');
  element.textContent = text;
  return element;
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
  if (!host.hasAttribute('data-prerendered')) host.innerHTML = sidebarMarkup(base, technologies, activeSlug);
  const overlay = document.createElement('div');
  overlay.className = 'sidebar-overlay';
  overlay.id = 'sidebar-overlay';
  document.body.append(overlay);
  overlay.addEventListener('click', closeMobileMenu);
  host.addEventListener('click', event => {
    const button = event.target.closest('.sidebar-label');
    if (button) {
      const list = document.getElementById(button.getAttribute('aria-controls'));
      const expanded = button.getAttribute('aria-expanded') !== 'true';
      button.setAttribute('aria-expanded', String(expanded));
      list.hidden = !expanded;
      return;
    }
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
  document.getElementById('site-footer').innerHTML = footerMarkup(base);
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
    fetch(`${base}data/technologies-index.json`),
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

export { escapeHtml, getTechnologyNavigationOrder, groupTechnologiesByCategory };
