using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SilvrbackToJekyll
{
    class Program
    {
        static void Main(string[] args)
        {
            var template = File.ReadAllText("template.md");
            var xml = XElement.Load("blog.xml");
            var outputPath = "_posts";

            var posts =
                (from post in xml.Descendants("article")
                    select new
                    {
                        title = (string)post.Element("title"),
                        subtitle = (string)post.Element("subtitle"),
                        slug = (string)post.Element("slug"),
                        content = (string)post.Element("content"),
                        datetime = DateTimeOffset.Parse(post.Element("created-at").Value),
                        position = (int) post.Element("position"),
                        published = ((string)post.Element("publish")) == "true"
                    })
                /* only convert published posts with content */
                .Where(x => x.published)
                .Where(x => !string.IsNullOrWhiteSpace(x.content))
                .ToList();

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            
            Console.WriteLine("Importing Posts");
            Console.WriteLine("=============");
            
            Parallel.ForEach(posts, post =>
            {
                var sb = new StringBuilder(template);

                sb.Replace("{title}", post.title.Quotes());
                sb.Replace("{subtitle}", post.subtitle.Quotes());
                sb.Replace("{slug}", post.slug);
                sb.Replace("{content}", post.content);
                sb.Replace("{datetime}", post.datetime.ToString("yyyy-MM-dd hh:mm:ss zz"));

                var file = $"{outputPath}/{post.datetime:yyyy-MM-dd}-{post.slug}.md";

                if (File.Exists(file))
                    File.Delete(file);
                
                File.WriteAllText(
                    $"{outputPath}/{post.datetime:yyyy-MM-dd}-{post.slug}.md",
                    sb.ToString()
                );
                
                Console.WriteLine($"* Converting \"{post.title}\" ({post.position})");
            });
            
            Console.WriteLine();
            Console.WriteLine($"Converted {posts.Count} Posts To Jekyll Format... Get To Work!");
            
            Console.WriteLine("Years: ");
            posts
                .GroupBy(p => p.datetime.Year)
                .ToList()
                .ForEach(x => Console.WriteLine($" - {x.Key} ({x.Count()})"));
            
        }
    }

    public static class Cleanup
    {
        public static string Quotes(this string value)
        {
            return value?.Replace("\"", @"\""");
        }
    }
}