using Autofac;
using AutoMapper;
using Business.Abstract;
using Business.Concrete;
using Business.DependencyRepository.AutoMapper;
using Core.Utilities.Security.JWT;
using DataAccess.Abstract;
using DataAccess.Concrete.Dapper;
using DataAccess.Concrete.Dapper.Context;
using DataAccess.Concrete.EntityFramework;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using Core.Utilities.Email;

namespace Business.DependencyRepository.Autofac
{
    public class AutofacBusinessModule : Module
    {

        private readonly IConfiguration _configuration;

        public AutofacBusinessModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {

            // Allow services to inject IConfiguration (e.g. AccountManager uses appsettings for encrypted conn string)
            builder.RegisterInstance(_configuration).As<IConfiguration>().SingleInstance();

            var emailSettings = _configuration
                .GetSection("EmailSettings")
                .Get<EmailSettings>();

            builder
             .RegisterInstance(emailSettings)
             .As<EmailSettings>()
             .SingleInstance();

            builder
             .RegisterType<EmailManager>()
             .As<IEmailService>()
             .InstancePerLifetimeScope();


            #region Dapper

            builder.Register(c =>
            {
                var connectionString = ContextDb.ConnectionStringDefault;
                return new SqlConnection(connectionString);
            }).As<IDbConnection>().InstancePerLifetimeScope();

            builder.RegisterType<LogErrorRepository>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<GenericRepository>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<MiddlewareRepository>().AsSelf().InstancePerLifetimeScope();

            #endregion Dapper

            builder.RegisterType<Enigma.Processor>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<JwtHelper>().As<ITokenHelper>();
            builder.RegisterType<CacheService>().As<ICacheService>();

            builder.RegisterType<EfErrorLogDal>().As<IErrorLogDal>();
            builder.RegisterType<ErrorLogManager>().As<IErrorLogService>();

            builder.RegisterType<EfUserActivityLogDal>().As<IUserActivityLogDal>();
            builder.RegisterType<EfUserDal>().As<IUserDal>().InstancePerLifetimeScope();

            builder.RegisterType<AccountManager>().As<IAccountService>();
            builder.RegisterType<EfRoleDal>().As<IRoleDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfUserRoleDal>().As<IUserRoleDal>().InstancePerLifetimeScope();

            builder.RegisterType<PermissionManager>().As<IPermissionService>();
            builder.RegisterType<SupplierManagementManager>().As<ISupplierManagementService>();

            builder.RegisterType<UserManager>().As<IUserService>();
            builder.RegisterType<EfPartDal>().As<IPartDal>().InstancePerLifetimeScope();

            builder.RegisterType<PartManager>().As<IPartService>();
            builder.RegisterType<EfApprovalRuleDal>().As<IApprovalRuleDal>().InstancePerLifetimeScope();

            builder.RegisterType<EfApprovalRuleApproverDal>().As<IApprovalRuleApproverDal>().InstancePerLifetimeScope();

            builder.RegisterType<ApprovalRuleManager>().As<IApprovalRuleService>();
            builder.RegisterType<EfTenderDal>().As<ITenderDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfTenderItemDal>().As<ITenderItemDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfApprovalRecordDal>().As<IApprovalRecordDal>().InstancePerLifetimeScope();

            builder.RegisterType<TenderManager>().As<ITenderService>();
            builder.RegisterType<EfPreApprovalApproverDal>().As<IPreApprovalApproverDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfTenderOfferDal>().As<ITenderOfferDal>().InstancePerLifetimeScope();

            builder.RegisterType<HomeManager>().As<IHomeService>();

            builder.RegisterType<ApprovalManager>().As<IApprovalService>();
            builder.RegisterType<EfAuditLogDal>().As<IAuditLogDal>().InstancePerLifetimeScope();

            builder.RegisterType<AuditLogManager>().As<IAuditLogService>();
            builder.RegisterType<EfSupplierFirmInfoDal>().As<ISupplierFirmInfoDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfSupplierContactInfoDal>().As<ISupplierContactInfoDal>().InstancePerLifetimeScope();

            builder.RegisterType<AuthorizationManager>().As<IAuthorizationService>();
            builder.RegisterType<EfContractsDal>().As<IContractsDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfContractFilesDal>().As<IContractFilesDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfContractPartsDal>().As<IContractPartsDal>().InstancePerLifetimeScope();

            builder.RegisterType<ContractManager>().As<IContractService>();

            builder.RegisterType<ExcelManager>().As<IExcelService>();
            builder.RegisterType<EfPurchaseCategoryDal>().As<IPurchaseCategoryDal>().InstancePerLifetimeScope();

            builder.RegisterType<PurchaseCategoryManager>().As<IPurchaseCategoryService>();

            builder.RegisterType<ProfileManager>().As<IProfileService>();

            builder.RegisterType<SupplierHomeManager>().As<ISupplierHomeService>();
            builder.RegisterType<EfDepartmentUserDal>().As<IDepartmentUserDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfDepartmentDal>().As<IDepartmentDal>().InstancePerLifetimeScope();

            builder.RegisterType<DepartmentManager>().As<IDepartmentService>();
            builder.RegisterType<EfDepartmentApprovalProcessDal>().As<IDepartmentApprovalProcessDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfApprovalProcessStepDal>().As<IApprovalProcessStepDal>().InstancePerLifetimeScope();

            builder.RegisterType<DepartmentApprovalProcessManager>().As<IDepartmentApprovalProcessService>();
            builder.RegisterType<EfRequestApprovalRecordDal>().As<IRequestApprovalRecordDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfPurchaseRequestDal>().As<IPurchaseRequestDal>().InstancePerLifetimeScope();

            builder.RegisterType<PurchaseRequestManager>().As<IPurchaseRequestService>();
            builder.RegisterType<EfPurchaseRequestItemDal>().As<IPurchaseRequestItemDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfRolePackageDal>().As<IRolePackageDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfPackageRoleDal>().As<IPackageRoleDal>().InstancePerLifetimeScope();
            builder.RegisterType<EfUserPackageDal>().As<IUserPackageDal>().InstancePerLifetimeScope();

            builder.RegisterType<RolePackageManager>().As<IRolePackageService>();

            builder.RegisterType<NotificationJobManager>().As<INotificationJobService>();





            builder.Register(context => new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<Maps>();
            }
            )).AsSelf().SingleInstance();

            builder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();
                var config = context.Resolve<MapperConfiguration>();
                return config.CreateMapper(context.Resolve);
            })
            .As<IMapper>()
            .InstancePerLifetimeScope();
        }
    }
}