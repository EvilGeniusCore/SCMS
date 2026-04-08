using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCMS.Data
{
    public class ThemeSetting
    {
        public int Id { get; set; }
        public string Name { get; set; } = ""; // Folder name
        public string DisplayName { get; set; } = "";
        public string? Description { get; set; }
        public string? PreviewImage { get; set; }
        public string? Favicon { get; set; } = "favicon.ico"; // Relative path to the favicon file
        public DateTime SetOn { get; set; }
    }

}
