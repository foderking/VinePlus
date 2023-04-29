
namespace ComicVine.API.Pages;

public static class Util
{
    public static int LastPage = 16600;

    public static string GetLock(bool isLocked) {
        return
            isLocked
            ? "<img src='https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-lock-16x16.png'>"
            : "";
    }
    
    public static string GetPin(bool isPinned) {
        return
            isPinned
            ? "<img src='https://comicvine.gamespot.com/a/bundles/phoenixsite/images/core/sprites/icons/icn-pin-16x16.png'>"
            : "";
    }

    public static string GetNavigation(int currPage, int lastPage, string path) {
        string s = $@"<div class='paginate'><a href='{path}/{1}'><span class='pag-item'>
            <i><svg width='28' height='28' viewBox='0 0 28 28' aria-hidden='true' class='symbol' ><path d='M24 14v2q0 .828-.508 1.414t-1.32.586h-11l4.578 4.594q.594.562.594 1.406t-.594 1.406l-1.172 1.188q-.578.578-1.406.578-.812 0-1.422-.578L1.578 16.407Q1 15.829 1 15.001q0-.812.578-1.422L11.75 3.423q.594-.594 1.422-.594.812 0 1.406.594l1.172 1.156q.594.594.594 1.422t-.594 1.422l-4.578 4.578h11q.812 0 1.32.586T24 14.001z'></path></svg></i>
            </span></a>";
        if (currPage == 1 && lastPage>2) {
            s += $@"
                <span class='pag-item pag-on'>{currPage}</span>
                <a href='{path}/{currPage + 1}'><span class='pag-item'>{currPage + 1}</span></a>";
        }
        else if (currPage+1 > 2) {
            s += $@"
                <a href='{path}/{currPage-1}'><span class='pag-item'>{currPage-1}</span></a>
                <span class='pag-item pag-on'>{currPage}</span>
                <a href='{path}/{currPage+1}'><span class='pag-item'>{currPage+1}</span></a>";
        }
        else {
            s += $@"<span class='pag-item pag-on'>{currPage}</span>";
        }
        
        s += $@"
            <a href='{path}/{lastPage}'><span class='pag-item'>
            <i><svg width='28' height='28' viewBox='0 0 28 28' aria-hidden='true' class='symbol'><path d='M23 15q0 .844-.578 1.422L12.25 26.594q-.609.578-1.422.578-.797 0-1.406-.578L8.25 25.422q-.594-.594-.594-1.422t.594-1.422L12.828 18h-11q-.812 0-1.32-.586T0 16v-2q0-.828.508-1.414T1.828 12h11L8.25 7.406Q7.656 6.844 7.656 6t.594-1.406l1.172-1.172q.594-.594 1.406-.594.828 0 1.422.594l10.172 10.172Q23 14.141 23 15z'></path></svg></i>
            </span></a></div>";
        return s;
    }

    public static string GetThreadRow(int index) {
        return index % 2  == 1? "thread-odd" : "";
    }
}