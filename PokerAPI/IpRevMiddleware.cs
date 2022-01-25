using DataService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ModelService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PokerAPI
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class IpRevMiddleware
    {
        private readonly RequestDelegate _next;
        //private readonly ApplicationDbContext _db;
        private IServiceScopeFactory serviceScopeFactory;

        public IpRevMiddleware(RequestDelegate next,  IServiceScopeFactory serviceScopeFactory)
        {//ApplicationDbContext applicationDbContext,
            _next = next;
            //_db = //applicationDbContext;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public void Test()
        {
            int incomingMac = 1;
            int incomingIp = 1;

            bool success = false;

            Dictionary<int, int> bannedIps = new Dictionary<int, int>();

            Dictionary<int, int> greenIps = new Dictionary<int, int>();

            if (greenIps.ContainsKey(incomingMac))
            {
                if (greenIps[incomingMac] == incomingIp)
                {
                    if (!bannedIps.ContainsKey(incomingMac))
                        bannedIps.Add(incomingMac, incomingIp);
                    success = false;
                }
                else
                {
                    success = true;
                    greenIps.Add(incomingMac, incomingIp);
                }
            }
            else
            {
                success = true;
                greenIps.Add(incomingMac, incomingIp);
            }
        }

        public async Task Invoke(HttpContext httpContext)
        {
            //using (var scope = serviceScopeFactory.CreateScope())
            //{
            //    var _db = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var ipRec = new UserIpInfo();

                var remoteIp = httpContext.Connection.RemoteIpAddress.Address;

                //var ipRecs = _db.IpRecords.ToList();

                //if (ipRecs.Count < 1) return;

                //ipRec.IpAddress = remoteIp.Address.ToString();
                ipRec.MacAddress = "test";//await GetMacAddress(ipRec.IpAddress);

                //var ipBytes = remoteIp.GetAddressBytes();

                //if (ipRecs.Count > 1)
                //    ipRec.BadIp = ipRecs.Any(_ => _.MacAddress == ipRec.MacAddress);
                    //ipRec.BadIp = ipRecs.Any(_ => IPAddress.Parse(_.IpAddress).GetAddressBytes().SequenceEqual(remoteIp.GetAddressBytes())) ? true : false;

                //foreach (var address in ipRecs)
                //{
                //    var testIp = IPAddress.Parse(address.IpAddress);
                //    if (testIp.GetAddressBytes().SequenceEqual(ipBytes))
                //    {
                //        ipRec.BadIp = false;
                //        break;
                //    }
                //}

                await RecordRequest(ipRec);

                if (ipRec.BadIp)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }

                //return 
                await _next(httpContext);
            //}
            //_logger.LogWarning(
            //    "Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
        }

        public async Task RecordRequest(UserIpInfo ipInfoUser)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var _db = scope.ServiceProvider.GetService<ApplicationDbContext>();

                using (var transaction = _db.Database.BeginTransaction())
                {
                    ipInfoUser.RecDate = DateTime.Now;

                    await _db.IpRecords.AddAsync(ipInfoUser);
                    await _db.SaveChangesAsync();
                }
            }
        }

        public async Task<string> GetMacAddress(string ip)
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface nic in nics)
            {
                foreach (UnicastIPAddressInformation unip in nic.GetIPProperties().UnicastAddresses)
                {
                    if (unip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (unip.Address.ToString() == ip)
                        {
                            PhysicalAddress address = nic.GetPhysicalAddress();
                            return BitConverter.ToString(address.GetAddressBytes());
                        }
                    }
                }
            }

            await Task.Delay(0);
            return string.Empty;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class IpRevMiddlewareExtensions
    {
        public static IApplicationBuilder UseIpRevMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IpRevMiddleware>();
        }
    }
}
