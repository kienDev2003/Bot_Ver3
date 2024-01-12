using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot_Telegram_Ver3
{
    internal class Controller
    {
        Model model = new Model();
        Home home = new Home();

        private string urlTkb = ConfigurationManager.AppSettings["urlTKB"].ToString();
        private string urlLt = ConfigurationManager.AppSettings["urlLichThi"].ToString();
        private static string hocKy = ConfigurationManager.AppSettings["hocKy"].ToString();
        private char _hocKy = hocKy.Last();

        public string GuiTKB(string chatID, string ngay, string thu)
        {
            int tuan = ChuyenNgaySangTuan(chatID, ngay);
            string queryTKB = $"SELECT * FROM tblDataTkb WHERE NgayHoc = '{thu}' AND NgayBatDauHoc <= '{ngay}' AND NgayKetThucHoc >= '{ngay}' AND ChatID = '{chatID}' ORDER By TietBD ASC;";
            string queryTTSV = $"SELECT * FROM tblTTSV WHERE ChatID = '{chatID}'";
            string data = model.GetTKB(queryTKB, tuan);
            string ttsv = model.GetTTSV(queryTTSV);
            return $"{ttsv}\n\n<b>Thời Khóa Biểu Thứ {thu}:</b>\n\n{data}";
        }

        public string GuiTKBTQTuan(int mode, string chatID, string ngay)
        {
            if (mode == 1)
            {
                int tuan = ChuyenNgaySangTuan(chatID, ngay);
                if (tuan == 0) return "Tuần 0\n\nNghỉ";
                string ngayBDTuan = ChuyenTuanSangNgayBatDauTuan(chatID, tuan);
                DateTime _ngay = DateTime.ParseExact(ngayBDTuan, "yyyy-MM-dd", CultureInfo.CurrentCulture);
                string queryTTSV = $"SELECT * FROM tblTTSV WHERE ChatID = '{chatID}'";
                string ttsv = model.GetTTSV(queryTTSV);
                string dataALL = "";

                for (int i = 0; i < 7; i++)
                {
                    string thu = ChuyenThuTiengAnhSangTiengViet(_ngay.AddDays(i).DayOfWeek);
                    string queryTKB = $"SELECT * FROM tblDataTkb WHERE NgayHoc = '{thu}' AND NgayBatDauHoc <= '{ngayBDTuan}' AND NgayKetThucHoc >= '{ngayBDTuan}' AND ChatID = '{chatID}' ORDER By TietBD ASC;";

                    string data = model.GetTKB(queryTKB, tuan);

                    dataALL += $"<b>Thời Khóa Biểu Thứ {thu}:</b>\n\n{data}";
                }

                return $"{ttsv}\n\n<b>TUẦN: {tuan}</b>\n{dataALL}";
            }
            if (mode == 2)
            {
                DateTime _ngay = DateTime.ParseExact(ngay, "yyyy-MM-dd", CultureInfo.CurrentCulture);
                ngay = _ngay.AddDays(7).ToString("yyyy-MM-dd");
                int tuan = ChuyenNgaySangTuan(chatID, ngay);
                if (tuan == 0) return "Tuần 0\n\nNghỉ";
                string ngayBDTuan = ChuyenTuanSangNgayBatDauTuan(chatID, tuan);
                DateTime __ngay = DateTime.ParseExact(ngayBDTuan, "yyyy-MM-dd", CultureInfo.CurrentCulture);
                string queryTTSV = $"SELECT * FROM tblTTSV WHERE ChatID = '{chatID}'";
                string ttsv = model.GetTTSV(queryTTSV);
                string dataALL = "";

                for (int i = 0; i < 7; i++)
                {
                    string thu = ChuyenThuTiengAnhSangTiengViet(__ngay.AddDays(i).DayOfWeek);
                    string queryTKB = $"SELECT * FROM tblDataTkb WHERE NgayHoc = '{thu}' AND NgayBatDauHoc <= '{ngayBDTuan}' AND NgayKetThucHoc >= '{ngayBDTuan}' AND ChatID = '{chatID}' ORDER By TietBD ASC;";

                    string data = model.GetTKB(queryTKB, tuan);

                    dataALL += $"<b>Thời Khóa Biểu Thứ {thu}:</b>\n\n{data}";
                }

                return $"{ttsv}\n\n<b>TUẦN: {tuan}</b>\n{dataALL}";
            }
            return "";
        }

        public string GuiLichThi(string chatID, string ngay)
        {
            string queryTTSV = $"SELECT * FROM tblTTSV WHERE ChatID = '{chatID}'";
            string queryLichThi = $"SELECT * FROM tblDataLichThi WHERE NgayThi >= '{ngay}' AND ChatID = '{chatID}';";
            string data = model.GetLichThi(queryLichThi);
            string ttsv = model.GetTTSV(queryTTSV);

            return $"{ttsv}\n\n<b>Có lịch thi kết thúc học phần:</b>\n\n{data}";
        }

        public void GuiLichThiAuto(TelegramBotClient bot)
        {
            string ngay = DateTime.Now.ToString("yyyy-MM-dd");
            string[] chatID = model.ListNguoiBatAuto();
            for (int i = 0; i < chatID.Length; i++)
            {
                string _chatID = chatID[i].ToString();

                string queryTTSV = $"SELECT * FROM tblTTSV WHERE ChatID = '{_chatID}'";
                string queryLichThi = $"SELECT * FROM tblDataLichThi WHERE NgayThi > '{ngay}' AND ChatID = '{_chatID}';";
                string data = model.GetLichThiModeAuto(queryLichThi);
                if (data == "") 
                {
                    continue;
                }
                string ttsv = model.GetTTSV(queryTTSV);

                string dataAll = $"{ttsv}\n\n<b>Có lịch thi kết thúc học phần:</b>\n\n{data}";
                bot.SendTextMessageAsync(chatID[i], dataAll, Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }

        public void GuiTKBAuto(TelegramBotClient bot)
        {
            DateTime date = DateTime.Now.AddDays(1);
            string ngay = date.ToString("yyyy-MM-dd");
            string thu = ChuyenThuTiengAnhSangTiengViet(date.DayOfWeek);
            string[] chatID = model.ListNguoiBatAuto();
            for (int i = 0; i < chatID.Length; i++)
            {
                string _chatID = chatID[i].ToString();
                int tuan = ChuyenNgaySangTuan(_chatID, ngay);

                string queryTKB = $"SELECT * FROM tblDataTkb WHERE NgayHoc = '{thu}' AND NgayBatDauHoc <= '{ngay}' AND NgayKetThucHoc >= '{ngay}' AND ChatID = '{_chatID}' ORDER By TietBD ASC;";
                string queryTTSV = $"SELECT * FROM tblTTSV WHERE ChatID = '{_chatID}'";
                string data = model.GetTKB(queryTKB, tuan);
                string ttsv = model.GetTTSV(queryTTSV);
                string dataAll =  $"{ttsv}\n\n<b>Thời Khóa Biểu Thứ {thu}:</b>\n\n{data}";

                bot.SendTextMessageAsync(_chatID, dataAll, ParseMode.Html);
            }
        }

        public void GuiThongBao(TelegramBotClient bot, string text)
        {
            string[] chatID = model.ListNguoiBatAuto();
            for(int i =0; i < chatID.Length; i++)
            {
                string _chatID = chatID[i].ToString();
                bot.SendTextMessageAsync(_chatID, text, ParseMode.Html);
            }
            
        }

        public int XoaDuLieu(string chatID)
        {
            string query = $"DELETE FROM tblDataLichThi WHERE ChatID = '{chatID}';" +
                                $"DELETE FROM tblDataTkb WHERE ChatID = '{chatID}';" +
                                $"DELETE FROM tblTTSV WHERE ChatID = '{chatID}';" +
                                $"DELETE FROM tblKiemTra WHERE ChatID = '{chatID}';";
            return model.Command(query);
        }

        public string ThemDuLieu(string chatId, string maSv)
        {
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            var chromeDriver = new ChromeDriver(service, chromeOptions);

            bool themTKB = ThemDuLieuTKB(chromeDriver, chatId, maSv);
            bool themLT = ThemDuLieuLT(chromeDriver, chatId, maSv);

            if (themTKB && themLT == false)
            {
                string text = $"Thêm dữ liệu TKB thành công!\n" +
                              $"Chưa có lịch thi";
                chromeDriver.Quit();
                return text;
            }
            else if (themLT && themTKB == false)
            {
                string text = $"Thêm dữ liệu TKB KHÔNG thành công!\n" +
                              $"Vui lòng kiểm tra lại MSV / WEB trường đang bảo trì!";
                chromeDriver.Quit();
                return text;
            }
            else if (themTKB && themLT)
            {
                string text = $"Thêm dữ liệu thành công";
                chromeDriver.Quit();
                return text;
            }
            else
            {
                string text = $"Thêm dữ liệu TKB KHÔNG thành công!\n" +
                              $"Vui lòng kiểm tra lại MSV / WEB trường đang bảo trì!";
                chromeDriver.Quit();
                return text;
            }
        }

        private bool ThemDuLieuLT(ChromeDriver chromeDriver, string chatId, string maSv)
        {
            try
            {
                string urlLT = urlLt + maSv;
                string html = LayHtmlLichThi(chromeDriver, maSv, urlLT, hocKy);
                if (html == "") return false;
                if (PhanTichDuLieuVaThemVaoCSDL_LT(html, chatId, maSv) == false) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool PhanTichDuLieuVaThemVaoCSDL_LT(string html, string chatId, string maSv)
        {
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);

                var tables = document.DocumentNode.Descendants("table")
                                    .Where(table => table.Attributes.Contains("class") && table.Attributes["class"].Value == "grid-view").FirstOrDefault();

                if (tables == null)
                {
                    throw new Exception("");
                }
                ThemChuoiHTMLTKBVaoLT(tables.OuterHtml.Trim(), chatId, maSv);

                var trNodes = document.DocumentNode.Descendants("tr")
                            .Where(tr => tr.Attributes.Contains("onmouseover") && tr.Attributes["onmouseover"].Value == "className ='rowOnmouseover-GridView '");
                foreach (var trNode in trNodes)
                {
                    var tdNodes = trNode.Descendants("td").ToArray();

                    string maMh = tdNodes[1].InnerText.Trim();
                    string tenMH = tdNodes[2].InnerText.Trim();
                    string phongThi = tdNodes[9].InnerText.Trim();
                    string tietBD = tdNodes[7].InnerText.Trim();
                    string soTiet = tdNodes[8].InnerText.Trim();
                    string ngayThi = tdNodes[6].InnerText.Trim();
                    ngayThi = DateTime.ParseExact(ngayThi, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                    string query = $"INSERT INTO tblDataLichThi (ChatID,MaSV,MaMH,TenMH,TietBD,SoTiet,NgayThi,PhongThi)" +
                                   $" VALUES ('{chatId}','{maSv}','{maMh}','{tenMH}','{tietBD}','{soTiet}','{ngayThi}','{phongThi}')";
                    int check = model.Command(query);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ThemChuoiHTMLTKBVaoLT(string v, object chatID, object maSV)
        {
            string query = $"UPDATE tblKiemTra SET LichThi = @htmlLt WHERE ChatID = '{chatID}' AND MaSV = '{maSV}'";
            int check = model.CommandModeKiemTra(query, "", v);
        }

        private string LayHtmlLichThi(ChromeDriver chromeDriver, string maSv, string urlLT, string hocKy)
        {
            try
            {
                chromeDriver.Navigate().GoToUrl(urlLT);

                if (CheckAlert(chromeDriver))
                {
                    IAlert alert = chromeDriver.SwitchTo().Alert();
                    alert.Accept();
                }

                string htmlLichThi = chromeDriver.PageSource;
                return htmlLichThi;

            }
            catch
            {
                return "";
            }
        }

        private bool ThemDuLieuTKB(ChromeDriver chromeDriver, string chatId, string maSv)
        {
            try
            {
                string urlTKB = urlTkb + maSv;
                string html = LayHtmlTKB(chromeDriver, maSv, urlTKB, hocKy);
                if (html == "") return false;
                if (PhanTichDuLieuVaThemVaoCSDL_TKB(html, chatId, maSv) == false) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool PhanTichDuLieuVaThemVaoCSDL_TKB(string html, string chatId, string maSv)
        {
            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);

                //Thêm Thông Tin Sinh Viên
                var ttsv = document.DocumentNode.Descendants("div")
                .Where(d => d.Attributes.Contains("id") && d.Attributes["id"].Value.Contains("ctl00_ContentPlaceHolder1_ctl00_pnlTKB")).FirstOrDefault();
                var tables = ttsv.Descendants("table");
                foreach (var table in tables)
                {
                    var span = table.Descendants("span").ToList();

                    string hoTen = span[3].InnerText.Trim();
                    string lop = span[5].InnerText.Trim();

                    string queryInsertTTSV = $"INSERT INTO tblTTSV (ChatID,MaSV,TenSV,Lop) VALUES ('{chatId}','{maSv}','{hoTen}','{lop}')";
                    int check = model.Command(queryInsertTTSV);
                }
                //----------------------------------

                var tuanBDHoc = document.DocumentNode.Descendants("span")
                    .Where(span => span.Attributes.Contains("id") && span.Attributes["id"].Value == "ctl00_ContentPlaceHolder1_ctl00_lblNote").FirstOrDefault();
                string _tuanBDHoc = TachLayNgayDauTienBDTuan(tuanBDHoc.InnerText.Trim());

                string queryInsertNBDTH = $"UPDATE tblTTSV SET NBDHK = '{_tuanBDHoc}' WHERE ChatID = '{chatId}' AND MaSV = '{maSv}'";
                int check1 = model.Command(queryInsertNBDTH);
                //----------------------------------
                // Thêm Dữ Liệu TKB
                int s = 0;
                string maMH = "";
                string tenMH = "";
                string nhomMH = "";
                string STC = "";
                string maLop = "";
                string NgayHoc = "";
                string tietBD = "";
                string soTiet = "";
                string Phong = "";
                string tuanHoc = "";
                string _ngayHocTong = "";
                string ngayBDHoc = "";
                string ngayKTHoc = "";

                var TKB = document.DocumentNode.Descendants("div")
                .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("grid-roll2")).FirstOrDefault();

                ThemChuoiHTMLTKBVaoCSDL(TKB.OuterHtml.Trim(), chatId, maSv);

                var tableS = TKB.Descendants("table");
                foreach (var table in tableS)
                {
                    var tdS = table.Descendants("td").ToList();
                    foreach (var _TKB in tdS)
                    {

                        maMH = tdS.ElementAt(0).InnerText.Trim();
                        tenMH = tdS.ElementAt(1).InnerText.Trim();
                        nhomMH = tdS.ElementAt(2).InnerText.Trim();
                        STC = tdS.ElementAt(3).InnerText.Trim();
                        maLop = tdS.ElementAt(4).InnerText.Trim();
                        NgayHoc = tdS.ElementAt(8).InnerText.Trim();
                        tietBD = tdS.ElementAt(9).InnerText.Trim();
                        soTiet = tdS.ElementAt(10).InnerText.Trim();
                        Phong = tdS.ElementAt(11).InnerText.Trim();
                        tuanHoc = tdS.ElementAt(13).InnerText.Trim();

                        var NgayHocTong = tdS.ElementAt(13).SelectSingleNode("div");
                        _ngayHocTong = NgayHocTong.GetAttributeValue("onmouseover", "").ToString();
                        //Cắt chuỗi lấy dữ liệu ngày học tổng
                        int diemBDCat = _ngayHocTong.IndexOf("('");
                        int diemDungCat = _ngayHocTong.LastIndexOf("')");
                        if (diemBDCat >= 0 && diemDungCat >= 0)
                        {
                            string desiredText = _ngayHocTong.Substring(diemBDCat + 2, diemDungCat - diemBDCat - 2);
                            string originalText = desiredText;
                            string[] parts = originalText.Split(new string[] { "--" }, StringSplitOptions.None);

                            if (parts.Length >= 2)
                            {
                                ngayBDHoc = parts[0];
                                ngayKTHoc = parts[1];
                                ngayBDHoc = DateTime.ParseExact(ngayBDHoc, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                                ngayKTHoc = DateTime.ParseExact(ngayKTHoc, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                            }

                            string query = $"INSERT INTO tblDataTkb (ChatID,MaSV,MaMH,TenMH,NhomMH,STC,MaLop,NgayHoc,TietBD,SoTiet,PhongHoc,TuanHoc,NgayBatDauHoc,NgayKetThucHoc) VALUES " +
                                                $"('{chatId}','{maSv}','{maMH}','{tenMH}','{nhomMH}','{STC}','{maLop}','{NgayHoc}','{tietBD}','{soTiet}','{Phong}','{tuanHoc}','{ngayBDHoc}','{ngayKTHoc}')";

                            int check3 = model.Command(query);
                            if(check3 > 0)
                            {
                                break;
                            }
                            }
                    }
                }
                return true;
                //----------------------------------
            }
            catch
            {
                return false;
            }
        }

        private void ThemChuoiHTMLTKBVaoCSDL(string v, object chatID, object maSV)
        {
            string query = $"INSERT INTO tblKiemTra (ChatID,MaSV,Tkb,LichThi) VALUES ('{chatID}','{maSV}',@htmlTkb,@htmlLt)";
            int check = model.CommandModeKiemTra(query, v, "");
        }

        private string LayHtmlTKB(ChromeDriver chromeDriver, string maSv, string urlTKB, string hocKy)
        {
            string html;
            string htmlAll = "";
            try
            {

                chromeDriver.Navigate().GoToUrl(urlTKB);
                htmlAll = chromeDriver.PageSource;

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlAll);

                var capcha = doc.DocumentNode.Descendants("span").Where(span => span.Attributes.Contains("id") && span.Attributes["id"].Value == "ctl00_ContentPlaceHolder1_ctl00_lblCapcha").FirstOrDefault();
                if (capcha != null)
                {
                    string _capcha = capcha.InnerText.Trim();
                    var txtCapcha = chromeDriver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_txtCaptcha"));
                    txtCapcha.SendKeys(_capcha);
                    chromeDriver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_btnXacNhan")).Click();
                    Thread.Sleep(1000);
                    chromeDriver.Navigate().GoToUrl(urlTKB);
                }
                Thread.Sleep(1000);
                if (CheckAlert(chromeDriver))
                {
                    IAlert alert = chromeDriver.SwitchTo().Alert();
                    alert.Accept();
                }
                IWebElement element = chromeDriver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_rad_ThuTiet"));
                element.Click();
                Thread.Sleep(1000);

                IWebElement _selectHocKy = chromeDriver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_ddlChonNHHK"));
                SelectElement select = new SelectElement(_selectHocKy);
                select.SelectByValue(hocKy);
                Thread.Sleep(1000);

                if (CheckAlert(chromeDriver))
                {
                    IAlert alert = chromeDriver.SwitchTo().Alert();
                    alert.Accept();
                }

                html = chromeDriver.PageSource;

                return html;

            }
            catch
            {
                return "";
            }
        }

        public void KiemTraThayDoi(TelegramBotClient bot)
        {
            string[] chatID = model.ListNguoiBatAuto();

            for(int i = 0; i < chatID.Length; i++)
            {
                string _chatID = chatID[i].ToString();

                string query = $"SELECT * FROM tblKiemTra WHERE ChatID = '{_chatID}'";
                string htmlTkbCu = model.GetChuoiHtmlTkb(query);
                string htmlHtmlLt = model.GetChuoiHtmlLt(query);
                string maSV = model.GetMaSVKiemTra(query);

                bool tkb = KiemTraTkb(maSV, _chatID, htmlTkbCu);
                bool lt = KiemTraLichThi(maSV, _chatID, htmlHtmlLt);

                if (tkb && lt)
                {
                    bot.SendTextMessageAsync(_chatID, $"<b>DỮ LIỆU</b> của bạn có sự thay đổi\n" +
                                                           $"Hãy thêm lại dữ liệu và bật tự động thông báo ( nếu cần )", ParseMode.Html);
                }
                else if (lt)
                {
                    bot.SendTextMessageAsync(_chatID, $"<b>LỊCH THI</b> của bạn có sự thay đổi\n" +
                                                           $"Hãy thêm lại dữ liệu và bật tự động thông báo ( nếu cần )", ParseMode.Html);
                }
                else if (tkb)
                {
                    bot.SendTextMessageAsync(_chatID, $"<b>THỜI KHÓA BIỂU</b> của bạn có sự thay đổi\n" +
                                                           $"Hãy thêm lại dữ liệu và bật tự động thông báo ( nếu cần )", ParseMode.Html);
                }
                else return;
            }
        }

        private bool KiemTraLichThi(string maSV, string chatID, string htmlLichThi)
        {
            string _urlLichThi = urlLt + maSV;
            string htmlLichThiNew;
            string html;

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            using (var chromeDriver = new ChromeDriver(service,chromeOptions))
            {
                chromeDriver.Navigate().GoToUrl("https://daotao.vnua.edu.vn/Default.aspx?page=gioithieu");
                string htmlAll = chromeDriver.PageSource;

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlAll);

                var capcha = doc.DocumentNode.Descendants("span").Where(span => span.Attributes.Contains("id") && span.Attributes["id"].Value == "ctl00_ContentPlaceHolder1_ctl00_lblCapcha").FirstOrDefault();
                if (capcha != null)
                {
                    string _capcha = capcha.InnerText.Trim();
                    var txtCapcha = chromeDriver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_txtCaptcha"));
                    txtCapcha.SendKeys(_capcha);
                    Thread.Sleep(1000);
                    chromeDriver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_btnXacNhan")).Click();
                    Thread.Sleep(1000);
                }
                Thread.Sleep(1000);
                chromeDriver.Navigate().GoToUrl(_urlLichThi);
                Thread.Sleep(1000);

                if (CheckAlert(chromeDriver))
                {
                    IAlert alert = chromeDriver.SwitchTo().Alert();
                    alert.Accept();
                }

                IWebElement _selectHocKy = chromeDriver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_dropNHHK"));
                SelectElement select = new SelectElement(_selectHocKy);
                select.SelectByValue(hocKy);
                Thread.Sleep(1000);

                html = chromeDriver.PageSource;

                chromeDriver.Quit();

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var tables = htmlDocument.DocumentNode.Descendants("table")
                    .Where(table => table.Attributes.Contains("class") && table.Attributes["class"].Value == "grid-view").FirstOrDefault();
                if (tables == null)
                {
                    htmlLichThiNew = "";
                }
                else
                {
                    htmlLichThiNew = tables.OuterHtml.Trim();
                }

                if (htmlLichThiNew != htmlLichThi)
                {
                    return true;
                }

            }
            return false;
        }

        private bool KiemTraTkb(string maSV, string chatID, string htmlTkb)
        {
            string urlTKB = urlTkb + maSV;
            string html;

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            using (var chromeDriver = new ChromeDriver(service,chromeOptions))
            {
                chromeDriver.Navigate().GoToUrl("https://daotao.vnua.edu.vn/Default.aspx?page=gioithieu");
                string htmlAll = chromeDriver.PageSource;

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlAll);

                var capcha = doc.DocumentNode.Descendants("span").Where(span => span.Attributes.Contains("id") && span.Attributes["id"].Value == "ctl00_ContentPlaceHolder1_ctl00_lblCapcha").FirstOrDefault();
                if (capcha != null)
                {
                    string _capcha = capcha.InnerText.Trim();
                    var txtCapcha = chromeDriver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_txtCaptcha"));
                    txtCapcha.SendKeys(_capcha);
                    Thread.Sleep(1000);
                    chromeDriver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_btnXacNhan")).Click();
                    Thread.Sleep(1000);
                }
                Thread.Sleep(1000);
                chromeDriver.Navigate().GoToUrl(urlTkb);
                Thread.Sleep(1000);

                if (CheckAlert(chromeDriver))
                {
                    IAlert alert = chromeDriver.SwitchTo().Alert();
                    alert.Accept();
                }
                IWebElement element = chromeDriver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_rad_ThuTiet"));
                element.Click();
                Thread.Sleep(1000);

                IWebElement _selectHocKy = chromeDriver.FindElement(By.Id("ctl00_ContentPlaceHolder1_ctl00_ddlChonNHHK"));
                SelectElement select = new SelectElement(_selectHocKy);
                select.SelectByValue(hocKy);
                Thread.Sleep(1000);

                if (CheckAlert(chromeDriver))
                {
                    IAlert alert = chromeDriver.SwitchTo().Alert();
                    alert.Accept();
                }

                html = chromeDriver.PageSource;

                chromeDriver.Quit();

                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(html);

                var _element = document.DocumentNode.Descendants("div")
            .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains("grid-roll2")).FirstOrDefault();

                string htmlTkbNew = _element.OuterHtml.Trim();
                if (htmlTkbNew != htmlTkb)
                {
                    return true;
                }
            }
            return false;
        }

        public string KiemTraTonTaiDuLieu(string chatID)
        {
            string query = $"SELECT * FROM tblTTSV WHERE ChatID = '{chatID}'";
            bool kt = model.KiemTraDuLieu(query);
            if (kt == false)
            {
                return $"<b>Chưa có dữ liệu</b>. Hãy chọn chức năng Thêm dữ liệu của bạn!";
            }
            return "";
        }

        private bool CheckAlert(IWebDriver driver)
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }

        private string TachLayNgayDauTienBDTuan(string input)
        {
            // Tìm vị trí bắt đầu của chuỗi ngày
            int startIndex = input.IndexOf("ngày ") + "ngày ".Length;

            // Tìm vị trí kết thúc của chuỗi ngày (là vị trí ký tự đầu tiên không phải là số hoặc "/")
            int endIndex = startIndex;
            while (endIndex < input.Length && (char.IsDigit(input[endIndex]) || input[endIndex] == '/'))
            {
                endIndex++;
            }

            // Tách và in ra chuỗi ngày
            if (startIndex != -1 && endIndex > startIndex)
            {
                string ngay = input.Substring(startIndex, endIndex - startIndex);
                return ngay;
            }
            return "";
        }

        private int ChuyenNgaySangTuan(string chatID, string ngay)
        {
            string queryGetNBDHK = $"SELECT * FROM tblTTSV WHERE ChatID = '{chatID}'";
            string ngayBatDauHocKy = model.GetNgayBatDauHocKy(queryGetNBDHK);
            if (ngayBatDauHocKy == "") return 0;
            DateTime date = DateTime.ParseExact(ngayBatDauHocKy, "dd/MM/yyyy", CultureInfo.CurrentCulture);
            DateTime _ngay = DateTime.ParseExact(ngay, "yyyy-MM-dd", CultureInfo.CurrentCulture);

            if (_hocKy == '1')
            {
                for (int i = 1; i <= 20; i++)
                {
                    if (_ngay >= date && _ngay < date.AddDays(7))
                    {
                        return i;
                    }
                    else date.AddDays(7);
                }
            }
            else if (_hocKy == '2')
            {
                for (int i = 1; i <= 22; i++)
                {
                    if (_ngay >= date && _ngay < date.AddDays(7))
                    {
                        return i;
                    }
                    else date = date.AddDays(7);
                }
            }
            return 0;
        }

        private string ChuyenTuanSangNgayBatDauTuan(string chatID, int tuan)
        {
            string queryGetNBDHK = $"SELECT * FROM tblTTSV WHERE ChatID = '{chatID}'";
            string ngayBatDauHocKy = model.GetNgayBatDauHocKy(queryGetNBDHK);
            DateTime date = DateTime.ParseExact(ngayBatDauHocKy, "dd/MM/yyyy", CultureInfo.CurrentCulture);

            for (int i = 1; i <= tuan; i++)
            {
                if (tuan == i)
                {
                    return date.ToString("yyyy-MM-dd");
                }
                else { date = date.AddDays(7); }
            }
            return "";
        }

        public string ChuyenThuTiengAnhSangTiengViet(DayOfWeek thuTiengAnh)
        {
            switch (thuTiengAnh)
            {
                case DayOfWeek.Monday:
                    return "Hai";
                case DayOfWeek.Tuesday:
                    return "Ba";
                case DayOfWeek.Wednesday:
                    return "Tư";
                case DayOfWeek.Thursday:
                    return "Năm";
                case DayOfWeek.Friday:
                    return "Sáu";
                case DayOfWeek.Saturday:
                    return "Bảy";
                case DayOfWeek.Sunday:
                    return "CN";
                default:
                    return string.Empty;
            }
        }
    }
}
