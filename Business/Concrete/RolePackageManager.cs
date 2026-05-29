// Business/Concrete/RolePackageManager.cs
using AutoMapper;
using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    public class RolePackageManager : IRolePackageService
    {
        private readonly IRolePackageDal _rolePackageDal;
        private readonly IPackageRoleDal _packageRoleDal;
        private readonly IUserPackageDal _userPackageDal;
        private readonly IUserRoleDal _userRoleDal;
        private readonly IUserDal _userDal;
        private readonly IRoleDal _roleDal;
        private readonly IMapper _mapper;

        public RolePackageManager(
            IRolePackageDal rolePackageDal,
            IPackageRoleDal packageRoleDal,
            IUserPackageDal userPackageDal,
            IUserRoleDal userRoleDal,
            IUserDal userDal,
            IRoleDal roleDal,
            IMapper mapper)
        {
            _rolePackageDal = rolePackageDal;
            _packageRoleDal = packageRoleDal;
            _userPackageDal = userPackageDal;
            _userRoleDal = userRoleDal;
            _userDal = userDal;
            _roleDal = roleDal;
            _mapper = mapper;
        }

        public IDataResult<List<RolePackageWithRolesDto>> GetAllPackages()
        {
            var packages = _rolePackageDal.GetList().ToList();
            var packageIds = packages.Select(p => p.PackageId).ToList();

            var prs = _packageRoleDal
                .GetList(pr => packageIds.Contains(pr.PackageId), includeProperties: pr => pr.Role!)
                .ToList();

            var rolesByPackage = prs
                .GroupBy(pr => pr.PackageId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(pr => new RoleDto
                    {
                        Id = pr.Role.RoleId,
                        Name = pr.Role.RoleName,
                        Description = pr.Role.Description
                    }).ToList()
                );

            var result = packages
                .Select(p => new RolePackageWithRolesDto
                {
                    PackageId = p.PackageId,
                    PackageName = p.PackageName,
                    Description = p.Description,
                    Roles = rolesByPackage.TryGetValue(p.PackageId, out var list) ? list : new()
                })
                .ToList();

            return new SuccessDataResult<List<RolePackageWithRolesDto>>(result);
        }

        public IResult CreatePackage(CreateRolePackageDto dto)
        {
            // 1. Gelen RoleId'lerin geçerli olup olmadığını denetle
            var validRoleIds = _roleDal
                .GetList(r => dto.RoleIds.Contains(r.RoleId))
                .Select(r => r.RoleId)
                .ToList();
            var invalid = dto.RoleIds.Except(validRoleIds).ToList();
            if (invalid.Any())
                return new Result(false, $"Aşağıdaki rol(ler) bulunamadı: {string.Join(", ", invalid)}");

            // 2. Yeni paket oluştur ve kaydet
            var entity = new RolePackage
            {
                PackageName = dto.PackageName,
                Description = dto.Description
            };

            try
            {
                _rolePackageDal.Add(entity);
            }
            catch (DbUpdateException ex)
            {
                return new Result(false, $"Paket oluşturulurken hata: {ex.Message}");
            }

            // 3. PaketRoller tablosuna ekle
            foreach (var rid in validRoleIds)
            {
                try
                {
                    var role = _roleDal.Get(r => r.RoleId == rid);
                    if (role == null)
                        return new Result(false, $"Rol bulunamadı (RoleId={rid}). Lütfen önce bu rolü ekleyin.");

                    _packageRoleDal.Add(new PackageRole
                    {
                        PackageId = entity.PackageId,
                        RoleId = rid
                    });
                }
                catch (DbUpdateException ex)
                {
                    return new Result(false, $"Rol eklenirken hata (RoleId={rid}): {ex.Message}");
                }
            }

            return new SuccessResult("Paket başarıyla oluşturuldu.");
        }

        public IResult UpdatePackageRoles(int packageId, UpdateRolePackageDto dto)
        {
            // 0) Paket kaydını al ve title/description güncelle
            var pkg = _rolePackageDal.Get(p => p.PackageId == packageId);
            if (pkg == null)
                return new Result(false, "Paket bulunamadı.");

            pkg.PackageName = dto.PackageName;
            pkg.Description = dto.Description;
            _rolePackageDal.Update(pkg);

            // 1) DTO içindeki RoleId’lerin geçerliliğini denetle
            var validRoleIds = _roleDal.GetList(r => dto.RoleIds.Contains(r.RoleId))
                                       .Select(r => r.RoleId)
                                       .ToList();
            var invalid = dto.RoleIds.Except(validRoleIds).ToList();
            if (invalid.Any())
                return new Result(false, $"Bulunamadı: {string.Join(", ", invalid)}");

            // 2) PaketRoller güncellemesi
            var existing = _packageRoleDal.GetList(pr => pr.PackageId == packageId).ToList();
            existing.Where(pr => !validRoleIds.Contains(pr.RoleId))
                    .ToList()
                    .ForEach(pr => _packageRoleDal.Delete(pr));
            validRoleIds.Except(existing.Select(pr => pr.RoleId))
                         .ToList()
                         .ForEach(rid => _packageRoleDal.Add(new PackageRole
                         {
                             PackageId = packageId,
                             RoleId = rid
                         }));

            // 3) Pakete atanmış tüm kullanıcıların rollerini yeniden ata
            var usersInPackage = _userPackageDal.GetList(up => up.PackageId == packageId)
                                                .Select(up => up.UserGuid)
                                                .ToList();
            foreach (var ug in usersInPackage)
            {
                ApplyPackageRolesToUser(ug, packageId);
            }

            return new SuccessResult("Paket bilgisi, rolleri ve kullanıcı atamaları güncellendi.");
        }

        // Yardımcı metot: belirtilen kullanıcının UserRole kayıtlarını silip
        // pakete ait rollerle yeniden oluşturur.
        private void ApplyPackageRolesToUser(Guid userGuid, int packageId)
        {
            // a) Mevcut user-role atamalarını sil
            _userRoleDal.GetList(ur => ur.UserGuid == userGuid)
                        .ToList()
                        .ForEach(ur => _userRoleDal.Delete(ur));

            // b) Paketteki RoleId’leri getir
            var roleIds = _packageRoleDal.GetList(pr => pr.PackageId == packageId)
                                         .Select(pr => pr.RoleId)
                                         .ToList();

            // c) Kullanıcı nesnesini al (UserId için)
            var user = _userDal.Get(u => u.Guid == userGuid);

            // d) Her RoleId için yeni UserRole kaydı oluştur
            foreach (var rid in roleIds)
            {
                _userRoleDal.Add(new UserRole
                {
                    UserGuid = userGuid,
                    RoleId = rid,
                    AssignedDate = DateTime.UtcNow,
                    UserId = user?.Id ?? 0
                });
            }
        }

        public IResult AssignPackageToUser(AssignRolePackageToUserDto dto)
        {
            // 1) Önce eski paket atamalarını kaldır
            var existingPackages = _userPackageDal.GetList(up => up.UserGuid == dto.UserGuid).ToList();
            existingPackages.ForEach(up => _userPackageDal.Delete(up));

            // 2) Yeni paket atamasını ekle
            var userPackage = new UserPackage
            {
                UserGuid = dto.UserGuid,
                PackageId = dto.PackageId,
                AssignedDate = DateTime.UtcNow
            };
            try
            {
                _userPackageDal.Add(userPackage);
            }
            catch (DbUpdateException ex)
            {
                return new Result(false, $"Paket atama hatası: {ex.Message}");
            }

            // 3) Bu kullanıcının rollerini paketteki rollerle güncelle
            try
            {
                ApplyPackageRolesToUser(dto.UserGuid, dto.PackageId);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Kullanıcıya paket rolleri atanırken hata: {ex.Message}");
            }

            return new SuccessResult("Paket başarıyla atandı ve roller güncellendi.");
        }

        public IResult DeletePackage(int packageId)
        {
            // 1) Paket var mı?
            var pkg = _rolePackageDal.Get(p => p.PackageId == packageId);
            if (pkg == null)
                return new Result(false, "Paket bulunamadı.");

            // 2) Kullanıcı–paket atamalarını sil
            var ups = _userPackageDal.GetList(up => up.PackageId == packageId).ToList();
            ups.ForEach(up => _userPackageDal.Delete(up));

            // 3) Paket–rol ilişkilerini sil
            var prs = _packageRoleDal.GetList(pr => pr.PackageId == packageId).ToList();
            prs.ForEach(pr => _packageRoleDal.Delete(pr));

            // 4) Paket kaydını sil
            _rolePackageDal.Delete(pkg);

            return new SuccessResult("Paket başarıyla silindi.");
        }
    }
}
