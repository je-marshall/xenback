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
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using System.Collections;
using System.Text.RegularExpressions;
using XenAPI;

namespace Citrix.XenServer.Commands
{
    [Cmdlet("Set", "XenServer:Secret.OtherConfig", SupportsShouldProcess=true)]
    public class SetXenServerSecret_OtherConfigCommand : PSCmdlet
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

        [Parameter]
        public SwitchParameter BestEffort
        {
            get { return bestEffort; }
            set { bestEffort = value; }
        }
        private bool bestEffort;

        

        
        
        [Parameter(Position = 0,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object Secret
        {
            get { return _secret; }
            set { _secret = value; }
        }
        private object _secret;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable OtherConfig
        {
            get { return _otherConfig; }
            set { _otherConfig = value; }
        }
        private Hashtable _otherConfig;

        
        #endregion
    
        #region Cmdlet Methods

        protected override void ProcessRecord()
        {
            XenServerSessions sessions;
            Session session = CommonCmdletFunctions.GetXenSession(this,
                                                    out sessions,
                                                    ref url,
                                                    Server, Port);
            
            // check commands for null-ness
            if (Secret == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Secret\" must be set"), "", ErrorCategory.InvalidArgument, Secret));
            }
            string secret = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (Secret is PSObject)
            {
                Secret = ((PSObject)Secret).BaseObject;
            }
  
            if (Secret is XenAPI.Secret) // case object is XenObject
            {
                secret = ((XenAPI.Secret)Secret).opaque_ref;
            }
            else if (Secret is XenRef<XenAPI.Secret>) // case object is XenRef
            {
                secret = ((XenRef<XenAPI.Secret>)Secret).opaque_ref;
            }
            else if (Secret is string && CommonCmdletFunctions.IsOpaqueRef((string)Secret)) // case object is OpaqueRef string
            {
                secret = (string)Secret;
            }
            else if ((Secret is string && CommonCmdletFunctions.IsUuid((string)Secret)) || (Secret is Guid)) // case object is uuid
            {
                if (Secret is Guid)
                    Secret = ((Guid)Secret).ToString();
                XenRef<XenAPI.Secret> obj_ref = XenAPI.Secret.get_by_uuid(session, (string)Secret);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Secret with uuid {0} does not exist",(string)Secret)), "", ErrorCategory.InvalidArgument, Secret));
                }
                secret = obj_ref.opaque_ref;
            }
            else if (Secret is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.Secret does not support get_by_name_label and Secret is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, Secret));
            }
            else if (Secret == null)
            {
                secret = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Secret must be of type XenAPI.Secret, XenRef<XenAPI.Secret>, Guid or string"), "", ErrorCategory.InvalidArgument, Secret));
            }

            if (OtherConfig == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"OtherConfig\" must be set"), "", ErrorCategory.InvalidArgument, OtherConfig));
            }
            

            
            if (!ShouldProcess(secret, "Secret.set_other_config"))
                return;

            try
            {
                XenAPI.Secret.set_other_config(session, secret, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(OtherConfig));
            }
            catch (Exception e)
            {
                // if you want to trap errors either set command-line switch "-BestEffort" or session-state variable "$BestEffort" to "$true"
                bool best_effort = (bool)GetVariableValue("BestEffort", false) || bestEffort;
                if (!best_effort)
                    throw;
                // catch exception and write it to the terminal then return
                // don't throw it because this will break piping a list into the cmd (wont run rest of list)
                ThrowTerminatingError(new ErrorRecord(e, "", ErrorCategory.InvalidOperation, null));
            }
            //save session dictionary back in the session variable (in case it was modified)
            CommonCmdletFunctions.SetXenServerSessions(this, sessions);
        }

        #endregion
   }
}
