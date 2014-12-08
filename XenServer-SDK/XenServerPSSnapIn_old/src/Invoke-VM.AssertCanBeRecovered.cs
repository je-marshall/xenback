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
    [Cmdlet("Invoke", "XenServer:VM.AssertCanBeRecovered", SupportsShouldProcess=true)]
    public class InvokeXenServerVM_AssertCanBeRecoveredCommand : PSCmdlet
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
        public object Self
        {
            get { return _self; }
            set { _self = value; }
        }
        private object _self;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object SessionTo
        {
            get { return _sessionTo; }
            set { _sessionTo = value; }
        }
        private object _sessionTo;

        
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
            if (Self == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Self\" must be set"), "", ErrorCategory.InvalidArgument, Self));
            }
            string self = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (Self is PSObject)
            {
                Self = ((PSObject)Self).BaseObject;
            }
  
            if (Self is XenAPI.VM) // case object is XenObject
            {
                self = ((XenAPI.VM)Self).opaque_ref;
            }
            else if (Self is XenRef<XenAPI.VM>) // case object is XenRef
            {
                self = ((XenRef<XenAPI.VM>)Self).opaque_ref;
            }
            else if (Self is string && CommonCmdletFunctions.IsOpaqueRef((string)Self)) // case object is OpaqueRef string
            {
                self = (string)Self;
            }
            else if ((Self is string && CommonCmdletFunctions.IsUuid((string)Self)) || (Self is Guid)) // case object is uuid
            {
                if (Self is Guid)
                    Self = ((Guid)Self).ToString();
                XenRef<XenAPI.VM> obj_ref = XenAPI.VM.get_by_uuid(session, (string)Self);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VM with uuid {0} does not exist",(string)Self)), "", ErrorCategory.InvalidArgument, Self));
                }
                self = obj_ref.opaque_ref;
            }
            else if (Self is string)
            {
                if ((string)Self == string.Empty)
                {
                    self = "";
                }
                else
                {
                    List<XenRef<XenAPI.VM>> obj_refs = XenAPI.VM.get_by_name_label(session, (string)Self);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VM with name label {0} does not exist",(string)Self)), "", ErrorCategory.InvalidArgument, Self));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.VM with name label {0} exist",(string)Self)), "", ErrorCategory.InvalidArgument, Self));
                    }
                    self = obj_refs[0].opaque_ref;
                }
            }
            else if (Self == null)
            {
                self = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Self must be of type XenAPI.VM, XenRef<XenAPI.VM>, Guid or string"), "", ErrorCategory.InvalidArgument, Self));
            }

            if (SessionTo == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"SessionTo\" must be set"), "", ErrorCategory.InvalidArgument, SessionTo));
            }
            string session_to = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (SessionTo is PSObject)
            {
                SessionTo = ((PSObject)SessionTo).BaseObject;
            }
  
            if (SessionTo is XenAPI.Session) // case object is XenObject
            {
                session_to = ((XenAPI.Session)SessionTo).opaque_ref;
            }
            else if (SessionTo is XenRef<XenAPI.Session>) // case object is XenRef
            {
                session_to = ((XenRef<XenAPI.Session>)SessionTo).opaque_ref;
            }
            else if (SessionTo is string && CommonCmdletFunctions.IsOpaqueRef((string)SessionTo)) // case object is OpaqueRef string
            {
                session_to = (string)SessionTo;
            }
            
            else if (SessionTo is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.Session does not support get_by_name_label and SessionTo is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, SessionTo));
            }
            else if (SessionTo == null)
            {
                session_to = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("SessionTo must be of type XenAPI.Session, XenRef<XenAPI.Session>, Guid or string"), "", ErrorCategory.InvalidArgument, SessionTo));
            }

            
            if (!ShouldProcess(self, "VM.assert_can_be_recovered"))
                return;

            try
            {
                if (RunAsync)
                {
                    XenRef<XenAPI.Task> task_ref = XenAPI.VM.async_assert_can_be_recovered(session, self, session_to);
                    XenAPI.Task taskRec = XenAPI.Task.get_record(session, task_ref.opaque_ref);
                    taskRec.opaque_ref = task_ref.opaque_ref;
                    WriteObject(taskRec, true);
                }
                else
                {
                    XenAPI.VM.assert_can_be_recovered(session, self, session_to);
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
