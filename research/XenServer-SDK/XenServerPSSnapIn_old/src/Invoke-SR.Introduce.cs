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
    [Cmdlet("Invoke", "XenServer:SR.Introduce", SupportsShouldProcess=true)]
    public class InvokeXenServerSR_IntroduceCommand : PSCmdlet
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
        public string Uuid
        {
            get { return _uuid; }
            set { _uuid = value; }
        }
        private string _uuid;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string NameLabel
        {
            get { return _nameLabel; }
            set { _nameLabel = value; }
        }
        private string _nameLabel;

        [Parameter(Position = 2,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string NameDescription
        {
            get { return _nameDescription; }
            set { _nameDescription = value; }
        }
        private string _nameDescription;

        [Parameter(Position = 3,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
        private string _type;

        [Parameter(Position = 4,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }
        private string _contentType;

        [Parameter(Position = 5,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool Shared
        {
            get { return _shared; }
            set { _shared = value; }
        }
        private bool _shared;

        [Parameter(Position = 6,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable SmConfig
        {
            get { return _smConfig; }
            set { _smConfig = value; }
        }
        private Hashtable _smConfig;

        
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
            if (Uuid == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Uuid\" must be set"), "", ErrorCategory.InvalidArgument, Uuid));
            }
            

            if (NameLabel == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"NameLabel\" must be set"), "", ErrorCategory.InvalidArgument, NameLabel));
            }
            

            if (NameDescription == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"NameDescription\" must be set"), "", ErrorCategory.InvalidArgument, NameDescription));
            }
            

            if (Type == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Type\" must be set"), "", ErrorCategory.InvalidArgument, Type));
            }
            

            if (ContentType == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"ContentType\" must be set"), "", ErrorCategory.InvalidArgument, ContentType));
            }
            

            

            if (SmConfig == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"SmConfig\" must be set"), "", ErrorCategory.InvalidArgument, SmConfig));
            }
            

            
            if (!ShouldProcess(Uuid, "SR.introduce"))
                return;

            try
            {
                if (RunAsync)
                {
                    XenRef<XenAPI.Task> task_ref = XenAPI.SR.async_introduce(session, Uuid, NameLabel, NameDescription, Type, ContentType, Shared, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(SmConfig));
                    XenAPI.Task taskRec = XenAPI.Task.get_record(session, task_ref.opaque_ref);
                    taskRec.opaque_ref = task_ref.opaque_ref;
                    WriteObject(taskRec, true);
                }
                else
                {
                    string obj_ref = XenAPI.SR.introduce(session, Uuid, NameLabel, NameDescription, Type, ContentType, Shared, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(SmConfig));
                if (obj_ref == "OpaqueRef:NULL")
                    WriteObject (null, true);
                else
                {
                    XenAPI.SR rec = XenAPI.SR.get_record(session, obj_ref);
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
