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
    [Cmdlet("Add", "XenServer:Host_cpu.OtherConfig", SupportsShouldProcess=true)]
    public class AddXenServerHost_cpu_OtherConfigCommand : PSCmdlet
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
        public object HostCpu
        {
            get { return _hostCpu; }
            set { _hostCpu = value; }
        }
        private object _hostCpu;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }
        private string _key;

        [Parameter(Position = 2,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
        private string _value;

        
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
            if (HostCpu == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"HostCpu\" must be set"), "", ErrorCategory.InvalidArgument, HostCpu));
            }
            string host_cpu = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (HostCpu is PSObject)
            {
                HostCpu = ((PSObject)HostCpu).BaseObject;
            }
  
            if (HostCpu is XenAPI.Host_cpu) // case object is XenObject
            {
                host_cpu = ((XenAPI.Host_cpu)HostCpu).opaque_ref;
            }
            else if (HostCpu is XenRef<XenAPI.Host_cpu>) // case object is XenRef
            {
                host_cpu = ((XenRef<XenAPI.Host_cpu>)HostCpu).opaque_ref;
            }
            else if (HostCpu is string && CommonCmdletFunctions.IsOpaqueRef((string)HostCpu)) // case object is OpaqueRef string
            {
                host_cpu = (string)HostCpu;
            }
            else if ((HostCpu is string && CommonCmdletFunctions.IsUuid((string)HostCpu)) || (HostCpu is Guid)) // case object is uuid
            {
                if (HostCpu is Guid)
                    HostCpu = ((Guid)HostCpu).ToString();
                XenRef<XenAPI.Host_cpu> obj_ref = XenAPI.Host_cpu.get_by_uuid(session, (string)HostCpu);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Host_cpu with uuid {0} does not exist",(string)HostCpu)), "", ErrorCategory.InvalidArgument, HostCpu));
                }
                host_cpu = obj_ref.opaque_ref;
            }
            else if (HostCpu is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.Host_cpu does not support get_by_name_label and HostCpu is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, HostCpu));
            }
            else if (HostCpu == null)
            {
                host_cpu = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("HostCpu must be of type XenAPI.Host_cpu, XenRef<XenAPI.Host_cpu>, Guid or string"), "", ErrorCategory.InvalidArgument, HostCpu));
            }

            if (Key == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Key\" must be set"), "", ErrorCategory.InvalidArgument, Key));
            }
            

            if (Value == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Value\" must be set"), "", ErrorCategory.InvalidArgument, Value));
            }
            

            
            if (!ShouldProcess(host_cpu, "Host_cpu.add_to_other_config"))
                return;

            try
            {
                XenAPI.Host_cpu.add_to_other_config(session, host_cpu, Key, Value);
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
