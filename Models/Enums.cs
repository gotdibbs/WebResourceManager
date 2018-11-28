using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebResourceManager.Models
{
    public enum WebResourceType
    {
        HTML = 1,
        CSS = 2,
        JavaScript = 3,
        XML = 4,
        PNG = 5,
        JPG = 6,
        GIF = 7,
        Silverlight = 8,
        Stylesheet_XSL = 9,
        ICO = 10,
        Vector = 11,
        String = 12
    }

    public enum WebResourceStatus
    {
        New = 1,
        Changed = 2,
        Exists = 3,
        NotInSolution = 4
    }

    public enum ComponentType
    {
        WebResource = 61
    }
}
