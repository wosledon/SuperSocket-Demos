using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace SuperSocket
{
    public class CertificateOptions
    {
        public X509Certificate Certificate { get; set; }


        /// <summary>
        /// Gets the certificate file path (pfx).
        /// 获取证书的路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets the password.
        /// 获取密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets the the store where certificate locates.
        /// 获取证书位于的存储区
        /// </summary>
        /// <value>
        /// The name of the store.
        /// </value>
        public string StoreName { get; set; }

        /// <summary>
        /// Gets the thumbprint.
        /// 获取指纹
        /// </summary>
        public string Thumbprint { get; set; }


        /// <summary>
        /// Gets the store location of the certificate.
        /// 获取证书的存储位置
        /// </summary>
        /// <value>
        /// The store location.
        /// </value>
        public StoreLocation StoreLocation { get; set; }


        /// <summary>
        /// Gets a value indicating whether [client certificate required].
        /// 获取是否需要获取证书
        /// </summary>
        /// <value>
        /// <c>true</c> if [client certificate required]; otherwise, <c>false</c>.
        /// </value>
        public bool ClientCertificateRequired { get; set; }

        /// <summary>
        /// Gets a value that will be used to instantiate the X509Certificate2 object in the CertificateManager
        /// 获取将用于实例化证书管理器中的 X509Certificate2 对象的值
        /// </summary>
        public X509KeyStorageFlags KeyStorageFlags { get; set; }


        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback { get; set; }
        /// <summary>
        /// 确认证书
        /// </summary>
        public void EnsureCertificate()
        {
            // The certificate is there already
            if (Certificate != null)
                return;            

            // load certificate from pfx file
            if (!string.IsNullOrEmpty(FilePath))
            {
                string filePath = FilePath;

                if (!Path.IsPathRooted(filePath))
                {
                    filePath = Path.Combine(AppContext.BaseDirectory, filePath);
                }

                Certificate = new X509Certificate2(filePath, Password, KeyStorageFlags);
            }
            else if (!string.IsNullOrEmpty(Thumbprint)) // load certificate from certificate store
            {
                var storeName = StoreName;
                if (string.IsNullOrEmpty(storeName))
                    storeName = "Root";

                var store = new X509Store((StoreName)Enum.Parse(typeof(StoreName), storeName), StoreLocation);

                store.Open(OpenFlags.ReadOnly);

                Certificate = store.Certificates.OfType<X509Certificate2>().Where(c =>
                        c.Thumbprint.Equals(Thumbprint, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                store.Close();
            }
            else
            {
                throw new Exception($"Either {FilePath} or {Thumbprint} is required to load the certificate.");
            }
        }
    }
}