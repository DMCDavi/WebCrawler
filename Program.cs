using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using WebCrawler.Data;
using HtmlAgilityPack;
using System.Linq;
using Microsoft.Data.SqlClient;
using WebCrawler.Models;

namespace WebCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            var imgs = new List<Image>();
            try
            {
                imgs = startCrawling(imgs);
                saveDB(imgs);
            }
            catch
            {
                Console.WriteLine("Não foi possível buscar os dados");
            }
        }

        private static void saveDB(List<Image> imgs)
        {
            using (var context = new DataContext())
            {
                foreach (var img in imgs)
                {
                    context.Images.Add(img);
                    context.SaveChanges();
                }
            }
        }

        private static List<Image> startCrawling(List<Image> imgs)
        {
            var url = "https://nacoesunidas.org/";
            var client = new WebClient();
            string pagina = client.DownloadString(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pagina);

            var articles = htmlDocument.DocumentNode
                .Descendants("article")
                .Where(node => node
                .GetAttributeValue("id", "")
                .Contains("post-"))
                .ToList();

            foreach (var article in articles)
            {

                var img_src = article
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .GetAttributeValue("src", "");

                var img_title = article
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .GetAttributeValue("data-image-title", "");

                var img_description = article
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .GetAttributeValue("data-image-description", "");

                string id = article.GetAttributeValue("id", "");

                var date = article
                    .FirstChild
                    .FirstChild
                    .Descendants("ul")
                    .FirstOrDefault()
                    .FirstChild
                    .InnerText;

                var post_title = article
                    .FirstChild
                    .FirstChild
                    .Descendants("h1")
                    .FirstOrDefault()
                    .FirstChild
                    .InnerText;

                var link = article
                    .FirstChild
                    .FirstChild
                    .Descendants("h1")
                    .FirstOrDefault()
                    .FirstChild
                    .GetAttributeValue("href", "");

                DateTime form_date = DateTime.ParseExact(date, "dd/MM/yyyy",
                                System.Globalization.CultureInfo.InvariantCulture);

                var post = new Post
                {
                    Date = form_date,
                    Title = post_title,
                    Link = link
                };

                var img = new Image
                {
                    PostId = post.PostId,
                    Post = post,
                    Path = img_src,
                    Title = img_title,
                    Description = img_description
                };

                imgs.Add(img);

            }

            return imgs;
        }
    }
}
