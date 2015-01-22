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
    [Cmdlet("Create", "XenServer:PBD", SupportsShouldProcess=true)]
    public class CreateXenServerPBD_CreateCommand : PSCmdlet
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
        public new object Host
        {
            get { return _host; }
            set { _host = value; }
        }
        private object _host;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object SR
        {
            get { return _sR; }
            set { _sR = value; }
        }
        private object _sR;

        [Parameter(Position = 2,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable DeviceConfig
        {
            get { return _deviceConfig; }
            set { _deviceConfig = value; }
        }
        private Hashtable _deviceConfig = new Hashtable();

        [Parameter(Position = 3,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable OtherConfig
        {
            get { return _otherConfig; }
            set { _otherConfig = value; }
        }
        private Hashtable _otherConfig = new Hashtable();

        
        [Parameter]
        public XenAPI.PBD Record
        {
            get { return _record; }
            set { _record = value; }
        }
        private XenAPI.PBD _record;

        
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
                Record = new XenAPI.PBD();
                
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
            Record.host = string.IsNullOrEmpty(host) ? null : new XenRef<XenAPI.Host>(host);
                
            string sr = null;

            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (SR is PSObject)
            {
                SR = ((PSObject)SR).BaseObject;
            }

            if (SR is XenAPI.SR) // case object is XenObject
            {
                sr = ((XenAPI.SR)SR).opaque_ref;
            }
            else if (SR is XenRef<XenAPI.SR>) // case object is XenRef
            {
                sr = ((XenRef<XenAPI.SR>)SR).opaque_ref;
            }
            else if (SR is string && CommonCmdletFunctions.IsOpaqueRef((string)SR)) // case object is OpaqueRef string
            {
                sr = (string)SR;
            }
            else if ((SR is string && CommonCmdletFunctions.IsUuid((string)SR)) || (SR is Guid)) // case object is uuid
             {
                 if (SR is Guid)
                     SR = ((Guid)SR).ToString();
                 XenRef<XenAPI.SR> obj_ref = XenAPI.SR.get_by_uuid(session, (string)SR);
                 if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                 {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.SR with uuid {0} does not exist",(string)SR)), "", ErrorCategory.InvalidArgument, SR));
                 }
                 sr = obj_ref.opaque_ref;
             }
            else if (SR is string)
            {
                if ((string)SR == string.Empty)
                {
                    sr = "";
                }
                else
                {
                    List<XenRef<XenAPI.SR>> obj_refs = XenAPI.SR.get_by_name_label(session, (string)SR);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.SR with name label {0} does not exist",(string)SR)), "", ErrorCategory.InvalidArgument, SR));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.SR with name label {0} exist",(string)SR)), "", ErrorCategory.InvalidArgument, SR));
                    }
                    sr = obj_refs[0].opaque_ref;
                }
            }
            else if (SR == null)
            {
                sr = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("SR must be of type XenAPI.SR, XenRef<XenAPI.SR>, Guid or string"), "", ErrorCategory.InvalidArgument, SR));
            }
            Record.SR = string.IsNullOrEmpty(sr) ? null : new XenRef<XenAPI.SR>(sr);
                Record.device_config = CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(DeviceConfig);
                Record.other_config = CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(OtherConfig);
                
            }
            else if (Record == null)
            {
                Record = new XenAPI.PBD(HashTable);
            }
            // check commands for null-ness
            if (Record == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Record\" must be set"), "", ErrorCategory.InvalidArgument, Record));
            }
            

            
            if (!ShouldProcess(session.Url, "PBD.create"))
                return;

            try
            {
                if (RunAsync)
                {
                    XenRef<XenAPI.Task> task_ref = XenAPI.PBD.async_create(session, Record);
                    XenAPI.Task taskRec = XenAPI.Task.get_record(session, task_ref.opaque_ref);
                    taskRec.opaque_ref = task_ref.opaque_ref;
                    WriteObject(taskRec, true);
                }
                else
                {
                    string obj_ref = XenAPI.PBD.create(session, Record);
                if (obj_ref == "OpaqueRef:NULL")
                    WriteObject (null, true);
                else
                {
                    XenAPI.PBD rec = XenAPI.PBD.get_record(session, obj_ref);
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