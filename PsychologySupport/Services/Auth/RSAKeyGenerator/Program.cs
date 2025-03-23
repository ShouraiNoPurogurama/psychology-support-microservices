using System.Security.Cryptography;
using System.Text;

var dirPath = Path.Combine(Environment.CurrentDirectory, "RSAKeys");
if (!Directory.Exists(dirPath))
{
    Directory.CreateDirectory(dirPath);
}

var rsa = RSA.Create();
string privateKeyXml = rsa.ToXmlString(true);
string publicKeyXml = rsa.ToXmlString(false);

using var privateFile = File.Create(Path.Combine(dirPath, "PrivateKey.xml"));
using var publicFile = File.Create(Path.Combine(dirPath, "PublicKey.xml"));

privateFile.Write(Encoding.UTF8.GetBytes(privateKeyXml));
publicFile.Write(Encoding.UTF8.GetBytes(publicKeyXml));