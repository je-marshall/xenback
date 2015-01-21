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
    [Cmdlet("Get", "XenServer:SM.Uuid", SupportsShouldProcess=false)]
    public class GetXenServerSM_UuidCommand : PSCmdlet
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
        public object SM
        {
            get { return _sM; }
            set { _sM = value; }
        }
        private object _sM;

        
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
            if (SM == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"SM\" must be set"), "", ErrorCategory.InvalidArgument, SM));
            }
            string sm = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (SM is PSObject)
            {
                SM = ((PSObject)SM).BaseObject;
            }
  
            if (SM is XenAPI.SM) // case object is XenObject
            {
                sm = ((XenAPI.SM)SM).opaque_ref;
            }
            else if (SM is XenRef<XenAPI.SM>) // case object is XenRef
            {
                sm = ((XenRef<XenAPI.SM>)SM).opaque_ref;
            }
            else if (SM is string && CommonCmdletFunctions.IsOpaqueRef((string)SM)) // case object is OpaqueRef string
            {
                sm = (string)SM;
            }
            else if ((SM is string && CommonCmdletFunctions.IsUuid((string)SM)) || (SM is Guid)) // case object is uuid
            {
                if (SM is Guid)
                    SM = ((Guid)SM).ToString();
                XenRef<XenAPI.SM> obj_ref = XenAPI.SM.get_by_uuid(session, (string)SM);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.SM with uuid {0} does not exist",(string)SM)), "", ErrorCategory.InvalidArgument, SM));
                }
                sm = obj_ref.opaque_ref;
            }
            else if (SM is string)
            {
                if ((string)SM == string.Empty)
                {
                    sm = "";
                }
                else
                {
                    List<XenRef<XenAPI.SM>> obj_refs = XenAPI.SM.get_by_name_label(session, (string)SM);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.SM with name label {0} does not exist",(string)SM)), "", ErrorCategory.InvalidArgument, SM));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.SM with name label {0} exist",(string)SM)), "", ErrorCategory.InvalidArgument, SM));
                    }
                    sm = obj_refs[0].opaque_ref;
                }
            }
            else if (SM == null)
            {
                sm = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("SM must be of type XenAPI.SM, XenRef<XenAPI.SM>, Guid or string"), "", ErrorCategory.InvalidArgument, SM));
            }

            
            
            try
            {
                WriteObject(XenAPI.SM.get_uuid(session, sm), true);
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
