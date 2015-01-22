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
    [Cmdlet("Invoke", "XenServer:Host.MigrateReceive", SupportsShouldProcess=true)]
    public class InvokeXenServerHost_MigrateReceiveCommand : PSCmdlet
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
        public new object Host
        {
            get { return _host; }
            set { _host = value; }
        }
        private object _host;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object Network
        {
            get { return _network; }
            set { _network = value; }
        }
        private object _network;

        [Parameter(Position = 2,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable Options
        {
            get { return _options; }
            set { _options = value; }
        }
        private Hashtable _options;

        
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
            if (Host == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Host\" must be set"), "", ErrorCategory.InvalidArgument, Host));
            }
            string host = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (Host is PSObject)
            {
                Host = ((PSObject)Host).BaseObject;
            }
  
            if (Host is XenAPI.Host) // case object is XenObject
            {
                host = ((XenAPI.Host)Host).opaque_ref;
            }
            else if (Host is XenRef<XenAPI.Host>) // case object is XenRef
            {
                host = ((XenRef<XenAPI.Host>)Host).opaque_ref;
            }
            else if (Host is string && CommonCmdletFunctions.IsOpaqueRef((string)Host)) // case object is OpaqueRef string
            {
                host = (string)Host;
            }
            else if ((Host is string && CommonCmdletFunctions.IsUuid((string)Host)) || (Host is Guid)) // case object is uuid
            {
                if (Host is Guid)
                    Host = ((Guid)Host).ToString();
                XenRef<XenAPI.Host> obj_ref = XenAPI.Host.get_by_uuid(session, (string)Host);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Host with uuid {0} does not exist",(string)Host)), "", ErrorCategory.InvalidArgument, Host));
                }
                host = obj_ref.opaque_ref;
            }
            else if (Host is string)
            {
                if ((string)Host == string.Empty)
                {
                    host = "";
                }
                else
                {
                    List<XenRef<XenAPI.Host>> obj_refs = XenAPI.Host.get_by_name_label(session, (string)Host);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Host with name label {0} does not exist",(string)Host)), "", ErrorCategory.InvalidArgument, Host));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.Host with name label {0} exist",(string)Host)), "", ErrorCategory.InvalidArgument, Host));
                    }
                    host = obj_refs[0].opaque_ref;
                }
            }
            else if (Host == null)
            {
                host = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Host must be of type XenAPI.Host, XenRef<XenAPI.Host>, Guid or string"), "", ErrorCategory.InvalidArgument, Host));
            }

            if (Network == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Network\" must be set"), "", ErrorCategory.InvalidArgument, Network));
            }
            string network = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (Network is PSObject)
            {
                Network = ((PSObject)Network).BaseObject;
            }
  
            if (Network is XenAPI.Network) // case object is XenObject
            {
                network = ((XenAPI.Network)Network).opaque_ref;
            }
            else if (Network is XenRef<XenAPI.Network>) // case object is XenRef
            {
                network = ((XenRef<XenAPI.Network>)Network).opaque_ref;
            }
            else if (Network is string && CommonCmdletFunctions.IsOpaqueRef((string)Network)) // case object is OpaqueRef string
            {
                network = (string)Network;
            }
            else if ((Network is string && CommonCmdletFunctions.IsUuid((string)Network)) || (Network is Guid)) // case object is uuid
            {
                if (Network is Guid)
                    Network = ((Guid)Network).ToString();
                XenRef<XenAPI.Network> obj_ref = XenAPI.Network.get_by_uuid(session, (string)Network);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Network with uuid {0} does not exist",(string)Network)), "", ErrorCategory.InvalidArgument, Network));
                }
                network = obj_ref.opaque_ref;
            }
            else if (Network is string)
            {
                if ((string)Network == string.Empty)
                {
                    network = "";
                }
                else
                {
                    List<XenRef<XenAPI.Network>> obj_refs = XenAPI.Network.get_by_name_label(session, (string)Network);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Network with name label {0} does not exist",(string)Network)), "", ErrorCategory.InvalidArgument, Network));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.Network with name label {0} exist",(string)Network)), "", ErrorCategory.InvalidArgument, Network));
                    }
                    network = obj_refs[0].opaque_ref;
                }
            }
            else if (Network == null)
            {
                network = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Network must be of type XenAPI.Network, XenRef<XenAPI.Network>, Guid or string"), "", ErrorCategory.InvalidArgument, Network));
            }

            if (Options == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Options\" must be set"), "", ErrorCategory.InvalidArgument, Options));
            }
            

            
            if (!ShouldProcess(host, "Host.migrate_receive"))
                return;

            try
            {
                if (RunAsync)
                {
                    XenRef<XenAPI.Task> task_ref = XenAPI.Host.async_migrate_receive(session, host, network, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(Options));
                    XenAPI.Task taskRec = XenAPI.Task.get_record(session, task_ref.opaque_ref);
                    taskRec.opaque_ref = task_ref.opaque_ref;
                    WriteObject(taskRec, true);
                }
                else
                {
                    WriteObject(CommonCmdletFunctions.ConvertDictionaryToHashtable(XenAPI.Host.migrate_receive(session, host, network, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(Options))), true);
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