using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFLeftJoinBug
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new BloggingContext())
            {
                var dbPosts = from p in db.Posts
                            // select p;
                            select new Post
                            {
                                PostId = p.PostId,
                                BlogId = p.BlogId,
                                Content = p.Content
                            };

                var query = from blog in db.Blogs
                            join post in dbPosts on blog.BlogId equals post.BlogId into posts
                            from xpost in posts.DefaultIfEmpty()
                            select new Blog
                            {
                                Url = blog.Url,
                                Post = xpost
                            };

                foreach (var b in query)
                {
                    Console.WriteLine(b.Url);

                    Console.WriteLine($"{b.Post?.PostId}:{b.Post?.Content}:{b.Post?.BlogId}");
                }
            }
        }
    }

    public class BloggingContext : DbContext
    {
        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder
            .AddFilter((category, level) =>
                category == DbLoggerCategory.Database.Command.Name
                && level == LogLevel.Information)
            .AddConsole();
        });

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=blogging.db").UseLoggerFactory(MyLoggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Blog>().HasData(new Blog { BlogId = 1, Url = "http://blogs.msdn.com/adonet" });
        }
    }

    public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; }

        [NotMapped]
        public Post Post { get; set; }

        public List<Post> Posts { get; set; } = new List<Post>();
    }

    public class Post
    {
        public int PostId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int BlogId { get; set; }

        public Blog Blog { get; set; }
    }
}