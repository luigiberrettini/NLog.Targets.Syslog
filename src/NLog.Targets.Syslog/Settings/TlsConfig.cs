// Licensed under the BSD license
// See the LICENSE file in the project root for more information

using System;
using System.Security.Cryptography.X509Certificates;

namespace NLog.Targets.Syslog.Settings
{
    /// <summary>TLS configuration</summary>
    public class TlsConfig : NotifyPropertyChanged
    {
        private bool enabled;
        private bool useClientCertificates;
        private StoreLocation certificateStoreLocation;
        private StoreName certificateStoreName;
        private X509FindType certificateFilterType;
        private string certificateFilterValue;

        /// <summary>Whether to use TLS or not (TLS 1.2 only)</summary>
        public bool Enabled
        {
            get { return enabled; }
            set { SetProperty(ref enabled, value); }
        }

        /// <summary>Whether to use client certificates or not</summary>
        public bool UseClientCertificates
        {
            get { return useClientCertificates; }
            set { SetProperty(ref useClientCertificates, value); }
        }

        /// <summary>The X.509 certificate store location</summary>
        public StoreLocation CertificateStoreLocation
        {
            get { return certificateStoreLocation; }
            set { SetProperty(ref certificateStoreLocation, value); }
        }

        /// <summary>The X.509 certificate store name</summary>
        public StoreName CertificateStoreName
        {
            get { return certificateStoreName; }
            set { SetProperty(ref certificateStoreName, value); }
        }

        /// <summary>The type of filter to apply to the certificate collection</summary>
        public X509FindType CertificateFilterType
        {
            get { return certificateFilterType; }
            set { SetProperty(ref certificateFilterType, value); }
        }

        /// <summary>The value against which to filter the certificate collection</summary>
        /// <remarks> If omitted the certificate collection is not filtered</remarks>
        public string CertificateFilterValue
        {
            get { return certificateFilterValue; }
            set { SetProperty(ref certificateFilterValue, value); }
        }

        /// <summary>Builds a new instance of the TlsConfig class</summary>
        public TlsConfig()
        {
            enabled = false;
            useClientCertificates = false;
            certificateStoreLocation = StoreLocation.CurrentUser;
            certificateStoreName = StoreName.My;
            certificateFilterType = X509FindType.FindBySubjectName;
            certificateFilterValue = null;
        }

        internal X509Certificate2Collection RetrieveClientCertificates()
        {
            if (!useClientCertificates)
                return null;

            var store = new X509Store(certificateStoreName, certificateStoreLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                return certificateFilterValue == null ? store.Certificates : store.Certificates.Find(certificateFilterType, BuildFindValue(), false);
            }
            finally
            {
                store.Close();
            }
        }

        private object BuildFindValue()
        {
            switch (certificateFilterType)
            {
                case X509FindType.FindByTimeExpired:
                case X509FindType.FindByTimeNotYetValid:
                case X509FindType.FindByTimeValid:
                {
                    return DateTime.Parse(certificateFilterValue);
                }
                case X509FindType.FindByKeyUsage:
                {
                    int keyUsages;
                    if (int.TryParse(certificateFilterValue, out keyUsages))
                        return keyUsages;
                    return certificateFilterValue;
                }
                default:
                {
                    return certificateFilterValue;
                }
            }
        }
    }
}