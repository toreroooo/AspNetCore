using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http.Internal
{
    public sealed class ReusableWebSocketManager : WebSocketManager
    {
        // Lambdas hoisted to static readonly fields to improve inlining https://github.com/dotnet/roslyn/issues/13624
        private readonly static Func<IFeatureCollection, IHttpRequestFeature> _nullRequestFeature = f => null;
        private readonly static Func<IFeatureCollection, IHttpWebSocketFeature> _nullWebSocketFeature = f => null;

        private FeatureReferences<FeatureInterfaces> _features;

        public ReusableWebSocketManager(IFeatureCollection features)
        {
            Initialize(features);
        }

        public void Initialize(IFeatureCollection features)
        {
            _features = new FeatureReferences<FeatureInterfaces>(features);
        }

        public void Uninitialize()
        {
            _features = default;
        }

        private IHttpRequestFeature HttpRequestFeature =>
            _features.Fetch(ref _features.Cache.Request, _nullRequestFeature);

        private IHttpWebSocketFeature WebSocketFeature =>
            _features.Fetch(ref _features.Cache.WebSockets, _nullWebSocketFeature);

        public override bool IsWebSocketRequest
        {
            get
            {
                return WebSocketFeature != null && WebSocketFeature.IsWebSocketRequest;
            }
        }

        public override IList<string> WebSocketRequestedProtocols
        {
            get
            {
                return ParsingHelpers.GetHeaderSplit(HttpRequestFeature.Headers, HeaderNames.WebSocketSubProtocols);
            }
        }

        public override Task<WebSocket> AcceptWebSocketAsync(string subProtocol)
        {
            if (WebSocketFeature == null)
            {
                throw new NotSupportedException("WebSockets are not supported");
            }
            return WebSocketFeature.AcceptAsync(new WebSocketAcceptContext() { SubProtocol = subProtocol });
        }

        struct FeatureInterfaces
        {
            public IHttpRequestFeature Request;
            public IHttpWebSocketFeature WebSockets;
        }
    }
}
