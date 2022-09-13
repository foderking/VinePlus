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
                .Distinct()
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
            Profile profile = await _userRepo.GetProfile(username, _testOutputHelper);
            Assert.Equal(username, profile.UserName);
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public async Task Parsed_User_Doesnt_Contain_Null_Values(string username) {
            Profile profile = await _userRepo.GetProfile(username, _testOutputHelper);
            Assert.NotNull(profile.Followers);
            Assert.NotNull(profile.Following);
            Assert.NotNull(profile.AvatarUrl);
            Assert.NotNull(profile.ProfileDescription);
            Assert.NotNull(profile.UserName);
        }
    }

    public class TestUserParser
    {
        private static string baseDir = $"{AppContext.BaseDirectory}/../../..";
        private static readonly Dictionary<string, Profile> //_userDic;

        // public TestUserParser() {
            _userDic = new Dictionary<string, Profile>(
                new []
                {
                    new KeyValuePair<string, Profile> ("static_shock", new Profile()
                        {
                            AvatarUrl = "https://comicvine.gamespot.com/a/uploads/square_tiny/0/7604/6352798-young-justice-outsiders.jpg",
                            UserName = "static_shock",
                            ForumPosts = 53107,
                            WikiPoints = 12480,
                            Followers = new (){ Link = "/profile/static_shock/follower/", Number = 739},
                            Following = new (){ Link = "/profile/static_shock/following/", Number = 166},
                            ProfileDescription = "Y&#039;all, I&#039;m not a moderator, and I haven&#039;t been a moderator in several years. Stop messaging me about forum rules and all that.",
                            CoverPicture = "https://comicvine.gamespot.com/a/uploads/scale_medium/4/43236/1972990-static_01_06___copy.jpg",
                            AboutMe = new () { DateJoined = new DateTime(2008, 6,6), Alignment = Alignment.Good, Points = 12480, Summary = "<p>Who wants to know?</p>"},
                            LatestImages = new []{  "https://comicvine.gamespot.com/a/uploads/square_small/0/7604/7640443-6481578-9596568912-reatj.jpg", "https://comicvine.gamespot.com/a/uploads/square_small/0/7604/6952657-cellgamesarena.jpg", "https://comicvine.gamespot.com/a/uploads/square_small/0/7604/6836629-justiceleagueeurope29p21.jpg", "https://comicvine.gamespot.com/a/uploads/square_small/0/7604/6836628-justiceleagueeurope29p20.jpg", "https://comicvine.gamespot.com/a/uploads/square_small/0/7604/6836627-justiceleagueeurope29p19.jpg", "https://comicvine.gamespot.com/a/uploads/square_small/0/7604/6836626-justiceleagueeurope29p18.jpg", "https://comicvine.gamespot.com/a/uploads/square_small/0/7604/6836615-action%20631-02.jpg" },
                        }
                    ),
                    new KeyValuePair<string, Profile> ("saboyaba", new Profile()
                        {
                            AvatarUrl = "https://comicvine.gamespot.com/a/uploads/square_tiny/11162/111629420/8610625-7556855352-images.jpg",
                            UserName = "saboyaba",
                            ForumPosts = 811,
                            WikiPoints = 0,
                            Followers = new (){ Link = "/profile/saboyaba/follower/", Number = 1},
                            Following = new (){ Link = "/profile/saboyaba/following/", Number = 1},
                            ProfileDescription = "testing 1...2..3",
                            CoverPicture = "https://comicvine.gamespot.com/a/uploads/scale_medium/11162/111629420/8610623-4463160778-manga.jpg",
                            AboutMe = new () { DateJoined = new DateTime(2022, 8,8), Alignment = Alignment.Evil, Points = 0, Summary = "<p> hey. Welcome to my bio!!</p>"},
                            LatestImages = new []{ "https://comicvine.gamespot.com/a/uploads/square_small/11162/111629420/8646949-images%287%29.jpeg" , "https://comicvine.gamespot.com/a/uploads/square_small/11162/111629420/8646722-capture.png" , "https://comicvine.gamespot.com/a/uploads/square_small/11162/111629420/8646316-perfectlybalanced.jpg" , "https://comicvine.gamespot.com/a/uploads/square_small/11162/111629420/8646314-capture.png" , "https://comicvine.gamespot.com/a/uploads/square_small/11162/111629420/8646041-6sf1uq.jpg" , "https://comicvine.gamespot.com/a/uploads/square_small/11162/111629420/8645874-9621050148-if-yo.jpg" , "https://comicvine.gamespot.com/a/uploads/square_small/11162/111629420/8645873-0548618617-if-yo.jpg" },
                        }
                    )
                }
            ) ;
        // }

        private static FileStream GetStream(string path) {
            return File.OpenRead($"{baseDir}/{path}");
        }
        


        private static IEnumerable<object[]> GetUsersWithoutUserActivity() {
            return new[]
            {
                new object[] {"willopedia1205"},
            };
        }

        public class WrapperNode
        {
            
        }

        public class ProfileHeaderNode
        {

            public static HtmlNode GetNode(string username) {
                using Stream stream = GetStream($"Html/{username}.html");
                HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
                HtmlNode wrapperNode = UserParser.GetWrapperNode(rootNode);
                return UserParser.GetProfileHeaderContainer(wrapperNode);
            }
            
            [Theory]
            [InlineData("saboyaba")]
            [InlineData("static_shock")]
            public void Parsing_AvatarUrL_Works(string username) {
                var url = _userDic[username].AvatarUrl;
                HtmlNode node = GetNode(username);
                var testUrl = UserParser.ParseAvatarUrl(node);
                Assert.Equal(url, testUrl);
            }
           
        }

        public class ProfileTitleNode
        {
            public static HtmlNode GetNode(string username) {
                using Stream stream = GetStream($"Html/{username}.html");
                HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
                HtmlNode wrapperNode = UserParser.GetWrapperNode(rootNode);
                HtmlNode profileHeaderNode = UserParser.GetProfileHeaderContainer(wrapperNode);
                return UserParser.GetProfileTitleNode(profileHeaderNode);
            }
         
            [Theory]
            [InlineData("saboyaba")]
            [InlineData("static_shock")]
            public void Parsing_Username_Works(string username) {
                using Stream stream = GetStream($"Html/{username}.html");
                HtmlNode node = GetNode(username);
                var testUsername = UserParser.ParseUserName(node );
                Assert.Equal(username, testUsername);
            }
          
            [Theory]
            [InlineData("saboyaba")]
            [InlineData("static_shock")]
            public void Parsing_ProfileTitle_Works(string username) {
                var title = _userDic[username].ProfileDescription;
                using Stream stream = GetStream($"Html/{username}.html");
                HtmlNode node = GetNode(username);
                var testTitle = UserParser.ParseProfileTitle(node);
                Assert.Equal(title, testTitle);
            }   
           
        }

        public class AsideNode
        {
            private static IEnumerable<object[]> GetUsersWithoutLatestImages() {
                return new[]
                {
                    new [] {"velentoelectric"}
                };
            }
            
            private static IEnumerable<object[]> GetUsersWithoutCoverImage() {
                return new[]
                {
                    new object[] {"infinitymatrix"},
                    // (object[]) new [] {"noxvenala"},
                    new object[] {"unhappy-hyena"}
                };
            }

            public static HtmlNode GetAsideNode(string username) {
                using Stream stream = GetStream($"Html/{username}.html");
                HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
                HtmlNode wrapperNode = UserParser.GetWrapperNode(rootNode);
                HtmlNode asideNode = UserParser.GetAsideNode(wrapperNode);
                return asideNode;
            }
 
            [Theory]
            [InlineData("saboyaba")]
            [InlineData("static_shock")]
            public void Parsing_CoverPicture_works(string username) {
                var coverPic = _userDic[username].CoverPicture;
                HtmlNode asideNode = GetAsideNode(username);
                var testCoverPic = UserParser.ParseCoverPicture(asideNode);
                
                if (coverPic == null) Assert.Null(testCoverPic);
                else Assert.True(coverPic.Equals(testCoverPic));
            }
     
            [Theory]
            [MemberData(nameof(GetUsersWithoutCoverImage))]
            public async Task Parsing_Users_Without_CoverPicture_Works(string username) {
                Stream stream = await  Repository.Repository.GetStream($"/profile/{username}");
                HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
                HtmlNode wrapperNode = UserParser.GetWrapperNode(rootNode);
                HtmlNode asideNode = UserParser.GetAsideNode(wrapperNode);
                var e = Record.Exception(() =>
                {
                    string? testCoverImage = UserParser.ParseCoverPicture(asideNode);
                    Assert.Null(testCoverImage);
                });
                Assert.Null(e);
            }
            
            [Theory]
            [InlineData("saboyaba")]
            [InlineData("static_shock")]
            public void Parsing_AboutMe_works(string username) {
                var about = _userDic[username].AboutMe!;
                HtmlNode asideNode = GetAsideNode(username);
                var testAbout = UserParser.ParseAboutMe(asideNode);
                
                Assert.True(about.Equals(testAbout));
            }          
            
            [Theory]
            [InlineData("saboyaba")]
            [InlineData("static_shock")]
            public void Parsing_LatestImages_works(string username) {
                var images = _userDic[username].LatestImages!;
                HtmlNode asideNode = GetAsideNode(username);
                var testImages = UserParser.ParseLatestImages(asideNode);
                
                Assert.Equal(images, testImages);
            }

            [Theory]
            [MemberData(nameof(GetUsersWithoutLatestImages))]
            public async Task Parsing_Users_Without_LatestImages_Works(string username) {
                Stream stream = await  Repository.Repository.GetStream($"/profile/{username}");
                HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
                HtmlNode wrapperNode = UserParser.GetWrapperNode(rootNode);
                HtmlNode asideNode = UserParser.GetAsideNode(wrapperNode);
                var e = Record.Exception(() =>
                {
                    string[]? testLatestImages = UserParser.ParseLatestImages(asideNode);
                    Assert.Null(testLatestImages);
                });
                Assert.Null(e);
            }
        
        }

        public class ProfileStatsNode
        {
            public static HtmlNode GetNode(string username) {
                using Stream stream = GetStream($"Html/{username}.html");
                HtmlNode rootNode = Repository.Repository.GetRootNode(stream);
                HtmlNode wrapperNode = UserParser.GetWrapperNode(rootNode);
                HtmlNode profileHeaderNode = UserParser.GetProfileHeaderContainer(wrapperNode);
                return UserParser.GetStatsNode(profileHeaderNode);
            }

            [Theory]
            [InlineData("saboyaba")]
            [InlineData("static_shock")]
            public void Parsing_ForumPosts_Works(string username) {
                var posts = _userDic[username].ForumPosts;
                HtmlNode node = GetNode(username);
                var testPost = Convert.ToInt32(UserParser.ParseForumPosts(node));
                Assert.Equal(posts, testPost);
            }

            [Theory]
            [InlineData("saboyaba")]
            [InlineData("static_shock")]
            public void Parsing_WikiPoints_Works(string username) {
                var points = _userDic[username].WikiPoints;
                HtmlNode node = GetNode(username);
                var testPoints = Convert.ToInt32(UserParser.ParseWikiPoints(node));
                Assert.Equal(points, testPoints);
            }

            [Theory]
            [InlineData("saboyaba")]
            [InlineData("static_shock")]
            public void Parsing_Following_Works(string username) {
                var following = _userDic[username].Following!;
                HtmlNode node = GetNode(username);
                var testFollowing = UserParser.ParseFollowing(node);
                Assert.True(following.Equals(testFollowing));
            }

            [Theory]
            [InlineData("saboyaba")]
            [InlineData("static_shock")]
            public void Parsing_Follower_Works(string username) {
                var follower = _userDic[username].Followers!;
                HtmlNode node = GetNode(username);
                var testFollower = UserParser.ParseFollowers(node);
                Assert.True(follower.Equals(testFollower));
            }
        }

    }
}
