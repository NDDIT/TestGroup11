using System.Text.RegularExpressions;
using System.Web;

namespace IBook.Content.module
{
    public class HtmlToText
    {
        public static string convert(string html)
        {
            Regex regex = new Regex("<(?:\"[^\"]*\"|'[^']*'|[^'\">])*>");
            var htmlPlainText = regex.Replace(html, string.Empty);
            
            return HttpUtility.HtmlDecode(htmlPlainText);
        }
    }
}