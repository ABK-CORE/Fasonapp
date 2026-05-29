using AutoMapper;
using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    public class AuthorizationManager : IAuthorizationService
    {
        private readonly IUserDal _userDal;
        private readonly IRoleDal _roleDal;
        private readonly IUserRoleDal _userRoleDal;
        private readonly IUserPackageDal _userPackageDal;
        private readonly IMapper _mapper;

        public AuthorizationManager(
            IUserDal userDal,
            IRoleDal roleDal,
            IUserRoleDal userRoleDal,
            IUserPackageDal userPackageDal,
            IMapper mapper)
        {
            _userDal = userDal;
            _roleDal = roleDal;
            _userRoleDal = userRoleDal;
            _userPackageDal = userPackageDal;
            _mapper = mapper;
        }

        public IDataResult<List<UserWithRolesDto>> GetUsers()
        {
            // 1) Kullanıcıları ve rolleri
            var users = _userDal.GetList(
                filter: u => true,
                includeProperties: u => u.UserRoles!
            ).ToList();

            var userGuids = users.Select(u => u.Guid).ToList();

            var userRoles = _userRoleDal.GetList(
                filter: ur => userGuids.Contains(ur.UserGuid),
                includeProperties: ur => ur.Role!
            ).ToList();

            var rolesByUser = userRoles
                .GroupBy(ur => ur.UserGuid)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ur => new RoleDto
                    {
                        Id = ur.Role!.RoleId,
                        Name = ur.Role.RoleName,
                        Description = ur.Role.Description
                    }).ToList()
                );

            // 2) Paket atamalarını yükle
            var userPackages = _userPackageDal.GetList(
                filter: up => userGuids.Contains(up.UserGuid),
                includeProperties: up => up.Package!
            ).ToList();

            var packageByUser = userPackages
                .GroupBy(up => up.UserGuid)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Package  // her kullanıcıya en fazla 1 paket atandığını varsayalım
                );

            // 3) DTO’ya dönüştür
            var result = users
                .Select(u => {
                    packageByUser.TryGetValue(u.Guid, out var pkg);
                    return new UserWithRolesDto
                    {
                        Guid = u.Guid,
                        Email = u.Email!,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Roles = rolesByUser.TryGetValue(u.Guid, out var list) ? list : new List<RoleDto>(),
                        PackageId = pkg?.PackageId,
                        PackageName = pkg?.PackageName
                    };
                })
                .ToList();

            return new SuccessDataResult<List<UserWithRolesDto>>(result);
        }

        public IDataResult<List<RoleDto>> GetRoles()
        {
            var roles = _roleDal.GetList(r => true).ToList();
            var dtos = _mapper.Map<List<RoleDto>>(roles);
            return new SuccessDataResult<List<RoleDto>>(dtos);
        }

        public IResult UpdateUserRoles(Guid userGuid, UpdateUserRolesDto dto)
        {
            // Kullanıcı var mı?
            var user = _userDal.Get(u => u.Guid == userGuid);
            if (user == null)
                return new Result(false, "Kullanıcı bulunamadı.");

            // Mevcut atamalar
            var mevcut = _userRoleDal.GetList(ur => ur.UserGuid == userGuid).ToList();

            // Silinecekleri kaldır
            var toRemove = mevcut.Where(ur => !dto.RoleIds.Contains(ur.RoleId)).ToList();
            foreach (var ur in toRemove)
                _userRoleDal.Delete(ur);

            // Eklenmesi gerekenler
            var mevcutRoleIds = mevcut.Select(ur => ur.RoleId).ToHashSet();
            var toAdd = dto.RoleIds.Where(rid => !mevcutRoleIds.Contains(rid));
            foreach (var rid in toAdd)
            {
                _userRoleDal.Add(new UserRole
                {
                    UserGuid = userGuid,
                    RoleId = rid,
                    AssignedDate = DateTime.UtcNow,
                    UserId = user.Id
                });
            }

            return new SuccessResult("Roller güncellendi.");
        }
    }
}
