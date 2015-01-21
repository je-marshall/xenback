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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

using XenAPI;

namespace Citrix.XenServer
{
    class CommonCmdletFunctions
    {
        private const string SessionsVariable = "Citrix.XenServer.Sessions";
        private const string DefaultSessionVariable = "XenServer_Default_Session";
        private static string CertificatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),@"Citrix\XenServerPSSnapIn\XenServer_Known_Certificates.xml");

        static CommonCmdletFunctions()
        {
            Session.UserAgent = string.Format("XenServerPSSnapIn/{0}", Assembly.GetExecutingAssembly().GetName().Version);
        }

        internal static XenServerSessions GetXenServerSessions(PSCmdlet cmdlet)
        {
            object obj = cmdlet.SessionState.PSVariable.GetValue(SessionsVariable);

            PSObject psObj = obj as PSObject;
            if (psObj != null)
                obj = psObj.BaseObject;
            
            return (XenServerSessions)obj;
        }

        /// <summary>
        /// Save the session dictionary as a PowerShell variable.  It includes the
        /// PSCredential in case the session times out any cmdlet can try to remake
        /// the session
        /// </summary>
        internal static void SetXenServerSessions(PSCmdlet cmdlet, XenServerSessions sessions)
        {
            cmdlet.SessionState.PSVariable.Set(SessionsVariable, sessions);
        }

        internal static Session GetXenServerDefaultSession(PSCmdlet cmdlet)
        {
            object obj = cmdlet.SessionState.PSVariable.GetValue(DefaultSessionVariable);

            PSObject psObj = obj as PSObject;
            if (psObj != null)
                obj = ((PSObject)obj).BaseObject;

            return (Session)obj;
        }

        internal static void SetXenServerDefaultSession(PSCmdlet cmdlet, Session session)
        {
            cmdlet.SessionState.PSVariable.Set(DefaultSessionVariable, session);
        }

        internal static Session GetXenSession(PSCmdlet cmdlet, out XenServerSessions sessions, ref string serverURL, string serverName, int port)
        {
            XenServerSessions sessions_ = GetXenServerSessions(cmdlet);
            if (sessions_ == null)
            {
                cmdlet.ThrowTerminatingError(new ErrorRecord(
                     new Exception("Could not find any open sessions to any XenServers."),
                     "",
                     ErrorCategory.InvalidArgument, null));
            }

            string svr = !string.IsNullOrEmpty(serverURL)
                             ? serverURL
                             : !string.IsNullOrEmpty(serverName)
                                   ? GetUrl(serverName, port)
                                   : null;

            Session session = GetXenServerSession(cmdlet, sessions_, ref svr);

            serverURL = svr;
            sessions = sessions_;
            return session;
        }


        private static Session GetXenServerSession(PSCmdlet cmdlet, XenServerSessions sessions, ref string serverURL)
        {
            Session xenSession = sessions.GetSession(ref serverURL, cmdlet);
            if (xenSession == null)
            {
                cmdlet.ThrowTerminatingError(new ErrorRecord(
                     new Exception("Could not find any open sessions to this XenServer."),
                     "",
                     ErrorCategory.InvalidArgument, serverURL));
            }

            Session session = xenSession;
            
            //test the session to see if it has timed out; if so, recreate
            if (!SessionAlive(session))
            {
                //make a new session connection with the Xen Server
                session = new Session(serverURL);
                PSCredential creds = (PSCredential)xenSession.Tag;
                string connUser = creds.UserName;
                if (connUser.StartsWith("\\"))
                    connUser = creds.GetNetworkCredential().UserName;

                IntPtr ptrPassword = Marshal.SecureStringToBSTR(creds.Password);
                string connPassword = Marshal.PtrToStringBSTR(ptrPassword);
                Marshal.FreeBSTR(ptrPassword);

                session.login_with_password(connUser, connPassword, API_Version.API_1_3);

                //replace the expired session in the session dictionary
                session.Tag = (xenSession.Tag);
                sessions.sessions[serverURL] = session;  
            }
            return session;
        }

        public static void DisconnectXenServerSession(PSCmdlet cmdlet, string serverURL, string serverName, int port)
        {
            XenServerSessions sessions_ = GetXenServerSessions(cmdlet);
            if (sessions_ == null || sessions_.sessions.Count == 0)
            {
                cmdlet.ThrowTerminatingError(new ErrorRecord(
                         new Exception("Could not find any open sessions to this XenServer."),
                         "",
                         ErrorCategory.InvalidArgument, serverURL));
            }

            string svr = !string.IsNullOrEmpty(serverURL)
                             ? serverURL
                             : !string.IsNullOrEmpty(serverName)
                                   ? GetUrl(serverName, port)
                                   : null;

            if(svr == null && sessions_.sessions.Count == 1)
            {
                foreach(string key in sessions_.sessions.Keys)
                {
                    svr = key;
                    break;
                }
            }

            if (svr != null && sessions_.sessions.ContainsKey(svr))
            {
                try
                {
                    sessions_.sessions[svr].logout();
                }
                finally
                {
                    sessions_.sessions.Remove(svr);
                }
            }
            else
            {
                cmdlet.ThrowTerminatingError(new ErrorRecord(
                     new Exception("Could not find any open sessions to this XenServer."),
                     "",
                     ErrorCategory.InvalidArgument, serverURL));
            }
        }

        internal static void GetUserPassword(PSCredential Creds, out string user, out string pwd)
        {
            user = Creds.UserName.StartsWith("\\")
                       ? Creds.GetNetworkCredential().UserName
                       : Creds.UserName;

            IntPtr ptrPassword = Marshal.SecureStringToBSTR(Creds.Password);
            pwd = Marshal.PtrToStringBSTR(ptrPassword);
            Marshal.FreeBSTR(ptrPassword);
        }


        internal static bool MatchesPropertyFilter(Hashtable PropFilter, Type RecordType, object record)
        {
            if (PropFilter == null)
                return true;

            foreach (DictionaryEntry de in PropFilter)
            {
                PropertyInfo pi = RecordType.GetProperty(de.Key.ToString(), BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

                if (pi == null)
                    return false;

                object obj = pi.GetValue(record, null);
                if (obj == null)
                    return false;

                if (!obj.ToString().Equals(de.Value.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    return false;
            }

            return true;
        }


        // TODO: make this public in XenAPI.Session.
        internal static string GetUrl(string hostname, int port)
        {
            return string.Format("{0}://{1}:{2}", port == 80 ? "http" : "https", hostname, port); // https, unless port=80
        }


        private static bool SessionAlive(Session session)
        {
            //TODO: some test
            return true;
        }

        private static Regex UuidRegex = new Regex(@"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex OpaqueRefRegex = new Regex(@"^OpaqueRef:[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsUuid(string uuid)
        {
            return UuidRegex.IsMatch(uuid);
        }

        public static bool IsOpaqueRef(string opaque_ref)
        {
            return OpaqueRefRegex.IsMatch(opaque_ref) || opaque_ref == "OpaqueRef:NULL";
        }
        
        public static Dictionary<string, string> LoadCertificates()
        {
            Dictionary<string, string> certificates = new Dictionary<string, string>();

            if(File.Exists(CertificatePath))
            {
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(CertificatePath);

                    foreach (XmlNode node in doc.GetElementsByTagName("certificate"))
                    {
                        XmlAttribute hostAtt = node.Attributes["hostname"];
                        XmlAttribute fngprtAtt = node.Attributes["fingerprint"];

                        if (hostAtt != null && fngprtAtt != null)
                            certificates[hostAtt.Value] = fngprtAtt.Value;
                    }
                }
                catch
                {}
            }
            return certificates;
        }
        
        public static void SaveCertificates(Dictionary<string, string> certificates)
        {
            string dirName = Path.GetDirectoryName(CertificatePath);

            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            
            XmlDocument doc = new XmlDocument();
            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(decl);
            XmlNode node = doc.CreateElement("certificates");

            foreach(KeyValuePair<string, string> cert in certificates)
            {
                XmlNode cert_node = doc.CreateElement("certificate");
                XmlAttribute hostname = doc.CreateAttribute("hostname");
                XmlAttribute fingerprint = doc.CreateAttribute("fingerprint");
                hostname.Value = cert.Key;
                fingerprint.Value = cert.Value;
                cert_node.Attributes.Append(hostname);
                cert_node.Attributes.Append(fingerprint);
                node.AppendChild(cert_node);
            }
            doc.AppendChild(node);
            try
            {
                doc.Save(CertificatePath);
            }
            catch
            {}
        }
        
        public static string FingerprintPrettyString(string fingerprint)
        {
            List<string> pairs = new List<string>();
            while(fingerprint.Length > 1)
            {
                pairs.Add(fingerprint.Substring(0,2));
                fingerprint = fingerprint.Substring(2);
            }
            if(fingerprint.Length > 0)
                pairs.Add(fingerprint);
            return string.Join(":", pairs.ToArray());
        }

        public static Dictionary<T, S> ConvertHashTableToDictionary<T, S>(Hashtable tbl)
        {
            Dictionary<T, S> dict = new Dictionary<T, S>();
            foreach (DictionaryEntry entry in tbl)
            {
                dict.Add((T)entry.Key, (S)entry.Value);
            }
            return dict;
        }
		
		public static Hashtable ConvertDictionaryToHashtable<T, S>(Dictionary<T, S> dict)
		{
			Hashtable tbl = new Hashtable();
			foreach(KeyValuePair<T, S> pair in dict)
			{
				tbl.Add(pair.Key, pair.Value);
			}
			return tbl;
		}
    }
}
