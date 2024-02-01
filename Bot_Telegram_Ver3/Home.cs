using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using HtmlAgilityPack;
using Telegram.Bot.Args;
using Telegram.Bot;
using System.Configuration;
using System.Data.SQLite;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Timers;
using System.Runtime.Remoting.Messaging;

namespace Bot_Telegram_Ver3
{
    internal class Home
    {
        private static Controller controller = new Controller();

        private static string tokenBot = ConfigurationManager.AppSettings["tokenBot"].ToString();
        private static TelegramBotClient bot = null;

        static Timer timer20H = new Timer();
        static Timer timer20H45 = new Timer();

        private static int run = 0;

        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (bot == null)
            {
                bot = new TelegramBotClient(tokenBot);
            }

            bot.StartReceiving();
            bot.OnMessage += Bot_OnMessage;

            SetupTimer23H_PM();
            SetupTimer20H_PM();

            Console.ReadLine();
        }
        static void SetupTimer23H_PM()
        {
            DateTime now = DateTime.Now;
            DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, 23, 55, 00);
            scheduledTime = (now > scheduledTime) ? scheduledTime.AddDays(1) : scheduledTime;

            TimeSpan timeUntilNextRun = scheduledTime - now;

            timer20H = new Timer(timeUntilNextRun.TotalMilliseconds);
            timer20H.Elapsed += (sender, e) => KiemTraThayDoi(sender, e, scheduledTime);
            timer20H.AutoReset = false;
            timer20H.Start();
        }

        static void SetupTimer20H_PM()
        {
            DateTime now = DateTime.Now;
            DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, 20, 45, 00);
            scheduledTime = (now > scheduledTime) ? scheduledTime.AddDays(1) : scheduledTime;

            TimeSpan timeUntilNextRun = scheduledTime - now;

            timer20H45 = new Timer(timeUntilNextRun.TotalMilliseconds);
            timer20H45.Elapsed += (sender, e) => GuiTKBVaLichThi(sender, e, scheduledTime);
            timer20H45.AutoReset = false;
            timer20H45.Start();
        }

        private static void KiemTraThayDoi(object sender, ElapsedEventArgs e, DateTime scheduledTime)
        {
            // Kiểm tra thời gian dự kiến
            if (DateTime.Now > scheduledTime)
            {
                // Thực hiện các hành động cần thiết
                Task.Run(() => controller.KiemTraThayDoi(bot));

                // Thiết lập Timer cho sự kiện tiếp theo
                SetupTimer23H_PM();
            }
        }

        private static void GuiTKBVaLichThi(object sender,ElapsedEventArgs e, DateTime scheduledTime)
        {
            // Kiểm tra thời gian dự kiến
            if (DateTime.Now > scheduledTime)
            {
                // Thực hiện các hành động cần thiết
                controller.GuiLichThiAuto(bot);
                controller.GuiTKBAuto(bot);

                // Thiết lập Timer cho sự kiện tiếp theo
                SetupTimer20H_PM();
            }
        }

        private static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            string message = e.Message.Text;
            string chatID = e.Message.Chat.Id.ToString();

            string infoNguoiDung = controller.ThongTinSV(chatID);
            if(infoNguoiDung == "") Console.WriteLine($"{DateTime.Now.ToString("HH:mm")} {chatID}-- Message: {message}");
            else Console.WriteLine($"{DateTime.Now.ToString("HH:mm")} {infoNguoiDung}-- Message: {message}");

            if (message == "/tkb")
            {
                string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                if(kiemTraDuLieu != "")
                {
                    bot.SendTextMessageAsync(chatID, kiemTraDuLieu,ParseMode.Html);
                    return;
                }
                DateTime date = DateTime.Now;
                string ngay = date.ToString("yyyy-MM-dd");
                string thu = controller.ChuyenThuTiengAnhSangTiengViet(date.DayOfWeek);

                string data = controller.GuiTKB(chatID, ngay, thu);
                bot.SendTextMessageAsync(chatID, data, ParseMode.Html);
            }
            else if (message == "/tkbm")
            {
                string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                if (kiemTraDuLieu != "")
                {
                    bot.SendTextMessageAsync(chatID, kiemTraDuLieu, ParseMode.Html);
                    return;
                }
                DateTime date = DateTime.Now.AddDays(1);
                string ngay = date.ToString("yyyy-MM-dd");
                string thu = controller.ChuyenThuTiengAnhSangTiengViet(date.DayOfWeek);

                string data = controller.GuiTKB(chatID, ngay, thu);
                bot.SendTextMessageAsync(chatID, data, ParseMode.Html);
            }
            else if (message == "/tkbtq")
            {
                string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                if (kiemTraDuLieu != "")
                {
                    bot.SendTextMessageAsync(chatID, kiemTraDuLieu, ParseMode.Html);
                    return;
                }
                DateTime date = DateTime.Now;
                string ngay = date.ToString("yyyy-MM-dd");
                string thu = controller.ChuyenThuTiengAnhSangTiengViet(date.DayOfWeek);

                string data = controller.GuiTKBTQTuan(1, chatID, ngay);
                bot.SendTextMessageAsync(chatID, data, ParseMode.Html);
            }
            else if (message == "/tkbtqts")
            {
                string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                if (kiemTraDuLieu != "")
                {
                    bot.SendTextMessageAsync(chatID, kiemTraDuLieu, ParseMode.Html);
                    return;
                }
                DateTime date = DateTime.Now;
                string ngay = date.ToString("yyyy-MM-dd");
                string thu = controller.ChuyenThuTiengAnhSangTiengViet(date.DayOfWeek);

                string data = controller.GuiTKBTQTuan(2, chatID, ngay);
                bot.SendTextMessageAsync(chatID, data, ParseMode.Html);
            }
            else if (message == "/lt")
            {
                string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                if (kiemTraDuLieu != "")
                {
                    bot.SendTextMessageAsync(chatID, kiemTraDuLieu, ParseMode.Html);
                    return;
                }
                DateTime date = DateTime.Now;
                string ngay = date.ToString("yyyy-MM-dd");

                string data = controller.GuiLichThi(chatID, ngay);
                bot.SendTextMessageAsync(chatID, data, ParseMode.Html);
            }
            else if (message == "/xoa")
            {
                int xoa = 0;
                if (run == 0)
                {
                    string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                    if (kiemTraDuLieu != "")
                    {
                        bot.SendTextMessageAsync(chatID, kiemTraDuLieu, ParseMode.Html);
                        return;
                    }
                    xoa = controller.XoaDuLieu(chatID);
                }
                else
                {
                    bot.SendTextMessageAsync(chatID, "Hệ thống đang bận. Vui lòng thực hiện lại sau 10 giây");
                    return;
                }
                if (xoa > 0)
                {
                    bot.SendTextMessageAsync(chatID, "Xóa dữ liệu thành công");
                }
                else bot.SendTextMessageAsync(chatID, "Xóa dữ liệu KHÔNG thành công");
            }
            else if (message == "/them")
            {
                string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                if (kiemTraDuLieu == "")
                {
                    bot.SendTextMessageAsync(chatID, $"<b>Đã có dữ liệu</b>. Nếu muốn thêm lại vui lòng Xóa dữ liệu cũ trước!", ParseMode.Html);
                    return;
                }
                bot.SendTextMessageAsync(chatID, "Vui lòng nhập Thêm_MSV\n<b>Ví dụ:</b> Thêm 6655010", ParseMode.Html);
            }
            else if (message.StartsWith("Thêm "))
            {
                if (run == 0) Task.Run(() => ThemDuLieu(chatID, message));
                else bot.SendTextMessageAsync(chatID, "Hệ thống đang bận. Vui lòng thêm dữ liệu lại sau 10 giây");
                
            }
            else if (message == "/start")
            {
                string text = $"<b>Chào mừng bạn đến với XemTKB_VNUA</b>\n\n" +
                              $"Hãy chọn chức năng trong MENU để sử dụng!\n\n" +
                              $"<a href=\"https://youtu.be/Dndiwb4CQ8w\">Video hướng dẫn</a>";
                bot.SendTextMessageAsync(chatID, text, ParseMode.Html);
            }
            else if (message == "/tgb")
            {
                string fileStream = ConfigurationManager.AppSettings["urlAnhTGB"].ToString();
                bot.SendPhotoAsync(chatID, fileStream, "THỜI GIAN BIỂU VNUA");
            }
            else if (message.StartsWith("/tb "))
            {
                string text = message.Substring("/tb ".Length);
                controller.GuiThongBao(bot, text);
            }
            else if (message.StartsWith("/tbid "))
            {
                int spaceIndex = message.IndexOf(' ', "/tbid ".Length);
                string _chatID = message.Substring("/tbid ".Length, spaceIndex - "/tbid ".Length);
                string text = message.Substring(spaceIndex + 1);
                bot.SendTextMessageAsync(_chatID, text, ParseMode.Html);
            }
            else if(message == "/btb")
            {
                int kq = controller.SetThongBao(1, chatID);
                if (kq > 0)
                {
                    bot.SendTextMessageAsync(chatID, "Bật thông báo <b>Thành công</b>",ParseMode.Html);
                }
            }
            else if(message == "/ttb")
            {
                int kq = controller.SetThongBao(0, chatID);
                if (kq > 0)
                {
                    bot.SendTextMessageAsync(chatID, "Tắt thông báo <b>Thành công</b>", ParseMode.Html);
                }
            }
            else if(message == "/kt")
            {
                Task.Run(() => KiemTraThayDoi()) ;
            }
            else if (message == "/diembefore")
            {
                string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                if (kiemTraDuLieu != "")
                {
                    bot.SendTextMessageAsync(chatID, kiemTraDuLieu, ParseMode.Html);
                    return;
                }
                Task.Run(() => XemDiem(chatID,1));
            }
            else if (message == "/diempresent")
            {
                string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                if (kiemTraDuLieu != "")
                {
                    bot.SendTextMessageAsync(chatID,kiemTraDuLieu, ParseMode.Html);
                    return;
                }
                Task.Run(() => XemDiem(chatID, 0));
            }
            else
            {
                bot.SendTextMessageAsync(chatID, "Sai cú pháp. Nhập /start để xem lại hướng dẫn");
            }
        }

        private static void ThemDuLieu(string chatID,string message)
        {
            run = 1;

            string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
            if (kiemTraDuLieu == "")
            {
                bot.SendTextMessageAsync(chatID, $"<b>Đã có dữ liệu</b>. Nếu muốn thêm lại vui lòng Xóa dữ liệu cũ trước!", ParseMode.Html);
                return;
            }
            string maSV = message.Substring("Thêm ".Length);
            if (long.TryParse(maSV, out long maSvValue) == false)
            {
                bot.SendTextMessageAsync(chatID, "Vui lòng kiểm tra lại mã sinh viên!");
                return;
            }
            bot.SendTextMessageAsync(chatID, "Đang lấy dữ liệu! Vui lòng chờ!");
            string kq = controller.ThemDuLieu(chatID, maSV);
            run = 0;
            bot.SendTextMessageAsync(chatID, kq);
        }

        private static void KiemTraThayDoi()
        {
            run = 1;
            controller.KiemTraThayDoi(bot);
            run = 0;
        }

        private static void XemDiem(string chatID,int mode)
        {
            bot.SendTextMessageAsync(chatID, "Đang truy vấn. Vui lòng chờ !");
            string diem = controller.GuiDiem(chatID, mode);
            bot.SendTextMessageAsync(chatID , diem,ParseMode.Html);
        }
    }
}
