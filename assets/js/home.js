import {
  bindSharedInteractions,
  getPortalData,
  githubCard,
  loadTemplates,
  renderError,
  renderFooter,
  renderNavbar,
  renderSidebar,
  searchable,
  searchResultMarkup,
  technologyCard
} from './components.js';

const base = '';

async function start() {
  try {
    const [{ technologies, samples }] = await Promise.all([getPortalData(base), loadTemplates(base)]);
    renderNavbar(base, technologies);
    renderSidebar(base, technologies);
    renderFooter(base);
    renderBlobs(technologies);
    renderFeatured(technologies, samples);
    renderRoadmap(technologies);
    renderLatest(technologies);
    bindHomeSearch(technologies);
    bindSharedInteractions();
  } catch (error) {
    renderError(error);
  }
}

function renderBlobs(technologies) {
  const cloud = document.getElementById('blob-cloud');
  const sizes = [118, 94, 132, 104, 88, 116, 98, 126, 92, 110, 88, 120, 102, 92, 114, 98, 124, 106, 90, 128, 100, 118, 96, 122, 108, 92, 114];
  cloud.replaceChildren(...technologies.map((technology, index) => {
    const blob = document.createElement('a');
    blob.className = 'tech-blob';
    blob.href = `technologies/${technology.slug}.html`;
    blob.textContent = technology.name;
    blob.style.setProperty('--blob-color', technology.color);
    blob.style.setProperty('--blob-size', `${sizes[index]}px`);
    blob.style.setProperty('--float-speed', `${6 + (index % 5)}s`);
    blob.setAttribute('aria-label', `Open ${technology.name} guide`);
    return blob;
  }));
}

function renderFeatured(technologies, samples) {
  const featuredSlugs = ['dotnet', 'aspnet-core', 'entity-framework-core'];
  const featured = featuredSlugs.map(slug => technologies.find(item => item.slug === slug));
  document.getElementById('featured-technologies').replaceChildren(...featured.map(item => technologyCard(item, base)));
  document.getElementById('featured-samples').replaceChildren(...samples.slice(0, 3).map(githubCard));
}

function renderRoadmap(technologies) {
  const steps = [
    { slug: 'csharp', title: 'Language foundations', text: 'C#, .NET, and dependency injection', color: '#6f52c7' },
    { slug: 'aspnet-core', title: 'Build for the web', text: 'ASP.NET Core, APIs, Blazor, and data', color: '#2678c8' },
    { slug: 'clean-architecture', title: 'Shape the system', text: 'Architecture, patterns, security, testing', color: '#188c77' },
    { slug: 'microservices', title: 'Operate at scale', text: 'Azure, containers, Kubernetes, performance', color: '#d26a2e' }
  ];
  document.getElementById('roadmap-list').replaceChildren(...steps.map(step => {
    const technology = technologies.find(item => item.slug === step.slug);
    const item = document.createElement('li');
    item.className = 'roadmap-step reveal';
    item.style.setProperty('--step-color', step.color);
    item.innerHTML = `<a href="technologies/${technology.slug}.html"><h3>${step.title}</h3><p>${step.text}</p></a>`;
    return item;
  }));
}

function renderLatest(technologies) {
  const latest = [...technologies].sort((a, b) => b.updated.localeCompare(a.updated)).slice(0, 3);
  document.getElementById('latest-additions').replaceChildren(...latest.map(item => {
    const link = document.createElement('a');
    link.className = 'latest-item reveal';
    link.href = `technologies/${item.slug}.html`;
    const date = new Date(`${item.updated}T00:00:00`);
    link.innerHTML = `<span class="latest-date">${date.toLocaleDateString('en', { month: 'short', day: 'numeric' })}</span><span><strong>${item.name}</strong><small>${item.latestNote}</small></span>`;
    return link;
  }));
}

function bindHomeSearch(technologies) {
  const input = document.getElementById('home-search');
  const results = document.getElementById('home-search-results');
  input.addEventListener('input', () => {
    const query = input.value.trim().toLowerCase();
    if (!query) { results.classList.remove('has-results'); results.replaceChildren(); return; }
    const matches = technologies.filter(item => searchable(item).includes(query)).slice(0, 6);
    results.innerHTML = matches.length ? matches.map(item => searchResultMarkup(item, base)).join('') : '<p class="search-empty">No matching guide yet.</p>';
    results.classList.add('has-results');
  });
}

start();
