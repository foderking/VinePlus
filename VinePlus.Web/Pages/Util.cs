
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Comicvine.Core;
using VinePlus.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace VinePlus.Web.Pages;

public record Nav(int CurrentPage, int LastPage, string DelegateParam);

public abstract class Navigator<T>: PageModel
{
    public IEnumerable<T> Entities = Enumerable.Empty<T>();
    public Nav NavRecord = new(0, 0, "");
    public abstract Func<string, int, string> PageDelegate();
}

public interface IForum
{
    public Func<ThreadView, string> GetThreadLink();
}

public static class ProfileHighlight
{
    public const string Main = "profile";
    public const string Images = "images";
    public const string Blogs = "blogs";
    public const string Threads = "threads";
    public const string Posts = "posts";
}

public static class Keys
{
    public const string Highlight = "highlight";
    public const string Headings  = "headings";
    public const string Title  = "title";
    public const string ProfileName  = "profile-name";
    public const string ProfileHasBlog  = "profile-blog";
    public const string ProfileHasImage  = "profile-image";
}

public static class MainHighlight
{
    public const string Vine = "vine";
    public const string Archives = "archives";
    public const string Search = "search";
    public const string Stats = "stats";
}

public enum SortForumBy
{
    DateCreated, // sorts by id
    NoViews,
    NoPosts
}



public static class Util
{
    public static int ThreadPerPage = 50;
    public static int PostsPerPage = 50;
}