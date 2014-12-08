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
    [Cmdlet("Invoke", "XenServer:VDI.Update", SupportsShouldProcess=true)]
    public class InvokeXenServerVDI_UpdateCommand : PSCmdlet
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

        [Parameter]
        public SwitchParameter RunAsync
        {
            get { return runAsync; }
            set { runAsync = value; }
        }
        private bool runAsync;

        
        
        [Parameter(Position = 0,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object VDI
        {
            get { return _vdi; }
            set { _vdi = value; }
        }
        private object _vdi;

        
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
            if (VDI == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"VDI\" must be set"), "", ErrorCategory.InvalidArgument, VDI));
            }
            string vdi = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (VDI is PSObject)
            {
                VDI = ((PSObject)VDI).BaseObject;
            }
  
            if (VDI is XenAPI.VDI) // case object is XenObject
            {
                vdi = ((XenAPI.VDI)VDI).opaque_ref;
            }
            else if (VDI is XenRef<XenAPI.VDI>) // case object is XenRef
            {
                vdi = ((XenRef<XenAPI.VDI>)VDI).opaque_ref;
            }
            else if (VDI is string && CommonCmdletFunctions.IsOpaqueRef((string)VDI)) // case object is OpaqueRef string
            {
                vdi = (string)VDI;
            }
            else if ((VDI is string && CommonCmdletFunctions.IsUuid((string)VDI)) || (VDI is Guid)) // case object is uuid
            {
                if (VDI is Guid)
                    VDI = ((Guid)VDI).ToString();
                XenRef<XenAPI.VDI> obj_ref = XenAPI.VDI.get_by_uuid(session, (string)VDI);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VDI with uuid {0} does not exist",(string)VDI)), "", ErrorCategory.InvalidArgument, VDI));
                }
                vdi = obj_ref.opaque_ref;
            }
            else if (VDI is string)
            {
                if ((string)VDI == string.Empty)
                {
                    vdi = "";
                }
                else
                {
                    List<XenRef<XenAPI.VDI>> obj_refs = XenAPI.VDI.get_by_name_label(session, (string)VDI);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VDI with name label {0} does not exist",(string)VDI)), "", ErrorCategory.InvalidArgument, VDI));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.VDI with name label {0} exist",(string)VDI)), "", ErrorCategory.InvalidArgument, VDI));
                    }
                    vdi = obj_refs[0].opaque_ref;
                }
            }
            else if (VDI == null)
            {
                vdi = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("VDI must be of type XenAPI.VDI, XenRef<XenAPI.VDI>, Guid or string"), "", ErrorCategory.InvalidArgument, VDI));
            }

            
            if (!ShouldProcess(vdi, "VDI.update"))
                return;

            try
            {
                if (RunAsync)
                {
                    XenRef<XenAPI.Task> task_ref = XenAPI.VDI.async_update(session, vdi);
                    XenAPI.Task taskRec = XenAPI.Task.get_record(session, task_ref.opaque_ref);
                    taskRec.opaque_ref = task_ref.opaque_ref;
                    WriteObject(taskRec, true);
                }
                else
                {
                    XenAPI.VDI.update(session, vdi);
                }
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
