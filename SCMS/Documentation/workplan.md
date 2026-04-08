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

**Status: NOT STARTED**

### 5.1 Add Unit Test Project
- [ ] Create `SCMS.Tests` xUnit project in the solution
- [ ] Add project reference to SCMS and SCMS.Data

### 5.2 Priority Test Targets
- [ ] **MenuBuilder** — menu generation, hierarchy, security filtering
- [ ] **ThemeEngine** — token replacement, missing token handling, partial injection
- [ ] **TemplateParser** — each/if blocks, recursion limits, edge cases
- [ ] **PageService** — slug resolution, missing pages
- [ ] **MenuService** — CRUD operations, reindexing, group management
- [ ] **PageContentService** — save with sanitization, image path migration

### 5.3 Integration Tests
- [ ] Page rendering end-to-end (request -> theme -> content -> HTML)
- [ ] Auth flow (login, role-based menu filtering, protected media access)
- [ ] Admin CRUD operations (create page, edit, delete, menu reorder)

## Phase 6: Future Enhancements (Lower Priority)

These are not bugs or debt — they're feature gaps to address when the core is solid.

- [x] ~~Migrate to .NET 10~~ (completed in Phase 0)
- [ ] Content versioning / draft-publish workflow
- [ ] SEO basics: sitemap.xml, robots.txt, meta tag management per page
- [ ] CI/CD pipeline (GitHub Actions: build, test, Docker image publish)
- [ ] Plugin/extension system (referenced in README but not implemented)
- [ ] Internationalization (i18n) support
- [ ] Image optimization and responsive image serving
- [ ] Temp upload folder cleanup (periodic job or on-save sweep)
- [ ] Nested menu depth: current hard limit on rendering depth needs to be configurable or removed

## Build Status

**Last build: 0 errors, 16 warnings** (all pre-existing nullable reference warnings in scaffolded Identity pages, Razor views, and TemplateParser)

## Notes

- Phases 1-4 are complete. The codebase is significantly cleaner and more secure.
- Phase 5 (testing) should be the next priority — the new services (MenuService, PageContentService, ThemeEngine) are all injectable and testable.
- Phase 6 is roadmap material, not debt.
- Warning count dropped from 29 (original .NET 8 build) to 16 after refactoring.
