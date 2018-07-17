using System.Security.Cryptography;
using System.Xml;
using System.Security.Cryptography.Xml;
using Microsoft.Azure.WebJobs.Host;
using System;

namespace Microsoft.Dynamics365.AppSource
{
    class XmlSignature
    {
        private readonly TraceWriter _traceWriterLog;
        public XmlSignature(TraceWriter traceWriter)
        {
            _traceWriterLog = traceWriter;
        }
        
        private RSACryptoServiceProvider GetRSAKeyUsingPublicKey()
        {
            RSACryptoServiceProvider Key = new RSACryptoServiceProvider();
            //Sample Key
            string PublicKeyXmlString = "<RSAKeyValue><Modulus>xlvui12R9N8MvJI7rA19LcUqiWeNAy2uBEYXG2QI0oruAbhUUCJ4JTL0/8syb/jgdNRkoXjEzgOgUmELfa/zftdE6MqcO/SJt4fC6E6RRx70RXizSjOy2ZxGcINP+MbkpfP/6XqKw/ciRNNTL6CxK2rwtcUZ0pB6zrKpZemFb7U=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";            
            Key.FromXmlString(PublicKeyXmlString);
            return Key;
        }

        private bool VerifyXmlFile(string LicenseXml)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                LicenseXml = LicenseXml.Replace("&lt;", "<").Replace("&gt;", ">");

                // Load the passed XML file into the document. 
                xmlDocument.LoadXml(LicenseXml);
                _traceWriterLog.Info("Loaded XML Successfully");
                SignedXml signedXml = new SignedXml(xmlDocument);
                XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature");

                // Load the signature node.
                signedXml.LoadXml((XmlElement)nodeList[0]);                
                // Check the signature and return the result.
                var Key = GetRSAKeyUsingPublicKey();                
                return signedXml.CheckSignature(Key);
            }
            catch (System.Exception)
            {                
                throw;
            }      
        }

        private void Decrypt(XmlDocument Doc, RSA Alg, string KeyName)
        {
            // Check the arguments.  
            if (Doc == null)
                throw new ArgumentNullException("Doc");
            if (Alg == null)
                throw new ArgumentNullException("Alg");
            if (KeyName == null)
                throw new ArgumentNullException("KeyName");

            // Create a new EncryptedXml object.
            EncryptedXml exml = new EncryptedXml(Doc);

            // Add a key-name mapping.
            // This method can only decrypt documents
            // that present the specified key name.
            exml.AddKeyNameMapping(KeyName, Alg);

            // Decrypt the element.
            exml.DecryptDocument();
        }
        public bool StartLicensingProcess(string LicenseXml)
        {
            return VerifyXmlFile(LicenseXml);
        }
    }
}