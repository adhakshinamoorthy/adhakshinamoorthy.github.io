// Topic pages are pre-rendered by scripts/build-pages.mjs. This script only adds
// behaviour: navbar + search, sidebar toggles, copy buttons, and the active TOC link.
import {
  bindSharedInteractions,
  getPortalData,
  renderFooter,
  renderNavbar,
  renderSidebar
} from './components.js?v=20261119';

const base = '../';
const slug = document.body.dataset.topic;

async function start() {
  // These work without any data, so bind them first.
  bindCopyButtons();
  bindTableOfContents();
  bindSharedInteractions();
  try {
    const { technologies } = await getPortalData(base);
    renderNavbar(base, technologies, slug);
    renderSidebar(base, technologies, slug);
    renderFooter(base);
  } catch (error) {
    // The article is already on the page, so never replace it with an error.
    console.error(error);
  }
}

function bindCopyButtons() {
  document.getElementById('topic-article')?.addEventListener('click', async event => {
    const button = event.target.closest('.copy-button');
    if (!button) return;
    const code = button.closest('.code-block')?.querySelector('code');
    if (!code) return;
    try {
      await navigator.clipboard.writeText(code.textContent);
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

start();
