using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Grpc.Net.Client;
using System.Net.Http;
using System.Threading;

namespace Cores.Grpc.Client
{
    public class GrpcChannelFactory
    {
        private GrpcChannel _channel;
        private List<GrpcChannel> _channelList;
        public readonly bool IsSettingOK = false;
        private readonly int _maxChannelCount = 5;
        // Instantiate random number generator.  
        private readonly Random _random = new Random();
        //
        /// <summary>
        /// Contructor & initalize channel
        /// </summary>
        /// <param name="inUrl">gRpc Url</param>
        /// <param name="inConfiguration">IConfiguration for appsetting.json</param>
        public GrpcChannelFactory(string url, int maxChannelCount = 5)
        {
            //Service is ready
            IsSettingOK = false;

            //Get max channel count
            _maxChannelCount = maxChannelCount;

            //Create list of channel with same Url -> Up perforemance
            if (!String.IsNullOrWhiteSpace(url))
            {
                InitChannel(url);
            }
            if (_channelList.Count > 0)
            {
                IsSettingOK = true;
            }
        }
        /// <summary>
        /// Reinit when service list has Url changed
        /// </summary>
        /// <param name="url">New Url</param>
        public void ReInitChannel(string url)
        {
            InitChannel(url);
        }

        private void InitChannel(string url)
        {
            var channelList = new List<GrpcChannel>();
            //
            for (int i = 0; i < _maxChannelCount; i++)
            {
                var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions
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
                //Add to list
                channelList.Add(channel);
            }
            //Set to main channel list
            _channelList = channelList;
        }

        public GrpcChannel GetChannel()
        {
            //Get the channel
            int channelIndex = _random.Next(0, _maxChannelCount - 1);
            _channel = _channelList[channelIndex];
            //
            return _channel;
        }
    }//End class
}//End namespace
