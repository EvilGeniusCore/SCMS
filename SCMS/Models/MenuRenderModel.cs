namespace SCMS.Models
{
    public class MenuRenderModel
    {
        public List<MenuItemModel> Items { get; set; } = new();
    }

    public class MenuItemModel
    {
        public string Text { get; set; } = "";
        public string Url { get; set; } = "";
        public string? Target { get; set; }
        public List<MenuItemModel> Items { get; set; } = new();
        public bool HasChildren => Items.Count > 0;
        public int Depth { get; set; }
        /// <summary>
        /// Pre-rendered HTML for all nested children (recursive).
        /// Use {{SubMenuHtml}} in templates to render the full subtree.
        /// </summary>
        public string SubMenuHtml { get; set; } = "";
    }
}
