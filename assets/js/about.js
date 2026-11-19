// The About page content is static HTML. This script adds the shared navbar, sidebar, and footer.
import {
  bindSharedInteractions,
  getPortalData,
  renderFooter,
  renderNavbar,
  renderSidebar
} from './components.js?v=20261119';

const base = '';

async function start() {
  bindSharedInteractions();
  renderFooter(base);
  try {
    const { technologies } = await getPortalData(base);
    renderNavbar(base, technologies);
    renderSidebar(base, technologies);
  } catch (error) {
    console.error(error);
  }
}

start();
