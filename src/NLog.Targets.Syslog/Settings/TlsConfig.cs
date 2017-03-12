// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Security.Cryptography.X509Certificates;

namespace NLog.Targets.Syslog.Settings
{
    public class TlsConfig
    {
        /// <summary>Whether to use TLS or not (TLS 1.2 only)</summary>
        public bool Enabled { get; set; }

        /// <summary>Whether to use client certificates or not</summary>
        public bool UseClientCertificates { get; set; }

        /// <summary>The location of the X.509 certificate store</summary>
        public StoreLocation CertificateStoreLocation { get; set; }

        /// <summary>The name of the X.509 certificate store</summary>
        public StoreName CertificateStoreName { get; set; }

        /// <summary>The type of filter to apply to the certificate collection</summary>
        public X509FindType CertificateFilterType { get; set; }

        /// <summary>The value against which to filter the certificate collection</summary>
        /// <remarks> If omitted the certificate collection is not filtered</remarks>
        public string CertificateFilterValue { get; set; }

        public TlsConfig()
        {
            Enabled = false;
            UseClientCertificates = false;
            CertificateStoreLocation = StoreLocation.CurrentUser;
            CertificateStoreName = StoreName.My;
            CertificateFilterType = X509FindType.FindBySubjectName;
            CertificateFilterValue = null;
        }

        internal X509Certificate2Collection RetrieveClientCertificates()
        {
            if (!UseClientCertificates)
                return null;

            var store = new X509Store(CertificateStoreName, CertificateStoreLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                return CertificateFilterValue == null ? store.Certificates : store.Certificates.Find(CertificateFilterType, BuildFindValue(), false);
            }
            finally
            {
                store.Close();
            }
        }

        internal object BuildFindValue()
        {
            switch (CertificateFilterType)
            {
                case X509FindType.FindByTimeExpired:
                case X509FindType.FindByTimeNotYetValid:
                case X509FindType.FindByTimeValid:
                {
                    return DateTime.Parse(CertificateFilterValue);
                }
                case X509FindType.FindByKeyUsage:
                {
                    int keyUsages;
                    if (int.TryParse(CertificateFilterValue, out keyUsages))
                        return keyUsages;
                    return CertificateFilterValue;
                }
                default:
                {
                    return CertificateFilterValue;
                }
            }
        }
    }
}