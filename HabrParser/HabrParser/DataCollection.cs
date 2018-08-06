﻿using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Globalization;
using System.Threading.Tasks;


namespace DataCollectionNameSpace
{
    public struct InfoSite
    {
        public string name;
        public string link;
        public int rating;
        public int bootmarks;
        public double views;
        public int numbOfComments;
        public double dateOfPublication;
        public List<string> labels;
        public List<string> hubs;

    }

    public struct InfoMoreBlogsWithHabr
    {
        public string hrefBlogs;
        public int searchDepth;
        public List<InfoSite> InfoSingeBlogs;
        public string pathOutFile;
    }




    class DataCollection
    {
        private readonly string NAME = "//span[@class='post__title-text']";
        private readonly string LINK = "//head/link[@rel='canonical']";
        private readonly string RAITING = "//ul/li/div[@class='voting-wjt voting-wjt_post js-post-vote']/span";
        private readonly string BOOTMARKS = "//span[@class='bookmark__counter js-favs_count']";
        private readonly string VIEWS = "//span[@class='post-stats__views-count']";
        private readonly string COMMENTS = "//a[@class='post-stats__comments-link']";
        private readonly string DATE = "//span[@class='post__time']";
        private readonly string LABELS = "//li[@class='inline-list__item inline-list__item_tag']/a";
        private readonly string HUBS = "//li[@class='inline-list__item inline-list__item_hub']/a";

        private readonly Dictionary<string, string> months = new Dictionary<string, string>()
                    {
                        {"января", "01"},
                        {"февраля", "02"},
                        {"марта", "03"},
                        {"апреля", "04" },
                        {"мая", "05" },
                        {"июня", "06" },
                        {"июля", "07" },
                        {"августа", "08" },
                        {"сентября", "09" },
                        {"октября", "10" },
                        {"ноября", "11" },
                        {"декабря", "12" }
                    };


        public List<InfoSite> MainDataCollection(List<string> links)
        {
            InfoSite infoSite = new InfoSite();
            List<InfoSite> myInfoSite = new List<InfoSite>();

            //Рабочее решение!!!!!!!!!!!!!!!
            List<Task<InfoSite>> tasks1 = new List<Task<InfoSite>>(10);

            for (int index = 0; index < links.Count; index++)
            {
                string url = links[index];
                tasks1.Add(new Task<InfoSite>(() => DataCollectionOnSite(url, infoSite)));
            }

            foreach (var t in tasks1)
            {
                t.Start();
            }

            Task.WaitAll(tasks1.ToArray());

            // ожидаем завершения задач 

            foreach (var t in tasks1)
                myInfoSite.Add(t.Result);
            //---------------------------------------------------


            //    for (int i = 0; i < links.Count / 2; i++)
            //{
            //    Task<InfoSite> task1 = new Task<InfoSite>(() => DataCollectionOnSite(links[i], infoSite));
            //    Task<InfoSite> task2 = new Task<InfoSite>(() => DataCollectionOnSite(links[i + 5], infoSite));
            //    task1.Start();
            //    task2.Start();
            //    //task1.Wait();
            //    //task2.Wait();
            //    myInfoSite1.Add(task1.Result);
            //    myInfoSite2.Add(task2.Result);

            //}
            //foreach (var info in myInfoSite1) {
            //    myInfoSite.Add(info);
            //}
            //foreach (var info in myInfoSite2) {
            //    myInfoSite.Add(info);
            //}
            //Task[] tasks = new Task[2]
            //{
            //    task1,
            //    task2
            //}; 
            //foreach(var task in tasks)
            //{
            //    task.Start();
            //}
            //Task.WaitAll(tasks);
            return myInfoSite;
        }

        private InfoSite DataCollectionOnSite(string url, InfoSite infoSite)
        {

            bool check = false;
            HtmlDocument htmlDoc = null;
            while (!check)
            {
                try
                {
                    htmlDoc = new HtmlWeb().Load(url);
                    check = true;
                }
                catch
                {
                    check = false;
                    Console.WriteLine("Проверьте соединение с интернетом и нажмите Enter");
                    Console.ReadKey();
                }
            }

            // Поиск названия сайта       
            infoSite.name = htmlDoc.DocumentNode
                                   .SelectSingleNode(NAME).InnerText;
            
            // Поиск ссылки сайта       
            infoSite.link = htmlDoc.DocumentNode
                                   .SelectSingleNode(LINK).Attributes["href"].Value;

            string buf = "";
            // Поиск общего рейтинга сайта       
            buf = htmlDoc.DocumentNode                     
                         .SelectSingleNode(RAITING).InnerText;
            
            if (   buf.StartsWith("–")
                || buf.StartsWith("‐")
                || buf.StartsWith("−")
                || buf.StartsWith("—"))
            {
                buf = buf.Remove(0, 1);
                infoSite.rating = Convert.ToInt32(buf) * (-1);
            }
            else
            {
                infoSite.rating = Convert.ToInt32(buf);
            }

            // Поиск колличества закладок данного сайта       
            buf = htmlDoc.DocumentNode
                         .SelectSingleNode(BOOTMARKS).InnerText;
            infoSite.bootmarks = Convert.ToInt32(buf);

            //Поиск колличества просмотров 
            buf = htmlDoc.DocumentNode
                         .SelectSingleNode(VIEWS).InnerText;

            if (buf.Contains('k'))
            {
                buf = buf.Substring(0, buf.Length - 1);
                infoSite.views = Convert.ToDouble(buf) * 1000;
            }
            else
            {
                infoSite.views = Convert.ToDouble(buf);
            }


            // Поиск колличества комментариев на сайте       
            buf = htmlDoc.DocumentNode
                         .SelectSingleNode(COMMENTS).InnerText;

            int.TryParse(buf, out infoSite.numbOfComments);

            // Поиск даты создания сайта       
            buf = htmlDoc.DocumentNode
                         .SelectSingleNode(DATE).InnerText;
            //Замена буквенного представления месяца на численное
            
            buf = buf.TrimStart(' ').TrimEnd(' ');
            string[] res = buf.Split(' ');

            string indexMonth;
            months.TryGetValue(res[1], out indexMonth);
            //----------------------------------------------------

            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime result = DateTime.Now;

            switch (res.Length)
            {
                case 5:
                    buf = buf.Replace(res[1], indexMonth);
                    result = DateTime.ParseExact(buf, "d MM yyyy в HH:mm", provider);
                    break;
                case 4:
                    buf = buf.Replace(res[1], indexMonth);
                    result = DateTime.ParseExact(buf, "d MM в HH:mm", provider);
                    break;
                case 3:
                    if (buf.Contains("сегодня"))
                    {
                        DateTime time = DateTime.ParseExact(buf, "сегодня в HH:mm", provider);
                        result = DateTime.Now.Date.Add(new TimeSpan(time.Hour, time.Minute, time.Second));
                    }
                    else if (buf.Contains("вчера"))
                    {
                        DateTime time = DateTime.ParseExact(buf, "вчера в HH:mm", provider);
                        result = DateTime.Now.AddDays(-1);
                        result = DateTime.Now.Date.Add(new TimeSpan(time.Hour, time.Minute, time.Second));
                    }
                    break;
                default:
                    result = DateTime.Now;
                    break;
            }


            //Перевод даты в UNIXTIME число в секундах
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan unixTimeDate = result - origin;
            infoSite.dateOfPublication = unixTimeDate.TotalSeconds;

            // Поиск меток, присутствующих на сайте       
            HtmlNodeCollection nodes = htmlDoc.DocumentNode
                               .SelectNodes(LABELS);

            infoSite.labels = new List<string>();
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    infoSite.labels.Add(node.InnerText);
                }
            }

            //Поиск хабов, расположенных на сайте
            nodes = htmlDoc.DocumentNode
                               .SelectNodes(HUBS);

            infoSite.hubs = new List<string>();
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    infoSite.hubs.Add(node.InnerText);
                }
            }


            return infoSite;
        }
    }
}
