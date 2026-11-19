// Pure string helpers shared by the browser and the Node build (scripts/build-pages.mjs).
// Nothing in this file may touch `document` or `window`.

export const escapeHtml = (value = '') => String(value)
  .replaceAll('&', '&amp;')
  .replaceAll('<', '&lt;')
  .replaceAll('>', '&gt;')
  .replaceAll('"', '&quot;')
  .replaceAll("'", '&#039;');

const csharpKeywords = new Set([
  'abstract','as','async','await','base','bool','break','case','catch','class','const','continue','default','delegate','do','else','enum','false','finally','for','foreach','from','get','if','in','init','interface','internal','is','lock','namespace','new','not','null','object','operator','or','out','override','params','private','protected','public','readonly','record','required','return','sealed','set','static','string','struct','switch','this','throw','true','try','using','var','virtual','void','when','where','while','with','yield'
]);
const csharpTypes = new Set(['Task','CancellationToken','WebApplication','IServiceCollection','DbContext','DbSet','HttpClient','Results','ActionResult','Guid','DateTime','TimeSpan','List','Dictionary','IEnumerable']);

export function highlight(source, language) {
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
    else if (csharpKeywords.has(token)) kind = 'keyword';
    else if (csharpTypes.has(token) || /^[A-Z][A-Za-z0-9_]*$/.test(token)) kind = 'type';
    output += kind ? `<span class="tok-${kind}">${escapeHtml(token)}</span>` : escapeHtml(token);
    cursor = match.index + token.length;
  }
  return output + escapeHtml(source.slice(cursor));
}

export function groupTechnologiesByCategory(technologies) {
  return technologies.reduce((groups, item) => {
    if (!groups.has(item.category)) groups.set(item.category, []);
    groups.get(item.category).push(item);
    return groups;
  }, new Map());
}

export function getTechnologyNavigationOrder(technologies) {
  return [...groupTechnologiesByCategory(technologies).values()].flat();
}

export function sidebarMarkup(base, technologies, activeSlug = '') {
  const groups = groupTechnologiesByCategory(technologies);
  return [...groups].map(([category, items], index) => {
    const containsActiveTopic = items.some(item => item.slug === activeSlug);
    const listId = `sidebar-group-${index}`;
    return `
    <nav class="sidebar-group" aria-label="${escapeHtml(category)}">
      <div class="sidebar-disclosure">
        <button class="sidebar-label" type="button" aria-expanded="${containsActiveTopic}" aria-controls="${listId}">
          <span>${escapeHtml(category)}</span>
          <span class="sidebar-count" aria-label="${items.length} topics">${items.length}</span>
          <span class="sidebar-chevron" aria-hidden="true"></span>
        </button>
        <ul id="${listId}" class="sidebar-list"${containsActiveTopic ? '' : ' hidden'}>
          ${items.map(item => `<li><a class="sidebar-link" href="${base}technologies/${item.slug}.html" ${item.slug === activeSlug ? 'aria-current="page"' : ''}><span class="sidebar-dot" style="--topic-color:${item.color}"></span>${escapeHtml(item.name)}</a></li>`).join('')}
        </ul>
      </div>
    </nav>`;
  }).join('');
}

export function footerMarkup(base = '', year = new Date().getFullYear()) {
  return `
    <footer class="site-footer">
      <div class="footer-inner">
        <div class="footer-brand">
          <a class="brand" href="${base}index.html"><span class="brand-mark">.N</span><span>.NET <em>Atlas</em></span></a>
          <p>A practical map of the modern .NET ecosystem, built for curious developers and architecture-minded teams.</p>
        </div>
        <div><strong class="footer-heading">Explore</strong><div class="footer-links"><a href="${base}technologies/dotnet.html">Start with .NET</a><a href="${base}index.html#roadmap">Learning roadmap</a><a href="${base}about.html">About the author</a><a href="${base}sitemap.xml">Sitemap</a></div></div>
        <div><strong class="footer-heading">Connect</strong><div class="footer-links"><a href="https://github.com/adhakshinamoorthy" target="_blank" rel="noreferrer">GitHub profile ↗</a><a href="https://www.linkedin.com/in/adhakshinamoorthy" target="_blank" rel="noreferrer">LinkedIn ↗</a><a href="${base}about.html#work-with-me">Work with me</a></div></div>
      </div>
      <div class="footer-bottom"><p>© ${year} Dhakshinamoorthy A. Built for learning.</p><p>HTML · CSS · JavaScript · GitHub Pages</p></div>
    </footer>`;
}

// Fields the browser needs for navigation, search, and the home page.
// Full article content is pre-rendered into each topic page at build time.
export const indexFields = ['slug', 'name', 'symbol', 'category', 'color', 'shortDescription', 'keywords', 'contentStatus', 'readingMinutes', 'level', 'updated', 'latestNote'];
