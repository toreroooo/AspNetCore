using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Http.Internal
{
    public sealed class ReusableConnectionInfo : ConnectionInfo
    {
        // Lambdas hoisted to static readonly fields to improve inlining https://github.com/dotnet/roslyn/issues/13624
        private readonly static Func<IFeatureCollection, IHttpConnectionFeature> _newHttpConnectionFeature = f => new HttpConnectionFeature();
        private readonly static Func<IFeatureCollection, ITlsConnectionFeature> _newTlsConnectionFeature = f => new TlsConnectionFeature();

        private FeatureReferences<FeatureInterfaces> _features;

        public ReusableConnectionInfo(IFeatureCollection features)
        {
            Initialize(features);
        }

        public void Initialize(IFeatureCollection features)
        {
            _features = new FeatureReferences<FeatureInterfaces>(features);
        }

        public void Uninitialize()
        {
            _features = default(FeatureReferences<FeatureInterfaces>);
        }

        private IHttpConnectionFeature HttpConnectionFeature =>
            _features.Fetch(ref _features.Cache.Connection, _newHttpConnectionFeature);

        private ITlsConnectionFeature TlsConnectionFeature =>
            _features.Fetch(ref _features.Cache.TlsConnection, _newTlsConnectionFeature);

        /// <inheritdoc />
        public override string Id
        {
            get { return HttpConnectionFeature.ConnectionId; }
            set { HttpConnectionFeature.ConnectionId = value; }
        }

        public override IPAddress RemoteIpAddress
        {
            get { return HttpConnectionFeature.RemoteIpAddress; }
            set { HttpConnectionFeature.RemoteIpAddress = value; }
        }

        public override int RemotePort
        {
            get { return HttpConnectionFeature.RemotePort; }
            set { HttpConnectionFeature.RemotePort = value; }
        }

        public override IPAddress LocalIpAddress
        {
            get { return HttpConnectionFeature.LocalIpAddress; }
            set { HttpConnectionFeature.LocalIpAddress = value; }
        }

        public override int LocalPort
        {
            get { return HttpConnectionFeature.LocalPort; }
            set { HttpConnectionFeature.LocalPort = value; }
        }

        public override X509Certificate2 ClientCertificate
        {
            get { return TlsConnectionFeature.ClientCertificate; }
            set { TlsConnectionFeature.ClientCertificate = value; }
        }

        public override Task<X509Certificate2> GetClientCertificateAsync(CancellationToken cancellationToken = default)
        {
            return TlsConnectionFeature.GetClientCertificateAsync(cancellationToken);
        }

        struct FeatureInterfaces
        {
            public IHttpConnectionFeature Connection;
            public ITlsConnectionFeature TlsConnection;
        }
    }
}
