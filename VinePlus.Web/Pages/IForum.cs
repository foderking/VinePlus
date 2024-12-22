namespace VinePlus.Web.Pages;

public interface IForum
{
    public Func<ThreadView, string> GetThreadLink();
}