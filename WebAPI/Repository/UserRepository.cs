using HtmlAgilityPack;
using WebAPI.Models;
using Exception = System.Exception;

namespace WebAPI.Repository;

public class UserRepository: IUserRepository
{
    private static User ParseUser(HtmlNode rootNode) {
        IEnumerable<HtmlNode> sectionNode = rootNode
            .Descendants("section")
            .ToArray();

        IEnumerable<HtmlNode> profileTitleNode = sectionNode
            .Where(
                node => node
                    .HasClass("profile-title")
            )
            .ToArray();

        IEnumerable<HtmlNode> profileFollowNode = sectionNode
            .Where(
                node => node
                    .HasClass("profile-follow")
            )
            .ToArray();

        IEnumerable<HtmlNode> profileStats = profileFollowNode
            .Select(
                node => node
                    .Descendants("table")
                    .First()
            )
            .ToArray();
            
         IEnumerable<HtmlNode> profileAvatarNode = sectionNode
            .Where(
                node => node
                    .HasClass("profile-avatar")
            )
            .ToArray();

         try {
             User user = new User();
             user.UserName = UserParser.ParseUserName(profileTitleNode);
             user.ProfileTitle = UserParser.ParseProfileTitle(profileTitleNode);
             user.AvatarUrl = UserParser.ParseAvatarUrl(profileAvatarNode);
             user.ForumPosts = Convert.ToInt32(UserParser.ParseForumPosts(profileStats));
             user.WikiPoints = Convert.ToInt32(UserParser.ParseWikiPoints(profileStats));
             user.Following = UserParser.ParseFollowing(profileStats);
             user.Followers = UserParser.ParseFollowers(profileStats);
             return user;
         }
         catch (Exception e) {
             Console.WriteLine(e.Message);
             throw new Exception("asfasf");
         }
    }
    
    public Task<Stream> GetStream(string username) {
        return Repository.GetStream($"/profile/{username}");
    }

    public async Task<User> GetProfile(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
        return ParseUser(rootNode);
    }

    public async Task GetUserBlog(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserImages(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserForumPosts(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetWikiPosts(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserFollowing(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserFollowers(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserLists(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }

    public async Task GetUserReviews(string username) {
        Stream stream = await GetStream(username);
        HtmlNode rootNode = Repository.GetRootNode(stream);
    }
}    

public static class UserParser
{
    public static string CleanString(string input) {
        return input.ToLower().Replace(" ", "_");
    }
    
    public static string ParseUserName(IEnumerable<HtmlNode> profileTitleNode) {
        return profileTitleNode
            .Select(
                node => node
                    .Descendants("h1")
                    .First()
                    .InnerHtml
            )
            .Select(CleanString)
            .FirstOrDefault() ?? throw new Exception("asfasf");
    }
    
    public static string ParseProfileTitle(IEnumerable<HtmlNode> profileTitleNode) {
        return profileTitleNode
            .Select(
                node => node
                    .Descendants("h4")
                    .First(
                        h4 => h4
                            .HasClass("js-status-message")
                    )
                    .InnerHtml
            )
            .FirstOrDefault() ?? throw new Exception("af");
    }

    public static string ParseAvatarUrl(IEnumerable<HtmlNode> profileAvatarNode) {
        return profileAvatarNode
            .Select(
                node => node
                    .Descendants("div")
                    .First(
                        div => div
                            .HasClass("avatar")
                    )
            )
            .Select(
                avatar => avatar
                    .Descendants("img")
                    .First()
                    .GetAttributeValue("src", "")
            )
            .FirstOrDefault() ?? throw new Exception("af");
    }

    public static string ParseForumPosts(IEnumerable<HtmlNode> profileStats) {
        return profileStats
            .Select(
                tbody => tbody
                    .Descendants("tr")
                    .First()
                    .Descendants("td")
                    .Skip(0)
                    .First()
                    .InnerHtml
            )
            .FirstOrDefault() ?? throw new Exception("asfasf");
    }
    
    public static string ParseWikiPoints(IEnumerable<HtmlNode> profileStats) {
        return profileStats
            .Select(
                tbody => tbody
                    .Descendants("tr")
                    .First()
                    .Descendants("td")
                    .Skip(1)
                    .First()
                    .InnerHtml
            )
            .FirstOrDefault() ?? throw new Exception("asfasf");
    }
    
    public static Follow ParseFollow(HtmlNode tdNode) {
        HtmlNode aNode = tdNode.Descendants("a").First();
        string link = aNode.GetAttributeValue("href", null) ?? throw new  Exception("error parsing follow");
        int number = Convert.ToInt32(aNode.InnerHtml);
        return new Follow() { Link = link, Number = number };
    }
           
    public static Follow ParseFollowing(IEnumerable<HtmlNode> profileStats) {
        return profileStats
            .Select(
                tbody => tbody
                    .Descendants("tr")
                    .First()
                    .Descendants("td")
                    .Skip(2)
                    .First()
            )
            .Select(ParseFollow)
            .FirstOrDefault() ?? throw new Exception("asfasf");
    }
    
    public static Follow ParseFollowers(IEnumerable<HtmlNode> profileStats) {
        return profileStats 
            .Select(
                tbody => tbody
                    .Descendants("tr")
                    .First()
                    .Descendants("td")
                    .Skip(3)
                    .First()
            )
            .Select(ParseFollow)
            .FirstOrDefault() ?? throw new Exception("asfasf");
    }
}
