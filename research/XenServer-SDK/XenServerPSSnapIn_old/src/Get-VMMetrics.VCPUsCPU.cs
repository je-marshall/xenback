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
    [Cmdlet("Get", "XenServer:VM_metrics.VCPUsCPU", SupportsShouldProcess=false)]
    public class GetXenServerVM_metrics_VCPUsCPUCommand : PSCmdlet
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
        public object VMMetrics
        {
            get { return _vMMetrics; }
            set { _vMMetrics = value; }
        }
        private object _vMMetrics;

        
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
            if (VMMetrics == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"VMMetrics\" must be set"), "", ErrorCategory.InvalidArgument, VMMetrics));
            }
            string vm_metrics = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (VMMetrics is PSObject)
            {
                VMMetrics = ((PSObject)VMMetrics).BaseObject;
            }
  
            if (VMMetrics is XenAPI.VM_metrics) // case object is XenObject
            {
                vm_metrics = ((XenAPI.VM_metrics)VMMetrics).opaque_ref;
            }
            else if (VMMetrics is XenRef<XenAPI.VM_metrics>) // case object is XenRef
            {
                vm_metrics = ((XenRef<XenAPI.VM_metrics>)VMMetrics).opaque_ref;
            }
            else if (VMMetrics is string && CommonCmdletFunctions.IsOpaqueRef((string)VMMetrics)) // case object is OpaqueRef string
            {
                vm_metrics = (string)VMMetrics;
            }
            else if ((VMMetrics is string && CommonCmdletFunctions.IsUuid((string)VMMetrics)) || (VMMetrics is Guid)) // case object is uuid
            {
                if (VMMetrics is Guid)
                    VMMetrics = ((Guid)VMMetrics).ToString();
                XenRef<XenAPI.VM_metrics> obj_ref = XenAPI.VM_metrics.get_by_uuid(session, (string)VMMetrics);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VM_metrics with uuid {0} does not exist",(string)VMMetrics)), "", ErrorCategory.InvalidArgument, VMMetrics));
                }
                vm_metrics = obj_ref.opaque_ref;
            }
            else if (VMMetrics is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.VM_metrics does not support get_by_name_label and VMMetrics is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, VMMetrics));
            }
            else if (VMMetrics == null)
            {
                vm_metrics = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("VMMetrics must be of type XenAPI.VM_metrics, XenRef<XenAPI.VM_metrics>, Guid or string"), "", ErrorCategory.InvalidArgument, VMMetrics));
            }

            
            
            try
            {
                WriteObject(CommonCmdletFunctions.ConvertDictionaryToHashtable(XenAPI.VM_metrics.get_VCPUs_CPU(session, vm_metrics)), true);
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
