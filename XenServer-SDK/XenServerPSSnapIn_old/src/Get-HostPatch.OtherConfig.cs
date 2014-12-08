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
    [Cmdlet("Get", "XenServer:Host_patch.OtherConfig", SupportsShouldProcess=false)]
    public class GetXenServerHost_patch_OtherConfigCommand : PSCmdlet
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
        public object HostPatch
        {
            get { return _hostPatch; }
            set { _hostPatch = value; }
        }
        private object _hostPatch;

        
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
            if (HostPatch == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"HostPatch\" must be set"), "", ErrorCategory.InvalidArgument, HostPatch));
            }
            string host_patch = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (HostPatch is PSObject)
            {
                HostPatch = ((PSObject)HostPatch).BaseObject;
            }
  
            if (HostPatch is XenAPI.Host_patch) // case object is XenObject
            {
                host_patch = ((XenAPI.Host_patch)HostPatch).opaque_ref;
            }
            else if (HostPatch is XenRef<XenAPI.Host_patch>) // case object is XenRef
            {
                host_patch = ((XenRef<XenAPI.Host_patch>)HostPatch).opaque_ref;
            }
            else if (HostPatch is string && CommonCmdletFunctions.IsOpaqueRef((string)HostPatch)) // case object is OpaqueRef string
            {
                host_patch = (string)HostPatch;
            }
            else if ((HostPatch is string && CommonCmdletFunctions.IsUuid((string)HostPatch)) || (HostPatch is Guid)) // case object is uuid
            {
                if (HostPatch is Guid)
                    HostPatch = ((Guid)HostPatch).ToString();
                XenRef<XenAPI.Host_patch> obj_ref = XenAPI.Host_patch.get_by_uuid(session, (string)HostPatch);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Host_patch with uuid {0} does not exist",(string)HostPatch)), "", ErrorCategory.InvalidArgument, HostPatch));
                }
                host_patch = obj_ref.opaque_ref;
            }
            else if (HostPatch is string)
            {
                if ((string)HostPatch == string.Empty)
                {
                    host_patch = "";
                }
                else
                {
                    List<XenRef<XenAPI.Host_patch>> obj_refs = XenAPI.Host_patch.get_by_name_label(session, (string)HostPatch);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Host_patch with name label {0} does not exist",(string)HostPatch)), "", ErrorCategory.InvalidArgument, HostPatch));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.Host_patch with name label {0} exist",(string)HostPatch)), "", ErrorCategory.InvalidArgument, HostPatch));
                    }
                    host_patch = obj_refs[0].opaque_ref;
                }
            }
            else if (HostPatch == null)
            {
                host_patch = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("HostPatch must be of type XenAPI.Host_patch, XenRef<XenAPI.Host_patch>, Guid or string"), "", ErrorCategory.InvalidArgument, HostPatch));
            }

            
            
            try
            {
                WriteObject(CommonCmdletFunctions.ConvertDictionaryToHashtable(XenAPI.Host_patch.get_other_config(session, host_patch)), true);
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
