using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebResourceManager.Models
{
    public static class Constants
    {
        public static string[] ValidExtensions = new string[]
        {
            "js",
            "png",
            "ico",
            "gif",
            "jpg",
            "htm",
            "html",
            "css",
            "xsl",
            "xml",
            "xap",
            "svg",
            "ico",
            "resx",
            "otf",
            "eot",
            "ttf",
            "woff",
            "woff2"
        };

        public static Dictionary<WebResourceType, string> WebResourceExtensionMap = new Dictionary<WebResourceType, string>
        {
            { WebResourceType.CSS, "css" },
            { WebResourceType.GIF, "gif" },
            { WebResourceType.HTML, "html" },
            { WebResourceType.ICO, "ico" },
            { WebResourceType.JavaScript, "js" },
            { WebResourceType.JPG, "jpg" },
            { WebResourceType.PNG, "png" },
            { WebResourceType.Silverlight, "xap" },
            { WebResourceType.String, "resx" },
            { WebResourceType.Stylesheet_XSL, "xsl" },
            { WebResourceType.Vector, "svg" },
            { WebResourceType.XML, "xml" },
            { WebResourceType.Eot, "eot" },
            { WebResourceType.Otf, "otf" },
            { WebResourceType.Ttf, "ttf" },
            { WebResourceType.Woff, "woff" },
            { WebResourceType.Woff2, "woff2" }
        };

        public static string WebResourceLogicalName = "webresource";

        public static class WebResourceStatus
        {
            public const string New = "New";
            public const string Exists = "Exists";
            public const string NotInSolution = "Not in solution";
            public const string Changed = "Changed";
        }
    }
}
