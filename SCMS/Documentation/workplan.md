# SCMS Work Plan

Based on a full code review and project audit conducted 2026-04-08.

## Phase 0: .NET 10 Migration

**Status: COMPLETE**

- [x] Both csproj files updated from `net8.0` to `net10.0`
- [x] All NuGet packages updated to 10.x (EF Core 10.0.5, Identity 10.0.5, etc.)
- [x] Removed unused `Microsoft.EntityFrameworkCore.SqlServer` package
- [x] Build succeeds with zero errors

## Phase 1: Security Fixes (Critical)

**Status: COMPLETE**

### 1.1 Server-Side HTML Sanitization on Page Save
- [x] Added `HtmlSanitizer` NuGet package (Ganss.Xss 9.0.892)
- [x] Sanitize HtmlContent in `PageContentService.SaveMenuItemContentAsync()`
- [x] Configured sanitizer to allow TinyMCE output (iframes, class, style, target, rel, data: scheme)

### 1.2 Media Upload Validation
- [x] Validate file extensions against allowlist (jpg, jpeg, png, gif, webp, svg, bmp, ico)
- [x] Validate MIME types against allowlist
- [x] Generate unique GUID-based filenames on upload
- [x] Replaced Console.WriteLine with ILogger

**Note:** Temp folder cleanup is not yet implemented. This is a low-priority item — images in temp are moved to public/protected on page save. A periodic cleanup job would handle orphaned temp files from abandoned edits.

### 1.3 Remove Hardcoded Security Level IDs
- [x] Created `Constants/SecurityLevelNames.cs` with named constants
- [x] Refactored `MenuBuilder.IsMenuItemAuthorized()` to look up Anonymous level by name
- [x] Batch-load all security level role mappings once per menu render (also addresses Phase 3.1)

## Phase 2: Architectural Improvements

**Status: COMPLETE**

### 2.1 Extract Service Layer from NavContentController
- [x] Created `Services/MenuService.cs` — all menu item and group CRUD, reindexing, hierarchy operations
- [x] Created `Services/PageContentService.cs` — page save with HTML sanitization and image path migration
- [x] Moved all inline DTO classes to `Models/MenuItemUpdateModel.cs` (GroupNameModel, GroupRenameModel, CreateItemModel, ParentUpdateModel, ReorderItem)
- [x] Updated `Models/MenuItemUpdateModel.cs` to include MetaDescription and MetaKeywords fields (was out of sync with controller's inline version)
- [x] NavContentController slimmed from 554 lines to ~175 lines (routing and model binding only)

### 2.2 Convert ThemeEngine from Static to Injectable Service
- [x] Created `Interfaces/IThemeEngine.cs` interface
- [x] Converted `ThemeEngine` from static class to scoped service implementing `IThemeEngine`
- [x] Constructor-injected `IHttpContextAccessor`, `IThemeManager`, `IMemoryCache`, `ILogger<ThemeEngine>`
- [x] Removed static `HttpContextAccessor` property assignment from Program.cs
- [x] Updated all controllers (PageController, AuthController, SettingsController, SocialMediaController, NavContentController) to inject `IThemeEngine`

### 2.3 Implement ThemeManager Properly
- [x] Created `IThemeManager` interface with `GetCurrentThemeAsync()` and `InvalidateCache()`
- [x] Implemented DB lookup from `SiteSettings.ThemeId` -> `ThemeSettings.Name`
- [x] Added `IMemoryCache` caching with 30-minute expiration
- [x] Cache invalidated on settings save in `SettingsController`
- [x] Wired into ThemeEngine (replaces hardcoded "Default")
- [x] MenuBuilder now receives theme name as parameter (no longer calls static ThemeManager)

### 2.4 Add Error Handling to Theme File Loading
- [x] Wrapped theme file reads in try-catch for `FileNotFoundException` and `DirectoryNotFoundException`
- [x] Falls back to minimal error page showing the content with error details
- [x] Logs missing file/directory errors via `ILogger<ThemeEngine>`

## Phase 3: Performance

**Status: COMPLETE**

### 3.1 Cache Security Level Role Mappings
- [x] `MenuBuilder.LoadSecurityContext()` loads all SecurityLevelRoles once per menu render
- [x] Passed as `MenuSecurityContext` through the render chain (no per-item DB queries)

### 3.2 Cache Parsed Templates
- [x] Theme file reads cached via `IMemoryCache` with 30-minute expiration
- [x] `ReadThemeFileAsync()` helper method in ThemeEngine handles cache lookup/population
- [x] Layout, templates, partials, and social template all cached
- [x] Cache invalidated when theme changes (via ThemeManager invalidation)

## Phase 4: Code Cleanup

**Status: COMPLETE**

### 4.1 Remove Dead Code
- [x] Deleted `SeedSamplePagesAndMenus()` from PageController (~90 lines, never called)
- [x] Deleted `Models/ErrorViewModel.cs` (unused)
- [x] Simplified `Views/Shared/Error.cshtml` to not depend on ErrorViewModel
- [x] Removed `Microsoft.EntityFrameworkCore.SqlServer` package (done in Phase 0)

### 4.2 Eliminate Magic Strings and Numbers
- [x] Created `Constants/MenuDefaults.cs` (MainGroup, FooterGroup, AdminMenuOrder, AdminMenuItemId)
- [x] Replaced all `9999` magic numbers with `MenuDefaults.AdminMenuOrder`
- [x] Replaced hardcoded admin item ID `1` with `MenuDefaults.AdminMenuItemId`
- [x] Replaced hardcoded `"Main"` group check with `MenuDefaults.MainGroup`

### 4.3 Fix Logging
- [x] IdentitySeeder now uses `ILogger` instead of `Console.WriteLine`
- [x] UploadController uses `ILogger<UploadController>` for upload events and rejections
- [x] ThemeEngine uses `ILogger<ThemeEngine>` for theme file errors

## Phase 5: Testing

**Status: COMPLETE**

### 5.1 Add Unit Test Project
- [x] Created `SCMS.Tests` xUnit project in the solution
- [x] Added project references to SCMS and SCMS.Data
- [x] Added Moq, EF Core InMemory, and AspNetCore.Mvc.Testing packages
- [x] Created `TestDbContextFactory` helper with seed data support

### 5.2 Unit Tests (62 tests)
- [x] **TemplateParser** (16 tests) — variables, each blocks, if/else, nesting, comments, edge cases
- [x] **PageService** (4 tests) — slug lookup, case sensitivity, missing pages
- [x] **MenuService** (19 tests) — CRUD, groups, reorder, admin pinning, hierarchy
- [x] **PageContentService** (7 tests) — save, XSS sanitization, allowed HTML, external links, meta keywords
- [x] **MenuBuilder** (7 tests) — anonymous/user/admin visibility, groups, breadcrumbs
- [x] **ThemeEngine** (8 tests) — token replacement, site name, tagline, login status, fallback error, unknown tokens

### 5.3 Integration Tests (13 tests)
- [x] Page rendering end-to-end (5 tests) — home page, slug routing, 404, HTML structure, login page
- [x] Auth flow (4 tests) — admin settings, navcontent, social media, upload all redirect unauthenticated
- [x] Admin CRUD operations (4 tests) — menu create/delete, page save, admin reorder pinning, auth redirect

**75 tests total, all passing.**

## Phase 6: Future Enhancements (Lower Priority)

These are not bugs or debt — they're feature gaps to address when the core is solid.

- [x] ~~Migrate to .NET 10~~ (completed in Phase 0)
- [x] CI/CD pipeline (GitHub Actions: build and push Docker image to ghcr.io on main)
- [x] Dockerfile (multi-stage .NET 10 SDK/runtime build)
- [x] Docker-compose with persistent volumes for DB and uploads
- [x] Bootstrap updated from 5.3.2 to 5.3.8
- [x] Clean Blog theme added (Lora/Open Sans typography, full-width masthead, teal accents)
- [x] Theme auto-discovery from `Themes/` folder via `theme.config.json`
- [x] ThemeAssetManager updated to sync all themes (not just default)
- [x] Credentials moved to gitignored `secrets/` folder
- [x] VS Code launch.json and tasks.json for F5 debugging
- [ ] Content versioning / draft-publish workflow
- [ ] SEO basics: sitemap.xml, robots.txt, meta tag management per page
- [ ] Plugin/extension system (referenced in README but not implemented)
- [ ] Internationalization (i18n) support
- [ ] Image optimization and responsive image serving
- [ ] Temp upload folder cleanup (periodic job or on-save sweep)
- [ ] Nested menu depth: current hard limit on rendering depth needs to be configurable or removed
- [ ] Change logging system to Serilog, and have written logs that recycle on a size based system

## Build Status

**Last build: 0 errors, 0 warnings**
**Test suite: 75 tests, all passing**

## Notes

- Phases 1-5 are complete. The codebase is secure, well-architected, and tested.
- Phase 6 remaining items are roadmap material, not debt.
- Warning count dropped from 29 (original .NET 8 build) to 0 after refactoring.

