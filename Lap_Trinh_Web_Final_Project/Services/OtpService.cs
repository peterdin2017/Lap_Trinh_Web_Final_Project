using Lap_Trinh_Web_Final_Project.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Helpers;

public class OtpService
{
    const int SO_LAN_THU_TOI_DA = 5;
    const int PHUT_HET_HAN = 5;
    const int GIAY_CHO_GUI_LAI = 60;
    EmailService emailService = new EmailService();

    // Sinh mã 6 chữ số ngẫu nhiên (100000 - 999999)
    private string TaoMa6So()
    {
        byte[] bytes = new byte[4];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        uint so = BitConverter.ToUInt32(bytes, 0);
        return (so % 900000 + 100000).ToString();
    }

    // Đánh dấu 1 OTP là đã dùng (để không dùng lại được nữa)
    private void VoHieuHoa(long otpId)
    {
        using (Lap_Trinh_Web_Final_Project_Entities db = new Lap_Trinh_Web_Final_Project_Entities())
        {
            Otp dong = db.Otps.FirstOrDefault(x => x.OtpId == otpId);
            if (dong == null) return;

            dong.UsedAt = DateTime.Now;
            dong.UpdatedAt = DateTime.Now;
            db.SaveChanges();
        }
    }

    // Tạo OTP mới, lưu DB (dạng băm) và gửi email.
    // purpose: "register" hoặc "reset_password".
    // Trả về null nếu thành công, hoặc câu báo lỗi nếu gửi mail thất bại.
    public string TaoVaGui(Account acc, string purpose)
    {
        string ma = TaoMa6So();
        long otpId;

        using (Lap_Trinh_Web_Final_Project_Entities db = new Lap_Trinh_Web_Final_Project_Entities())
        {
            DateTime bayGio = DateTime.Now;

            // Chặn gửi liên tục: nếu OTP chưa dùng gần nhất vừa tạo chưa quá 60 giây thì không gửi thêm
            Otp ganNhat = db.Otps
                .Where(x => x.AccountId == acc.AccountId
                         && x.Purpose == purpose
                         && x.UsedAt == null)
                .OrderByDescending(x => x.OtpId)
                .FirstOrDefault();

            if (ganNhat != null)
            {
                int daQua = (int)(bayGio - ganNhat.CreatedAt).TotalSeconds;
                if (daQua < GIAY_CHO_GUI_LAI)
                {
                    return "Vui lòng đợi " + (GIAY_CHO_GUI_LAI - daQua) + " giây trước khi gửi lại mã.";
                }
            }

            // Vô hiệu hóa các OTP cũ chưa dùng, để mỗi lúc chỉ có 1 mã hợp lệ
            var cacOtpCu = db.Otps
                .Where(x => x.AccountId == acc.AccountId
                         && x.Purpose == purpose
                         && x.UsedAt == null)
                .ToList();

            foreach (Otp cu in cacOtpCu)
            {
                cu.UsedAt = bayGio;
                cu.UpdatedAt = bayGio;
            }

            // Lưu OTP mới (chỉ lưu bản băm), hết hạn sau 5 phút
            Otp moi = new Otp();
            moi.AccountId = acc.AccountId;
            moi.AccountType = acc.Role;
            moi.Email = acc.Email;
            moi.OtpHash = BCrypt.Net.BCrypt.HashPassword(ma);
            moi.Attempts = 0;
            moi.Purpose = purpose;
            moi.ExpiresAt = bayGio.AddMinutes(PHUT_HET_HAN);
            moi.CreatedAt = bayGio;    // EF không tự lấy DEFAULT của SQL nên phải gán
            moi.UpdatedAt = bayGio;
            db.Otps.Add(moi);

            // Một lần SaveChanges: cập nhật OTP cũ và thêm OTP mới cùng một giao dịch
            db.SaveChanges();
            otpId = moi.OtpId;
        }

        try
        {
            emailService.GuiOtp(acc.Email, ma, purpose);
            return null;
        }
        catch (Exception ex)
        {
            // Lỗi thật (sai mật khẩu ứng dụng, mất mạng...) xem ở cửa sổ Output của Visual Studio
            Debug.WriteLine("Gui OTP that bai: " + ex);

            VoHieuHoa(otpId);
            return "Chưa gửi được mã OTP. Vui lòng thử lại sau.";
        }
    }

    // Kiểm tra mã người dùng nhập.
    // Trả về null nếu đúng, hoặc câu báo lỗi nếu sai.
    public string XacMinh(long accountId, string purpose, string maNhap)
    {
        using (Lap_Trinh_Web_Final_Project_Entities db = new Lap_Trinh_Web_Final_Project_Entities())
        {
            // Lấy OTP mới nhất chưa dùng
            Otp dong = db.Otps
                .Where(x => x.AccountId == accountId
                         && x.Purpose == purpose
                         && x.UsedAt == null)
                .OrderByDescending(x => x.OtpId)
                .FirstOrDefault();

            if (dong == null)
            {
                return "Không tìm thấy mã OTP. Vui lòng gửi lại mã.";
            }

            if (dong.ExpiresAt <= DateTime.Now)
            {
                return "Mã OTP đã hết hạn. Vui lòng gửi lại mã.";
            }

            int soLanThu = Convert.ToInt32(dong.Attempts);
            if (soLanThu >= SO_LAN_THU_TOI_DA)
            {
                return "Bạn đã nhập sai quá số lần cho phép. Vui lòng gửi mã mới.";
            }

            if (!BCrypt.Net.BCrypt.Verify(maNhap, dong.OtpHash))
            {
                // Nhập sai: tăng số lần thử
                dong.Attempts = soLanThu + 1;
                dong.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                int conLai = Math.Max(0, SO_LAN_THU_TOI_DA - (soLanThu + 1));
                return "Mã OTP không đúng. Bạn còn " + conLai + " lần thử.";
            }

            // Nhập đúng: đánh dấu đã dùng
            dong.UsedAt = DateTime.Now;
            dong.UpdatedAt = DateTime.Now;
            db.SaveChanges();
            return null;
        }
    }
}