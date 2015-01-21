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
    [Cmdlet("Create", "XenServer:VMPP", SupportsShouldProcess=true)]
    public class CreateXenServerVMPP_CreateCommand : PSCmdlet
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
        public string NameLabel
        {
            get { return _nameLabel; }
            set { _nameLabel = value; }
        }
        private string _nameLabel;

        [Parameter(Position = 1,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string NameDescription
        {
            get { return _nameDescription; }
            set { _nameDescription = value; }
        }
        private string _nameDescription;

        [Parameter(Position = 2,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool IsPolicyEnabled
        {
            get { return _isPolicyEnabled; }
            set { _isPolicyEnabled = value; }
        }
        private bool _isPolicyEnabled;

        [Parameter(Position = 3,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public vmpp_backup_type BackupType
        {
            get { return _backupType; }
            set { _backupType = value; }
        }
        private vmpp_backup_type _backupType;

        [Parameter(Position = 4,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long BackupRetentionValue
        {
            get { return _backupRetentionValue; }
            set { _backupRetentionValue = value; }
        }
        private long _backupRetentionValue;

        [Parameter(Position = 5,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public vmpp_backup_frequency BackupFrequency
        {
            get { return _backupFrequency; }
            set { _backupFrequency = value; }
        }
        private vmpp_backup_frequency _backupFrequency;

        [Parameter(Position = 6,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable BackupSchedule
        {
            get { return _backupSchedule; }
            set { _backupSchedule = value; }
        }
        private Hashtable _backupSchedule = new Hashtable();

        [Parameter(Position = 7,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public vmpp_archive_target_type ArchiveTargetType
        {
            get { return _archiveTargetType; }
            set { _archiveTargetType = value; }
        }
        private vmpp_archive_target_type _archiveTargetType;

        [Parameter(Position = 8,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable ArchiveTargetConfig
        {
            get { return _archiveTargetConfig; }
            set { _archiveTargetConfig = value; }
        }
        private Hashtable _archiveTargetConfig = new Hashtable();

        [Parameter(Position = 9,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public vmpp_archive_frequency ArchiveFrequency
        {
            get { return _archiveFrequency; }
            set { _archiveFrequency = value; }
        }
        private vmpp_archive_frequency _archiveFrequency;

        [Parameter(Position = 10,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable ArchiveSchedule
        {
            get { return _archiveSchedule; }
            set { _archiveSchedule = value; }
        }
        private Hashtable _archiveSchedule = new Hashtable();

        [Parameter(Position = 11,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool IsAlarmEnabled
        {
            get { return _isAlarmEnabled; }
            set { _isAlarmEnabled = value; }
        }
        private bool _isAlarmEnabled;

        [Parameter(Position = 12,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable AlarmConfig
        {
            get { return _alarmConfig; }
            set { _alarmConfig = value; }
        }
        private Hashtable _alarmConfig = new Hashtable();

        
        [Parameter]
        public XenAPI.VMPP Record
        {
            get { return _record; }
            set { _record = value; }
        }
        private XenAPI.VMPP _record;

        
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
                Record = new XenAPI.VMPP();
                Record.name_label = NameLabel;
                Record.name_description = NameDescription;
                Record.is_policy_enabled = IsPolicyEnabled;
                Record.backup_type = BackupType;
                Record.backup_retention_value = BackupRetentionValue;
                Record.backup_frequency = BackupFrequency;
                Record.backup_schedule = CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(BackupSchedule);
                Record.archive_target_type = ArchiveTargetType;
                Record.archive_target_config = CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(ArchiveTargetConfig);
                Record.archive_frequency = ArchiveFrequency;
                Record.archive_schedule = CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(ArchiveSchedule);
                Record.is_alarm_enabled = IsAlarmEnabled;
                Record.alarm_config = CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(AlarmConfig);
                
            }
            else if (Record == null)
            {
                Record = new XenAPI.VMPP(HashTable);
            }
            // check commands for null-ness
            if (Record == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Record\" must be set"), "", ErrorCategory.InvalidArgument, Record));
            }
            

            
            if (!ShouldProcess(session.Url, "VMPP.create"))
                return;

            try
            {
                if (RunAsync)
                {
                    XenRef<XenAPI.Task> task_ref = XenAPI.VMPP.async_create(session, Record);
                    XenAPI.Task taskRec = XenAPI.Task.get_record(session, task_ref.opaque_ref);
                    taskRec.opaque_ref = task_ref.opaque_ref;
                    WriteObject(taskRec, true);
                }
                else
                {
                    string obj_ref = XenAPI.VMPP.create(session, Record);
                if (obj_ref == "OpaqueRef:NULL")
                    WriteObject (null, true);
                else
                {
                    XenAPI.VMPP rec = XenAPI.VMPP.get_record(session, obj_ref);
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
