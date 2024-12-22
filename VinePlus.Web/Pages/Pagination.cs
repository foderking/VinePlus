using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VinePlus.Web.Pages;

public abstract class Pagination<T>: PageModel
{
    public IEnumerable<T> Entities = Enumerable.Empty<T>();
    public Nav NavRecord = new(0, 0, "");
    public abstract Func<string, int, string> PageDelegate();
}