using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;

namespace Business.Concrete
{
    public class AuditLogManager : IAuditLogService
    {
        private readonly IAuditLogDal _auditLogDal;
        private readonly IUserDal _userDal;
        private readonly IMapper _mapper;

        public AuditLogManager(
            IAuditLogDal auditLogDal,
            IUserDal userDal,
            IMapper mapper)
        {
            _auditLogDal = auditLogDal;
            _userDal = userDal;
            _mapper = mapper;
        }

        public IResult Log(
            Guid userGuid,
            string message,
            string type,
            string? category = null,
            int? requestId = null
            )
        {
            var log = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = userGuid,
                Message = message,
                Category = category,
                RequestId = requestId,
                Type = type

            };
            _auditLogDal.Add(log);
            return new SuccessResult();
        }

        public IDataResult<List<AuditLogDto>> GetAll()
        {
            var logs = _auditLogDal
                .GetList()
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            var dto = _mapper.Map<List<AuditLogDto>>(logs);
            return new SuccessDataResult<List<AuditLogDto>>(dto);
        }

        public IDataResult<List<AuditLogDto>> GetByFilter(int? requestId, string? category, int? take = null)
        {
            var query = _auditLogDal.GetList().AsQueryable();

            if (requestId.HasValue)
                query = query.Where(x => x.RequestId == requestId.Value);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(x => x.Category == category);

            query = query.OrderByDescending(x => x.Timestamp);

            if (take.HasValue && take.Value > 0)
                query = query.Take(take.Value);

            var entities = query.ToList();

            var dtos = _mapper.Map<List<AuditLogDto>>(entities);

            foreach (var dto in dtos)
            {
                var user = _userDal.Get(u => u.Guid == dto.UserGuid);
                dto.UserName = user != null
                    ? $"{user.FirstName} {user.LastName}"
                    : "—";
            }

            return new SuccessDataResult<List<AuditLogDto>>(dtos);
        }
    }
}