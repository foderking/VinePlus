using HtmlAgilityPack;
using WebAPI.Models;
using WebAPI.Repository;
using Xunit.Abstractions;

namespace WebAPI.Test;

public class TestUserRepository {
    public class TestGetProfile
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly UserRepository userRepo;
        private readonly IEnumerable<string> validUsers;

        public TestGetProfile(ITestOutputHelper testOutputHelper) {
            _testOutputHelper = testOutputHelper;
            userRepo = new UserRepository();
            validUsers = GetValidUsers();
        }

        private static IEnumerable<string> ParseValidUsers(HtmlNode rootNode) {
            return rootNode
                .Descendants("a")
                .Where(
                    a => a.HasClass("neutral") // a.HasClass("good") || a.HasClass("evil") ||
                )
                .Select(a => a.InnerHtml.Trim());
        }

        private static IEnumerable<string> GetValidUsers() {
            Dictionary<string, string> query = new (
                new[]
                {
                    new KeyValuePair<string, string>("page", "3")
                }
            );
            Stream stream = Repository.Repository.GetStream("/forums", query).Result;
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            return ParseValidUsers(rootNode);
        }

        
        // [Fact]
        [Theory]
        [MemberData(nameof(GetValidUsers))]
        public async Task Parsing_User_Doesnt_Throw(string username) {
            // _testOutputHelper.WriteLine(string.Join(", ", validUsers.TakeLast(15)));
            
            // foreach (string username in validUsers.TakeLast(15)) {
                var e = await Record.ExceptionAsync(async () => await userRepo.GetProfile(username));
                Assert.Null(e);
            // }
            
        }

        // [Fact]
        [Theory]
        [MemberData(nameof(GetValidUsers))]
        public async Task Parsing_User_Parses_Username_Properly(string username) {
            // foreach (string username in validUsers.TakeLast(20)) {
                User user = await userRepo.GetProfile(username);
                Assert.Equal(username, user.UserName);
            // }
        }
        
        [Fact]
        public async Task Parsed_User_Doesnt_Contain_Null_Values() {
            foreach (string username in validUsers.TakeLast(20)) {
                User user = await userRepo.GetProfile(username);
                Assert.NotNull(user.Followers);
                Assert.NotNull(user.Following);
                Assert.NotNull(user.AvatarUrl);
                Assert.NotNull(user.ProfileTitle);
                Assert.NotNull(user.UserName);
            }
        }
    }

    public class TestUserParser
    {
        private static string baseDir = $"{AppContext.BaseDirectory}/../../..";
        private readonly Dictionary<string, User> userDic;

        public TestUserParser() {
            userDic = new Dictionary<string, User>(
                new []
                {
                    new KeyValuePair<string, User> ("static_shock", new User()
                        {
                            AvatarUrl = "https://comicvine.gamespot.com/a/uploads/square_tiny/0/7604/6352798-young-justice-outsiders.jpg",
                            UserName = "static_shock",
                            ForumPosts = 53107,
                            WikiPoints = 12480,
                            Followers = new (){ Link = "/profile/static_shock/follower/", Number = 739},
                            Following = new (){ Link = "/profile/static_shock/following/", Number = 166},
                            ProfileTitle = "Y&#039;all, I&#039;m not a moderator, and I haven&#039;t been a moderator in several years. Stop messaging me about forum rules and all that."
                        }
                    ),
                    new KeyValuePair<string, User> ("saboyaba", new User()
                        {
                            AvatarUrl = "https://comicvine.gamespot.com/a/uploads/square_tiny/11162/111629420/8610625-7556855352-images.jpg",
                            UserName = "saboyaba",
                            ForumPosts = 811,
                            WikiPoints = 0,
                            Followers = new (){ Link = "/profile/saboyaba/follower/", Number = 1},
                            Following = new (){ Link = "/profile/saboyaba/following/", Number = 1},
                            ProfileTitle = "testing 1...2..3"
                        }
                    )
                }
            ) ;
        }

        private static FileStream GetStream(string path) {
            return File.OpenRead($"{baseDir}/{path}");
        }

        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_Username_Works(string value) {
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.Equal(value, UserParser.ParseUserName(new [] {rootNode}));
        }
        
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_AvatarUrL_Works(string value) {
            var url = userDic[value].AvatarUrl;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.Equal(url, UserParser.ParseAvatarUrl(new [] {rootNode}));
        }   
         
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_ProfileTitle_Works(string value) {
            var title = userDic[value].ProfileTitle;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.Equal(title, UserParser.ParseProfileTitle(new [] {rootNode}));
        }   
         
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_ForumPosts_Works(string value) {
            var posts = userDic[value].ForumPosts;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.Equal(posts, Convert.ToInt32(UserParser.ParseForumPosts(new [] {rootNode})));
        }   
          
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_WikiPoints_Works(string value) {
            var points = userDic[value].WikiPoints;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.Equal(points, Convert.ToInt32(UserParser.ParseWikiPoints(new [] {rootNode})));
        }   
           
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_Following_Works(string value) {
            var following = userDic[value].Following;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.True(following.Equals( UserParser.ParseFollowing(new [] {rootNode})));
        }   
            
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_Follower_Works(string value) {
            var follower = userDic[value].Followers;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.True(follower.Equals( UserParser.ParseFollowers(new [] {rootNode})));
        }   
               
    }
}
