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
    [Cmdlet("Get", "XenServer:PIF.TunnelAccessPIFOf", SupportsShouldProcess=false)]
    public class GetXenServerPIF_TunnelAccessPIFOfCommand : PSCmdlet
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
        public object PIF
        {
            get { return _pIF; }
            set { _pIF = value; }
        }
        private object _pIF;

        
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
            if (PIF == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"PIF\" must be set"), "", ErrorCategory.InvalidArgument, PIF));
            }
            string pif = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (PIF is PSObject)
            {
                PIF = ((PSObject)PIF).BaseObject;
            }
  
            if (PIF is XenAPI.PIF) // case object is XenObject
            {
                pif = ((XenAPI.PIF)PIF).opaque_ref;
            }
            else if (PIF is XenRef<XenAPI.PIF>) // case object is XenRef
            {
                pif = ((XenRef<XenAPI.PIF>)PIF).opaque_ref;
            }
            else if (PIF is string && CommonCmdletFunctions.IsOpaqueRef((string)PIF)) // case object is OpaqueRef string
            {
                pif = (string)PIF;
            }
            else if ((PIF is string && CommonCmdletFunctions.IsUuid((string)PIF)) || (PIF is Guid)) // case object is uuid
            {
                if (PIF is Guid)
                    PIF = ((Guid)PIF).ToString();
                XenRef<XenAPI.PIF> obj_ref = XenAPI.PIF.get_by_uuid(session, (string)PIF);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.PIF with uuid {0} does not exist",(string)PIF)), "", ErrorCategory.InvalidArgument, PIF));
                }
                pif = obj_ref.opaque_ref;
            }
            else if (PIF is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.PIF does not support get_by_name_label and PIF is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, PIF));
            }
            else if (PIF == null)
            {
                pif = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("PIF must be of type XenAPI.PIF, XenRef<XenAPI.PIF>, Guid or string"), "", ErrorCategory.InvalidArgument, PIF));
            }

            
            
            try
            {
                List<XenRef<XenAPI.Tunnel>> refs = XenAPI.PIF.get_tunnel_access_PIF_of(session, pif);
                List<XenAPI.Tunnel> records = new List<XenAPI.Tunnel>();
                foreach (XenRef<XenAPI.Tunnel> _ref in refs)
                {
                    if (_ref.opaque_ref == "OpaqueRef:NULL")
                        continue;
                        
                    XenAPI.Tunnel rec = XenAPI.Tunnel.get_record(session, _ref);
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
