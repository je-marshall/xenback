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
    [Cmdlet("Get", "XenServer:GPU_group.GPUTypes", SupportsShouldProcess=false)]
    public class GetXenServerGPU_group_GPUTypesCommand : PSCmdlet
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
        public object GPUGroup
        {
            get { return _gPUGroup; }
            set { _gPUGroup = value; }
        }
        private object _gPUGroup;

        
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
            if (GPUGroup == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"GPUGroup\" must be set"), "", ErrorCategory.InvalidArgument, GPUGroup));
            }
            string gpu_group = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (GPUGroup is PSObject)
            {
                GPUGroup = ((PSObject)GPUGroup).BaseObject;
            }
  
            if (GPUGroup is XenAPI.GPU_group) // case object is XenObject
            {
                gpu_group = ((XenAPI.GPU_group)GPUGroup).opaque_ref;
            }
            else if (GPUGroup is XenRef<XenAPI.GPU_group>) // case object is XenRef
            {
                gpu_group = ((XenRef<XenAPI.GPU_group>)GPUGroup).opaque_ref;
            }
            else if (GPUGroup is string && CommonCmdletFunctions.IsOpaqueRef((string)GPUGroup)) // case object is OpaqueRef string
            {
                gpu_group = (string)GPUGroup;
            }
            else if ((GPUGroup is string && CommonCmdletFunctions.IsUuid((string)GPUGroup)) || (GPUGroup is Guid)) // case object is uuid
            {
                if (GPUGroup is Guid)
                    GPUGroup = ((Guid)GPUGroup).ToString();
                XenRef<XenAPI.GPU_group> obj_ref = XenAPI.GPU_group.get_by_uuid(session, (string)GPUGroup);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.GPU_group with uuid {0} does not exist",(string)GPUGroup)), "", ErrorCategory.InvalidArgument, GPUGroup));
                }
                gpu_group = obj_ref.opaque_ref;
            }
            else if (GPUGroup is string)
            {
                if ((string)GPUGroup == string.Empty)
                {
                    gpu_group = "";
                }
                else
                {
                    List<XenRef<XenAPI.GPU_group>> obj_refs = XenAPI.GPU_group.get_by_name_label(session, (string)GPUGroup);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.GPU_group with name label {0} does not exist",(string)GPUGroup)), "", ErrorCategory.InvalidArgument, GPUGroup));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.GPU_group with name label {0} exist",(string)GPUGroup)), "", ErrorCategory.InvalidArgument, GPUGroup));
                    }
                    gpu_group = obj_refs[0].opaque_ref;
                }
            }
            else if (GPUGroup == null)
            {
                gpu_group = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("GPUGroup must be of type XenAPI.GPU_group, XenRef<XenAPI.GPU_group>, Guid or string"), "", ErrorCategory.InvalidArgument, GPUGroup));
            }

            
            
            try
            {
                WriteObject(XenAPI.GPU_group.get_GPU_types(session, gpu_group), true);
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
