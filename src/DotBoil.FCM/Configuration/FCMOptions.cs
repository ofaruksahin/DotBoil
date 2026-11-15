using DotBoil.Configuration;

namespace DotBoil.FCM.Configuration
{
    internal class FCMOptions : IOptions
    {
        public string Key => "DotBoil:FCM";
        public FCMFirebaseOptions Firebase { get; set; }
        public FCMPersistenceOptions Persistence { get; set; }
    }

    internal class FCMFirebaseOptions
    {
        // Service account JSON dosyasının yolu
        public string CredentialPath { get; set; }

        // Service account JSON içeriği (CredentialPath yerine kullanılabilir)
        public string CredentialJson { get; set; }
    }

    internal class FCMPersistenceOptions
    {
        public string ConnectionString { get; set; }
    }
}
