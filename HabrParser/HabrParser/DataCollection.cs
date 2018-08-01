﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;


namespace DataCollectionNameSpace
{
    struct InfoSite
    {
        public string name;
        public string link;
        public int rating;
        public int bootmarks;
        public double views;
        public int numbOfComments;
        public double dateOfPublication;
        public List<string> labels;
        public List<string> habs;

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
        private readonly string HABS = "//li[@class='inline-list__item inline-list__item_hub']/a";

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

        public List<InfoSite> MainDataCollection(List<string> links, List<InfoSite> myInfoSite)
       {
            InfoSite infoSite = new InfoSite();
            foreach (string link in links)
            {
                myInfoSite.Add(DataCollectionOnSite(link, infoSite));
                Console.WriteLine(myInfoSite.Count);
            }
            return myInfoSite;
        }

        private InfoSite DataCollectionOnSite(string url, InfoSite infoSite)
        {
            var htmlDoc = new HtmlWeb().Load(url);
            
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
            string indexMonth;
            buf = buf.TrimStart(' ').TrimEnd(' ');
            var res = buf.Split(' ');
            months.TryGetValue(res[1], out indexMonth);
            buf = buf.Replace(res[1], indexMonth);
            //----------------------------------------------------

            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime result = DateTime.Now;

            switch (res.Length)
            {
                case 5:
                    result = DateTime.ParseExact(buf, "d MM yyyy в HH:mm", provider);
                    break;
                case 4:
                    result = DateTime.ParseExact(buf, "d MM в HH:mm", provider);
                    break;
                case 3:
                    if (buf.Contains("сегодня"))
                    {
                        result = DateTime.Now;
                    }
                    else if (buf.Contains("вчера"))
                    {
                        result = DateTime.Now.AddDays(-1);
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
            var nodes = htmlDoc.DocumentNode
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
                               .SelectNodes(HABS);

            infoSite.habs = new List<string>();
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    infoSite.habs.Add(node.InnerText);
                }
            }


            return infoSite;
        }
    }
}
