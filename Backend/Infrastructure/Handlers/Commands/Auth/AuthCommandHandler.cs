using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Claims;
using Application.Commands.Auth;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Auth;
using Application.Handlers.Commands.Auth;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Nobi.Core.Authorization.Interfaces;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Auth;

public class AuthCommandHandler : IAuthCommandHandlers
{
    private readonly ILoggerService _loggerService;
    private readonly IUserReadWriteRepository _userReadWriteRepository;
    private readonly IPasswordGeneratorService _passwordGeneratorService;
    private readonly IJwtService _jwtService;
    private readonly IUserTokenReadWriteRepository _userTokenReadWriteRepository;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISystemConfigReadOnlyRepository _systemConfigReadOnlyRepository;

    public AuthCommandHandler(ILoggerService loggerService, IUserReadWriteRepository userReadWriteRepository, IPasswordGeneratorService passwordGeneratorService, IJwtService jwtService, IUserTokenReadWriteRepository userTokenReadWriteRepository, ISnowflakeIdService snowflakeIdService, IHttpContextAccessor httpContextAccessor, ISystemConfigReadOnlyRepository systemConfigReadOnlyRepository)
    {
        _loggerService = loggerService;
        _userReadWriteRepository = userReadWriteRepository;
        _passwordGeneratorService = passwordGeneratorService;
        _jwtService = jwtService;
        _userTokenReadWriteRepository = userTokenReadWriteRepository;
        _snowflakeIdService = snowflakeIdService;
        _httpContextAccessor = httpContextAccessor;
        _systemConfigReadOnlyRepository = systemConfigReadOnlyRepository;
    }


    public async Task<RequestResult<SignInResponse>> Handle(SignInCommands request, CancellationToken cancellationToken)
    {
        try
        {
            // var allIPs = GetAllLocalIPv4();
            // foreach (var ip in allIPs)
                // Console.WriteLine(ip);

            var ipAddress = GetIPAddress();
            Console.WriteLine($"ipAddress: {ipAddress}");

            var hostAddress = GetHostMachineIP();
            foreach (var item in hostAddress)
            {
                Console.WriteLine($"hostAddress: {item}");
            }
            var allIPs = NetworkHelper.GetAllIPs();
            var localHostIPs = NetworkHelper.GetLocalHostIPs();

            foreach (var allIP in allIPs)
            {
                Console.WriteLine($"allIP: {allIP}");
            }

            foreach (var item in localHostIPs)
            {
                Console.WriteLine($"localHostIPs: {item}");
            }

            var userIp = GetUserIPAddress(_httpContextAccessor.HttpContext);
            Console.WriteLine($"userIp {userIp}");
            var user = await _userReadWriteRepository.GetUserByUserNameOrEmailAsync(request.UserName, cancellationToken);
            if (user == null)
            {
                return RequestResult<SignInResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.UserName),
                        Error = $"Tên đăng nhập không tồn tại."
                    }
                });
            }

            if (user.Status == EntityStatus.Deactivate)
            {
                var listSystemConfig = await _systemConfigReadOnlyRepository.GetListSystemConfigAsync(cancellationToken);
                return RequestResult<SignInResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Password),
                        Error = $"Không thể đăng nhập bằng tài khoản này, vui lòng liên hệ {listSystemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.SystemAdministrationFullname).Value} qua {listSystemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.SystemAdministrationPhone).Value}, {listSystemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.SystemAdministrationEmail).Value}"
                    }
                });
            }

            if (user.IsLocked && user.TimeLocked >= DateTimeOffset.Now)
            {
                var remainingTime = user.TimeLocked - DateTimeOffset.Now;
                var remainingMinutes = (int)remainingTime.TotalMinutes;
                var remainingSeconds = (int)remainingTime.TotalSeconds % 60;

                return RequestResult<SignInResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Password),
                        Error = $"Tài khoản đang bị khóa trong {remainingMinutes} phút {remainingSeconds} giây, vui lòng thử lại sau"
                    }
                });
            }


            // Verify password or temporary password, if both of them are wrong, return
            if (!_passwordGeneratorService.VerifyHashPassword(user.Password, request.Password))
            {
                var systemConfig = await _systemConfigReadOnlyRepository.GetListSystemConfigAsync(cancellationToken);
                user.CountLocked += 1;
                if (user.CountLocked > int.Parse(systemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.NumberOfIncorrectEntries)?.Value ?? "5"))
                {
                    user.CountLocked = 1;
                    user.IsLocked = false;
                    user.TimeLocked = DateTimeOffset.Now;
                }
                if (user.CountLocked == int.Parse(systemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.NumberOfIncorrectEntries)?.Value ?? "5"))
                {
                    user.IsLocked = true;
                    user.TimeLocked = DateTime.Now.AddMinutes(int.Parse(systemConfig.FirstOrDefault(x => x.Key == Constants.SystemConfig.LockLogin)?.Value ?? "5"));
                }
                await _userReadWriteRepository.UpdateAsync(user, cancellationToken);
                await _userReadWriteRepository.SaveChangesAsync(cancellationToken);

                return RequestResult<SignInResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Password),
                        Error = "Tên đăng nhập hoặc mật khẩu không chính xác. Vui lòng thử lại"
                    }
                });
            }

            user.CountLocked = 0;
            user.IsLocked = false;
            user.TimeLocked = DateTime.Now;
            await _userReadWriteRepository.UpdateAsync(user, cancellationToken);
            await _userReadWriteRepository.SaveChangesAsync(cancellationToken);

            var claimsIdentity = BuildClaimsIdentity(user);
            var tokenResponse = await _jwtService.GenerateJwtAsync(user.Id, claimsIdentity, cancellationToken);
            var userToken = new UserTokenEntity()
            {
                Id = await _snowflakeIdService.GenerateId(cancellationToken),
                UserId = user.Id,
                AccessToken = tokenResponse.Token,
                RefreshToken = tokenResponse.RefreshToken,
            };
            await _userTokenReadWriteRepository.AddAsync(userToken, cancellationToken);
            await _userTokenReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<SignInResponse>.Succeed(new SignInResponse()
            {
                AccessToken = tokenResponse.Token,
                RefreshToken = tokenResponse.RefreshToken,
                Role = user.RoleName,
                UserName = user.UserName,
                UserProfile = new UserProfile()
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                }
            });
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
    private static ClaimsIdentity BuildClaimsIdentity(UserEntity user)
    {
        var claims = new List<Claim>();

        claims.Add(new Claim(JwtClaimTypes.Id, user.Id.ToString()));
        claims.Add(new Claim(JwtClaimTypes.UserName, user.UserName));
        claims.Add(new Claim(JwtClaimTypes.Role, user.RoleName));

        var claimsIdentity = new ClaimsIdentity(claims);
        return claimsIdentity;
    }

    private string GetIPAddress()
    {
        var context = _httpContextAccessor.HttpContext;

        // Get the forwarded IP from the X-Forwarded-For header
        string ipAddress = context.Request.Headers["X-Forwarded-For"];

        if (!string.IsNullOrEmpty(ipAddress))
        {
            // Split by commas and get the first IP in the list (original client IP)
            string[] addresses = ipAddress.Split(',');
            if (addresses.Length > 0)
            {
                // Return the first IP in the list (client's IP)
                return addresses[0].Trim();
            }
        }

        // If X-Forwarded-For is not present, fall back to REMOTE_ADDR
        return context.Connection.RemoteIpAddress?.ToString();
    }

    private static List<string> GetAllLocalIPv4()
    {
        List<string> ipAddrList = new List<string>();

        // Get all network interfaces
        foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            // Check if the interface is up and operational
            if (networkInterface.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation unicastIPAddress in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    // Filter only IPv4 addresses that are not loopback or Docker-related
                    if (unicastIPAddress.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(unicastIPAddress.Address))
                    {
                        ipAddrList.Add(unicastIPAddress.Address.ToString());
                    }
                }
            }
        }
        return ipAddrList;
    }


    private static string GetUserIPAddress(HttpContext httpContext)
    {
        string? ipAddress = null;

        // Check if the request is forwarded by a proxy
        if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        }

        if (string.IsNullOrEmpty(ipAddress))
        {
            // Fallback to RemoteIpAddress
            ipAddress = httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }

        if (string.IsNullOrEmpty(ipAddress))
        {
            throw new Exception("Unable to determine user IP address.");
        }

        return ipAddress;
    }

    private static List<string> GetHostMachineIP()
    {
        List<string> ipList = new List<string>();
        try
        {
            // Try to resolve host.docker.internal
            var hostEntry = Dns.GetHostEntry("host.docker.internal");
            foreach (var ip in hostEntry.AddressList)
            {
                ipList.Add(ip.ToString());
            }
        }
        catch
        {
            // Fallback method - try to get the default route
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    // Connect to a public DNS server (doesn't actually send any data)
                    socket.Connect("8.8.8.8", 65530);
                    if (socket.LocalEndPoint is IPEndPoint endPoint)
                    {
                        ipList.Add(endPoint.Address.ToString());
                    }
                }
            }
            catch
            {
                // If all else fails, try localhost
                return ["127.0.0.1"];
            }
        }
        return ipList;
    }

    public static class NetworkHelper
    {
        public static List<string> GetAllIPs()
        {
            var allIPs = new List<string>();

            try
            {
                // Try to get host.docker.internal IP
                try
                {
                    var hostEntry = Dns.GetHostEntry("host.docker.internal");
                    var hostIp = hostEntry.AddressList
                        .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        ?.ToString();

                    if (!string.IsNullOrEmpty(hostIp))
                    {
                        allIPs.Add($"Host IP: {hostIp}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to get host.docker.internal: {ex.Message}");
                }

                // Get all network interfaces
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (var nic in networkInterfaces)
                {
                    var ipProps = nic.GetIPProperties();

                    // Get Gateway IPs
                    foreach (var gateway in ipProps.GatewayAddresses)
                    {
                        if (gateway?.Address != null)
                        {
                            allIPs.Add($"Gateway ({nic.Name}): {gateway.Address}");
                        }
                    }

                    // Get Interface IPs
                    foreach (var unicastAddr in ipProps.UnicastAddresses)
                    {
                        if (unicastAddr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            allIPs.Add($"Interface ({nic.Name}): {unicastAddr.Address}");
                        }
                    }
                }

                // Try to get default route
                try
                {
                    var defaultRoute = NetworkInterface
                        .GetAllNetworkInterfaces()
                        .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                        .FirstOrDefault()?.Address.ToString();

                    if (!string.IsNullOrEmpty(defaultRoute))
                    {
                        allIPs.Add($"Default Route: {defaultRoute}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to get default route: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting IPs: {ex.Message}");
            }

            return allIPs.Distinct().ToList();
        }

        // Method to specifically get local host IPs
        public static List<string> GetLocalHostIPs()
        {
            var localIPs = new List<string>();

            try
            {
                // Get host name and IPs
                var hostName = Dns.GetHostName();
                var hostEntry = Dns.GetHostEntry(hostName);

                foreach (var ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIPs.Add(ip.ToString());
                    }
                }

                // Try to get host.docker.internal
                try
                {
                    var dockerHostEntry = Dns.GetHostEntry("host.docker.internal");
                    var dockerHostIp = dockerHostEntry.AddressList
                        .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        ?.ToString();

                    if (!string.IsNullOrEmpty(dockerHostIp))
                    {
                        localIPs.Add(dockerHostIp);
                    }
                }
                catch
                {
                    // host.docker.internal not available
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting local host IPs: {ex.Message}");
            }

            return localIPs.Distinct().ToList();
        }
    }
}