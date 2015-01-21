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
    [Cmdlet("Set", "XenServer:PCI.OtherConfig", SupportsShouldProcess=true)]
    public class SetXenServerPCI_OtherConfigCommand : PSCmdlet
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
        public object PCI
        {
            get { return _pCI; }
            set { _pCI = value; }
        }
        private object _pCI;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable OtherConfig
        {
            get { return _otherConfig; }
            set { _otherConfig = value; }
        }
        private Hashtable _otherConfig;

        
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
            if (PCI == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"PCI\" must be set"), "", ErrorCategory.InvalidArgument, PCI));
            }
            string pci = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (PCI is PSObject)
            {
                PCI = ((PSObject)PCI).BaseObject;
            }
  
            if (PCI is XenAPI.PCI) // case object is XenObject
            {
                pci = ((XenAPI.PCI)PCI).opaque_ref;
            }
            else if (PCI is XenRef<XenAPI.PCI>) // case object is XenRef
            {
                pci = ((XenRef<XenAPI.PCI>)PCI).opaque_ref;
            }
            else if (PCI is string && CommonCmdletFunctions.IsOpaqueRef((string)PCI)) // case object is OpaqueRef string
            {
                pci = (string)PCI;
            }
            else if ((PCI is string && CommonCmdletFunctions.IsUuid((string)PCI)) || (PCI is Guid)) // case object is uuid
            {
                if (PCI is Guid)
                    PCI = ((Guid)PCI).ToString();
                XenRef<XenAPI.PCI> obj_ref = XenAPI.PCI.get_by_uuid(session, (string)PCI);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.PCI with uuid {0} does not exist",(string)PCI)), "", ErrorCategory.InvalidArgument, PCI));
                }
                pci = obj_ref.opaque_ref;
            }
            else if (PCI is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.PCI does not support get_by_name_label and PCI is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, PCI));
            }
            else if (PCI == null)
            {
                pci = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("PCI must be of type XenAPI.PCI, XenRef<XenAPI.PCI>, Guid or string"), "", ErrorCategory.InvalidArgument, PCI));
            }

            if (OtherConfig == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"OtherConfig\" must be set"), "", ErrorCategory.InvalidArgument, OtherConfig));
            }
            

            
            if (!ShouldProcess(pci, "PCI.set_other_config"))
                return;

            try
            {
                XenAPI.PCI.set_other_config(session, pci, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(OtherConfig));
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
