using Business.Abstract;
using Core.Utilities.Email;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class EmailManager : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailManager(EmailSettings settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPass)
            };

            var msg = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            msg.To.Add(toEmail);

            await client.SendMailAsync(msg);
        }

        public Task SendVerificationCodeAsync(string toEmail, string code)
        {
            var subject = "E‑Posta Doğrulama Kodu";
            var body = BuildVerificationCodeTemplate(code, expireMinutes: 15);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        private string BuildVerificationCodeTemplate(string code, int expireMinutes)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <style>
    body,html {{ margin:0; padding:0; background-color:#f0f2f5; font-family:Arial,sans-serif; }}
    .wrapper {{ width:100%; padding:20px 0; }}
    .content {{ max-width:600px; margin:0 auto; background:#fff; border-radius:8px; overflow:hidden; }}
    .header {{ background:linear-gradient(90deg,#3366FF,#00CCFF); color:#fff; padding:40px 20px; text-align:center; }}
    .header h1 {{ margin:0; font-size:24px; }}
    .body {{ padding:30px 20px; color:#444; font-size:16px; line-height:1.5; }}
    .code-box {{ display:block; width:200px; margin:30px auto; padding:20px; background:#f4f8fd;
                 border-radius:6px; text-align:center; font-size:32px; letter-spacing:6px; color:#3366FF; }}
    .footer {{ background:#fafafa; text-align:center; padding:15px; font-size:12px; color:#888; }}
    @media(max-width:600px) {{
      .content {{ margin:0 10px; }}
      .code-box {{ width:100%; box-sizing:border-box; }}
    }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""content"">
      <div class=""header"">
        <h1>E‑Posta Doğrulama</h1>
      </div>
      <div class=""body"">
        <p>Merhaba,</p>
        <p>Hesabınızı doğrulamak için aşağıdaki kodu girin:</p>
        <span class=""code-box"">{code}</span>
        <p>Bu kod <strong>{expireMinutes} dakika</strong> için geçerli olacaktır.</p>
        <p>Eğer bu isteği siz yapmadıysanız, lütfen bu e‑postayı dikkate almayın.</p>
      </div>
      <div class=""footer"">
        &copy; {DateTime.Now:yyyy} ABK CORE.
      </div>
    </div>
  </div>
</body>
</html>
";
        }

        public Task SendPasswordResetCodeAsync(string toEmail, string code)
        {
            var subject = "Şifre Sıfırlama Kodu";
            var body = BuildPasswordResetTemplate(code, 15);
            return SendEmailAsync(toEmail, subject, body, true);
        }

        private string BuildPasswordResetTemplate(string code, int expireMinutes)
        {
            // Aynı tasarım, sadece başlık ve metin değişti:
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <style>
    body,html {{ margin:0; padding:0; background-color:#f0f2f5; font-family:Arial,sans-serif; }}
    .wrapper {{ width:100%; padding:20px 0; }}
    .content {{ max-width:600px; margin:0 auto; background:#fff; border-radius:8px; overflow:hidden; }}
    .header {{ background:linear-gradient(90deg,#3366FF,#00CCFF); color:#fff; padding:40px 20px; text-align:center; }}
    .header h1 {{ margin:0; font-size:24px; }}
    .body {{ padding:30px 20px; color:#444; font-size:16px; line-height:1.5; }}
    .code-box {{ display:block; width:200px; margin:30px auto; padding:20px; background:#f4f8fd;
                 border-radius:6px; text-align:center; font-size:32px; letter-spacing:6px; color:#3366FF; }}
    .footer {{ background:#fafafa; text-align:center; padding:15px; font-size:12px; color:#888; }}
    @media(max-width:600px) {{
      .content {{ margin:0 10px; }}
      .code-box {{ width:100%; box-sizing:border-box; }}
    }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""content"">
      <div class=""header"">
        <h1>Şifre Sıfırlama</h1>
      </div>
      <div class=""body"">
        <p>Merhaba,</p>
        <p>Şifrenizi sıfırlamak için aşağıdaki kodu girin:</p>
        <span class=""code-box"">{code}</span>
        <p>Bu kod <strong>{expireMinutes} dakika</strong> için geçerlidir.</p>
        <p>Eğer bu isteği siz yapmadıysanız, bu e‑postayı dikkate almayın.</p>
      </div>
      <div class=""footer"">
        &copy; {DateTime.Now:yyyy} {_settings.FromName}. Tüm hakları saklıdır.
      </div>
    </div>
  </div>
</body>
</html>
";
        }


        public Task SendOfferReceivedNotificationAsync(string toEmail, string tenderTitle)
        {
            var subject = "Teklifiniz Alındı";
            var body = BuildOfferReceivedTemplate(tenderTitle);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        private string BuildOfferReceivedTemplate(string tenderTitle)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <style>
    body,html {{ margin:0; padding:0; background:#f0f2f5; font-family:Arial,sans-serif; }}
    .wrapper {{ width:100%; padding:20px 0; }}
    .content {{ max-width:600px; margin:0 auto; background:#fff; border-radius:8px; overflow:hidden; }}
    .header {{ background:linear-gradient(90deg,#3366FF,#00CCFF); color:#fff; padding:40px 20px; text-align:center; }}
    .header h1 {{ margin:0; font-size:24px; }}
    .body {{ padding:30px 20px; font-size:16px; color:#444; line-height:1.5; }}
    .footer {{ background:#fafafa; text-align:center; padding:15px; font-size:12px; color:#888; }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""content"">
      <div class=""header"">
        <h1>Teklif Alındı</h1>
      </div>
      <div class=""body"">
        <p>Merhaba,</p>
        <p><strong>{tenderTitle}</strong> başlıklı Satın Alma yaptığınız teklif başarıyla kaydedildi.</p>
        <p>Teklifinizi en kısa sürede değerlendireceğiz.</p>
      </div>
      <div class=""footer"">
        &copy; {DateTime.Now:yyyy} {_settings.FromName}
      </div>
    </div>
  </div>
</body>
</html>";
        }

        public Task SendTenderWinnerNotificationAsync(string toEmail, string tenderTitle, string detailsUrl)
        {
            var subject = "Satın Almayı Kazandınız!";
            var body = BuildTenderWinnerTemplate(tenderTitle, detailsUrl);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        private string BuildTenderWinnerTemplate(string tenderTitle, string detailsUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <style>
    body,html {{ margin:0; padding:0; background:#f0f2f5; font-family:Arial,sans-serif; }}
    .wrapper {{ width:100%; padding:20px 0; }}
    .content {{ max-width:600px; margin:0 auto; background:#fff; border-radius:8px; overflow:hidden; }}
    .header {{ background:linear-gradient(90deg,#28a745,#20c997); color:#fff; padding:40px 20px; text-align:center; }}
    .header h1 {{ margin:0; font-size:24px; }}
    .body {{ padding:30px 20px; font-size:16px; color:#444; line-height:1.5; }}
    .link-button {{
      display:inline-block;
      margin-top:20px;
      padding:12px 24px;
      background:#28a745;
      color:#fff;
      border-radius:4px;
      text-decoration:none;
      font-weight:bold;
    }}
    .footer {{ background:#fafafa; text-align:center; padding:15px; font-size:12px; color:#888; }}
    @media(max-width:600px) {{
      .content {{ margin:0 10px; }}
    }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""content"">
      <div class=""header"">
        <h1>Satın Almayı Kazandınız!</h1>
      </div>
      <div class=""body"">
        <p>Merhaba,</p>
        <p>Tebrikler! <strong>{tenderTitle}</strong> başlıklı satın almayı kazandınız.</p>
        <p>Detaylı bilgi ve sonraki adımları görmek için aşağıdaki butona tıklayabilirsiniz:</p>
        <a href=""{detailsUrl}"" class=""link-button"">Satın Alma Detaylarına Git</a>
      </div>
      <div class=""footer"">
        &copy; {DateTime.Now:yyyy} {_settings.FromName}. Tüm hakları saklıdır.
      </div>
    </div>
  </div>
</body>
</html>";
        }


        public Task SendApprovalPendingNotificationAsync(string toEmail, string tenderTitle, string detailsUrl)
        {
            var subject = "Onay Sırası Geldi";
            var body = BuildApprovalPendingTemplate(tenderTitle, detailsUrl);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        private string BuildApprovalPendingTemplate(string tenderTitle, string detailsUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Onay Bekleniyor</title>
</head>
<body style=""margin:0; padding:0; background-color:#f0f2f5; font-family:Arial,sans-serif;"">
  <div style=""width:100%; padding:20px 0;"">
    <div style=""max-width:600px; margin:0 auto; background:#fff; border-radius:8px; overflow:hidden;"">
      <div style=""background:linear-gradient(90deg,#3366FF,#00CCFF); color:#fff; padding:40px 20px; text-align:center;"">
        <h1 style=""margin:0; font-size:24px;"">Onay Bekleniyor</h1>
      </div>
      <div style=""padding:30px 20px; color:#444; font-size:16px; line-height:1.5;"">
        <p>Merhaba,</p>
        <p><strong>{tenderTitle}</strong> başlıklı satın alma talebi için onayınız beklenmektedir.<br>
        Detayları incelemek ve işlemi gerçekleştirmek için aşağıdaki bağlantıyı kullanabilirsiniz.</p>
        <a href=""{detailsUrl}""
           style=""display:inline-block; margin-top:20px; padding:12px 24px; background:#3366FF; color:#fff; border-radius:4px; text-decoration:none; font-weight:bold;"">
           Onaya Gitmek İçin Tıklayın
        </a>
      </div>
      <div style=""background:#fafafa; text-align:center; padding:15px; font-size:12px; color:#888;"">
        &copy; {DateTime.Now:yyyy} {_settings.FromName}
      </div>
    </div>
  </div>
</body>
</html>";
        }

        public Task SendApprovalRejectedNotificationAsync(string toEmail, string tenderTitle, string detailsUrl)
        {
            var subject = "Satın Almanız Reddedildi";
            var body = BuildApprovalRejectedTemplate(tenderTitle, detailsUrl);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        private string BuildApprovalRejectedTemplate(string tenderTitle, string detailsUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <style>
    body,html {{ margin:0; padding:0; background-color:#f0f2f5; font-family:Arial,sans-serif; }}
    .wrapper {{ width:100%; padding:20px 0; }}
    .content {{ max-width:600px; margin:0 auto; background:#fff; border-radius:8px; overflow:hidden; }}
    .header {{ background:linear-gradient(90deg,#dc3545,#e4606d); color:#fff; padding:40px 20px; text-align:center; }}
    .header h1 {{ margin:0; font-size:24px; }}
    .body {{ padding:30px 20px; font-size:16px; color:#444; line-height:1.5; }}
    .link-button {{
      display:inline-block;
      margin-top:20px;
      padding:12px 24px;
      background:#dc3545;
      color:#fff;
      border-radius:4px;
      text-decoration:none;
      font-weight:bold;
    }}
    .footer {{ background:#fafafa; text-align:center; padding:15px; font-size:12px; color:#888; }}
    @media(max-width:600px) {{
      .content {{ margin:0 10px; }}
    }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""content"">
      <div class=""header"">
        <h1>Satın Almanız Reddedildi</h1>
      </div>
      <div class=""body"">
        <p>Merhaba,</p>
        <p>“<strong>{tenderTitle}</strong>” başlıklı satın alma talebiniz maalesef reddedildi.</p>
        <p>Detayları görmek için aşağıdaki butona tıklayabilirsiniz:</p>
        <a href=""{detailsUrl}"" class=""link-button"">Satın Alma Detaylarına Git</a>
      </div>
      <div class=""footer"">
        &copy; {DateTime.Now:yyyy} {_settings.FromName}. Tüm hakları saklıdır.
      </div>
    </div>
  </div>
</body>
</html>
";
        }


        public Task SendPurchaseRequestCreatedNotificationAsync(string toEmail, string requestTitle, string nextApproverName, string detailsUrl)
        {
            var subject = "Talebiniz Oluşturuldu";
            var body = BuildPurchaseRequestCreatedTemplate(requestTitle, nextApproverName, detailsUrl);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        public Task SendPurchaseRequestPendingApprovalNotificationAsync(string toEmail, string requestTitle, string detailsUrl)
        {
            var subject = "Onayınızda Bekliyor";
            var body = BuildPurchaseRequestPendingApprovalTemplate(requestTitle, detailsUrl);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        public Task SendPurchaseRequestProcurementApprovedNotificationAsync(string toEmail, string requestTitle, string detailsUrl)
        {
            var subject = "Talebiniz Satınalmacı Tarafından Onaylandı";
            var body = BuildPurchaseRequestProcurementApprovedTemplate(requestTitle, detailsUrl);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        public Task SendPurchaseRequestProcessedNotificationAsync(string toEmail, string requestTitle, string detailsUrl)
        {
            var subject = "Talebiniz İşleme Alındı";
            var body = BuildPurchaseRequestProcessedTemplate(requestTitle, detailsUrl);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }


        private string BuildPurchaseRequestCreatedTemplate(string title, string nextApprover, string detailsUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <style>
    body,html {{ margin:0; padding:0; background-color:#f0f2f5; font-family:Arial,sans-serif; }}
    .wrapper {{ width:100%; padding:20px 0; }}
    .content {{ max-width:600px; margin:0 auto; background:#fff; border-radius:8px; overflow:hidden; }}
    .header {{
      background:linear-gradient(90deg,#3366FF,#00CCFF);
      color:#fff;
      padding:40px 20px;
      text-align:center;
    }}
    .header h1 {{ margin:0; font-size:24px; }}
    .body {{ padding:30px 20px; color:#444; font-size:16px; line-height:1.5; }}
    .link-button {{
      display:inline-block;
      margin-top:20px;
      padding:12px 24px;
      background:#3366FF;
      color:#fff;
      border-radius:4px;
      text-decoration:none;
      font-weight:bold;
    }}
    .footer {{
      background:#fafafa;
      text-align:center;
      padding:15px;
      font-size:12px;
      color:#888;
    }}
    @media(max-width:600px) {{
      .content {{ margin:0 10px; }}
    }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""content"">
      <div class=""header""><h1>Talep Oluşturuldu</h1></div>
      <div class=""body"">
        <p>Merhaba,</p>
        <p>
          “<strong>{title}</strong>” başlıklı satın alma talebiniz oluşturuldu
          ve <strong>{nextApprover}</strong> kullanıcısının onayına gönderildi.
        </p>
        <p>Detayları görüntülemek için aşağıdaki butona tıklayın:</p>
        <a href=""{detailsUrl}"" class=""link-button"">Talep Detaylarına Git</a>
      </div>
      <div class=""footer"">&copy; {DateTime.Now:yyyy} {_settings.FromName}</div>
    </div>
  </div>
</body>
</html>";
        }

        private string BuildPurchaseRequestPendingApprovalTemplate(string title, string detailsUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Onay Bekleniyor</title>
</head>
<body style=""margin:0; padding:0; background-color:#f0f2f5; font-family:Arial,sans-serif;"">
  <div style=""width:100%; padding:20px 0;"">
    <div style=""max-width:600px; margin:0 auto; background:#fff; border-radius:8px; overflow:hidden;"">
      <div style=""background:linear-gradient(90deg,#3366FF,#00CCFF); color:#fff; padding:40px 20px; text-align:center;"">
        <h1 style=""margin:0; font-size:24px;"">Onay Bekleniyor</h1>
      </div>
      <div style=""padding:30px 20px; color:#444; font-size:16px; line-height:1.5;"">
        <p>Merhaba,</p>
        <p><strong>{title}</strong> başlıklı satın alma talebi için onayınız beklenmektedir.<br>
        Detayları incelemek ve işlemi gerçekleştirmek için aşağıdaki bağlantıyı kullanabilirsiniz.</p>
        <a href=""{detailsUrl}""
           style=""display:inline-block; margin-top:20px; padding:12px 24px; background:#3366FF; color:#fff; border-radius:4px; text-decoration:none; font-weight:bold;"">
           Onaya Gitmek İçin Tıklayın
        </a>
      </div>
      <div style=""background:#fafafa; text-align:center; padding:15px; font-size:12px; color:#888;"">
        &copy; {DateTime.Now:yyyy} {_settings.FromName}
      </div>
    </div>
  </div>
</body>
</html>";
        }


        private string BuildPurchaseRequestProcurementApprovedTemplate(string title, string detailsUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <style>
    body,html {{ margin:0; padding:0; background-color:#f0f2f5; font-family:Arial,sans-serif; }}
    .wrapper {{ width:100%; padding:20px 0; }}
    .content {{ max-width:600px; margin:0 auto; background:#fff; border-radius:8px; overflow:hidden; }}
    .header {{
      background:linear-gradient(90deg,#3366FF,#00CCFF);
      color:#fff;
      padding:40px 20px;
      text-align:center;
    }}
    .header h1 {{ margin:0; font-size:24px; }}
    .body {{ padding:30px 20px; color:#444; font-size:16px; line-height:1.5; }}
    .link-button {{
      display:inline-block;
      margin-top:20px;
      padding:12px 24px;
      background:#3366FF;
      color:#fff;
      border-radius:4px;
      text-decoration:none;
      font-weight:bold;
    }}
    .footer {{
      background:#fafafa;
      text-align:center;
      padding:15px;
      font-size:12px;
      color:#888;
    }}
    @media(max-width:600px) {{
      .content {{ margin:0 10px; }}
    }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""content"">
      <div class=""header""><h1>Satınalmacı Onayı</h1></div>
      <div class=""body"">
        <p>Merhaba,</p>
        <p>
          “<strong>{title}</strong>” başlıklı talebiniz satın almacı tarafından işleme alındı.
        </p>
        <a href=""{detailsUrl}"" class=""link-button"">Detayları Görüntüle</a>
      </div>
      <div class=""footer"">&copy; {DateTime.Now:yyyy} {_settings.FromName}</div>
    </div>
  </div>
</body>
</html>";
        }

        private string BuildPurchaseRequestProcessedTemplate(string title, string detailsUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <style>
    body,html {{ margin:0; padding:0; background-color:#f0f2f5; font-family:Arial,sans-serif; }}
    .wrapper {{ width:100%; padding:20px 0; }}
    .content {{ max-width:600px; margin:0 auto; background:#fff; border-radius:8px; overflow:hidden; }}
    .header {{
      background:linear-gradient(90deg,#3366FF,#00CCFF);
      color:#fff;
      padding:40px 20px;
      text-align:center;
    }}
    .header h1 {{ margin:0; font-size:24px; }}
    .body {{ padding:30px 20px; color:#444; font-size:16px; line-height:1.5; }}
    .link-button {{
      display:inline-block;
      margin-top:20px;
      padding:12px 24px;
      background:#3366FF;
      color:#fff;
      border-radius:4px;
      text-decoration:none;
      font-weight:bold;
    }}
    .footer {{
      background:#fafafa;
      text-align:center;
      padding:15px;
      font-size:12px;
      color:#888;
    }}
    @media(max-width:600px) {{
      .content {{ margin:0 10px; }}
    }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""content"">
      <div class=""header""><h1>Talep İşleme Alındı</h1></div>
      <div class=""body"">
        <p>Merhaba,</p>
        <p>
          “<strong>{title}</strong>” başlıklı talebiniz işleme alındı ve süreç tamamlandı.
        </p>
        <a href=""{detailsUrl}"" class=""link-button"">Süreç Detayları</a>
      </div>
      <div class=""footer"">&copy; {DateTime.Now:yyyy} {_settings.FromName}</div>
    </div>
  </div>
</body>
</html>";
        }

        public Task SendPurchaseRequestRejectedNotificationAsync(string toEmail, string requestTitle, string detailsUrl)
        {
            var subject = "Talebiniz Reddedildi";
            var body = BuildPurchaseRequestRejectedTemplate(requestTitle, detailsUrl);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        private string BuildPurchaseRequestRejectedTemplate(string title, string detailsUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Talep Reddedildi</title>
</head>
<body style=""margin:0; padding:0; background-color:#f0f2f5; font-family:Arial, sans-serif;"">
  <div style=""width:100%; padding:20px 0;"">
    <div style=""max-width:600px; margin:0 auto; background:#ffffff; border-radius:8px; overflow:hidden;"">
      
      <!-- Header -->
      <div style=""background:linear-gradient(90deg,#dc3545,#e4606d); color:#ffffff; padding:40px 20px; text-align:center;"">
        <h1 style=""margin:0; font-size:24px;"">Talebiniz Reddedildi</h1>
      </div>

      <!-- Body -->
      <div style=""padding:30px 20px; color:#444444; font-size:16px; line-height:1.5;"">
        <p style=""margin:0 0 12px 0;"">Merhaba,</p>
        <p style=""margin:0 0 12px 0;"">
          “<strong>{title}</strong>” başlıklı satın alma talebiniz 
          <strong>reddedilmiştir</strong>.
        </p>
        <p style=""margin:0 0 20px 0;"">
          Detayları görüntülemek için aşağıdaki butona tıklayabilirsiniz.
        </p>

        <a href=""{detailsUrl}""
           style=""display:inline-block; margin-top:8px; padding:12px 24px; background:#dc3545; color:#ffffff; 
                  border-radius:4px; text-decoration:none; font-weight:bold;"">
          Talep Detaylarına Git
        </a>
      </div>

      <!-- Footer -->
      <div style=""background:#fafafa; text-align:center; padding:15px; font-size:12px; color:#888888;"">
        &copy; {DateTime.Now:yyyy} {_settings.FromName}. Tüm hakları saklıdır.
      </div>

    </div>
  </div>
</body>
</html>";
        }


        //================================================================================
        // HANGFIRE JOB EMAILS
        //================================================================================

        public Task SendPendingApprovalReminderAsync(string toEmail, string approverName, int requestCount, string detailsUrl)
        {
            var subject = "Bekleyen Onayınız Var!";
            var body = BuildPendingApprovalReminderTemplate(approverName, requestCount, detailsUrl);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        private string BuildPendingApprovalReminderTemplate(string approverName, int requestCount, string detailsUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Onay Hatırlatması</title>
</head>
<body style=""margin:0; padding:0; background-color:#f0f2f5; font-family:Arial, sans-serif;"">
  <div style=""width:100%; padding:20px 0;"">
    <div style=""max-width:600px; margin:0 auto; background:#ffffff; border-radius:8px; overflow:hidden;"">
      
      <!-- Header -->
      <div style=""background:linear-gradient(90deg,#ffc107,#ff9800); color:#ffffff; padding:40px 20px; text-align:center;"">
        <h1 style=""margin:0; font-size:24px;"">BEKLEYEN ONAYINIZ VAR!</h1>
      </div>

      <!-- Body -->
      <div style=""padding:30px 20px; color:#444444; font-size:16px; line-height:1.5;"">
        <p style=""margin:0 0 12px 0;"">Merhaba {approverName},</p>
        <p style=""margin:0 0 12px 0;"">
          Sistemde onayınızı bekleyen <strong>{requestCount} adet</strong> satın alma talebi bulunmaktadır.
        </p>
        <p style=""margin:0 0 20px 0;"">
          Talepleri incelemek ve işlem yapmak için lütfen aşağıdaki butona tıklayınız.
        </p>

        <a href=""{detailsUrl}""
           style=""display:inline-block; margin-top:8px; padding:12px 24px; background:#ffc107; color:#ffffff; 
                  border-radius:4px; text-decoration:none; font-weight:bold;"">
          Onayları Görüntüle
        </a>
      </div>

      <!-- Footer -->
      <div style=""background:#fafafa; text-align:center; padding:15px; font-size:12px; color:#888888;"">
        © {DateTime.Now:yyyy} {_settings.FromName}. Tüm hakları saklıdır.
      </div>

    </div>
  </div>
</body>
</html>";
        }

        public Task SendDailySummaryToRequesterAsync(string toEmail, string requesterName, int requestCount, string summaryText, string detailsUrl)
        {
            var subject = "Günlük Özet: Onay Bekleyen Talepleriniz";
            var body = BuildDailySummaryToRequesterTemplate(requesterName, requestCount, summaryText, detailsUrl);
            return SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        private string BuildDailySummaryToRequesterTemplate(string requesterName, int requestCount, string summaryText, string detailsUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Günlük Talep Özeti</title>
</head>
<body style=""margin:0; padding:0; background-color:#f0f2f5; font-family:Arial, sans-serif;"">
  <div style=""width:100%; padding:20px 0;"">
    <div style=""max-width:600px; margin:0 auto; background:#ffffff; border-radius:8px; overflow:hidden;"">
      
      <!-- Header -->
      <div style=""background:linear-gradient(90deg,#3366FF,#00CCFF); color:#ffffff; padding:40px 20px; text-align:center;"">
        <h1 style=""margin:0; font-size:24px;"">Günlük Talep Özeti</h1>
      </div>

      <!-- Body -->
      <div style=""padding:30px 20px; color:#444444; font-size:16px; line-height:1.5;"">
        <p style=""margin:0 0 12px 0;"">Merhaba {requesterName},</p>
        <p style=""margin:0 0 20px 0;"">
          Şu anda onay sürecinde olan <strong>{requestCount} adet</strong> talebiniz bulunmaktadır.
        </p>
        
        <div style=""background-color:#f8f9fa; border-left: 4px solid #007bff; padding: 15px; margin-bottom: 20px;"">
            <h3 style=""margin-top:0; margin-bottom:10px; font-size:18px;"">Onaydaki Talepleriniz:</h3>
            <pre style=""white-space: pre-wrap; margin:0; font-family:inherit; font-size:15px;"">{summaryText}</pre>
        </div>

        <p style=""margin:0 0 20px 0;"">
          Tüm taleplerinizin durumunu görmek için aşağıdaki butona tıklayabilirsiniz.
        </p>

        <a href=""{detailsUrl}""
           style=""display:inline-block; margin-top:8px; padding:12px 24px; background:#3366FF; color:#ffffff; 
                  border-radius:4px; text-decoration:none; font-weight:bold;"">
          Taleplerimi Görüntüle
        </a>
      </div>

      <!-- Footer -->
      <div style=""background:#fafafa; text-align:center; padding:15px; font-size:12px; color:#888888;"">
        © {DateTime.Now:yyyy} {_settings.FromName}. Tüm hakları saklıdır.
      </div>

    </div>
  </div>
</body>
</html>";
        }

    } // Schließende Klammer für die EmailManager-Klasse
}
