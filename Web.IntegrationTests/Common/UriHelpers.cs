using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.IntegrationTests.Common;
internal class UriHelpers
{
    public static Dictionary<string, string> GetQueryParams(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
        {
            uri = new(new Uri("http://localhost"), uri.ToString());
        }
        return uri.Query.TrimStart('?').Split('&').Select(qp => qp.Split('=')).ToDictionary(qp => qp[0], qp => qp[1]);
    }
}
