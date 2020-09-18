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
            try
            {
                Console.WriteLine("Gostaria de iniciar o WebCrawler? (S/N): ");
                string res = Console.ReadLine();
                Console.WriteLine("========================================================================================");

                if (res == "S")
                    startCrawling();

                searchData();
            }
            catch
            {
                Console.WriteLine("Não foi possível buscar os dados");
            }
        }

        /// <summary>
        /// Pesquisa postagens pelo título
        /// </summary>
        private static void searchData()
        {
            List<Image> imgs = new List<Image>();

            Console.WriteLine("========================================================================================");
            Console.WriteLine("Pesquise uma postagem pelo título: ");
            string query = Console.ReadLine();
            Console.WriteLine("========================================================================================");

            using (var context = new DataContext())
            {
                imgs = context.Images
                    .Include(a => a.Post)
                    .Where(i => i.Post.Title.Contains(query))
                    .ToList();
            }

            printData(imgs);
        }

        /// <summary>
        /// Imprime os dados das postagens
        /// </summary>
        /// <param name="imgs"></param>
        private static void printData(List<Image> imgs)
        {
            foreach (var img in imgs)
            {
                Console.WriteLine("ID da postagem: " + img.PostId);
                Console.WriteLine("Título da postagem: " + img.Post.Title);
                Console.WriteLine("Data da postagem: " + img.Post.Date);
                Console.WriteLine("Link da postagem: " + img.Post.Link);
                Console.WriteLine("ID da imagem: " + img.ImageId);
                Console.WriteLine("Título da imagem: " + img.ImgTitle);
                Console.WriteLine("URL da imagem: " + img.Path);
                Console.WriteLine("Descrição da imagem: " + img.Description);
                Console.WriteLine("========================================================================================");
            }
        }

        /// <summary>
        /// Salva os dados no banco
        /// </summary>
        /// <param name="imgs">Lista de imagens</param>
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

        /// <summary>
        /// Inicia o WebCrawler
        /// </summary>
        /// <param name="page">Número da página</param>
        private static void startCrawling(int page = 1)
        {
            var url = "https://nacoesunidas.org";
            var client = new WebClient();
            string pagina;
            var htmlDocument = new HtmlDocument();
            List<HtmlNode> articles = new List<HtmlNode>();
            List<Image> imgs = new List<Image>();
            IEnumerable<HtmlNode> nav;
            int max_page = 1;

            // Altera a URL para a página correta
            if (page > 1)
                url += "/page/" + page + "/";

            pagina = client.DownloadString(url);
            htmlDocument.LoadHtml(pagina);

            // Procura as tags em que os posts se encontram
            try
            {
                articles = htmlDocument.DocumentNode
                    .Descendants("article")
                    .Where(node => node
                    .GetAttributeValue("id", "")
                    .Contains("post-"))
                    .ToList();
            }
            catch
            {
                Console.WriteLine("Não foi possível achar os posts");
            }

            // Pega os dados de todos os posts
            foreach (var article in articles)
            {
                string img_src = "", img_title = "", img_description = "", date = "", post_title = "", link = "";
                DateTime form_date = new DateTime();

                // Pega a URL da imagem
                try
                {
                    img_src = article
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .GetAttributeValue("src", "");
                }
                catch
                {
                    Console.WriteLine("Não foi possível pegar a URL da imagem");
                }

                // Pega o título da imagem
                try
                {
                    img_title = article
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .GetAttributeValue("data-image-title", "");
                }
                catch
                {
                    Console.WriteLine("Não foi possível pegar o título da imagem");
                }

                // Pega a descrição da imagem
                try
                {
                    img_description = article
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .FirstChild
                    .GetAttributeValue("data-image-description", "");
                }
                catch
                {
                    Console.WriteLine("Não foi possível pegar a descrição da imagem");
                }

                // Pega a data da postagem
                try
                {
                    date = article
                    .FirstChild
                    .FirstChild
                    .Descendants("ul")
                    .FirstOrDefault()
                    .FirstChild
                    .InnerText;

                    form_date = DateTime.ParseExact(date, "dd/MM/yyyy",
                                System.Globalization.CultureInfo.InvariantCulture);
                }
                catch
                {
                    Console.WriteLine("Não foi possível pegar a data da postagem");
                }

                // Pega o título da postagem
                try
                {
                    post_title = article
                    .FirstChild
                    .FirstChild
                    .Descendants("h1")
                    .FirstOrDefault()
                    .FirstChild
                    .InnerText;
                }
                catch
                {
                    Console.WriteLine("Não foi possível pegar o título da postagem");
                }

                // Pega a URL da postagem
                try
                {
                    link = article
                    .FirstChild
                    .FirstChild
                    .Descendants("h1")
                    .FirstOrDefault()
                    .FirstChild
                    .GetAttributeValue("href", "");
                }
                catch
                {
                    Console.WriteLine("Não foi possível pegar a URL da postagem");
                }

                Console.WriteLine("URL da imagem: " + img_src);
                Console.WriteLine("Título da imagem: " + img_title);
                Console.WriteLine("Descrição da imagem: " + img_description);
                Console.WriteLine("Link da postagem: " + link);
                Console.WriteLine("Data da postagem: " + date);
                Console.WriteLine("Título da postagem: " + post_title);
                Console.WriteLine("========================================================================================");

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
                    ImgTitle = img_title,
                    Description = img_description
                };

                imgs.Add(img);

            }

            // Busca qual é a última página de notícias
            if (page == 1)
            {
                try
                {
                    nav = htmlDocument.DocumentNode
                        .Descendants("nav")
                        .Where(node => node
                        .GetAttributeValue("id", "")
                        .Equals("pagenav"));

                    var pages = nav.FirstOrDefault().Descendants("a");

                    foreach (var pag in pages)
                    {
                        int pag_int;
                        bool isParsable = Int32.TryParse(pag.InnerText, out pag_int);

                        if (isParsable && pag_int > max_page)
                            max_page = pag_int;
                    }
                }
                catch
                {
                    Console.WriteLine("Não foi possível achar a quantidade de páginas");
                }

                saveDB(imgs);

                // Navega para outras páginas
                if (max_page > 1)
                    for (int i = 2; i < max_page; i++)
                    {
                        Console.WriteLine("========================================================================================");
                        Console.WriteLine("Digite 0 para parar o WebCrawler ou qualquer outra tecla para ir para a próxima página: ");
                        Console.WriteLine("========================================================================================");
                        string res = Console.ReadLine();

                        if (res == "0")
                            break;
                        else
                            startCrawling(i);

                    }

            }
        }
    }
}
