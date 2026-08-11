# .NET Atlas

A modern, responsive static learning portal for the Microsoft .NET ecosystem. It is built with semantic HTML, modular CSS, vanilla JavaScript, and JSON data—there is no build step or runtime dependency.

## What is included

- 78 technology topics with honest `Outline`, `In progress`, or `Complete` status labels
- Three gold-standard complete guides: ASP.NET Core, Entity Framework Core, and Dapper, each covering decisions, implementation, testing, security, performance, deployment, troubleshooting, and production readiness
- Nine GitHub samples, including dedicated runnable ASP.NET Core, EF Core, and Dapper implementations with focused tests
- Light and dark themes with system detection and persisted manual selection
- Responsive sticky navigation, collapsible mobile sidebar, global search, and keyboard shortcuts
- Animated, accessible technology blob cloud
- Reusable technology cards, GitHub sample cards, callouts, highlighted code blocks, FAQ items, breadcrumbs, table of contents, and previous/next links
- SEO metadata, Open Graph metadata, sitemap, robots file, favicon, reduced-motion support, lazy reveal effects, and print styles
- Real starter repository links sourced from the owner's public GitHub profile

## Run locally

Browsers block `fetch()` for local JSON when a page is opened directly with `file://`. Run any small static server from this folder instead.

```powershell
cd D:\path\to\dotnet-technology-portal
py -m http.server 8080
```

Open `http://127.0.0.1:8080/`.

If Python is unavailable, Node can serve the folder without changing the project:

```powershell
npx serve .
```

## Deploy to GitHub Pages

1. Create the user-site repository `adhakshinamoorthy.github.io` and push this folder to its `main` branch.
2. In **Settings → Pages**, choose **Deploy from a branch**.
3. Select `main` and `/ (root)`, then save.
4. The expected site URL is `https://adhakshinamoorthy.github.io/`.

All asset and page references are relative. If the repository owner changes, update the canonical URLs in `index.html`, `robots.txt`, and the generator's `BaseUrl`.

## Replace GitHub samples

Edit [`data/github-samples.json`](data/github-samples.json). Each card uses this shape:

```json
{
  "id": "stable-sample-id",
  "name": "Repository display name",
  "description": "What the project teaches",
  "technologies": [".NET", "Azure"],
  "tags": ["architecture", "sample"],
  "githubUrl": "https://github.com/owner/repository",
  "liveDemoUrl": "https://optional-demo.example"
}
```

Set `liveDemoUrl` to `null` to hide the demo button. Use a sample's `id` in a technology's `sampleId` field to connect it to an article.

A topic marked `complete` must reference a unique sample with matching `topicSlug`, `status: "complete"`, a local README path, and documented run and test commands. Verification fails when this contract is broken. Related flagship repositories may still appear on outline guides, but they do not qualify a guide as complete.

## Content completion contract

Outline topics remain searchable while their full learning path is being built. A guide can be marked `complete` only when it includes:

- Learning objectives and prerequisites
- When-to-use and when-not-to-use guidance
- At least six core concepts
- Architecture and an implementation walkthrough
- Runnable code connected to a dedicated sample
- Testing, security, performance, and deployment guidance
- Troubleshooting scenarios and a production checklist
- At least six best practices and five interview questions
- A realistic reading time of at least 20 minutes
- A unique verified sample with run and test commands

The contract is enforced by `scripts/verify.mjs`; completeness is not based on page count or word count alone.

## Add or edit technologies

1. Add a complete entry to [`data/technologies.json`](data/technologies.json).
2. Use a unique URL-safe `slug`, a category, color, searchable keywords, and the article content fields shown by existing entries.
3. Regenerate the SEO-addressable page shells and sitemap:

```powershell
./scripts/generate-topic-pages.ps1
```

To publish under a different URL:

```powershell
./scripts/generate-topic-pages.ps1 -BaseUrl "https://owner.github.io"
```

The shared renderers live in [`assets/js/components.js`](assets/js/components.js), the landing page behavior in [`assets/js/home.js`](assets/js/home.js), and article composition in [`assets/js/topic.js`](assets/js/topic.js). Reusable HTML templates are in [`components/templates.html`](components/templates.html).

## Keyboard and accessibility

- Press `/` outside a form field to focus global search.
- Press `Esc` to close search results.
- Use `Tab`, `Enter`, and `Space` for navigation, blobs, theme controls, copy buttons, and FAQ items.
- A skip link appears on keyboard focus.
- Motion is minimized when the operating system requests reduced motion.
- Semantic landmarks, visible focus, readable contrast, and print styles are included.

## Project structure

```text
/
├── index.html
├── technologies/          # Generated SEO-addressable article shells
├── assets/
│   ├── css/styles.css
│   ├── js/
│   ├── images/
│   ├── blobs/
│   └── icons/
├── components/templates.html
├── data/
│   ├── technologies.json
│   └── github-samples.json
├── scripts/generate-topic-pages.ps1
├── sitemap.xml
├── robots.txt
└── README.md
```

## Design notes

The CSS uses semantic tokens for surfaces, text, borders, brand color, elevation, radius, and motion. This follows Fluent-inspired design principles while keeping the implementation framework-independent. No external fonts, analytics, trackers, or CDNs are used.

## License

Use and adapt the portal for your own learning content and portfolio.
