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
    [Cmdlet("Set", "XenServer:VDI.SnapshotOf", SupportsShouldProcess=true)]
    public class SetXenServerVDI_SnapshotOfCommand : PSCmdlet
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
        public object Self
        {
            get { return _self; }
            set { _self = value; }
        }
        private object _self;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }
        private object _value;

        
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
  
            if (Self is XenAPI.VDI) // case object is XenObject
            {
                self = ((XenAPI.VDI)Self).opaque_ref;
            }
            else if (Self is XenRef<XenAPI.VDI>) // case object is XenRef
            {
                self = ((XenRef<XenAPI.VDI>)Self).opaque_ref;
            }
            else if (Self is string && CommonCmdletFunctions.IsOpaqueRef((string)Self)) // case object is OpaqueRef string
            {
                self = (string)Self;
            }
            else if ((Self is string && CommonCmdletFunctions.IsUuid((string)Self)) || (Self is Guid)) // case object is uuid
            {
                if (Self is Guid)
                    Self = ((Guid)Self).ToString();
                XenRef<XenAPI.VDI> obj_ref = XenAPI.VDI.get_by_uuid(session, (string)Self);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VDI with uuid {0} does not exist",(string)Self)), "", ErrorCategory.InvalidArgument, Self));
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
                    List<XenRef<XenAPI.VDI>> obj_refs = XenAPI.VDI.get_by_name_label(session, (string)Self);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VDI with name label {0} does not exist",(string)Self)), "", ErrorCategory.InvalidArgument, Self));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.VDI with name label {0} exist",(string)Self)), "", ErrorCategory.InvalidArgument, Self));
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
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Self must be of type XenAPI.VDI, XenRef<XenAPI.VDI>, Guid or string"), "", ErrorCategory.InvalidArgument, Self));
            }

            if (Value == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Value\" must be set"), "", ErrorCategory.InvalidArgument, Value));
            }
            string value = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (Value is PSObject)
            {
                Value = ((PSObject)Value).BaseObject;
            }
  
            if (Value is XenAPI.VDI) // case object is XenObject
            {
                value = ((XenAPI.VDI)Value).opaque_ref;
            }
            else if (Value is XenRef<XenAPI.VDI>) // case object is XenRef
            {
                value = ((XenRef<XenAPI.VDI>)Value).opaque_ref;
            }
            else if (Value is string && CommonCmdletFunctions.IsOpaqueRef((string)Value)) // case object is OpaqueRef string
            {
                value = (string)Value;
            }
            else if ((Value is string && CommonCmdletFunctions.IsUuid((string)Value)) || (Value is Guid)) // case object is uuid
            {
                if (Value is Guid)
                    Value = ((Guid)Value).ToString();
                XenRef<XenAPI.VDI> obj_ref = XenAPI.VDI.get_by_uuid(session, (string)Value);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VDI with uuid {0} does not exist",(string)Value)), "", ErrorCategory.InvalidArgument, Value));
                }
                value = obj_ref.opaque_ref;
            }
            else if (Value is string)
            {
                if ((string)Value == string.Empty)
                {
                    value = "";
                }
                else
                {
                    List<XenRef<XenAPI.VDI>> obj_refs = XenAPI.VDI.get_by_name_label(session, (string)Value);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VDI with name label {0} does not exist",(string)Value)), "", ErrorCategory.InvalidArgument, Value));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.VDI with name label {0} exist",(string)Value)), "", ErrorCategory.InvalidArgument, Value));
                    }
                    value = obj_refs[0].opaque_ref;
                }
            }
            else if (Value == null)
            {
                value = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Value must be of type XenAPI.VDI, XenRef<XenAPI.VDI>, Guid or string"), "", ErrorCategory.InvalidArgument, Value));
            }

            
            if (!ShouldProcess(self, "VDI.set_snapshot_of"))
                return;

            try
            {
                XenAPI.VDI.set_snapshot_of(session, self, value);
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
