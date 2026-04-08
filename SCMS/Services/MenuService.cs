using Microsoft.EntityFrameworkCore;
using SCMS.Constants;
using SCMS.Data;
using SCMS.Models;

namespace SCMS.Services
{
    public class MenuService
    {
        private readonly ApplicationDbContext _context;

        public MenuService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MenuItem>> GetAllMenuItemsWithIncludesAsync()
        {
            return await _context.MenuItems
                .Include(m => m.PageContent)
                .Include(m => m.SecurityLevel)
                .ToListAsync();
        }

        public async Task<MenuItem?> GetMenuItemAsync(int id)
        {
            return await _context.MenuItems
                .Include(m => m.PageContent)
                .Include(m => m.SecurityLevel)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MenuItem?> GetMenuItemWithPageAsync(int id)
        {
            return await _context.MenuItems
                .Include(m => m.PageContent)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<MenuItem>> GetGroupItemsAsync(string groupName)
        {
            return await _context.MenuItems
                .Where(m => m.MenuGroup == groupName && m.Title != "Admin")
                .OrderBy(m => m.Order)
                .ToListAsync();
        }

        public async Task<List<object>> GetGroupStructureAsync(string groupName)
        {
            var items = await _context.MenuItems
                .Where(m => m.MenuGroup == groupName)
                .OrderBy(m => m.Order)
                .Select(m => new { m.Id, m.Title, m.Order, m.ParentId })
                .ToListAsync();

            var flattened = new List<object>();

            void AddWithChildren(int? parentId)
            {
                var children = items.Where(i => i.ParentId == parentId).OrderBy(i => i.Order).ToList();
                foreach (var child in children)
                {
                    flattened.Add(child);
                    AddWithChildren(child.Id);
                }
            }

            AddWithChildren(null);
            return flattened;
        }

        public async Task CreateGroupAsync(string groupName)
        {
            var page = new PageContent
            {
                Title = "New Page",
                HtmlContent = "<p>New page content.</p>",
                PageKey = Slugify(groupName) + "-root"
            };

            var newItem = new MenuItem
            {
                Title = "New Menu Item",
                MenuGroup = groupName,
                Order = 0,
                IsVisible = true,
                SecurityLevelId = 3,
                PageContent = page
            };

            _context.PageContents.Add(page);
            _context.MenuItems.Add(newItem);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RenameGroupAsync(string oldName, string newName)
        {
            var items = await _context.MenuItems
                .Where(m => m.MenuGroup == oldName)
                .ToListAsync();

            if (!items.Any()) return false;

            foreach (var item in items)
                item.MenuGroup = newName;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteGroupAsync(string groupName)
        {
            var items = await _context.MenuItems
                .Where(m => m.MenuGroup == groupName)
                .ToListAsync();

            if (!items.Any()) return false;

            var pageIds = items
                .Where(i => i.PageContentId.HasValue)
                .Select(i => i.PageContentId!.Value)
                .ToList();

            var pages = await _context.PageContents
                .Where(p => pageIds.Contains(p.Id))
                .ToListAsync();

            _context.PageContents.RemoveRange(pages);
            _context.MenuItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CreateMenuItemAsync(CreateItemModel model)
        {
            var siblings = await _context.MenuItems
                .Where(m => m.MenuGroup == model.Group && m.ParentId == model.ParentId)
                .OrderBy(m => m.Order)
                .ToListAsync();

            int insertIndex = siblings.FindIndex(i => i.Id == model.InsertAfterId);
            if (insertIndex == -1) insertIndex = siblings.Count;

            for (int i = insertIndex + 1; i < siblings.Count; i++)
                siblings[i].Order = i + 1;

            var newItem = new MenuItem
            {
                Title = model.Title,
                MenuGroup = model.Group,
                ParentId = model.ParentId,
                Order = insertIndex + 1,
                IsVisible = true,
                SecurityLevelId = 3,
                PageContent = new PageContent
                {
                    Title = model.Title,
                    HtmlContent = "<p>New content</p>",
                    PageKey = Guid.NewGuid().ToString("N")
                }
            };

            _context.PageContents.Add(newItem.PageContent);
            _context.MenuItems.Add(newItem);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            var item = await _context.MenuItems
                .Include(m => m.PageContent)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null) return false;

            if (item.PageContent != null)
                _context.PageContents.Remove(item.PageContent);

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SetParentAsync(int id, int? parentId)
        {
            var item = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return;

            item.ParentId = parentId;
            await ReindexSiblingsAsync(item.MenuGroup, item.ParentId);
            await _context.SaveChangesAsync();
        }

        public async Task ReorderAsync(List<ReorderItem> items)
        {
            foreach (var entry in items)
            {
                var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == entry.Id);
                if (menuItem != null)
                {
                    if (menuItem.Id == MenuDefaults.AdminMenuItemId && menuItem.Title == "Admin")
                        menuItem.Order = MenuDefaults.AdminMenuOrder;
                    else
                        menuItem.Order = entry.Order;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task ReindexSiblingsAsync(string menuGroup, int? parentId)
        {
            var siblings = await _context.MenuItems
                .Where(m => m.MenuGroup == menuGroup && m.ParentId == parentId && m.Id != MenuDefaults.AdminMenuItemId)
                .OrderBy(m => m.Order)
                .ToListAsync();

            for (int i = 0; i < siblings.Count; i++)
                siblings[i].Order = i;

            var admin = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == MenuDefaults.AdminMenuItemId);
            if (admin != null)
                admin.Order = MenuDefaults.AdminMenuOrder;

            await _context.SaveChangesAsync();
        }

        public HashSet<int> GetAdminNodeIds(List<MenuItem> menuItems)
        {
            var adminRoot = menuItems.FirstOrDefault(m => m.Title == "Admin");
            var adminIds = new HashSet<int>();

            if (adminRoot != null)
            {
                void CollectDescendants(int parentId)
                {
                    var children = menuItems.Where(m => m.ParentId == parentId).ToList();
                    foreach (var child in children)
                    {
                        if (adminIds.Add(child.Id))
                            CollectDescendants(child.Id);
                    }
                }

                adminIds.Add(adminRoot.Id);
                CollectDescendants(adminRoot.Id);
            }

            return adminIds;
        }

        private static string Slugify(string input)
        {
            return input
                .ToLowerInvariant()
                .Trim()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace("/", "")
                .Replace("\\", "")
                .Replace(":", "")
                .Replace("?", "")
                .Replace("&", "")
                .Replace("#", "")
                .Replace("--", "-");
        }
    }
}
