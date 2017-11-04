namespace FDMC
{
    using FDMC.Data;
    using FDMC.Models;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CatsDbContext>(options =>
            options.UseSqlServer("Server=.;Database=FMDC;Integrated Security=True;"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            app.Use((context, next) =>
            {
                context.RequestServices.GetRequiredService<CatsDbContext>().Database.Migrate();
                return next();
            });

            // use static files
             app.UseStaticFiles();

            // set usage of html
            app.Use((context, next) =>
            {
                context.Response.Headers.Add("Content-Type", "text/html");
                return next();
            });

            // home
            app.MapWhen(
                ctx => ctx.Request.Path.Value == "/"
                     && ctx.Request.Method == HttpMethods.Get,
                home =>
                {
                    home.Run(async context =>
                     {
                         await context.Response.WriteAsync($"<h1>{env.ApplicationName}</h1>");

                         var db = context.RequestServices.GetService<CatsDbContext>();

                         var cats = db.Cats
                           .Select(cat => new
                           {
                               cat.Id,
                               cat.Name
                           })
                           .ToList();

                         await context.Response.WriteAsync("<ul>");

                         foreach (var cat in cats)
                         {
                             await context.Response.WriteAsync($@"<li><a href = ""/cat/{cat.Id}"">{cat.Name}</a></li>");
                         }
                         await context.Response.WriteAsync("</ul>");
                         await context.Response.WriteAsync(@"
                            <form action=""/cat/add"">
                                <input type = ""submit"" value = ""Add cat""/>
                            </form>");
                     });
                });

            // cat/add
            app.MapWhen(req => req.Request.Path.Value == "/cat/add",
            catAdd =>
            {
                catAdd.Run(async (context) =>
                {
                    if (context.Request.Method == HttpMethods.Get)
                    {
                        await context.Response.WriteAsync("<h1>Add Cat</h1>");

                        await context.Response.WriteAsync(
                            @"<form method=""post"" action=""/cat/add"">
                            <label for=""Name"">Name:</label>
                            <input type= ""text"" name=""Name"" id=""Name""/>
                            </br>
                            <label for=""Age"">Age:</label>
                            <input type= ""number"" name=""Age"" id=""Age""/>
                            </br>
                            <label for=""Breed"">Breed:</label>
                            <input type= ""text"" name=""Breed"" id=""Breed""/>
                            </br>
                            <label for=""ImageUrl"">ImageUrl:</label>
                            <input type= ""text"" name=""ImageUrl"" id=""ImageUrl""/>
                            </br>
                            <input type=""submit"" value=""Add Cat""/>
                        </form>");
                    }

                    // add cat to db
                    else if (context.Request.Method == HttpMethods.Post)
                    {
                        var db = context.RequestServices.GetService<CatsDbContext>();

                        var formData = context.Request.Form;

                        var cat = new Cat
                        {
                            Name = formData["Name"],
                            Age = int.Parse(formData["Age"]),
                            Breed = (Breed)Enum.Parse(typeof(Breed), formData["Breed"]),
                            ImageUrl = formData["ImageUrl"]
                        };
   
                        db.Add(cat);
                        await db.SaveChangesAsync();
                        context.Response.Redirect("/");
                    }
                });

                // show cat
                app.MapWhen(
                    ctx => ctx.Request.Path.Value.StartsWith("/cat")
                        && ctx.Request.Method == HttpMethods.Get,
                    catDetails =>
                    {
                        catDetails.Run(async (context) =>
                        {
                            var urlParts = context.Request.Path.Value.Split("/", StringSplitOptions.RemoveEmptyEntries);

                            var catId = int.Parse(urlParts[1]);

                            var db = context.RequestServices.GetService<CatsDbContext>();

                            var cat = await db.Cats.FindAsync(catId);

                            await context.Response.WriteAsync($"<h1>{cat.Name}</h1>");
                            await context.Response.WriteAsync($@"<img src=""{cat.ImageUrl}"" alt=""{cat.Name}"" width=""300""/>");
                            await context.Response.WriteAsync($"<p>Age:{cat.Age}</p>");
                            await context.Response.WriteAsync($"<p>Breed:{cat.Breed}</p>");

                        });
                    }

                );

            });

            app.Run(async (context) =>
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("404 page not found");
            });
        }
    }
}
