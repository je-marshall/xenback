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
    [Cmdlet("Create", "XenServer:VTPM", SupportsShouldProcess=true)]
    public class CreateXenServerVTPM_CreateCommand : PSCmdlet
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

        [Parameter]
        public Hashtable HashTable
        {
            get { return hashtable; }
            set { hashtable = value; }
        }
        private Hashtable hashtable;

        [Parameter(Position = 0,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object VM
        {
            get { return _vM; }
            set { _vM = value; }
        }
        private object _vM;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object Backend
        {
            get { return _backend; }
            set { _backend = value; }
        }
        private object _backend;

        
        [Parameter]
        public XenAPI.VTPM Record
        {
            get { return _record; }
            set { _record = value; }
        }
        private XenAPI.VTPM _record;

        
        #endregion
    
        #region Cmdlet Methods

        protected override void ProcessRecord()
        {
            XenServerSessions sessions;
            Session session = CommonCmdletFunctions.GetXenSession(this,
                                                    out sessions,
                                                    ref url,
                                                    Server, Port);
            if (Record == null && HashTable == null)
            {
                Record = new XenAPI.VTPM();
                
            string vm = null;

            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (VM is PSObject)
            {
                VM = ((PSObject)VM).BaseObject;
            }

            if (VM is XenAPI.VM) // case object is XenObject
            {
                vm = ((XenAPI.VM)VM).opaque_ref;
            }
            else if (VM is XenRef<XenAPI.VM>) // case object is XenRef
            {
                vm = ((XenRef<XenAPI.VM>)VM).opaque_ref;
            }
            else if (VM is string && CommonCmdletFunctions.IsOpaqueRef((string)VM)) // case object is OpaqueRef string
            {
                vm = (string)VM;
            }
            else if ((VM is string && CommonCmdletFunctions.IsUuid((string)VM)) || (VM is Guid)) // case object is uuid
             {
                 if (VM is Guid)
                     VM = ((Guid)VM).ToString();
                 XenRef<XenAPI.VM> obj_ref = XenAPI.VM.get_by_uuid(session, (string)VM);
                 if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                 {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VM with uuid {0} does not exist",(string)VM)), "", ErrorCategory.InvalidArgument, VM));
                 }
                 vm = obj_ref.opaque_ref;
             }
            else if (VM is string)
            {
                if ((string)VM == string.Empty)
                {
                    vm = "";
                }
                else
                {
                    List<XenRef<XenAPI.VM>> obj_refs = XenAPI.VM.get_by_name_label(session, (string)VM);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VM with name label {0} does not exist",(string)VM)), "", ErrorCategory.InvalidArgument, VM));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.VM with name label {0} exist",(string)VM)), "", ErrorCategory.InvalidArgument, VM));
                    }
                    vm = obj_refs[0].opaque_ref;
                }
            }
            else if (VM == null)
            {
                vm = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("VM must be of type XenAPI.VM, XenRef<XenAPI.VM>, Guid or string"), "", ErrorCategory.InvalidArgument, VM));
            }
            Record.VM = string.IsNullOrEmpty(vm) ? null : new XenRef<XenAPI.VM>(vm);
                
            string backend = null;

            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (Backend is PSObject)
            {
                Backend = ((PSObject)Backend).BaseObject;
            }

            if (Backend is XenAPI.VM) // case object is XenObject
            {
                backend = ((XenAPI.VM)Backend).opaque_ref;
            }
            else if (Backend is XenRef<XenAPI.VM>) // case object is XenRef
            {
                backend = ((XenRef<XenAPI.VM>)Backend).opaque_ref;
            }
            else if (Backend is string && CommonCmdletFunctions.IsOpaqueRef((string)Backend)) // case object is OpaqueRef string
            {
                backend = (string)Backend;
            }
            else if ((Backend is string && CommonCmdletFunctions.IsUuid((string)Backend)) || (Backend is Guid)) // case object is uuid
             {
                 if (Backend is Guid)
                     Backend = ((Guid)Backend).ToString();
                 XenRef<XenAPI.VM> obj_ref = XenAPI.VM.get_by_uuid(session, (string)Backend);
                 if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                 {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VM with uuid {0} does not exist",(string)Backend)), "", ErrorCategory.InvalidArgument, Backend));
                 }
                 backend = obj_ref.opaque_ref;
             }
            else if (Backend is string)
            {
                if ((string)Backend == string.Empty)
                {
                    backend = "";
                }
                else
                {
                    List<XenRef<XenAPI.VM>> obj_refs = XenAPI.VM.get_by_name_label(session, (string)Backend);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VM with name label {0} does not exist",(string)Backend)), "", ErrorCategory.InvalidArgument, Backend));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.VM with name label {0} exist",(string)Backend)), "", ErrorCategory.InvalidArgument, Backend));
                    }
                    backend = obj_refs[0].opaque_ref;
                }
            }
            else if (Backend == null)
            {
                backend = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Backend must be of type XenAPI.VM, XenRef<XenAPI.VM>, Guid or string"), "", ErrorCategory.InvalidArgument, Backend));
            }
            Record.backend = string.IsNullOrEmpty(backend) ? null : new XenRef<XenAPI.VM>(backend);
                
            }
            else if (Record == null)
            {
                Record = new XenAPI.VTPM(HashTable);
            }
            // check commands for null-ness
            if (Record == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Record\" must be set"), "", ErrorCategory.InvalidArgument, Record));
            }
            

            
            if (!ShouldProcess(session.Url, "VTPM.create"))
                return;

            try
            {
                if (RunAsync)
                {
                    XenRef<XenAPI.Task> task_ref = XenAPI.VTPM.async_create(session, Record);
                    XenAPI.Task taskRec = XenAPI.Task.get_record(session, task_ref.opaque_ref);
                    taskRec.opaque_ref = task_ref.opaque_ref;
                    WriteObject(taskRec, true);
                }
                else
                {
                    string obj_ref = XenAPI.VTPM.create(session, Record);
                if (obj_ref == "OpaqueRef:NULL")
                    WriteObject (null, true);
                else
                {
                    XenAPI.VTPM rec = XenAPI.VTPM.get_record(session, obj_ref);
                    rec.opaque_ref = obj_ref;
                    WriteObject(rec, true);
                }
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
