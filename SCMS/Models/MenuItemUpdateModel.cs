namespace SCMS.Models
{
    public class MenuItemUpdateModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Url { get; set; }
        public bool IsExternal { get; set; }
        public bool IsVisible { get; set; }
        public int SecurityLevelId { get; set; }
        public string? HtmlContent { get; set; }
        public string? PageTitle { get; set; }
        public string? MetaDescription { get; set; }
        public List<string>? MetaKeywords { get; set; }
    }

    public class GroupNameModel
    {
        public string Name { get; set; } = "";
    }

    public class GroupRenameModel
    {
        public string OldName { get; set; } = "";
        public string NewName { get; set; } = "";
    }

    public class CreateItemModel
    {
        public string Title { get; set; } = "";
        public string Group { get; set; } = "";
        public int? ParentId { get; set; }
        public int? InsertAfterId { get; set; }
    }

    public class ParentUpdateModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
    }

    public class ReorderItem
    {
        public int Id { get; set; }
        public int Order { get; set; }
    }
}
