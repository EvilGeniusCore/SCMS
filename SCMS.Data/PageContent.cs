using System;

namespace SCMS.Data
{
    public class PageContent
    {
        public int Id { get; set; }
        public string PageKey { get; set; } = ""; // e.g. "about", "contact"
        public string Title { get; set; } = "";
        public string HtmlContent { get; set; } = "";
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string Visibility { get; set; } = "Public"; // or "MembersOnly"
        public string TemplateKey { get; set; } = "Display"; // NEW: e.g. Display, Poster
        public DateTime LastUpdated { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
