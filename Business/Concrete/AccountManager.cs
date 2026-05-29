using Business.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using Core.Utilities.Security.JWT;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Enigma;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using System.Security.Cryptography;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;


namespace Business.Concrete;

public class AccountManager : IAccountService
{
    private readonly IUserDal _userDal;
    private readonly ISupplierFirmInfoDal _firmInfoDal;
    private readonly ISupplierContactInfoDal _contactInfoDal;
    private readonly IErrorLogService _errorLogService;
    private readonly Processor _processor;
    private readonly ITokenHelper _tokenHelper;
    private readonly IRoleDal _roleDal;
    private readonly IUserRoleDal _userRoleDal;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AccountManager(
        IUserDal userDal,
        ISupplierFirmInfoDal firmInfoDal,
        ISupplierContactInfoDal contactInfoDal,
        IErrorLogService errorLogService,
        Processor processor,
        ITokenHelper tokenHelper,
        IRoleDal roleDal,
        IUserRoleDal userRoleDal,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userDal = userDal;
        _firmInfoDal = firmInfoDal;
        _contactInfoDal = contactInfoDal;
        _errorLogService = errorLogService;
        _processor = processor;
        _tokenHelper = tokenHelper;
        _roleDal = roleDal;
        _userRoleDal = userRoleDal;
        _emailService = emailService;
        _configuration = configuration;
    }

    public IDataResult<AccessToken> Login(LoginDto loginDto)
    {
        try
        {
            int userId, roleId;
            string? companyName;

            // 1) Appsettings'teki şifreli connection string'i çöz
            var encryptedConnStr = _configuration.GetConnectionString("DatabaseConnection");
            var keyStr = _configuration["AesSettings:Key"];
            var vectorStr = _configuration["AesSettings:Vektor"];
            if (string.IsNullOrEmpty(encryptedConnStr) || string.IsNullOrEmpty(keyStr) || string.IsNullOrEmpty(vectorStr))
                return new ErrorDataResult<AccessToken>(new AccessToken(), "Bağlantı ayarları eksik");

            var aesKey = Convert.FromBase64String(keyStr);
            var aesVector = Convert.FromBase64String(vectorStr);

            string connStr;
            using (Aes aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.IV = aesVector;
                connStr = _processor.DecryptorSymmetric(encryptedConnStr, aes);
            }

            // 2) SP çağır
            using (var con = new SqlConnection(connStr))
            {
                var p = new DynamicParameters();
                p.Add("@Username", loginDto.Username);   // email veya username
                p.Add("@Password", loginDto.Password);   // SP içinde MD4 hash kontrolü yapılacak
                p.Add("@UserID", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@RoleID", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@CompanyName", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

                con.Execute("sp_UserLogin_fason", p, commandType: CommandType.StoredProcedure);

                userId = p.Get<int>("@UserID");
                roleId = p.Get<int>("@RoleID");
                companyName = p.Get<string>("@CompanyName");
            }

            // 3) Kullanıcı kontrolü
            if (userId <= 0)
                return new ErrorDataResult<AccessToken>(new AccessToken(), "Geçersiz kullanıcı adı veya şifre");

            var user = _userDal.Get(u => u.Id == userId);
            if (user == null)
                return new ErrorDataResult<AccessToken>(new AccessToken(), "Kullanıcı bulunamadı");

            if (!user.IsActive)
                return new ErrorDataResult<AccessToken>(new AccessToken(), "Kullanıcı pasif.");
            if (!user.EmailConfirmed)
                return new ErrorDataResult<AccessToken>(new AccessToken(), "E-posta doğrulanmamış.");

            // 4) Rol bilgisi
            var userRoles = new List<string>();
            var roleEntity = _roleDal.Get(r => r.RoleId == roleId);
            if (roleEntity != null)
                userRoles.Add(roleEntity.RoleName);

            // 5) Token oluştur
            var token = _tokenHelper.CreateToken(new TokenUser
            {
                UserId = user.Id,
                UserGuid = user.Guid,
                Username = !string.IsNullOrEmpty(user.Username) ? user.Username : loginDto.Username,
                CompanyName = companyName,
                Roles = userRoles
            });

            token.IsVerified = true;
            return new SuccessDataResult<AccessToken>(token, "Giriş başarılı");
        }
        catch (Exception ex)
        {
            _errorLogService.LogError(ex, new { loginDto }, null, "Login", "AccountManager");
            return new ErrorDataResult<AccessToken>(new AccessToken(), "Bilinmeyen hata");
        }
    }

    public IDataResult<Guid> Register(RegisterDto registerDto)
    {
        try
        {
            // 1) E-posta kontrol
            if (_userDal.GetList(x => x.Email == registerDto.Email).Any())
                return new ErrorDataResult<Guid>(SystemMessages.ResourceAlreadyExists);

            // 2) Şifreyi şifrele
            //  using (Aes aes = Aes.Create())
            //  {
            //     registerDto.Password = _processor.EncryptorSymmetric(registerDto.Password, aes);
            // }

            // 3) User oluştur
            var user = new User
            {
                Guid = Guid.NewGuid(),
                Email = registerDto.Email,
                Password = registerDto.Password,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Username = registerDto.Username,
                PhoneNumber = registerDto.PhoneNumber,
                DateOfBirth = registerDto.DateOfBirth,
                ProfilePictureUrl = registerDto.ProfilePictureUrl,
                Gender = registerDto.Gender,
                Address = registerDto.Address,
                Country = registerDto.Country,
                PreferredLanguage = registerDto.PreferredLanguage,
                TwoFactorEnabled = registerDto.TwoFactorEnabled,
                CreatedDate = DateTime.Now,
                IsActive = true,
                EmailConfirmed = false
            };
            _userDal.Add(user);

            // 4) Firma Bilgileri ekle
            var firmInfo = new SupplierFirmInfo
            {
                UserGuid = user.Guid,
                CompanyName = registerDto.CompanyName,
                TaxOffice = registerDto.TaxOffice,
                TaxNumber = registerDto.TaxNumber,
                TradeRegisterNumber = registerDto.TradeRegisterNumber,
                CompanyType = registerDto.CompanyType,
                MerisNo = registerDto.MerisNo
            };
            _firmInfoDal.Add(firmInfo);

            // 5) İletişim Bilgileri ekle
            var contactInfo = new SupplierContactInfo
            {
                UserGuid = user.Guid,
                ContactName = registerDto.ContactName,
                ContactPosition = registerDto.ContactPosition,
                PhoneNumber = registerDto.ContactPhoneNumber,
                Email = registerDto.ContactEmail,
                Website = registerDto.ContactWebsite,
                Address = registerDto.ContactAddress
            };
            _contactInfoDal.Add(contactInfo);

            // 6) Role ata
            var supplierRole = _roleDal.Get(r => r.RoleName == "Supplier");
            _userRoleDal.Add(new UserRole
            {
                UserGuid = user.Guid,
                RoleId = supplierRole.RoleId,
                AssignedDate = DateTime.Now,
                UserId = user.Id
            });

            SendEmailVerificationCode(new SendEmailVerificationDto { Email = registerDto.Email });

            return new SuccessDataResult<Guid>(user.Guid, "Kayıt başarılı. Lütfen e‑postanızı doğrulayın.");
        }
        catch (Exception ex)
        {
            _errorLogService.LogError(ex, new { registerDto }, null, "Register", "AccountManager");
            return new ErrorDataResult<Guid>(SystemMessages.UnknownError);
        }
    }

    public IDataResult<List<string>> GetUserRoles(Guid user)
    {
        var userInfo = _userRoleDal
            .GetList(u => u.UserGuid == user, u => u.Role)
            .Select(u => u.Role.RoleName)
            .ToList();

        return new SuccessDataResult<List<string>>(userInfo);
    }


    public IResult SendEmailVerificationCode(SendEmailVerificationDto dto)
    {
        var user = _userDal.Get(u => u.Email == dto.Email);
        if (user == null)
            return new Result(false, "Kullanıcı bulunamadı.");

        var code = new Random().Next(100000, 999999).ToString();
        user.EmailConfirmationCode = code;
        user.EmailConfirmationExpireDate = DateTime.Now.AddMinutes(15);
        _userDal.Update(user);

        // Inline şablonlu gönderim
        if (!string.IsNullOrEmpty(user.Email))
            _emailService.SendVerificationCodeAsync(user.Email, code).Wait();

        return new SuccessResult("Doğrulama kodu gönderildi.");
    }

    public IDataResult<AccessToken> ConfirmEmail(ConfirmEmailDto dto)
    {
        var user = _userDal.Get(u => u.Guid == dto.UserGuid);
        if (user == null)
            return new ErrorDataResult<AccessToken>(new AccessToken(), "Kullanıcı bulunamadı.");

        if (user.EmailConfirmed)
            return new ErrorDataResult<AccessToken>(new AccessToken(), "E‑posta zaten onaylı.");

        if (user.EmailConfirmationExpireDate == null
            || user.EmailConfirmationExpireDate < DateTime.Now)
        {
            return new ErrorDataResult<AccessToken>(new AccessToken(), "Doğrulama kodu süresi dolmuş.");
        }

        if (user.EmailConfirmationCode != dto.Code)
            return new ErrorDataResult<AccessToken>(new AccessToken(), "Geçersiz doğrulama kodu.");

        // Onay başarılı, durumu güncelle
        user.EmailConfirmed = true;
        user.EmailConfirmationCode = null;
        user.EmailConfirmationExpireDate = null;
        _userDal.Update(user);

        // Kullanıcının rollerini al
        var userRoles = _userRoleDal
            .GetList(x => x.UserGuid == user.Guid, x => x.Role)
            .Select(x => x.Role.RoleName)
            .ToList();

        // Token oluştur
        var token = _tokenHelper.CreateToken(new TokenUser
        {
            UserId = user.Id,
            UserGuid = user.Guid,
            Username = user.Username,
            CompanyName = _firmInfoDal.Get(x => x.UserGuid == user.Guid)?.CompanyName
        });

        return new SuccessDataResult<AccessToken>(token, "E‑posta doğrulaması başarılı.");
    }

    public IDataResult<bool> SendPasswordResetCode(SendPasswordResetDto dto)
    {
        var user = _userDal.Get(u => u.Email == dto.Email);
        if (user == null)
            return new ErrorDataResult<bool>(false, "Bu e‑posta adresine ait kullanıcı bulunamadı.");

        // 1) Kod üret
        var code = new Random().Next(100000, 999999).ToString();
        user.PasswordResetCode = code;
        user.PasswordResetExpireDate = DateTime.Now.AddMinutes(15);
        _userDal.Update(user);

        // 2) E‑posta gönder (inline template ile)
        if (!string.IsNullOrEmpty(user.Email))
            _emailService.SendPasswordResetCodeAsync(user.Email, code).Wait();

        return new SuccessDataResult<bool>(true, "Şifre sıfırlama kodu e‑postanıza gönderildi.");
    }
    public IDataResult<bool> ResetPassword(ResetPasswordDto dto)
    {
        var user = _userDal.Get(u => u.Email == dto.Email);
        if (user == null)
            return new ErrorDataResult<bool>(false, "Kullanıcı bulunamadı.");

        if (user.PasswordResetExpireDate == null
            || user.PasswordResetExpireDate < DateTime.Now)
            return new ErrorDataResult<bool>(false, "Şifre sıfırlama kodu süresi dolmuş.");

        if (user.PasswordResetCode != dto.Code)
            return new ErrorDataResult<bool>(false, "Geçersiz şifre sıfırlama kodu.");

        // 3) Yeni şifreyi şifrele
        using var aes = Aes.Create();
        user.Password = _processor.EncryptorSymmetric(dto.NewPassword, aes);

        // 4) Alanları temizle
        user.PasswordResetCode = null;
        user.PasswordResetExpireDate = null;
        _userDal.Update(user);

        return new SuccessDataResult<bool>(true, "Şifreniz başarıyla sıfırlandı.");
    }
}