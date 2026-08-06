import {
  bindSharedInteractions,
  callout,
  codeBlock,
  escapeHtml,
  faqItem,
  getPortalData,
  getTechnologyNavigationOrder,
  githubCard,
  loadTemplates,
  renderError,
  renderFooter,
  renderNavbar,
  renderSidebar
} from './components.js';

const base = '../';
const slug = document.body.dataset.topic;

async function start() {
  try {
    const [{ technologies, samples }] = await Promise.all([getPortalData(base), loadTemplates(base)]);
    const topic = technologies.find(item => item.slug === slug);
    if (!topic) throw new Error(`Unknown technology topic: ${slug}`);
    renderNavbar(base, technologies, slug);
    renderSidebar(base, technologies, slug);
    renderFooter(base);
    renderArticle(topic, technologies, samples);
    bindSharedInteractions();
    bindTableOfContents();
  } catch (error) {
    renderError(error);
  }
}

function renderArticle(topic, technologies, samples) {
  const article = document.getElementById('topic-article');
  document.documentElement.style.setProperty('--topic-color', topic.color);
  article.innerHTML = `
    <nav class="breadcrumb" aria-label="Breadcrumb"><a href="../index.html">Home</a><span class="breadcrumb-separator" aria-hidden="true">/</span><span>Technologies</span><span class="breadcrumb-separator" aria-hidden="true">/</span><span aria-current="page">${escapeHtml(topic.name)}</span></nav>
    <header class="article-header">
      <span class="topic-badge">${escapeHtml(topic.category)}</span>
      <h1>${escapeHtml(topic.name)}</h1>
      <p class="article-lede">${escapeHtml(topic.overview)}</p>
      <div class="article-meta"><span>⏱ ${topic.readingMinutes} min read</span><span>Updated ${formatDate(topic.updated)}</span><span>Level: ${escapeHtml(topic.level)}</span></div>
    </header>
    <section id="overview" class="article-section"><h2>Overview</h2><p>${escapeHtml(topic.detail)}</p><div class="official-resources"></div></section>
    <section id="key-concepts" class="article-section"><h2>Key concepts</h2><div class="concept-grid"></div></section>
    <section id="architecture" class="article-section"><h2>Architecture</h2><p>${escapeHtml(topic.architecture.description)}</p><div class="architecture-placeholder"><div class="diagram-flow"></div><p class="diagram-caption">Conceptual architecture · Adapt to your system context</p></div></section>
    <section id="code-example" class="article-section"><h2>Code example</h2><p>${escapeHtml(topic.code.introduction)}</p><div class="code-host"></div></section>
    <section id="best-practices" class="article-section"><h2>Best practices</h2><ul class="practice-list"></ul></section>
    <section id="interview-questions" class="article-section"><h2>Common interview questions</h2><div class="faq-list"></div></section>
    <section id="github-sample" class="article-section article-sample"><h2>Related GitHub sample</h2><p>Move from concept to implementation with a repository selected for this topic.</p><div class="sample-host"></div></section>
    <nav class="article-nav" aria-label="Previous and next topics"></nav>`;

  article.querySelector('.concept-grid').replaceChildren(...topic.concepts.map(concept => {
    const card = document.createElement('article');
    card.className = 'concept-card';
    const title = document.createElement('strong'); title.textContent = concept.title;
    const body = document.createElement('p'); body.textContent = concept.description;
    card.append(title, body);
    return card;
  }));

  const resources = article.querySelector('.official-resources');
  if (topic.resources?.length) {
    const label = document.createElement('strong');
    label.textContent = 'Official resources';
    const links = document.createElement('div');
    links.className = 'official-resource-links';
    links.replaceChildren(...topic.resources.map(resource => {
      const link = document.createElement('a');
      link.className = 'button button-secondary button-small';
      link.href = resource.url;
      link.target = '_blank';
      link.rel = 'noopener noreferrer';
      link.textContent = resource.label;
      link.setAttribute('aria-label', `${resource.label} (opens in a new tab)`);
      return link;
    }));
    resources.append(label, links);
  } else {
    resources.remove();
  }

  const diagram = article.querySelector('.diagram-flow');
  topic.architecture.nodes.forEach((node, index) => {
    const element = document.createElement('div');
    element.className = 'diagram-node';
    element.style.setProperty('--node-color', topic.architecture.colors[index] || topic.color);
    element.textContent = node;
    diagram.append(element);
    if (index < topic.architecture.nodes.length - 1) {
      const arrow = document.createElement('span'); arrow.className = 'diagram-arrow'; arrow.setAttribute('aria-hidden', 'true'); arrow.textContent = '→'; diagram.append(arrow);
    }
  });

  article.querySelector('.code-host').append(codeBlock(topic.code));
  article.querySelector('#code-example').append(callout('note', topic.note));
  article.querySelector('.practice-list').replaceChildren(...topic.bestPractices.map(practice => {
    const item = document.createElement('li'); item.textContent = practice; return item;
  }));
  article.querySelector('#best-practices').append(callout('tip', topic.tip));
  article.querySelector('#best-practices').append(callout('warning', topic.warning));
  article.querySelector('.faq-list').replaceChildren(...topic.interviewQuestions.map(faqItem));

  const sample = samples.find(item => item.id === topic.sampleId) || samples[0];
  article.querySelector('.sample-host').append(githubCard(sample));
  renderAdjacentNavigation(article.querySelector('.article-nav'), topic, technologies);
  renderToc(topic);
}

function renderAdjacentNavigation(host, topic, technologies) {
  const navigationOrder = getTechnologyNavigationOrder(technologies);
  const index = navigationOrder.findIndex(item => item.slug === topic.slug);
  const previous = navigationOrder[index - 1];
  const next = navigationOrder[index + 1];
  if (previous) host.append(navLink(previous, '← Previous'));
  else host.append(navLink(navigationOrder.at(-1), '← Last guide'));
  if (next) host.append(navLink(next, 'Next →'));
  else host.append(navLink(navigationOrder[0], 'First guide →'));
}

function navLink(topic, label) {
  const link = document.createElement('a');
  link.href = `${topic.slug}.html`;
  link.innerHTML = `<small>${label}</small><strong>${escapeHtml(topic.name)}</strong>`;
  return link;
}

function renderToc(topic) {
  const sections = [
    ['overview','Overview'],['key-concepts','Key concepts'],['architecture','Architecture'],['code-example','Code example'],['best-practices','Best practices'],['interview-questions','Interview questions'],['github-sample','GitHub sample']
  ];
  document.getElementById('article-toc').innerHTML = `<p class="toc-title">On this page</p><ul class="toc-list">${sections.map(([id,label]) => `<li><a class="toc-link" href="#${id}">${label}</a></li>`).join('')}</ul>`;
}

function bindTableOfContents() {
  const links = [...document.querySelectorAll('.toc-link')];
  const sections = links.map(link => document.querySelector(link.hash));
  const observer = new IntersectionObserver(entries => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        links.forEach(link => link.classList.toggle('is-active', link.hash === `#${entry.target.id}`));
      }
    });
  }, { rootMargin: '-25% 0px -65% 0px' });
  sections.forEach(section => section && observer.observe(section));
}

function formatDate(value) {
  return new Date(`${value}T00:00:00`).toLocaleDateString('en', { month: 'short', day: 'numeric', year: 'numeric' });
}

start();
