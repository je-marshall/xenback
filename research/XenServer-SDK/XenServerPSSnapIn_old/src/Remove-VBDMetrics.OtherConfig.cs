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
    [Cmdlet("Remove", "XenServer:VBD_metrics.OtherConfig", SupportsShouldProcess=true)]
    public class RemoveXenServerVBD_metrics_OtherConfigCommand : PSCmdlet
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
        public object VBDMetrics
        {
            get { return _vBDMetrics; }
            set { _vBDMetrics = value; }
        }
        private object _vBDMetrics;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }
        private string _key;

        
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
            if (VBDMetrics == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"VBDMetrics\" must be set"), "", ErrorCategory.InvalidArgument, VBDMetrics));
            }
            string vbd_metrics = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (VBDMetrics is PSObject)
            {
                VBDMetrics = ((PSObject)VBDMetrics).BaseObject;
            }
  
            if (VBDMetrics is XenAPI.VBD_metrics) // case object is XenObject
            {
                vbd_metrics = ((XenAPI.VBD_metrics)VBDMetrics).opaque_ref;
            }
            else if (VBDMetrics is XenRef<XenAPI.VBD_metrics>) // case object is XenRef
            {
                vbd_metrics = ((XenRef<XenAPI.VBD_metrics>)VBDMetrics).opaque_ref;
            }
            else if (VBDMetrics is string && CommonCmdletFunctions.IsOpaqueRef((string)VBDMetrics)) // case object is OpaqueRef string
            {
                vbd_metrics = (string)VBDMetrics;
            }
            else if ((VBDMetrics is string && CommonCmdletFunctions.IsUuid((string)VBDMetrics)) || (VBDMetrics is Guid)) // case object is uuid
            {
                if (VBDMetrics is Guid)
                    VBDMetrics = ((Guid)VBDMetrics).ToString();
                XenRef<XenAPI.VBD_metrics> obj_ref = XenAPI.VBD_metrics.get_by_uuid(session, (string)VBDMetrics);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VBD_metrics with uuid {0} does not exist",(string)VBDMetrics)), "", ErrorCategory.InvalidArgument, VBDMetrics));
                }
                vbd_metrics = obj_ref.opaque_ref;
            }
            else if (VBDMetrics is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.VBD_metrics does not support get_by_name_label and VBDMetrics is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, VBDMetrics));
            }
            else if (VBDMetrics == null)
            {
                vbd_metrics = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("VBDMetrics must be of type XenAPI.VBD_metrics, XenRef<XenAPI.VBD_metrics>, Guid or string"), "", ErrorCategory.InvalidArgument, VBDMetrics));
            }

            if (Key == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Key\" must be set"), "", ErrorCategory.InvalidArgument, Key));
            }
            

            
            if (!ShouldProcess(vbd_metrics, "VBD_metrics.remove_from_other_config"))
                return;

            try
            {
                XenAPI.VBD_metrics.remove_from_other_config(session, vbd_metrics, Key);
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
