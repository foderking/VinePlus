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
    public static void AddApiEndpoints(this IEndpointRouteBuilder app) {
        app.MapGet("/api/post", async ([FromQuery] string path) => {
            try {
                var res = await Parsers.PostParser.ParseMultiple(path);
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });

        app.MapGet("/api/post/{page}", async (
            [FromQuery] string path,
            int page
        ) =>
        {
            try {
                var res = await Parsers.PostParser.ParsePage(page, path);
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/forum", async () =>
        {
            try {
                var res = await Parsers.ThreadParser.ParsePage(1, "forums");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }

        });


        app.MapGet("/api/forum/{page}", async (
            int page
        ) =>
        {
            try {
                var res = await Parsers.ThreadParser.ParsePage(page, "forums");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/profile/{username}", async (
            string username
        ) =>
        {
            try {
                var res = await Parsers.ProfileParser.ParseDefault($"/profile/{username}");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/profile/{username}/images", async (
            string username
        ) =>
        {
            try {
                var res = await Parsers.ImageParser.ParseDefault($"/profile/{username}/images");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/profile/{username}/blog", async (
            string username
        ) =>
        {
            try {
                var res = await Parsers.BlogParser.ParseMultiple($"/profile/{username}/blog");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/profile/{username}/follower", async (
            string username
        ) =>
        {
            try {
                var res = await Parsers.FollowerParser.ParseMultiple($"/profile/{username}/follower");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });


        app.MapGet("/api/profile/{username}/following", async (
            string username
        ) =>
        {
            try {
                var res = await Parsers.FollowingParser.ParseMultiple($"/profile/{username}/following");
                return Results.Ok(res);
            }
            catch (HttpRequestException) {
                return Results.NotFound();
            }
        });

    }
}