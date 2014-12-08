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
    [Cmdlet("Get", "XenServer:VIF.MAC", SupportsShouldProcess=false)]
    public class GetXenServerVIF_MACCommand : PSCmdlet
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
        public object VIF
        {
            get { return _vIF; }
            set { _vIF = value; }
        }
        private object _vIF;

        
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
            if (VIF == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"VIF\" must be set"), "", ErrorCategory.InvalidArgument, VIF));
            }
            string vif = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (VIF is PSObject)
            {
                VIF = ((PSObject)VIF).BaseObject;
            }
  
            if (VIF is XenAPI.VIF) // case object is XenObject
            {
                vif = ((XenAPI.VIF)VIF).opaque_ref;
            }
            else if (VIF is XenRef<XenAPI.VIF>) // case object is XenRef
            {
                vif = ((XenRef<XenAPI.VIF>)VIF).opaque_ref;
            }
            else if (VIF is string && CommonCmdletFunctions.IsOpaqueRef((string)VIF)) // case object is OpaqueRef string
            {
                vif = (string)VIF;
            }
            else if ((VIF is string && CommonCmdletFunctions.IsUuid((string)VIF)) || (VIF is Guid)) // case object is uuid
            {
                if (VIF is Guid)
                    VIF = ((Guid)VIF).ToString();
                XenRef<XenAPI.VIF> obj_ref = XenAPI.VIF.get_by_uuid(session, (string)VIF);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VIF with uuid {0} does not exist",(string)VIF)), "", ErrorCategory.InvalidArgument, VIF));
                }
                vif = obj_ref.opaque_ref;
            }
            else if (VIF is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.VIF does not support get_by_name_label and VIF is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, VIF));
            }
            else if (VIF == null)
            {
                vif = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("VIF must be of type XenAPI.VIF, XenRef<XenAPI.VIF>, Guid or string"), "", ErrorCategory.InvalidArgument, VIF));
            }

            
            
            try
            {
                WriteObject(XenAPI.VIF.get_MAC(session, vif), true);
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
