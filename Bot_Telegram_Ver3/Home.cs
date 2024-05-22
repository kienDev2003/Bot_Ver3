using System;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot;
using System.Configuration;
using Telegram.Bot.Types.Enums;
using System.Timers;
using Telegram.Bot.Types.ReplyMarkups;
using thread = System.Threading.Thread;

namespace Bot_Telegram_Ver3
{
    internal class Home
    {
        private static Controller controller = new Controller();

        private static string tokenBot = ConfigurationManager.AppSettings["tokenBot"].ToString();
        private static TelegramBotClient bot = null;

        static Timer timer21H30 = new Timer();
        static Timer timer23H = new Timer();

        private static int run = 0;
        private static int modeThem = 0;

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
            SetupTimer21H30_PM();

            Console.ReadLine();
        }
        static void SetupTimer23H_PM()
        {
            DateTime now = DateTime.Now;
            DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, 23, 00, 00);

            if (now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            double interval = (scheduledTime - now).TotalMilliseconds;

            timer23H = new Timer(interval);
            timer23H.Elapsed += (sender, e) => KiemTraThayDoi(sender, e, scheduledTime);
            timer23H.Start();
        }

        static void SetupTimer21H30_PM()
        {
            DateTime now = DateTime.Now;
            DateTime scheduledTime = new DateTime(now.Year, now.Month, now.Day, 21, 35, 00);

            if (now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            double interval = (scheduledTime - now).TotalMilliseconds;

            timer21H30 = new Timer(interval);
            timer21H30.Elapsed += (sender, e) => GuiTKBVaLichThi(sender, e, scheduledTime);
            timer21H30.Start();
        }

        private static void KiemTraThayDoi(object sender, ElapsedEventArgs e, DateTime scheduledTime)
        {
            ((Timer)sender).Stop();

            // Thực hiện các hành động cần thiết
            Task.Run(() => controller.KiemTraThayDoi(bot));

            // Thiết lập Timer cho sự kiện tiếp theo
            ((Timer)sender).Interval = TimeSpan.FromDays(1).TotalMilliseconds;
            ((Timer)sender).Start();
        }

        private static void GuiTKBVaLichThi(object sender, ElapsedEventArgs e, DateTime scheduledTime)
        {
            ((Timer)sender).Stop();

            // Thực hiện các hành động cần thiết
            controller.GuiLichThiAuto(bot);
            controller.GuiTKBAuto(bot);

            // Thiết lập Timer cho sự kiện tiếp theo
            ((Timer)sender).Interval = TimeSpan.FromDays(1).TotalMilliseconds;
            ((Timer)sender).Start();

        }

        static KeyboardButton thoiKhoaBieu = new KeyboardButton("Thời Khóa Biểu Hôm Nay");
        static KeyboardButton thoiKhoaBieuMai = new KeyboardButton("Thời Khóa Biểu Ngày Mai");
        static KeyboardButton themDuLieu = new KeyboardButton("Thêm Dữ Liệu");
        static KeyboardButton xoaDuLieu = new KeyboardButton("Xóa Dữ Liệu");
        static KeyboardButton lichThi = new KeyboardButton("Lịch Thi");
        static KeyboardButton diemHKTruoc = new KeyboardButton("Điểm Học Kỳ Trước");
        static KeyboardButton diemHKNay = new KeyboardButton("Điểm Học Kỳ Này");
        static KeyboardButton lichHocTuanNay = new KeyboardButton("Lịch Học Tuần Này");
        static KeyboardButton lichHocTuanSau = new KeyboardButton("Lịch Học Tuần Sau");
        static KeyboardButton lichHoc3TuanSau = new KeyboardButton("Lịch Học 3 Tuần Sau");
        static KeyboardButton thoiGianBieu = new KeyboardButton("Thời Gian Biểu VNUA");
        static KeyboardButton batThongBao = new KeyboardButton("Bật Thông Báo");
        static KeyboardButton tatThongBao = new KeyboardButton("Tắt Thông Báo");
        static KeyboardButton hocPhi = new KeyboardButton("Học Phí");

        static ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(
                            new[]
                {
                        new[]
                        {
                        themDuLieu
                        },

                        new[]
                        {
                            thoiKhoaBieu,
                            thoiKhoaBieuMai
                        },

                        new[]
                        {
                            lichHocTuanNay,
                            lichHocTuanSau
                        },

                        new[]
                        {
                            diemHKTruoc,
                            diemHKNay
                        },

                        new[]
                        {
                            lichThi,
                            thoiGianBieu
                        },

                        new[]
                        {
                            batThongBao,
                            tatThongBao
                        },
                        new[]
                        {
                            lichHoc3TuanSau,
                            hocPhi
                        },

                        new[]
                        {
                            xoaDuLieu,
                        },
                }
                            );
        static ReplyKeyboardRemove removeKeyboard = new ReplyKeyboardRemove();

        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            string message = e.Message.Text;
            string chatID = e.Message.Chat.Id.ToString();

            string infoNguoiDung = controller.ThongTinSV(chatID);
            if (infoNguoiDung == "") Console.WriteLine($"{DateTime.Now.ToString("HH:mm")} {chatID}-- Message: {message}");
            else Console.WriteLine($"{DateTime.Now.ToString("HH:mm")} {infoNguoiDung}-- Message: {message}");

            try
            {
                if (message == "Thời Khóa Biểu Hôm Nay")
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

                    string data = controller.GuiTKB(chatID, ngay, thu);
                    bot.SendTextMessageAsync(chatID, data, ParseMode.Html);
                }
                else if (message == "Thời Khóa Biểu Ngày Mai")
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
                else if (message == "Lịch Học Tuần Này")
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
                else if (message == "Lịch Học Tuần Sau")
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
                else if (message == "Lịch Thi")
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
                else if (message == "Xóa Dữ Liệu")
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
                else if (message == "Thêm Dữ Liệu")
                {
                    string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                    if (kiemTraDuLieu == "")
                    {
                        bot.SendTextMessageAsync(chatID, $"<b>Đã có dữ liệu</b>. Nếu muốn thêm lại vui lòng Xóa dữ liệu cũ trước!", ParseMode.Html);
                        return;
                    }
                    bot.SendTextMessageAsync(chatID, "Nhập Mã Sinh Viên của Bạn\n<b>Ví dụ:</b> 6655010", ParseMode.Html, replyMarkup: removeKeyboard);
                    modeThem = 1;
                }
                else if (message != "" && modeThem != 0)
                {
                    if (run == 0)
                    {
                        string kq = await ThemDuLieu(chatID, message);
                        modeThem = 0;
                        bot.SendTextMessageAsync(chatID, kq, ParseMode.Html, replyMarkup: keyboard);
                    }
                    else bot.SendTextMessageAsync(chatID, "Hệ thống đang bận. Vui lòng thêm dữ liệu lại sau 10 giây");

                }
                else if (message == "/start")
                {
                    string text = $"<b>Chào mừng bạn đến với XemTKB_VNUA</b>\n\n" +
                                  $"Hãy chọn chức năng <b>Bên Dưới</b> để sử dụng!";

                    bot.SendTextMessageAsync(chatID, text, ParseMode.Html, replyMarkup: keyboard);
                }
                else if (message == "Thời Gian Biểu VNUA")
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
                else if (message == "Bật Thông Báo")
                {
                    int kq = controller.SetThongBao(1, chatID);
                    if (kq > 0)
                    {
                        bot.SendTextMessageAsync(chatID, "Bật thông báo <b>Thành công</b>", ParseMode.Html);
                    }
                }
                else if (message == "Tắt Thông Báo")
                {
                    int kq = controller.SetThongBao(0, chatID);
                    if (kq > 0)
                    {
                        bot.SendTextMessageAsync(chatID, "Tắt thông báo <b>Thành công</b>", ParseMode.Html);
                    }
                }
                else if (message == "/kt")
                {
                    Task.Run(() => KiemTraThayDoi());
                }
                else if (message == "Điểm Học Kỳ Trước")
                {
                    string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                    if (kiemTraDuLieu != "")
                    {
                        bot.SendTextMessageAsync(chatID, kiemTraDuLieu, ParseMode.Html);
                        return;
                    }
                    bot.SendTextMessageAsync(chatID, "Đang truy vấn. Vui lòng chờ !");
                    if (run == 0)
                    {
                        XemDiem(chatID, 1);
                    }
                    else
                    {
                        bot.SendTextMessageAsync(chatID, "Hệ thống đang bận. Vui lòng thêm dữ liệu lại sau 10 giây");
                    }

                }
                else if (message == "Điểm Học Kỳ Này")
                {
                    string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                    if (kiemTraDuLieu != "")
                    {
                        bot.SendTextMessageAsync(chatID, kiemTraDuLieu, ParseMode.Html);
                        return;
                    }
                    bot.SendTextMessageAsync(chatID, "Đang truy vấn. Vui lòng chờ !");
                    if (run == 0)
                    {
                        XemDiem(chatID, 0);
                    }
                    else
                    {
                        bot.SendTextMessageAsync(chatID, "Hệ thống đang bận. Vui lòng thêm dữ liệu lại sau 10 giây");
                    }
                }
                else if (message == "Lịch Học 3 Tuần Sau")
                {
                    string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                    if (kiemTraDuLieu != "")
                    {
                        bot.SendTextMessageAsync(chatID, kiemTraDuLieu, ParseMode.Html);
                        return;
                    }
                    DateTime date = DateTime.Now;
                    for (int i = 0; i < 3; i++)
                    {
                        date = date.AddDays(7);

                        string ngay = date.ToString("yyyy-MM-dd");
                        string thu = controller.ChuyenThuTiengAnhSangTiengViet(date.DayOfWeek);

                        string data = controller.GuiTKBTQTuan(1, chatID, ngay);
                        bot.SendTextMessageAsync(chatID, data, ParseMode.Html);

                        thread.Sleep(800);
                    }

                }
                else if(message == "Học Phí")
                {
                    string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
                    if (kiemTraDuLieu != "")
                    {
                        bot.SendTextMessageAsync(chatID, kiemTraDuLieu, ParseMode.Html);
                        return;
                    }
                    bot.SendTextMessageAsync(chatID, "Đang truy vấn. Vui lòng chờ !");
                    if (run == 0)
                    {
                        string test = await controller.GuiHocPhi(chatID);
                        bot.SendTextMessageAsync(chatID, test, ParseMode.Html);
                    }
                    else
                    {
                        bot.SendTextMessageAsync(chatID, "Hệ thống đang bận. Vui lòng thêm dữ liệu lại sau 10 giây");
                    }
                }
                else
                {
                    bot.SendTextMessageAsync(chatID, "Sai cú pháp. Nhập /start để xem lại hướng dẫn");
                }
            }
            catch (Exception ex)
            {
                run = 0;
                bot.SendTextMessageAsync(chatID, "Sai cú pháp. Nhập /start để xem lại hướng dẫn");
            }
        }

        private static async Task<string> ThemDuLieu(string chatID, string message)
        {
            run = 1;

            string kiemTraDuLieu = controller.KiemTraTonTaiDuLieu(chatID);
            if (kiemTraDuLieu == "")
            {
                run = 0;
                return $"<b>Đã có dữ liệu</b>. Nếu muốn thêm lại vui lòng Xóa dữ liệu cũ trước!";
            }
            string maSV = message.Substring("".Length);
            if (long.TryParse(maSV, out long maSvValue) == false)
            {
                run = 0;
                return "Vui lòng kiểm tra lại mã sinh viên!";
            }
            bot.SendTextMessageAsync(chatID, "Đang lấy dữ liệu! Vui lòng chờ!");
            string kq = await controller.ThemDuLieu(chatID, maSV);
            run = 0;
            return kq;
        }

        private static void KiemTraThayDoi()
        {
            run = 1;
            controller.KiemTraThayDoi(bot);
            run = 0;
        }

        private static void XemDiem(string chatID, int mode)
        {
            run = 1;
            string diem = controller.GuiDiem(chatID, mode);
            bot.SendTextMessageAsync(chatID, diem, ParseMode.Html);
            run = 0;
        }
    }
}
