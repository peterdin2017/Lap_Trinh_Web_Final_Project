using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Lap_Trinh_Web_Final_Project.Models;

public class AccountService
{
    // Tìm theo id. Không có thì trả về null.
    // Lưu ý: context bị đóng khi ra khỏi using, nên đừng đọc thuộc tính điều hướng
    // (ví dụ acc.Users, acc.Otps) của đối tượng trả về, chỉ đọc các cột thường.
    public Account TimTheoId(long accountId)
    {
        using (Lap_Trinh_Web_Final_Project_Entities db = new Lap_Trinh_Web_Final_Project_Entities())
        {
            return db.Accounts.FirstOrDefault(x => x.AccountId == accountId);
        }
    }

    // Tìm theo email VÀ role (cùng email có thể vừa là khách vừa là admin)
    public Account TimTheoEmail(string email, string role)
    {
        using (Lap_Trinh_Web_Final_Project_Entities db = new Lap_Trinh_Web_Final_Project_Entities())
        {
            return db.Accounts.FirstOrDefault(x => x.Email == email && x.Role == role);
        }
    }

    // Đặt mật khẩu mới (nhận vào mật khẩu đã băm bằng BCrypt)
    public void DatMatKhau(long accountId, string passwordHash)
    {
        using (Lap_Trinh_Web_Final_Project_Entities db = new Lap_Trinh_Web_Final_Project_Entities())
        {
            Account a = db.Accounts.FirstOrDefault(x => x.AccountId == accountId);
            if (a == null) return;

            a.PasswordHash = passwordHash;
            a.UpdatedAt = DateTime.Now;
            db.SaveChanges();
        }
    }

    // ---------- Dành cho đăng ký ----------

    // Bước 1 đăng ký: lấy tài khoản khách "tạm" (chưa có mật khẩu, chưa xác minh email).
    //  - Email chưa có trong hệ thống: tạo mới.
    //  - Email đã có nhưng chưa xác minh (người dùng bỏ dở lần trước): dùng lại tài khoản đó.
    //  - Email đã đăng ký xong (đã xác minh): trả về null, controller báo "email đã được đăng ký".
    // Email nên được chuẩn hóa (Trim, ToLower) ở controller trước khi truyền vào.
    public Account LayHoacTaoTaiKhoanTam(string email)
    {
        using (Lap_Trinh_Web_Final_Project_Entities db = new Lap_Trinh_Web_Final_Project_Entities())
        {
            Account daCo = db.Accounts.FirstOrDefault(x => x.Email == email && x.Role == "user");
            if (daCo != null)
            {
                return daCo.IsVerified ? null : daCo;
            }

            DateTime bayGio = DateTime.Now;

            Account a = new Account();
            a.Email = email;
            a.PasswordHash = null;
            a.Role = "user";
            a.Status = "active";
            a.IsVerified = false;
            a.CreatedAt = bayGio;    // EF không tự lấy DEFAULT của SQL nên phải gán
            a.UpdatedAt = bayGio;

            db.Accounts.Add(a);

            try
            {
                db.SaveChanges();
                return a;
            }
            catch (DbUpdateException)
            {
                // Hai yêu cầu cùng lúc với cùng một email: yêu cầu kia đã tạo rồi (vi phạm UNIQUE).
                // Lấy lại tài khoản đó bằng context mới, vì context cũ đã ở trạng thái lỗi.
            }
        }

        using (Lap_Trinh_Web_Final_Project_Entities db2 = new Lap_Trinh_Web_Final_Project_Entities())
        {
            Account taoRoi = db2.Accounts.FirstOrDefault(x => x.Email == email && x.Role == "user");
            return (taoRoi != null && !taoRoi.IsVerified) ? taoRoi : null;
        }
    }

    // Bước cuối đăng ký: đặt mật khẩu, đánh dấu đã xác minh và tạo hồ sơ trong bảng Users.
    // Chỉ làm được với tài khoản khách CHƯA xác minh; ngược lại trả về false và không đổi gì.
    // Một lần SaveChanges() là một giao dịch: hoặc lưu cả hai bảng, hoặc không lưu gì.
    public bool HoanTatDangKy(long accountId, string hoTen, string passwordHash)
    {
        using (Lap_Trinh_Web_Final_Project_Entities db = new Lap_Trinh_Web_Final_Project_Entities())
        {
            Account a = db.Accounts.FirstOrDefault(x => x.AccountId == accountId);
            if (a == null || a.Role != "user" || a.IsVerified) return false;

            DateTime bayGio = DateTime.Now;

            a.PasswordHash = passwordHash;
            a.IsVerified = true;
            a.UpdatedAt = bayGio;

            User u = new User();
            u.AccountId = accountId;
            u.FullName = hoTen;
            u.CreatedAt = bayGio;
            u.UpdatedAt = bayGio;
            db.Users.Add(u);

            db.SaveChanges();
            return true;
        }
    }
}