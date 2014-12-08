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
    [Cmdlet("Create", "XenServer:Message", SupportsShouldProcess=true)]
    public class CreateXenServerMessage_CreateCommand : PSCmdlet
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
        public Hashtable HashTable
        {
            get { return hashtable; }
            set { hashtable = value; }
        }
        private Hashtable hashtable;

        [Parameter]
        public XenAPI.Message Record
        {
            get { return record; }
            set { record = value; }
        }
        private XenAPI.Message record;

        [Parameter(Position = 0,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _name;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }
        private long _priority;

        [Parameter(Position = 2,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public cls Cls
        {
            get { return _cls; }
            set { _cls = value; }
        }
        private cls _cls;

        [Parameter(Position = 3,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string ObjUuid
        {
            get { return _objUuid; }
            set { _objUuid = value; }
        }
        private string _objUuid;

        [Parameter(Position = 4,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }
        private string _body;

        
        #endregion
    
        #region Cmdlet Methods

        protected override void ProcessRecord()
        {
            XenServerSessions sessions;
            Session session = CommonCmdletFunctions.GetXenSession(this,
                                                    out sessions,
                                                    ref url,
                                                    Server, Port);
            if (Record != null)
            {
                Name = Record.name;
                Priority = Record.priority;
                Cls = Record.cls;
                ObjUuid = Record.obj_uuid;
                Body = Record.body;
                
            }
            else if (HashTable != null)
            {
                Name = Record.name;
                Priority = Record.priority;
                Cls = Record.cls;
                ObjUuid = Record.obj_uuid;
                Body = Record.body;
                
            }
            // check commands for null-ness
            if (Name == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Name\" must be set"), "", ErrorCategory.InvalidArgument, Name));
            }
            

            

            

            if (ObjUuid == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"ObjUuid\" must be set"), "", ErrorCategory.InvalidArgument, ObjUuid));
            }
            

            if (Body == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Body\" must be set"), "", ErrorCategory.InvalidArgument, Body));
            }
            

            
            if (!ShouldProcess(session.Url, "Message.create"))
                return;

            try
            {
                string obj_ref = XenAPI.Message.create(session, Name, Priority, Cls, ObjUuid, Body);
                if (obj_ref == "OpaqueRef:NULL")
                    WriteObject (null, true);
                else
                {
                    XenAPI.Message rec = XenAPI.Message.get_record(session, obj_ref);
                    rec.opaque_ref = obj_ref;
                    WriteObject(rec, true);
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
