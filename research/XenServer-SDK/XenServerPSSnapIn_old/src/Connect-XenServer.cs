/*
 * Copyright (c) Citrix Systems, Inc.
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 
 *   1) Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 * 
 *   2) Redistributions in binary form must reproduce the above
 *      copyright notice, this list of conditions and the following
 *      disclaimer in the documentation and/or other materials
 *      provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Management.Automation;
using System.Security;
using System.Security.Cryptography;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using XenAPI;

namespace Citrix.XenServer.Commands
{
    [Cmdlet("Connect", "XenServer")]
    public class ConnectXenServerCommand : PSCmdlet
    {
        #region Cmdlet Parameters

        // Url and Server/Port are mutually exclusive (URL
        // will be used in priority).

        [Parameter]
        public string Url
        {
            get { return url; }
            set { url = value; }
        }
        private string url = null;

        [Parameter]
        [Alias("svr")]
        public string Server
        {
            get { return server; }
            set { server = value; }
        }
        private string server = null;

        [Parameter]
        public int Port
        {
            get { return port; }
            set { port = value; }
        }
        private int port = 443;

        [Parameter(Position = 0,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        [Alias("cred")]
        public PSCredential Creds
        {
            get { return creds; }
            set { creds = value; }
        }
        private PSCredential creds = null;

        [Parameter]
        [Alias("user")]
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }
        private string userName = null;

        [Parameter]
        [Alias("pwd")]
        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        private string password = null;

        [Parameter]
        public string OpaqueRef
        {
            get { return opaqueref; }
            set { opaqueref = value; }
        }
        private string opaqueref;

        [Parameter]
        public SwitchParameter NoWarnNewCertificates
        {
            get { return _noWarnNewCertificates; }
            set { _noWarnNewCertificates = value; }
        }
        private bool _noWarnNewCertificates;

        [Parameter]
        public SwitchParameter NoWarnCertificates
        {
            get { return _noWarnCertificates; }
            set { _noWarnCertificates = value; }
        }
        private bool _noWarnCertificates;

        [Parameter]
        public SwitchParameter Force
        {
            get { return _force; }
            set { _force = value; }
        }
        private bool _force;

        #endregion

        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(Url) && string.IsNullOrEmpty(Server))
            {
                ThrowTerminatingError(new ErrorRecord(
                      new Exception("You must provide a URL, Name or IP Address for the XenServer."),
                      "",
                      ErrorCategory.InvalidArgument,
                      null));
            }

            if (Creds == null && 
                (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password)) &&
                 string.IsNullOrEmpty(OpaqueRef))
            {
                Creds = Host.UI.PromptForCredential("XenServer Credential Request",
                    "",
                    string.IsNullOrEmpty(UserName) ? "root" : UserName,
                    "");
                
                if (Creds == null)
                {
                    // Just bail out at this point, theyve clicked cancel on the credentials pop up dialog
                    ThrowTerminatingError(new ErrorRecord(
                          new Exception("Credentials must be supplied when connecting to the XenServer."),
                          "",
                          ErrorCategory.InvalidArgument,
                          null));
                }
            }

            string connUser = "";
            string connPassword = "";
            if (Creds == null && string.IsNullOrEmpty(OpaqueRef))
            {
                connUser = UserName;
                connPassword = Password;

                SecureString secPwd = new SecureString();
                foreach (char ch in connPassword)
                {
                    secPwd.AppendChar(ch);
                }
                Creds = new PSCredential(UserName, secPwd);
            }
            else if (string.IsNullOrEmpty(OpaqueRef))
            {
                CommonCmdletFunctions.GetUserPassword(Creds, out connUser, out connPassword);
            }

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

            Session session;

            if (string.IsNullOrEmpty(OpaqueRef))
            {
                if (Url == null)
                {
                    session = new Session(Server, Port);
                    Url = session.Url;
                }
                else
                {
                    session = new Session(Url);
                }

                session.login_with_password(connUser, connPassword);
            }
            else
            {
                session = new Session(Url, OpaqueRef);
            }

            XenServerSessions sessions = CommonCmdletFunctions.GetXenServerSessions(this) ?? new XenServerSessions();

            sessions.sessions[Url] = session;
            session.Tag = Creds;

            CommonCmdletFunctions.SetXenServerSessions(this, sessions);

            WriteObject(session, true);
        }

        #region Messages

        const string CERT_HAS_CHANGED_CAPTION = "Security Certificate Changed";

        const string CERT_CHANGED = "The certificate fingerprint of the server you have connected to is:\n{0}\nBut was expected to be:\n{1}\n{2}\nDo you wish to continue?";

        const string CERT_FOUND_CAPTION = "New Security Certificate";

        const string CERT_FOUND = "The certificate fingerprint of the server you have connected to is :\n{0}\n{1}\nDo you wish to continue?";

        const string CERT_TRUSTED = "The certificate on this server is trusted. It is recommended you re-issue this server's certificate.";

        const string CERT_NOT_TRUSTED = "The certificate on this server is not trusted.";

        #endregion
        
        private readonly object certificateValidationLock = new object();

        private bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            lock (certificateValidationLock)
            {
                bool ignoreChanged = NoWarnCertificates || (bool)GetVariableValue("NoWarnCertificates", false);
                bool ignoreNew = ignoreChanged || NoWarnNewCertificates || (bool)GetVariableValue("NoWarnNewCertificates", false);
    
                HttpWebRequest webreq = (HttpWebRequest)sender;
                string hostname = webreq.Address.Host;
                string fingerprint = CommonCmdletFunctions.FingerprintPrettyString(certificate.GetCertHashString());
    
                string trusted = VerifyInAllStores(new X509Certificate2(certificate))
                                     ? CERT_TRUSTED : CERT_NOT_TRUSTED;
    
                var certificates = CommonCmdletFunctions.LoadCertificates();
                bool ok;
    
                if (certificates.ContainsKey(hostname))
                {
                    string fingerprint_old = certificates[hostname];
                    if (fingerprint_old == fingerprint)
                        return true;
    
                    ok = Force || ignoreChanged || ShouldContinue(string.Format(CERT_CHANGED, fingerprint, fingerprint_old, trusted), CERT_HAS_CHANGED_CAPTION);
                }
                else
                {
                    ok = Force || ignoreNew || ShouldContinue(string.Format(CERT_FOUND, fingerprint, trusted), CERT_FOUND_CAPTION);
                }
    
                if (ok)
                {
                    certificates[hostname] = fingerprint;
                    CommonCmdletFunctions.SaveCertificates(certificates);
                }
                return ok;
            }
        }

        private bool VerifyInAllStores(X509Certificate2 certificate2)
        {
            try
            {
                X509Chain chain = new X509Chain(true);
                return chain.Build(certificate2) || certificate2.Verify();
            }
            catch (CryptographicException)
            {
                return false;
            }
        }
    }
}
