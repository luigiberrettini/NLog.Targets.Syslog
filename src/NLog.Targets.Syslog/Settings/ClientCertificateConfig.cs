
namespace NLog.Targets.Syslog.Settings
{
    public class ClientCertificateConfig
    {
        /// <summary> Helper boolean used to see if this was defined </summary>
        public bool Enabled { get { return StoreLocation != null && StoreName != null; } }

        /// <summary> Certificate Store Location, passed to X509Store() </summary>
        public string StoreLocation { get; set; }
        /// <summary> Certificate Store Name, passed to X509Store() </summary>
        public string StoreName { get; set; }
        /// <summary> (Optional) Subject Name (CN) of the certificate to use. 
        /// <remarks> If omitted, all certificates found in the selected store will be available for use in the connection to the syslog server. </remarks>
        public string SubjectName { get; set; }

        public ClientCertificateConfig()
        {
            StoreLocation = null;
            StoreName = null;
            SubjectName = null;
        }
    }
}
