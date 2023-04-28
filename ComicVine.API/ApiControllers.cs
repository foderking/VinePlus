using Comicvine.Core;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;

namespace ComicVine.API;

public static class Controller
{
    /** TODO *
     * 
     * - add documentation
     * - add proper restful errors
     */
    public static void AddEndpoints(this IEndpointRouteBuilder app) {
        app.MapGet("/api/post", async (
            Parsers.IMultiple<Parsers.Post> parser,
            [FromQuery] string path
        ) =>
        {
            try {
                var res = await Parsers.Common.ParseMultiple(parser, path);
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });

        app.MapGet("/api/post/{page}", async (
            Parsers.IMultiple<Parsers.Post> parser,
            [FromQuery] string path,
            int page
        ) =>
        {
            try {
                // Stream stream = await Net.getStreamByPage(page, path);
                // HtmlNode rootNode = Net.getRootNode(stream);
                // var res = parser.ParseSingle(rootNode);
                var res = await Parsers.Common.ParseSingle(parser, page, path);
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/forum", async (
            Parsers.IMultiple<Parsers.Thread> parser
        ) =>
        {
            try {
                // Stream stream = await Net.getStreamByPage(1, "forums");
                // HtmlNode rootNode = Net.getRootNode(stream);
                // var res = parser.ParseSingle(rootNode);
                var res = await Parsers.Common.ParseSingle(parser, 1, "forums");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }

        });


        app.MapGet("/api/forum/{page}", async (
            Parsers.IMultiple<Parsers.Thread> parser,
            int page
        ) =>
        {
            try {
                // Stream stream = await Net.getStreamByPage(page, "forums");
                // HtmlNode rootNode = Net.getRootNode(stream);
                // var res = parser.ParseSingle(rootNode);
                var res = await Parsers.Common.ParseSingle(parser, page, "forums");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/profile/{username}", async (
            Parsers.ISingle<Parsers.Profile> parser,
            string username
        ) =>
        {
            try {
                // Stream stream = await Net.getStream($"/profile/{username}");
                // HtmlNode rootNode = Net.getRootNode(stream);
                // var res = parser.ParseSingle(rootNode);
                var res = await Parsers.Common.ParseDefault(parser, $"/profile/{username}");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/profile/{username}/images", async (
            Parsers.ISingle<Parsers.Image> parser,
            string username
        ) =>
        {
            try {
                // Stream stream = await Net.getStream($"/profile/{username}/images");
                // HtmlNode rootNode = Net.getRootNode(stream);
                // var res = parser.ParseSingle(rootNode);
                var res = await Parsers.Common.ParseDefault(parser, $"/profile/{username}/images");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/profile/{username}/blog", async (
            Parsers.IMultiple<Parsers.Blog> parser,
            string username
        ) =>
        {
            try {
                var res = await Parsers.Common.ParseMultiple(parser, $"/profile/{username}/blog");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/profile/{username}/follower", async (
            Parsers.IMultiple<Parsers.Follower> parser,
            string username
        ) =>
        {
            try {
                var res = await Parsers.Common.ParseMultiple(parser, $"/profile/{username}/follower");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/profile/{username}/following", async (
            Parsers.IMultiple<Parsers.Following> parser,
            string username
        ) =>
        {
            try {
                var res = await Parsers.Common.ParseMultiple(parser, $"/profile/{username}/following");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });
        // .Produces<IEnumerable<Parsers.Following>>();

    }
}