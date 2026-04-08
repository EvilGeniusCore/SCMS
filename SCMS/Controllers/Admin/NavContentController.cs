using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCMS.Data;
using SCMS.Constants;
using SCMS.Interfaces;
using SCMS.Models;
using SCMS.Services;

namespace SCMS.Controllers.Admin
{
    [Authorize(Roles = "Administrator")]
    [Route("admin/[controller]")]
    public class NavContentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RazorRenderer _razorRenderer;
        private readonly IThemeEngine _themeEngine;
        private readonly MenuService _menuService;
        private readonly PageContentService _pageContentService;

        public NavContentController(
            ApplicationDbContext context,
            RazorRenderer razorRenderer,
            IThemeEngine themeEngine,
            MenuService menuService,
            PageContentService pageContentService)
        {
            _context = context;
            _razorRenderer = razorRenderer;
            _themeEngine = themeEngine;
            _menuService = menuService;
            _pageContentService = pageContentService;
        }

        [HttpGet("/admin/navcontent")]
        public async Task<IActionResult> Index()
        {
            var menuItems = await _menuService.GetAllMenuItemsWithIncludesAsync();
            var adminIds = _menuService.GetAdminNodeIds(menuItems);

            var filteredItems = menuItems
                .Where(m => !adminIds.Contains(m.Id))
                .ToList();

            var model = new NavContentViewModel
            {
                Groups = filteredItems
                    .GroupBy(m => m.MenuGroup)
                    .Select(g => new MenuGroupView
                    {
                        GroupName = g.Key,
                        Items = g.OrderBy(m => m.Order).ToList()
                    })
                    .ToList()
            };

            var html = await _razorRenderer.RenderViewAsync<NavContentViewModel>(
                HttpContext,
                "/Views/Admin/NavContent/Index.cshtml",
                model,
                new Dictionary<string, object>()
            );

            var wrapped = await _themeEngine.RenderAsync(new PageContent
            {
                Title = "Menu/Page Editor",
                HtmlContent = html
            }, _context);

            var result = wrapped;
            var isAdminPage = HttpContext.Request.Path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase);

            // Font Awesome
            if (isAdminPage && !result.Contains("font-awesome/6.5.1/css/all.min.css"))
            {
                result = result.Replace("</head>", @"
                <link href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css"" rel=""stylesheet"">
                </head>");
            }

            // Bootstrap
            if (isAdminPage && !result.Contains("bootstrap@5.3.2/dist/css/bootstrap.min.css"))
            {
                result = result.Replace("</head>", @"
                <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css""
                      rel=""stylesheet""
                      integrity=""sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN""
                      crossorigin=""anonymous"">
                </head>");
            }

            //tinymce
            if (!result.Contains("/lib/tinymce/tinymce.min.js"))
            {
                result = result.Replace("</body>", @"
                <script src=""/lib/tinymce/tinymce.min.js""></script>
                </body>");
            }

            // TinyMCE Fullscreen css
            if (isAdminPage && !result.Contains("/css/tiny-full.css"))
            {
                result = result.Replace("</head>", @"
                <link href=""/css/tiny-full.css"" rel=""stylesheet"">
                </head>");
            }

            return Content(result, "text/html");
        }

        [HttpGet("load/{id}")]
        public async Task<IActionResult> Load(int id)
        {
            var item = await _menuService.GetMenuItemAsync(id);
            if (item == null) return NotFound();

            return Json(new
            {
                item.Id,
                item.Title,
                item.Url,
                item.IsVisible,
                item.SecurityLevelId,
                pageTitle = item.PageContent?.Title,
                item.PageContent?.HtmlContent,
                item.PageContent?.MetaDescription,
                MetaKeywords = item.PageContent?.MetaKeywords?.Split(',').Select(k => k.Trim()).ToList()
            });
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] MenuItemUpdateModel model)
        {
            var item = await _menuService.GetMenuItemWithPageAsync(model.Id);
            if (item == null) return NotFound();

            var success = await _pageContentService.SaveMenuItemContentAsync(model);
            if (!success) return NotFound();

            await _menuService.ReindexSiblingsAsync(item.MenuGroup, item.ParentId);
            return Ok();
        }

        [HttpPost("group/add")]
        public async Task<IActionResult> AddGroup([FromBody] GroupNameModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Group name required.");

            await _menuService.CreateGroupAsync(model.Name);
            return Ok();
        }

        [HttpPost("group/rename")]
        public async Task<IActionResult> RenameGroup([FromBody] GroupRenameModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewName) || string.IsNullOrWhiteSpace(model.OldName))
                return BadRequest("Both old and new group names are required.");

            var success = await _menuService.RenameGroupAsync(model.OldName, model.NewName);
            return success ? Ok() : NotFound("Group not found.");
        }

        [HttpPost("group/delete")]
        public async Task<IActionResult> DeleteGroup([FromBody] GroupNameModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Group name required.");

            if (model.Name.Equals(MenuDefaults.MainGroup, StringComparison.OrdinalIgnoreCase))
                return BadRequest("The 'Main' group cannot be deleted.");

            var success = await _menuService.DeleteGroupAsync(model.Name);
            return success ? Ok() : NotFound("Group not found.");
        }

        [HttpGet("group/items/{groupName}")]
        public async Task<IActionResult> LoadGroupItems(string groupName)
        {
            var items = await _menuService.GetGroupItemsAsync(groupName);
            return PartialView("_MenuTreePartial", items);
        }

        [HttpPost("item/create")]
        public async Task<IActionResult> CreateMenuItem([FromBody] CreateItemModel model)
        {
            await _menuService.CreateMenuItemAsync(model);
            return Ok();
        }

        [HttpPost("item/delete/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var success = await _menuService.DeleteMenuItemAsync(id);
            return success ? Ok() : NotFound();
        }

        [HttpGet("group/structure/{groupName}")]
        public async Task<IActionResult> GetGroupStructure(string groupName)
        {
            var structure = await _menuService.GetGroupStructureAsync(groupName);
            return Json(structure);
        }

        [HttpPost("item/set-parent")]
        public async Task<IActionResult> SetParent([FromBody] ParentUpdateModel model)
        {
            await _menuService.SetParentAsync(model.Id, model.ParentId);
            return Ok();
        }

        [HttpPost("item/reorder")]
        public async Task<IActionResult> Reorder([FromBody] List<ReorderItem> items)
        {
            await _menuService.ReorderAsync(items);
            return Ok();
        }
    }
}
