using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot_Telegram_Ver3
{
    internal class Model
    {
        DBConnection DBConn = new DBConnection();

        public int Command(string query)
        {
            int check = 0;

            DBConn.GetConn();

            using (SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                check = cmd.ExecuteNonQuery();
            }

            DBConn.CloseConn();

            return check;
        }

        public int CommandModeKiemTra(string query,string htmlTkb,string htmlLt)
        {
            DBConn.GetConn();

            using(SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                cmd.Parameters.AddWithValue("@htmlTkb", htmlTkb);
                cmd.Parameters.AddWithValue("@htmlLt", htmlLt);

                return cmd.ExecuteNonQuery();
            }
        }

        public string GetNgayBatDauHocKy(string query)
        {
            DBConn.GetConn();

            using(SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                string ngayBatDauHocKy = "";

                using(SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ngayBatDauHocKy = reader["NBDHK"].ToString();
                    }
                }
                DBConn.CloseConn();
                return ngayBatDauHocKy;
            }
        }

        public string GetTTSV(string query)
        {
            DBConn.GetConn();

            using (SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                string hoTen = "";
                string lop = "";

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        hoTen = reader["TenSV"].ToString();
                        lop = reader["Lop"].ToString();
                    }
                    string ttsv = $"Sinh viên: {hoTen} Lớp {lop}";
                    return ttsv;
                }
            }
        }

        public string GetInfoNguoiDung(string query)
        {
            DBConn.GetConn();

            using (SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                string hoTen = "";

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        hoTen = reader["TenSV"].ToString();
                    }
                    return hoTen;
                }
            }
        }

        public string GetTKB(string query,int tuanHT)
        {
            DBConn.GetConn();

            using (SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                string tenMH = "";
                string nhomMH = "";
                string thoiGian = "";
                string phongHoc = "";
                string data = "";

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tenMH = reader["TenMH"].ToString();
                        nhomMH = reader["NhomMH"].ToString();
                        int tietBD = int.Parse(reader["TietBD"].ToString());
                        int soTiet = int.Parse(reader["SoTiet"].ToString());
                        thoiGian = ChuyenDoiTietDBVaSoTiet(tietBD, soTiet);
                        phongHoc = reader["PhongHoc"].ToString();
                        string tuanHoc = reader["TuanHoc"].ToString();
                        int[] _tuanHoc = ChuyenDayTuanHocSangMang(tuanHoc);

                        bool check = false;
                        for (int i = 0; i < _tuanHoc.Length; i++)
                        {
                            if (_tuanHoc[i] == tuanHT)
                            {
                                check = true;
                            }
                        }
                        if (check)
                        {
                            data += $"Môn: {tenMH}\nNhóm: {nhomMH}\nTiết bắt đầu: {tietBD} - Số tiết: {soTiet} {thoiGian}\nPhòng: {phongHoc}\n\n";
                        }
                    }
                    if(data == "")
                    {
                        data = "Không có môn học\n\n";
                    }
                }

                DBConn.CloseConn();
                return data;
            }
        }

        public string GetLichThi(string query)
        {
            DBConn.GetConn();

            using (SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                string tenMH = "";
                string thoiGian = "";
                string phongThi = "";
                string ngayThi = "";
                string data = "";
                string soNgayConLai = "";

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tenMH = reader["TenMH"].ToString();
                        int tietBD = int.Parse(reader["TietBD"].ToString());
                        int soTiet = int.Parse(reader["SoTiet"].ToString());
                        thoiGian = ChuyenDoiTietDBVaSoTiet(tietBD, soTiet);
                        phongThi = reader["PhongThi"].ToString();
                        ngayThi = reader["NgayThi"].ToString();
                        ngayThi = DateTime.ParseExact(ngayThi, "yyyy-MM-dd", CultureInfo.CurrentCulture).ToString("dd-MM-yyyy");
                        DateTime dateNow = DateTime.Now;
                        DateTime dateNgayThi = DateTime.ParseExact(ngayThi, "dd-MM-yyyy", CultureInfo.CurrentCulture);
                        TimeSpan __soNgayConLai = dateNgayThi - dateNow;
                        int _soNgayConLai = __soNgayConLai.Days;
                        if (_soNgayConLai > 0) soNgayConLai = $"\n<b><i>BẠN CÒN {_soNgayConLai} ĐỂ VỀ ĐÍCH</i></b>";
                        else if (_soNgayConLai == 0 && __soNgayConLai.Hours > 0) soNgayConLai = "\n<b><i>CHÚC BẠN NGÀY MAI THI TỐT</i></b>";
                        else soNgayConLai = "\n<b><i>CHÚC BẠN THI TỐT</i></b>";
                        data += $"Môn: {tenMH}\nThời gian thi: {thoiGian}\nPhòng thi: {phongThi}\nNgày thi: {ngayThi}\n{soNgayConLai}\n\n";
                    }
                    if (data == "")
                    {
                        data = "Chưa có lịch thi";
                    }
                }
                DBConn.CloseConn();
                return data;
            }
        }

        public string GetLichThiModeAuto(string query)
        {
            DBConn.GetConn();

            using (SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                string tenMH = "";
                string thoiGian = "";
                string phongThi = "";
                string ngayThi = "";
                string soNgayConLai = "";
                string data = "";

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        tenMH = reader["TenMH"].ToString();
                        int tietBD = int.Parse(reader["TietBD"].ToString());
                        int soTiet = int.Parse(reader["SoTiet"].ToString());
                        thoiGian = ChuyenDoiTietDBVaSoTiet(tietBD, soTiet);
                        phongThi = reader["PhongThi"].ToString();
                        ngayThi = reader["NgayThi"].ToString();
                        ngayThi = DateTime.ParseExact(ngayThi, "yyyy-MM-dd", CultureInfo.CurrentCulture).ToString("dd-MM-yyyy");
                        DateTime dateNow = DateTime.Now;
                        DateTime dateNgayThi = DateTime.ParseExact(ngayThi, "dd-MM-yyyy", CultureInfo.CurrentCulture);
                        TimeSpan __soNgayConLai = dateNgayThi - dateNow;
                        int _soNgayConLai = __soNgayConLai.Days;
                        if (_soNgayConLai > 0) soNgayConLai = $"\n<b><i>BẠN CÒN {_soNgayConLai} ĐỂ VỀ ĐÍCH</i></b>";
                        else if (_soNgayConLai == 0 && __soNgayConLai.Hours > 0) soNgayConLai = "\n<b><i>CHÚC BẠN NGÀY MAI THI TỐT</i></b>";
                        else soNgayConLai = "\n<b><i>CHÚC BẠN THI TỐT</i></b>";
                        data += $"Môn: {tenMH}\nThời gian thi: {thoiGian}\nPhòng thi: {phongThi}\nNgày thi: {ngayThi}\n{soNgayConLai}\n\n";
                    }
                }

                DBConn.CloseConn();
                return data;
            }
        }

        public string GetChuoiHtmlTkb(string query)
        {
            DBConn.GetConn();

            using(SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                using(SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader["Tkb"].ToString() ;
                    }
                }
            }
            return "";
        }

        public string GetMaSVKiemTra(string query)
        {
            DBConn.GetConn();

            using (SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader["MaSV"].ToString();
                    }
                }
            }
            return "";
        }

        public string GetChuoiHtmlLt(string query)
        {
            DBConn.GetConn();

            using (SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader["LichThi"].ToString();
                    }
                }
            }
            return "";
        }

        public bool KiemTraDuLieu(string query)
        {
            DBConn.GetConn();

            using(SQLiteCommand cmd = new SQLiteCommand(query, DBConn.conn))
            {
                using(SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) return true;
                }
            }
            return false;
        }

        public string[] ListNguoiBatAuto(int mode)
        {
            string query = $"";
            if (mode == 1) query = $"SELECT * FROM tblTTSV";
            else query = $"SELECT * FROM tblTTSV WHERE Auto = '1'";
            DBConn.GetConn();
            List<string> data = new List<string>();
            
            using(SQLiteCommand cmd = new SQLiteCommand(query,DBConn.conn))
            {
                using(SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        string chatID = reader["ChatID"].ToString();
                        data.Add(chatID);
                    }
                }
            }
            return data.ToArray();
        }

        private static string ChuyenDoiTietDBVaSoTiet(int tietBD, int soTiet)
        {
            if (tietBD == 1 && soTiet == 2) return "(7:00-8:45)";
            else if (tietBD == 1 && soTiet == 3) return "(7:00-9:40)";
            else if (tietBD == 1 && soTiet == 5) return "(7:00-11:40)";

            if (tietBD == 2 && soTiet == 2) return "(7:55-9:40)";
            else if (tietBD == 2 && soTiet == 4) return "(7:55-11:40)";

            if (tietBD == 4 && soTiet == 2) return "(9:55-11:40)";

            if (tietBD == 6 && soTiet == 2) return "(12:45-14:30)";
            else if (tietBD == 6 && soTiet == 3) return "(12:45-15:25)";
            else if (tietBD == 6 && soTiet == 5) return "(12:45-17:25)";

            if (tietBD == 7 && soTiet == 2) return "(13:40-15:25)";
            else if (tietBD == 7 && soTiet == 4) return "(13:40-17:25)";

            if (tietBD == 9 && soTiet == 2) return "(15:40-17:25)";

            if (tietBD == 11 && soTiet == 2) return "(18:00-19:45)";
            else if (tietBD == 11 && soTiet == 3) return "(18:00-20:40)";

            return $"Tiết BD: {tietBD}, Số tiết: {soTiet}";
        }

        private static int[] ChuyenDayTuanHocSangMang(string tuanHoc)
        {
            List<int> numbers = new List<int>();
            int x = 1;

            for (int i = 0; i < tuanHoc.Length; i++)
            {
                if (tuanHoc[i] == '-')
                {
                    numbers.Add(x);
                    x += 1;
                }
                else
                {
                    if (int.Parse(tuanHoc[i].ToString()) < x)
                    {
                        numbers.Add(x);
                    }
                    else
                    {
                        numbers.Add(int.Parse(tuanHoc[i].ToString()));
                    }
                    x += 1;
                }
            }

            for (int i = 0; i < tuanHoc.Length; i++)
            {
                if (tuanHoc[i] == '-')
                {
                    numbers.RemoveAll(item => item == i + 1);
                }
            }

            return numbers.ToArray();
        }
    }
}
