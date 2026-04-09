# SCMS Work Plan

Open items and roadmap. Completed work is tracked in [workplan_completed.md](workplan_completed.md).

## Build Status

**Last build: 0 errors, 0 warnings**
**Test suite: 75 tests, all passing**

## Open Items

| # | Item | Priority | Notes |
|---|------|----------|-------|
| 1 | Plugin/extension system | Top | Module loader architecture — prerequisite for most modules below |
| 2 | Serilog logging | High | Replace built-in logging with Serilog, file sink with size-based rolling |
| 3 | Notices module | High | Sticky site notices, auto-expire, user-dismissable |
| 4 | Comment module | High | Threaded comments, votes, emoji reactions |
| 5 | Feedback module | High | Lightweight feedback/suggestion collection |
| 6 | Donations module | High | Ko-fi, Patreon, PayPal integration |
| 7 | Content versioning / draft-publish workflow | Medium | Page history, revert, draft vs published state |
| 8 | Temp upload folder cleanup | Medium | Periodic job or sweep on page save for orphaned temp files |
| 9 | External authentication | Medium | OAuth, SAML, OpenID Connect providers |
| 10 | 2FA module | Medium | TOTP / Passkeys for user accounts |
| 11 | Forms module | Medium | Custom form builder, submissions, admin review |
| 12 | Captcha module | Medium | reCAPTCHA / hCaptcha integration for forms and auth |
| 13 | Survey module | Medium | Polls and surveys with analytics |
| 14 | Messages module | Medium | Direct messages, @username mentions |
| 15 | Forums module | Medium | Threaded discussion with voting, sticky posts |
| 16 | Bookmarks/Links module | Medium | Public/private link collections, categories, browser import |
| 17 | Internationalization (i18n) | Low | Multi-language content and UI localization |
| 18 | Image optimization and responsive images | Low | Resize on upload, srcset generation, lazy loading |
| 19 | Storefront integration | Low | E-commerce foundation |
| 20 | Shopping cart and product modules | Low | Product catalog, cart, checkout — depends on storefront |
| 21 | Maps module | Low | Interactive maps via `<scms:Map />` token |
| 22 | Ratings module | ? | Content ratings and voting |
| 23 | Module store/registry | ? | Versioning, discovery, and upgrade system for modules — High if tied to plugin system, discuss |

## #1 — Plugin/Extension System

**Goal:** Allow modules to register CMS tokens, admin pages, DB migrations, and services without modifying core SCMS code.

### The Problem Today

ThemeEngine.RenderAsync() is a 200+ line monolith. Every `<cms:*>` token is a hardcoded string replace or regex. Adding a new token (e.g. `<cms:Comments />`) means editing ThemeEngine directly. There's no way for an external module to participate in the render pipeline, register admin routes, or add its own database tables.

### Architecture Concept

```
SCMS.Core (host app)
│
├── IModule                     ← interface every module implements
│   ├── Name, Version
│   ├── ConfigureServices(IServiceCollection)   ← register DI
│   ├── ConfigureDatabase(ModelBuilder)          ← add entity mappings
│   └── GetTokenHandlers()                       ← return token processors
│
├── ITokenHandler               ← interface for token rendering
│   ├── TokenPattern (regex)
│   └── RenderAsync(Match, HttpContext, DbContext) → string
│
├── ModuleLoader                ← discovers and loads IModule implementations
│   ├── Scans /Modules folder for assemblies
│   ├── Calls ConfigureServices at startup
│   └── Calls ConfigureDatabase during migration
│
└── ThemeEngine (refactored)
    ├── Built-in token handlers (PageTitle, Menu, SiteName, etc.)
    └── Loops through registered ITokenHandler list for custom tokens
```

### Key Design Decisions

**Module discovery:** Modules ship as a .NET class library DLL dropped into a `/Modules` folder. The ModuleLoader scans for types implementing `IModule` at startup. No NuGet dependency — just copy a DLL.

**Token registration:** Each module returns a list of `ITokenHandler` instances. ThemeEngine collects all handlers at startup and runs them in order during render. Built-in tokens become handlers too (refactor the monolith).

**Database:** Each module can define its own EF entities and supply `IEntityTypeConfiguration<T>` instances. A module migration runner applies them separately from the core migration. SQLite supports this since it's just adding tables.

**Admin pages:** Modules can register admin routes/controllers. The admin menu auto-discovers module admin pages via a `GetAdminMenuItems()` method on `IModule`.

**No module-to-module dependencies for v1.** Keep it simple — each module talks to core only.

### Implementation Phases

#### Phase A: Core Interfaces — COMPLETE
- [x] Created `SCMS.Abstractions` project with `IModule`, `ITokenHandler`, `TokenRenderContext`, `ModuleAdminMenuItem`
- [x] `ITokenHandler`: Name, TokenPattern (regex) or SimpleToken (string), Priority, RenderAsync
- [x] `IModule`: Name, Version, Description, ConfigureServices, GetTokenHandlers, GetAdminMenuItems
- [x] Referenced from SCMS and SCMS.Tests projects

#### Phase B: Refactor ThemeEngine into Token Handlers — COMPLETE
- [x] Extracted 15 built-in tokens into individual `ITokenHandler` classes in `Services/TokenHandlers/`
- [x] PageTitle, Content, Favicon, MetaTags, Copyright, Tagline, LoginStatus, UserName, ErrorMessage, SiteName, SiteLogo, Menu, Antiforgery, SocialLinks, Breadcrumb
- [x] ThemeEngine.RenderAsync() now: load layout/partials → build TokenRenderContext → loop handlers by Priority → catch unknowns
- [x] All handlers registered in DI as `IEnumerable<ITokenHandler>`
- [x] Async regex replace helper for regex-based handlers
- [x] 75 tests still passing — behaviour unchanged

#### Phase C: Module Loader — COMPLETE
- [x] Created `ModuleLoader` static service — scans `/Modules` folder for DLLs at startup
- [x] Loads assemblies via isolated `ModuleLoadContext` (prevents dependency conflicts)
- [x] Finds `IModule` implementations, calls `ConfigureServices`, collects token handlers
- [x] Registers module token handlers into DI as `IEnumerable<ITokenHandler>`
- [x] Wired into Program.cs before `builder.Build()` with startup logging

#### Phase D: Module Database Support — COMPLETE
- [x] Created `IModuleDbSetup` interface — optional, modules implement to create their own tables
- [x] Added `GetDbSetup()` default method to `IModule` (returns null if no DB needed)
- [x] `ModuleLoader.RunModuleDbSetupAsync()` runs all module setups after core migrations
- [x] Wired into Program.cs startup sequence

#### Phase E: Admin Integration — COMPLETE
- [x] `ModuleAdminMenuTokenHandler` renders `<cms:ModuleAdminMenu />` from loaded module menu items
- [x] Only visible to Administrator role
- [x] `/admin/modules` page lists all loaded modules with name, version, description, admin links
- [x] Module admin pages rendered via ThemeEngine pipeline

### Example: What a Module Looks Like

```csharp
public class NoticesModule : IModule
{
    public string Name => "Notices";
    public string Version => "1.0.0";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<NoticeService>();
    }

    public IEnumerable<ITokenHandler> GetTokenHandlers()
    {
        yield return new NoticeTokenHandler();
    }

    public IEnumerable<ModuleAdminMenuItem> GetAdminMenuItems()
    {
        yield return new("Notices", "/admin/notices", "fas fa-bell");
    }
}

public class NoticeTokenHandler : ITokenHandler
{
    public Regex TokenPattern => new(@"<cms:Notices\s*\/>", RegexOptions.IgnoreCase);

    public async Task<string> RenderAsync(Match match, HttpContext context, ApplicationDbContext db)
    {
        var notices = await db.Set<Notice>()
            .Where(n => n.IsActive && n.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        return string.Join("", notices.Select(n =>
            $"<div class='alert alert-info alert-dismissible'>{n.Message}</div>"));
    }
}
```

### Open Questions

- Should modules have their own DbContext or share one? Separate is cleaner but adds complexity.
- Should module DLLs be hot-reloadable or require app restart? Restart-only for v1 is safer.
- Should the module store/registry (#23) be part of this or a later layer on top?

### Dependencies

- None — this is foundational. Items #3-23 all depend on this.

### Estimated Scope

Phase A-B is the critical path — it refactors ThemeEngine and defines the contracts. That's where to start. Phases C-E build on top and can be done incrementally. The first real module (Notices, #3) would validate the architecture end to end.
