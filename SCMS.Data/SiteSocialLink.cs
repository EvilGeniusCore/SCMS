using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCMS.Data
{
    public class SiteSocialLink
    {
        public int Id { get; set; }
        public string Url { get; set; } = "";
        public string? IconColor { get; set; }
        public int SiteSettingsId { get; set; }
        public SiteSettings SiteSettings { get; set; } = default!;
        public int SocialMediaPlatformId { get; set; }
        public SocialMediaPlatform Platform { get; set; } = default!;
    }
}
