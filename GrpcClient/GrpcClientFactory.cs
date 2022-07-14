using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using Cores.Helpers;
using MongoDB.Entities;
using Database.Models;

namespace Cores.Grpc.Client
{
    public static class GrpcClientFactory
    {
        private static Dictionary<string, string> _serviceDict;
        private static Dictionary<string, GrpcChannelFactory> _channelFactoryDict;
        private static int _maxChannelCount = 5;
        private static string _systemConfigUrl = "";

        /// <summary>
        /// Init before using: Create list of GrpcClient (per channel) from Service list
        /// </summary>
        /// <param name="inConfiguration"></param>
        /// <returns></returns>
        public static async Task InitAsync(int maxChannelCount, string systemConfigUrl)
        {
            Console.WriteLine(systemConfigUrl);

            //Max channel count
            _maxChannelCount = maxChannelCount;
            _systemConfigUrl = systemConfigUrl;

            //Load grpc service list
            _serviceDict = new Dictionary<string, string>();
            //
            var serviceList = await GetServiceList();
            if (serviceList != null && serviceList.Count > 0)
            {
                serviceList.ForEach(item =>
                {
                    //Key: ServiceName  Value: Url
                    _serviceDict.Add(item.ServiceName, item.Url);
                });
            }
            else
            {
                //For debug without master
                _serviceDict.Add(ServiceList.Admin, "http://222.253.79.223:5099");
                _serviceDict.Add(ServiceList.Insure, "http://222.253.79.223:5050");
                _serviceDict.Add(ServiceList.Resource, "http://222.253.79.223:5001");
            }

            //Create dict of GrpcChannelFactory
            _channelFactoryDict = new Dictionary<string, GrpcChannelFactory>();
            foreach (var item in _serviceDict)
            {
                _channelFactoryDict.Add(item.Key, new GrpcChannelFactory(item.Value, _maxChannelCount));
            }
        }

        public static async Task ReloadServiceListConfig()
        {
            //Load grpc service list
            var refreshServiceDict = new Dictionary<string, string>();
            //
            var serviceList = await GetServiceList();
            if (serviceList != null)
            {
                serviceList.ForEach(item =>
                {
                    //Key: ServiceName  Value: Url
                    refreshServiceDict.Add(item.ServiceName, item.Url);
                });
            }
            //Create dict of GrpcChannelFactory
            var refreshChannelFactoryDict = new Dictionary<string, GrpcChannelFactory>();
            foreach (var item in refreshServiceDict)
            {
                refreshChannelFactoryDict.Add(item.Key, new GrpcChannelFactory(item.Value, _maxChannelCount));
            }
            //Switch
            _channelFactoryDict = refreshChannelFactoryDict;
            _serviceDict = refreshServiceDict;
        }

        public async static Task<List<ServiceListModel>> GetServiceList()
        {
            var serviceList = new List<ServiceListModel>();
            try
            {
                var records = await DB.Find<mdServiceList>()
                                      .ExecuteAsync();

                foreach (var record in records)
                {
                    var serviceListItem = new ServiceListModel();
                    ClassHelper.CopyPropertiesData(record, serviceListItem);
                    serviceList.Add(serviceListItem);
                }
            }
            catch { }

            //
            return serviceList;
        }

        /// <summary>
        /// Call gRpc service Async mode
        /// </summary>
        /// <typeparam name="TResponse">Return type</typeparam>
        /// <param name="func">Function to execute the call</param>
        /// <returns>TResponse</returns>
        public static async Task<TResponse> CallServiceAsync<TResponse>(string serviceName, Func<GrpcChannel, Task<TResponse>> func)
        {
            try
            {
                //Get the channel
                var channelFactory = _channelFactoryDict[serviceName];
                if (channelFactory == null) return default;
                var channel = channelFactory.GetChannel();

                //Execute the call
                return await func(channel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return default;
            }
        }

        /// <summary>
        /// Call gRpc service Async mode
        /// </summary>
        /// <typeparam name="TResponse">Return type</typeparam>
        /// <param name="func">Function to execute the call</param>
        /// <returns>TResponse</returns>
        public static void CallServiceFireForget(string serviceName, Action<GrpcChannel> action)
        {
            try
            {
                //Get the channel
                var channelFactory = _channelFactoryDict[serviceName];
                if (channelFactory == null) return;
                var channel = channelFactory.GetChannel();

                //Execute the call
                TaskHelper.RunBg(() =>
                {
                    action(channel);
                    return default;
                });
            }
            catch
            {
            }
        }
        /// <summary>
        /// Call service with specificed Url
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="grpcUrl"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static async Task<TResponse> CallServiceAtSpecificUrl<TResponse>(string grpcUrl, Func<GrpcChannel, Task<TResponse>> func)
        {
            var channel = GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions
            {
                HttpHandler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                },
                MaxReceiveMessageSize = 100 * 1024 * 1024, // 100 MB
                MaxSendMessageSize = 100 * 1024 * 1024 // 100 MB
            });
            //
            try
            {
                return await func(channel);
            }
            catch
            {
                return default;
            }
        }

    }//End class
}//End namespace
