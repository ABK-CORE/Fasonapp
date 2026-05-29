using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Enigma;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class ProfileManager : IProfileService
    {
        private readonly IUserDal _userDal;
        private readonly Processor _processor;

        public ProfileManager(IUserDal userDal, Processor processor)
        {
            _userDal = userDal;
            _processor = processor;
        }

        public IDataResult<ProfileDto> GetProfile(Guid userGuid)
        {
            var user = _userDal.Get(u => u.Guid == userGuid);
            if (user == null)
                return new ErrorDataResult<ProfileDto>(null, "Kullanıcı bulunamadı.");

            var dto = new ProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return new SuccessDataResult<ProfileDto>(dto);
        }

        public IResult UpdateProfile(ProfileDto profileDto, Guid userGuid)
        {
            var user = _userDal.Get(u => u.Guid == userGuid);
            if (user == null)
                return new Result(false, "Kullanıcı bulunamadı.");

            if (user.Email == profileDto.Email)
            {
                var checkEmail = _userDal.GetList(u => u.Email == profileDto.Email && u.Guid != userGuid);
                if (checkEmail.Any())
                    return new Result(false, "Bu e-posta adresi zaten kullanımda.");
            }

            user.FirstName = profileDto.FirstName;
            user.LastName = profileDto.LastName;
            user.Email = profileDto.Email;
            user.PhoneNumber = profileDto.PhoneNumber;

            _userDal.Update(user);
            return new SuccessResult("Profil güncellendi.");
        }

        public IResult ChangePassword(ChangePasswordDto changePasswordDto, Guid userGuid)
        {
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
                return new Result(false, "Yeni şifre ile onay şifresi eşleşmiyor.");

            var user = _userDal.Get(u => u.Guid == userGuid);
            if (user == null)
                return new Result(false, "Kullanıcı bulunamadı.");

            // Mevcut şifre kontrolü
            using Aes aes = Aes.Create();
            var encryptedCurrent = _processor.EncryptorSymmetric(changePasswordDto.CurrentPassword, aes);
            if (user.Password != encryptedCurrent)
                return new Result(false, "Mevcut şifre hatalı.");

            // Yeni şifreyi şifrele
            using Aes aes2 = Aes.Create();
            user.Password = _processor.EncryptorSymmetric(changePasswordDto.NewPassword, aes2);

            _userDal.Update(user);
            return new SuccessResult("Şifre başarıyla değiştirildi.");
        }
    }
}
