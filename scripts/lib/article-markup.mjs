// Build-time article renderer. Produces the same markup that assets/js/topic.js used to
// build in the browser, as a plain string, so every topic page ships with its content.
import { escapeHtml as e, getTechnologyNavigationOrder, highlight } from '../../assets/js/markup.js';

const list = (items = []) => (items || []).map(text => `<li>${e(text)}</li>`).join('');

export function formatDate(value) {
  return new Date(`${value}T00:00:00Z`).toLocaleDateString('en', { month: 'short', day: 'numeric', year: 'numeric', timeZone: 'UTC' });
}

function callout(type, text) {
  return `<aside class="callout callout-${type}"><div class="callout-icon" aria-hidden="true"></div><div><strong class="callout-label">${type}</strong><p>${e(text ?? '')}</p></div></aside>`;
}

function codeBlock(example) {
  return `<div class="code-block"><div class="code-toolbar"><span class="code-language">${e(example.label || example.language)}</span><button class="copy-button" type="button"><span aria-hidden="true">▣</span> Copy</button></div><pre><code class="language-${e(example.language)}">${highlight(example.code, example.language)}</code></pre></div>`;
}

function faqItem(question) {
  return `<details class="faq-item"><summary><span class="faq-question">${e(question.question)}</span><span class="faq-chevron" aria-hidden="true">+</span></summary><div class="faq-answer"><p>${e(question.answer)}</p></div></details>`;
}

function githubCard(sample) {
  const commandRow = (label, command) => `<div><dt>${label}</dt><dd><code>${e(command)}</code></dd></div>`;
  const commands = sample.runCommand || sample.testCommand
    ? `<dl class="sample-commands">${sample.runCommand ? commandRow('Run', sample.runCommand) : ''}${sample.testCommand ? commandRow('Test', sample.testCommand) : ''}</dl>`
    : '';
  const demo = sample.liveDemoUrl
    ? `<a class="button button-small button-secondary demo-link" target="_blank" rel="noreferrer" href="${e(sample.liveDemoUrl)}">Live demo <span aria-hidden="true">↗</span></a>`
    : '';
  return `<article class="github-card card reveal"><div class="github-card-head"><span class="repo-icon" aria-hidden="true"></span><span class="repo-visibility">${sample.status === 'complete' ? 'Verified sample' : 'Public'}</span></div><h3>${e(sample.name)}</h3><p class="repo-description">${e(sample.description)}</p><div class="tech-pills" aria-label="Technologies">${sample.technologies.map(t => `<span>${e(t)}</span>`).join('')}</div><div class="tag-list" aria-label="Tags">${sample.tags.map(t => `<span>${e(t)}</span>`).join('')}</div>${commands}<div class="repo-actions"><a class="button button-small button-primary github-link" target="_blank" rel="noreferrer" href="${e(sample.githubUrl)}">GitHub <span aria-hidden="true">↗</span></a>${demo}</div></article>`;
}

function guidance(id, title, block) {
  if (!block?.items?.length) return '';
  return `<section id="${id}" class="article-section"><h2>${title}</h2><p>${e(block.introduction)}</p><ul class="practice-list guidance-list">${list(block.items)}</ul></section>`;
}

function resources(items) {
  if (!items?.length) return '';
  const links = items.map(r => `<a class="button button-secondary button-small" href="${e(r.url)}" target="_blank" rel="noopener noreferrer" aria-label="${e(`${r.label} (opens in a new tab)`)}">${e(r.label)}</a>`).join('');
  return `<div class="official-resources"><strong>Official resources</strong><div class="official-resource-links">${links}</div></div>`;
}

function diagram(topic) {
  return topic.architecture.nodes.map((node, index) => {
    const arrow = index < topic.architecture.nodes.length - 1 ? '<span class="diagram-arrow" aria-hidden="true">→</span>' : '';
    return `<div class="diagram-node" style="--node-color:${e(topic.architecture.colors?.[index] || topic.color)}">${e(node)}</div>${arrow}`;
  }).join('');
}

function steps(items = []) {
  return (items || []).map((step, index) => `<li><span>${String(index + 1).padStart(2, '0')}</span><div><strong>${e(step.title)}</strong><p>${e(step.description)}</p></div></li>`).join('');
}

function troubleshooting(items = []) {
  return (items || []).map(p => `<article class="troubleshooting-card"><h3>${e(p.symptom)}</h3><p><strong>Likely cause:</strong> ${e(p.cause)}</p><p><strong>Fix:</strong> ${e(p.fix)}</p></article>`).join('');
}

function adjacent(topic, technologies) {
  const order = getTechnologyNavigationOrder(technologies);
  const index = order.findIndex(item => item.slug === topic.slug);
  const link = (item, label) => `<a href="${item.slug}.html"><small>${label}</small><strong>${e(item.name)}</strong></a>`;
  const previous = order[index - 1] ? link(order[index - 1], '← Previous') : link(order.at(-1), '← Last guide');
  const next = order[index + 1] ? link(order[index + 1], 'Next →') : link(order[0], 'First guide →');
  return previous + next;
}

export function renderArticle(topic, technologies, samples) {
  const sample = samples.find(item => item.id === topic.sampleId);
  const status = topic.contentStatus || 'outline';
  const statusLabel = status === 'complete' ? 'Complete guide' : status === 'in-progress' ? 'In progress' : 'Outline guide';
  const dedicated = sample?.topicSlug === topic.slug;
  const sampleHtml = sample
    ? githubCard(sample)
    : '<div class="error-panel"><h3>Dedicated sample pending</h3><p>This guide does not yet have a verified GitHub sample.</p></div>';

  return `
    <nav class="breadcrumb" aria-label="Breadcrumb"><a href="../index.html">Home</a><span class="breadcrumb-separator" aria-hidden="true">/</span><span>Technologies</span><span class="breadcrumb-separator" aria-hidden="true">/</span><span aria-current="page">${e(topic.name)}</span></nav>
    <header class="article-header">
      <div class="topic-labels"><span class="topic-badge">${e(topic.category)}</span><span class="topic-status topic-status-${e(status)}">${statusLabel}</span></div>
      <h1>${e(topic.name)}</h1>
      <p class="article-lede">${e(topic.overview)}</p>
      <div class="article-meta"><span>⏱ ${topic.readingMinutes} min read</span><span>Updated <time datetime="${e(topic.updated)}">${formatDate(topic.updated)}</time></span><span>Level: ${e(topic.level)}</span></div>
    </header>
    <section id="overview" class="article-section"><h2>Overview</h2><p>${e(topic.detail)}</p>${resources(topic.resources)}</section>
    ${topic.learningObjectives?.length ? `<section id="learning-objectives" class="article-section"><h2>What you will learn</h2><ul class="practice-list learning-objectives">${list(topic.learningObjectives)}</ul><h3>Prerequisites</h3><ul class="plain-list prerequisites">${list(topic.prerequisites)}</ul></section>` : ''}
    ${topic.decisionGuide ? `<section id="decision-guide" class="article-section"><h2>When to use ${e(topic.name)}</h2><div class="decision-grid"><article class="decision-panel decision-use"><h3>Good fit</h3><ul>${list(topic.decisionGuide.use)}</ul></article><article class="decision-panel decision-avoid"><h3>Choose another approach when</h3><ul>${list(topic.decisionGuide.avoid)}</ul></article></div></section>` : ''}
    <section id="key-concepts" class="article-section"><h2>Key concepts</h2><div class="concept-grid">${topic.concepts.map(c => `<article class="concept-card"><strong>${e(c.title)}</strong><p>${e(c.description)}</p></article>`).join('')}</div></section>
    <section id="architecture" class="article-section"><h2>Architecture</h2><p>${e(topic.architecture.description)}</p><div class="architecture-placeholder"><div class="diagram-flow">${diagram(topic)}</div><p class="diagram-caption">Conceptual architecture · Adapt to your system context</p></div></section>
    ${topic.implementationSteps?.length ? `<section id="implementation" class="article-section"><h2>Implementation walkthrough</h2><p>${e(topic.implementationIntroduction || 'Build the critical path deliberately, then verify its observable behavior.')}</p><ol class="step-list">${steps(topic.implementationSteps)}</ol></section>` : ''}
    <section id="code-example" class="article-section"><h2>Code example</h2><p>${e(topic.code.introduction)}</p><div class="code-host">${codeBlock(topic.code)}</div>${callout('note', topic.note)}</section>
    ${guidance('testing', 'Testing strategy', topic.testing)}
    ${guidance('security', 'Security considerations', topic.security)}
    ${guidance('performance', 'Performance and scalability', topic.performance)}
    ${guidance('deployment', 'Deployment and operations', topic.deployment)}
    ${topic.troubleshooting?.length ? `<section id="troubleshooting" class="article-section"><h2>Common problems and fixes</h2><div class="troubleshooting-list">${troubleshooting(topic.troubleshooting)}</div></section>` : ''}
    <section id="best-practices" class="article-section"><h2>Best practices</h2><ul class="practice-list">${list(topic.bestPractices)}</ul>${callout('tip', topic.tip)}${callout('warning', topic.warning)}</section>
    ${topic.productionChecklist?.length ? `<section id="production-checklist" class="article-section"><h2>Production checklist</h2><ul class="checklist">${list(topic.productionChecklist)}</ul></section>` : ''}
    <section id="interview-questions" class="article-section"><h2>Common interview questions</h2><div class="faq-list">${topic.interviewQuestions.map(faqItem).join('')}</div></section>
    <section id="github-sample" class="article-section article-sample"><h2>${dedicated ? 'Dedicated GitHub sample' : 'Related GitHub sample'}</h2><p>${dedicated ? 'Clone, run, test, and modify the focused implementation created for this guide.' : 'Move from concept to implementation with a repository selected for this topic.'}</p><div class="sample-host">${sampleHtml}</div></section>
    <nav class="article-nav" aria-label="Previous and next topics">${adjacent(topic, technologies)}</nav>`;
}

// Mirrors the section ids/titles that are present in the rendered article.
export function renderToc(articleHtml) {
  const sections = [...articleHtml.matchAll(/<section id="([^"]+)" class="article-section[^"]*"><h2>(.*?)<\/h2>/g)];
  return `<p class="toc-title">On this page</p><ul class="toc-list">${sections.map(([, id, label]) => `<li><a class="toc-link" href="#${id}">${label}</a></li>`).join('')}</ul>`;
}
