using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bot_Telegram_Ver3
{
    internal class HttpConnect
    {
        private HttpClient httpClient = new HttpClient();
        private string hocKy = ConfigurationManager.AppSettings["hocKy"].ToString();

        public async Task<string> GetRequest(string url)
        {
            // Gửi yêu cầu GET để lấy trang
            HttpResponseMessage response = await httpClient.GetAsync(url); // Sử dụng await
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync(); // Sử dụng await

            return responseBody;
        }

        public async Task<string> SendPostRequest(string viewState, string url, string mode, string msv, string hocKyXemDiem)
        {
            FormUrlEncodedContent formData = null;

            try
            {
                
                if (mode == "XemDiem")
                {
                    formData = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("__EVENTTARGET", "ctl00$ContentPlaceHolder1$ctl00$btnChonHK"),
                    new KeyValuePair<string, string>("__EVENTARGUMENT", ""),
                    new KeyValuePair<string, string>("__VIEWSTATE", viewState),
                    new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", ""),
                    new KeyValuePair<string, string>("ctl00$ContentPlaceHolder1$ctl00$txtChonHK", hocKyXemDiem),
                    new KeyValuePair<string, string>("ctl00$ContentPlaceHolder1$ctl00$btnChonHK", "Xem"),
                });
                }
                else if (mode == "ThemDuLieu")
                {
                    formData = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("__EVENTTARGET", "ctl00$ContentPlaceHolder1$ctl00$rad_ThuTiet"),
                    new KeyValuePair<string, string>("__EVENTARGUMENT", ""),
                    new KeyValuePair<string, string>("__LASTFOCUS", ""),
                    //new KeyValuePair<string, string>("__VIEWSTATE", viewState),
                    new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", ""),
                    new KeyValuePair<string, string>("ctl00$ContentPlaceHolder1$ctl00$ddlChonNHHK", hocKy),
                    new KeyValuePair<string, string>("ctl00$ContentPlaceHolder1$ctl00$rad_ThuTiet", "rad_ThuTiet"),
                });
                }
                else
                {
                    formData = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("__EVENTTARGET", ""),
                    new KeyValuePair<string, string>("__EVENTARGUMENT", ""),
                    new KeyValuePair<string, string>("__LASTFOCUS", ""),
                    new KeyValuePair<string, string>("__VIEWSTATE", viewState),
                    new KeyValuePair<string, string>("__EVENTVALIDATION", ""),
                    new KeyValuePair<string, string>("ctl00$ContentPlaceHolder1$ctl00$txtCaptcha", mode),
                    new KeyValuePair<string, string>("ctl00$ContentPlaceHolder1$ctl00$btnXacNhan", "Vào website"),
                    });
                }
            }
            catch (UriFormatException ex)
            {
                Console.WriteLine("URI format error: " + ex.Message);
            }

            HttpResponseMessage response = await httpClient.PostAsync(url, formData);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;

        }

        public string GetViewSate(string html)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();

            htmlDoc.LoadHtml(html);
            var viewSateNode = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='__VIEWSTATE']");
            string viewState = viewSateNode.GetAttributeValue("value", "");

            return viewState;


        }
    }
}
