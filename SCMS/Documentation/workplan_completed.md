# SCMS Work Plan — Completed Items

Items completed during the 2026-04-08 audit and refactoring session.

## Phase 0: .NET 10 Migration

- [x] Both csproj files updated from `net8.0` to `net10.0`
- [x] All NuGet packages updated to 10.x (EF Core 10.0.5, Identity 10.0.5, etc.)
- [x] Removed unused `Microsoft.EntityFrameworkCore.SqlServer` package
- [x] Build succeeds with zero errors
- [x] Fixed EF Core 10 seed data: static DateTime values, hardcoded IdentityRole IDs/ConcurrencyStamps
- [x] Updated EF tools from 8.0.2 to 10.0.5
- [x] Regenerated migration with EF 10 tooling

## Phase 1: Security Fixes

- [x] Added `HtmlSanitizer` NuGet package — sanitize all HTML on page save
- [x] Configured sanitizer to allow TinyMCE output (iframes, class, style, target, rel, data: scheme)
- [x] Media upload validation: file extension allowlist, MIME type check, GUID-based filenames
- [x] Replaced Console.WriteLine with ILogger in UploadController
- [x] Created `Constants/SecurityLevelNames.cs` — replaced hardcoded security level IDs with name-based lookups
- [x] Batch-load all security level role mappings once per menu render

## Phase 2: Architectural Improvements

- [x] Created `Services/MenuService.cs` — menu item and group CRUD, reindexing, hierarchy
- [x] Created `Services/PageContentService.cs` — page save with sanitization and image path migration
- [x] Moved inline DTO classes from NavContentController to `Models/MenuItemUpdateModel.cs`
- [x] NavContentController slimmed from 554 lines to ~175 lines
- [x] Created `IThemeEngine` interface, converted ThemeEngine from static to scoped DI service
- [x] All controllers updated to inject `IThemeEngine`
- [x] Implemented `IThemeManager` with DB lookup and IMemoryCache (30-min expiration)
- [x] Cache invalidated on settings save
- [x] MenuBuilder receives theme name as parameter
- [x] Theme file loading wrapped in try-catch with fallback error page

## Phase 3: Performance

- [x] `MenuBuilder.LoadSecurityContext()` loads all SecurityLevelRoles once per render
- [x] Theme file reads cached via `IMemoryCache` (30-min expiration)
- [x] Layout, templates, partials, and social template all cached

## Phase 4: Code Cleanup

- [x] Deleted dead code: `SeedSamplePagesAndMenus()`, `ErrorViewModel.cs`, duplicate Login.cshtml.cs block
- [x] Simplified `Error.cshtml` to not depend on ErrorViewModel
- [x] Created `Constants/MenuDefaults.cs` — replaced magic `9999`, admin ID `1`, `"Main"` group string
- [x] IdentitySeeder, UploadController, ThemeEngine all use ILogger
- [x] Development logging level set to Information
- [x] Fixed all nullable reference warnings (CS8618, CS8602, CS8603, CS8604)
- [x] Suppressed migration CS8981 warnings

## Phase 5: Testing (75 tests)

- [x] `SCMS.Tests` xUnit project with Moq, EF Core InMemory, Mvc.Testing
- [x] `TestDbContextFactory` helper with HasData seed support
- [x] TemplateParser (16 tests), PageService (4), MenuService (19), PageContentService (7), MenuBuilder (7), ThemeEngine (8)
- [x] Integration: page rendering (5), auth redirects (4), admin CRUD (4)

## Phase 6: Completed Enhancements

- [x] CI/CD pipeline (GitHub Actions: build and push Docker image to ghcr.io)
- [x] Dockerfile (multi-stage .NET 10 SDK/runtime build)
- [x] Docker-compose with persistent volumes for DB and uploads
- [x] Bootstrap updated from 5.3.2 to 5.3.8
- [x] Clean Blog theme (Lora/Open Sans, full-width masthead, teal accents)
- [x] Theme auto-discovery from `Themes/` folder via `theme.config.json`
- [x] ThemeAssetManager syncs all themes (not just default)
- [x] Credentials moved to gitignored `secrets/` folder
- [x] VS Code launch.json and tasks.json for F5 debugging
- [x] SEO: sitemap.xml, robots.txt, `<cms:MetaTags />` for description/keywords
- [x] Nested menu depth: unlimited via recursive `SubMenuHtml` + CSS multilevel dropdowns
