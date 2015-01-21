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
    [Cmdlet("Get", "XenServer:Role.ByPermission", SupportsShouldProcess=false)]
    public class GetXenServerRole_ByPermissionCommand : PSCmdlet
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
        public object Permission
        {
            get { return _permission; }
            set { _permission = value; }
        }
        private object _permission;

        
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
            if (Permission == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Permission\" must be set"), "", ErrorCategory.InvalidArgument, Permission));
            }
            string permission = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (Permission is PSObject)
            {
                Permission = ((PSObject)Permission).BaseObject;
            }
  
            if (Permission is XenAPI.Role) // case object is XenObject
            {
                permission = ((XenAPI.Role)Permission).opaque_ref;
            }
            else if (Permission is XenRef<XenAPI.Role>) // case object is XenRef
            {
                permission = ((XenRef<XenAPI.Role>)Permission).opaque_ref;
            }
            else if (Permission is string && CommonCmdletFunctions.IsOpaqueRef((string)Permission)) // case object is OpaqueRef string
            {
                permission = (string)Permission;
            }
            else if ((Permission is string && CommonCmdletFunctions.IsUuid((string)Permission)) || (Permission is Guid)) // case object is uuid
            {
                if (Permission is Guid)
                    Permission = ((Guid)Permission).ToString();
                XenRef<XenAPI.Role> obj_ref = XenAPI.Role.get_by_uuid(session, (string)Permission);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Role with uuid {0} does not exist",(string)Permission)), "", ErrorCategory.InvalidArgument, Permission));
                }
                permission = obj_ref.opaque_ref;
            }
            else if (Permission is string)
            {
                if ((string)Permission == string.Empty)
                {
                    permission = "";
                }
                else
                {
                    List<XenRef<XenAPI.Role>> obj_refs = XenAPI.Role.get_by_name_label(session, (string)Permission);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Role with name label {0} does not exist",(string)Permission)), "", ErrorCategory.InvalidArgument, Permission));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.Role with name label {0} exist",(string)Permission)), "", ErrorCategory.InvalidArgument, Permission));
                    }
                    permission = obj_refs[0].opaque_ref;
                }
            }
            else if (Permission == null)
            {
                permission = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Permission must be of type XenAPI.Role, XenRef<XenAPI.Role>, Guid or string"), "", ErrorCategory.InvalidArgument, Permission));
            }

            
            
            try
            {
                List<XenRef<XenAPI.Role>> refs = XenAPI.Role.get_by_permission(session, permission);
                List<XenAPI.Role> records = new List<XenAPI.Role>();
                foreach (XenRef<XenAPI.Role> _ref in refs)
                {
                    if (_ref.opaque_ref == "OpaqueRef:NULL")
                        continue;
                        
                    XenAPI.Role rec = XenAPI.Role.get_record(session, _ref);
                    rec.opaque_ref = _ref.opaque_ref;
                    records.Add(rec);
                }
                WriteObject(records, true);
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
