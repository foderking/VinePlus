using HtmlAgilityPack;
using WebAPI.Controllers;
using WebAPI.Models;
using WebAPI.Repository;
using WebAPI.Repository.Parsers;
using Xunit.Abstractions;

namespace WebAPI.Test;

public class TestUserRepository {
    public class TestGetProfile
    {
        private readonly TestLogger<ProfileController> _testOutputHelper;
        private readonly UserRepository _userRepo;
        private readonly IEnumerable<string> _validUsers;
        private const int NoTests = 5;

        public TestGetProfile(ITestOutputHelper testOutputHelper) {
            _testOutputHelper = new TestLogger<ProfileController>(testOutputHelper);
            _userRepo = new UserRepository();
            _validUsers = GetValidUsers();
        }

        private static IEnumerable<string> ParseValidUsers(HtmlNode rootNode) {
            return rootNode
                .Descendants("a")
                .Where(
                    a => a.HasClass("neutral")  || a.HasClass("none") || a.HasClass("good") || a.HasClass("evil") 
                )
                .Select(a => a.InnerHtml.Trim())
                .Select(UserParser.CleanUsername)
                .Where(each => !each.Contains("deactivated"))
                .ToArray();
        }

        private static IEnumerable<string> GetValidUsers() {
            Dictionary<string, string> query = new (
                new[]
                {
                    new KeyValuePair<string, string>("page", "2")
                }
            );
            Stream stream = Repository.Repository.GetStream("/forums", query).Result;
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            return ParseValidUsers(rootNode);
        }

        private static IEnumerable<object[]> GetTestData() {
            return GetValidUsers()
                .Select(each => new [] { (object) each } )
                .TakeLast(NoTests);
        }


        [Theory]
        [MemberData(nameof(GetTestData))]
        public async Task Parsing_User_Doesnt_Throw(string username) {
            // _testOutputHelper.WriteLine(string.Join(", ", validUsers.TakeLast(15)));
            Exception? e = await Record.ExceptionAsync(
                async () => await _userRepo.GetProfile(username, _testOutputHelper)
            );
            Assert.Null(e);
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public async Task Parsing_User_Parses_Username_Properly( string username) {
            User user = await _userRepo.GetProfile(username, _testOutputHelper);
            Assert.Equal(username, user.UserName);
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public async Task Parsed_User_Doesnt_Contain_Null_Values(string username) {
            User user = await _userRepo.GetProfile(username, _testOutputHelper);
            Assert.NotNull(user.Followers);
            Assert.NotNull(user.Following);
            Assert.NotNull(user.AvatarUrl);
            Assert.NotNull(user.ProfileDescription);
            Assert.NotNull(user.UserName);
        }
    }

    public class TestUserParser
    {
        private static string baseDir = $"{AppContext.BaseDirectory}/../../..";
        private readonly Dictionary<string, User> _userDic;

        public TestUserParser() {
            _userDic = new Dictionary<string, User>(
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
                            ProfileDescription = "Y&#039;all, I&#039;m not a moderator, and I haven&#039;t been a moderator in several years. Stop messaging me about forum rules and all that.",
                            CoverPicture = "https://comicvine.gamespot.com/a/uploads/scale_medium/4/43236/1972990-static_01_06___copy.jpg",
                            AboutMe = new () { DateJoined = new DateTime(2008, 6,6), Alignment = Alignment.Good, Points = 12480, Summary = "Who wants to know?"},
                            LatestImages = new []{ "" },
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
                            ProfileDescription = "testing 1...2..3",
                            CoverPicture = "https://comicvine.gamespot.com/a/uploads/scale_medium/11162/111629420/8610623-4463160778-manga.jpg",
                            AboutMe = new () { DateJoined = new DateTime(2022, 8,8), Alignment = Alignment.Evil, Points = 0, Summary = " hey. Welcome to my bio!!"},
                            LatestImages = new []{ "" },
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
            var url = _userDic[value].AvatarUrl;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.Equal(url, UserParser.ParseAvatarUrl(new [] {rootNode}));
        }   
         
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_ProfileTitle_Works(string value) {
            var title = _userDic[value].ProfileDescription;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.Equal(title, UserParser.ParseProfileTitle(new [] {rootNode}));
        }   
         
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_ForumPosts_Works(string value) {
            var posts = _userDic[value].ForumPosts;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.Equal(posts, Convert.ToInt32(UserParser.ParseForumPosts(new [] {rootNode})));
        }   
          
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_WikiPoints_Works(string value) {
            var points = _userDic[value].WikiPoints;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.Equal(points, Convert.ToInt32(UserParser.ParseWikiPoints(new [] {rootNode})));
        }   
           
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_Following_Works(string value) {
            var following = _userDic[value].Following!;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            Assert.True(following.Equals( UserParser.ParseFollowing(new [] {rootNode})));
        }   
            
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_Follower_Works(string value) {
            var follower = _userDic[value].Followers!;
            using Stream stream = GetStream($"Html/{value}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            // var e = await Record.Exception(() => userRepo.GetProfile(username));
            // Assert.Null(e);
            Assert.True(follower.Equals( UserParser.ParseFollowers(new [] {rootNode})));
        }

        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_CoverPicture_works(string username) {
            var coverPic = _userDic[username].CoverPicture;
            using Stream stream = GetStream($"Html/{username}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            
            if (coverPic == null) Assert.Null(UserParser.ParseCoverPicture(rootNode));
            else Assert.True(coverPic.Equals(UserParser.ParseCoverPicture(rootNode)));
        }
        
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_AboutMe_works(string username) {
            var about = _userDic[username].AboutMe!;
            using Stream stream = GetStream($"Html/{username}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            
            Assert.True(about.Equals(UserParser.ParseAboutMe(rootNode)));
        }
         
        [Theory]
        [InlineData("saboyaba")]
        [InlineData("static_shock")]
        public void Parsing_LatestImages_works(string username) {
            var images = _userDic[username].LatestImages!;
            using Stream stream = GetStream($"Html/{username}.html");
            HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
            
            Assert.StrictEqual(images, UserParser.ParseLatestImages(rootNode));
        }
        
    }
}
