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
    [Cmdlet("Invoke", "XenServer:Pool.DetectNonhomogeneousExternalAuth", SupportsShouldProcess=true)]
    public class InvokeXenServerPool_DetectNonhomogeneousExternalAuthCommand : PSCmdlet
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
        public object Pool
        {
            get { return _pool; }
            set { _pool = value; }
        }
        private object _pool;

        
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
            if (Pool == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Pool\" must be set"), "", ErrorCategory.InvalidArgument, Pool));
            }
            string pool = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (Pool is PSObject)
            {
                Pool = ((PSObject)Pool).BaseObject;
            }
  
            if (Pool is XenAPI.Pool) // case object is XenObject
            {
                pool = ((XenAPI.Pool)Pool).opaque_ref;
            }
            else if (Pool is XenRef<XenAPI.Pool>) // case object is XenRef
            {
                pool = ((XenRef<XenAPI.Pool>)Pool).opaque_ref;
            }
            else if (Pool is string && CommonCmdletFunctions.IsOpaqueRef((string)Pool)) // case object is OpaqueRef string
            {
                pool = (string)Pool;
            }
            else if ((Pool is string && CommonCmdletFunctions.IsUuid((string)Pool)) || (Pool is Guid)) // case object is uuid
            {
                if (Pool is Guid)
                    Pool = ((Guid)Pool).ToString();
                XenRef<XenAPI.Pool> obj_ref = XenAPI.Pool.get_by_uuid(session, (string)Pool);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Pool with uuid {0} does not exist",(string)Pool)), "", ErrorCategory.InvalidArgument, Pool));
                }
                pool = obj_ref.opaque_ref;
            }
            else if (Pool is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.Pool does not support get_by_name_label and Pool is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, Pool));
            }
            else if (Pool == null)
            {
                pool = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Pool must be of type XenAPI.Pool, XenRef<XenAPI.Pool>, Guid or string"), "", ErrorCategory.InvalidArgument, Pool));
            }

            
            if (!ShouldProcess(session.Url, "Pool.detect_nonhomogeneous_external_auth"))
                return;

            try
            {
                XenAPI.Pool.detect_nonhomogeneous_external_auth(session, pool);
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
