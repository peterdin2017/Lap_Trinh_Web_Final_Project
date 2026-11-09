using System.Configuration;
using System.Net;
using System.Net.Mail;

public class EmailService
{
    // Gửi 1 email HTML bất kỳ
    public void Gui(string denEmail, string tieuDe, string noiDungHtml)
    {
        string tuEmail = ConfigurationManager.AppSettings["GmailSender"];
        string matKhauUngDung = ConfigurationManager.AppSettings["GmailAppPassword"];

        // Gmail bắt buộc dùng TLS 1.2
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

        using (MailMessage mail = new MailMessage(tuEmail, denEmail, tieuDe, noiDungHtml))
        using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
        {
            mail.IsBodyHtml = true;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(tuEmail, matKhauUngDung);
            smtp.Send(mail);
        }
    }

    // Gửi email chứa mã OTP. purpose: "register" hoặc "reset_password"
    public void GuiOtp(string denEmail, string maOtp, string purpose)
    {
        bool datLaiMatKhau = (purpose == "reset_password");

        string tieuDe = datLaiMatKhau ? "Đặt lại mật khẩu" : "Xác nhận đăng ký tài khoản";

        string loiMoDau = datLaiMatKhau
            ? "Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Nhập mã bên dưới để tiếp tục."
            : "Cảm ơn bạn đã đăng ký tài khoản. Hãy nhập mã bên dưới để xác nhận email.";

        string loiCuoi = datLaiMatKhau
            ? "Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này."
            : "Nếu bạn không đăng ký tài khoản, hãy bỏ qua email này.";

        string html =
            "<div style='max-width:480px;margin:0 auto;padding:32px;background:#ffffff;font-family:Arial,sans-serif'>" +
              "<h2 style='margin-top:0;color:#003B73'>" + tieuDe + "</h2>" +
              "<p style='color:#333333'>" + loiMoDau + "</p>" +
              "<div style='margin:24px 0;text-align:center'>" +
                "<span style='display:inline-block;padding:12px 24px;background:#eaf2fb;border-radius:8px;" +
                "color:#003B73;font-size:32px;font-weight:bold;letter-spacing:8px'>" + maOtp + "</span>" +
              "</div>" +
              "<p style='color:#666666;font-size:14px'>Mã OTP chỉ dùng được một lần và hết hạn sau 5 phút. " +
              "Vui lòng không chia sẻ mã này với bất kỳ ai.</p>" +
              "<p style='border-top:1px solid #eeeeee;padding-top:16px;color:#999999;font-size:13px'>" + loiCuoi + "</p>" +
            "</div>";

        Gui(denEmail, tieuDe, html);
    }
}