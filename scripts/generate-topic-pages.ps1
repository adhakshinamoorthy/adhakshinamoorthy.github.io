[CmdletBinding()]
param(
    [string]$BaseUrl = "https://adhakshinamoorthy.github.io",
    [switch]$OnlyMissing
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$technologyPath = Join-Path $projectRoot "data\technologies.json"
$outputDirectory = Join-Path $projectRoot "technologies"
$technologies = Get-Content -Raw -Encoding UTF8 -LiteralPath $technologyPath | ConvertFrom-Json
$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)

New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

foreach ($technology in $technologies) {
    $name = [System.Net.WebUtility]::HtmlEncode([string]$technology.name)
    $description = [System.Net.WebUtility]::HtmlEncode([string]$technology.shortDescription)
    $slug = [string]$technology.slug
    $page = @"
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta name="description" content="$description">
  <meta name="author" content="Dhakshinamoorthy A">
  <meta name="theme-color" content="#f7f9fc" media="(prefers-color-scheme: light)">
  <meta name="theme-color" content="#111827" media="(prefers-color-scheme: dark)">
  <meta property="og:type" content="article">
  <meta property="og:title" content="$name &mdash; .NET Atlas">
  <meta property="og:description" content="$description">
  <meta property="og:image" content="$BaseUrl/assets/images/og-card.svg">
  <meta property="og:url" content="$BaseUrl/technologies/$slug.html">
  <meta name="twitter:card" content="summary_large_image">
  <title>$name &mdash; .NET Atlas</title>
  <link rel="canonical" href="$BaseUrl/technologies/$slug.html">
  <link rel="icon" href="../assets/icons/favicon.svg" type="image/svg+xml">
  <link rel="stylesheet" href="../assets/css/styles.css">
  <script>
    (() => {
      const saved = localStorage.getItem('dotnet-atlas-theme');
      const theme = saved || (matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
      document.documentElement.dataset.theme = theme;
    })();
  </script>
</head>
<body data-page="topic" data-topic="$slug" data-base="../">
  <a class="skip-link" href="#main-content">Skip to main content</a>
  <div id="site-navbar"></div>
  <div class="site-shell">
    <aside id="site-sidebar" class="sidebar" aria-label="Technology navigation"></aside>
    <main id="main-content" class="main-content" tabindex="-1">
      <div class="article-main">
        <article id="topic-article" class="article">
          <noscript><div class="error-panel"><h1>JavaScript is required</h1><p>This static portal uses JavaScript to assemble reusable article components from local JSON data.</p></div></noscript>
        </article>
        <aside id="article-toc" class="toc" aria-label="Table of contents"></aside>
      </div>
    </main>
  </div>
  <div id="site-footer"></div>
  <button id="back-to-top" class="back-to-top" type="button" aria-label="Back to top" title="Back to top">&uarr;</button>
  <script type="module" src="../assets/js/topic.js"></script>
</body>
</html>
"@
    $pagePath = Join-Path $outputDirectory "$slug.html"
    if (-not $OnlyMissing -or -not (Test-Path -LiteralPath $pagePath)) {
        [System.IO.File]::WriteAllText($pagePath, ($page + [Environment]::NewLine), $utf8WithoutBom)
    }
}

$urls = @("$BaseUrl/") + ($technologies | ForEach-Object { "$BaseUrl/technologies/$($_.slug).html" })
$urlEntries = $urls | ForEach-Object { "  <url><loc>$([System.Security.SecurityElement]::Escape($_))</loc></url>" }
$sitemap = @"
<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
$($urlEntries -join "`n")
</urlset>
"@
[System.IO.File]::WriteAllText((Join-Path $projectRoot "sitemap.xml"), ($sitemap + [Environment]::NewLine), $utf8WithoutBom)

Write-Host "Generated $($technologies.Count) topic pages and sitemap.xml"
