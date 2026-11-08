using Application.DataTransferObjects.Responses.SystemConfig;
using Application.Repositories.ReadOnly;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class SystemConfigReadOnlyRepository : RestfulRepository<SystemConfigEntity, AppReadOnlyDbContext>,
    ISystemConfigReadOnlyRepository
{
    private readonly DbSet<SystemConfigEntity> _systemConfigEntities;

    public SystemConfigReadOnlyRepository(
        AppReadOnlyDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(applicationDbContext, snowflakeIdService)
    {
        _systemConfigEntities = applicationDbContext.Set<SystemConfigEntity>();
    }

    public async Task<List<SystemConfigDto>> GetListSystemConfigAsync(CancellationToken cancellationToken)
    {
        var existing = await _systemConfigEntities.Select(x => new SystemConfigDto()
        {
            Id = x.Id,
            Key = x.Key,
            Value = x.Value
        }).ToListAsync(cancellationToken);

        var defaultConfigs = new (string Key, string Value)[]
        {
            (Application.Common.Constants.SystemConfig.LockLogin, "5"),
            (Application.Common.Constants.SystemConfig.NumberOfIncorrectEntries, "5"),
            (Application.Common.Constants.SystemConfig.SystemAdministrationFullname, "Administrator"),
            (Application.Common.Constants.SystemConfig.SystemAdministrationPhone, "0901234567"),
            (Application.Common.Constants.SystemConfig.SystemAdministrationEmail, "admin@mdms.vn"),
            (Application.Common.Constants.SystemConfig.CustomerCareFullname, "CSKH"),
            (Application.Common.Constants.SystemConfig.CustomerCarePhone, "19001006"),
            (Application.Common.Constants.SystemConfig.CustomerCareEmail, "cskh@mdms.vn"),

            // Thông tin công ty / đơn vị cung cấp điện mặc định
            (Application.Common.Constants.SystemConfig.CompanyName, "CÔNG TY TNHH VSIP HẢI PHÒNG"),
            (Application.Common.Constants.SystemConfig.CompanyTaxCode, "0200856991"),
            (Application.Common.Constants.SystemConfig.CompanyPhone, "84-225-3959868"),
            (Application.Common.Constants.SystemConfig.CompanyFax, "84-225-3959886"),
            (Application.Common.Constants.SystemConfig.CompanyEmail, "CustomerCare@vsip.com.vn"),
            (Application.Common.Constants.SystemConfig.CompanyWebsite, "https://vsip.com.vn"),
            (Application.Common.Constants.SystemConfig.CompanyProvince, "TP. Hải Phòng"),
            (Application.Common.Constants.SystemConfig.CompanyDistrict, "Huyện Thủy Nguyên"),
            (Application.Common.Constants.SystemConfig.CompanyWard, "Xã An Lư"),
            (Application.Common.Constants.SystemConfig.CompanyAddressDetail, "Văn phòng Điều hành VSIP Hải Phòng, KCN VSIP Hải Phòng"),
            (Application.Common.Constants.SystemConfig.CompanyCountry, "Vietnam"),
            (Application.Common.Constants.SystemConfig.CompanyAddress, "VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam")
        };

        var result = new List<SystemConfigDto>();
        long tempId = 1;

        foreach (var def in defaultConfigs)
        {
            var found = existing.FirstOrDefault(x => string.Equals(x.Key, def.Key, StringComparison.OrdinalIgnoreCase));
            if (found != null)
            {
                result.Add(found);
            }
            else
            {
                result.Add(new SystemConfigDto
                {
                    Id = tempId++,
                    Key = def.Key,
                    Value = def.Value
                });
            }
        }

        foreach (var item in existing)
        {
            if (!result.Any(x => string.Equals(x.Key, item.Key, StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(item);
            }
        }

        return result;
    }
}