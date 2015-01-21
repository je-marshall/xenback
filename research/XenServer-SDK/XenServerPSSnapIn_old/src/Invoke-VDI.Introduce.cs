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
    [Cmdlet("Invoke", "XenServer:VDI.Introduce", SupportsShouldProcess=true)]
    public class InvokeXenServerVDI_IntroduceCommand : PSCmdlet
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
        public object SR
        {
            get { return _sR; }
            set { _sR = value; }
        }
        private object _sR;

        [Parameter(Position = 4,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public vdi_type Type
        {
            get { return _type; }
            set { _type = value; }
        }
        private vdi_type _type;

        [Parameter(Position = 5,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool Sharable
        {
            get { return _sharable; }
            set { _sharable = value; }
        }
        private bool _sharable;

        [Parameter(Position = 6,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool ReadOnly
        {
            get { return _readOnly; }
            set { _readOnly = value; }
        }
        private bool _readOnly;

        [Parameter(Position = 7,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable OtherConfig
        {
            get { return _otherConfig; }
            set { _otherConfig = value; }
        }
        private Hashtable _otherConfig;

        [Parameter(Position = 8,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string Location
        {
            get { return _location; }
            set { _location = value; }
        }
        private string _location;

        [Parameter(Position = 9,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable XenstoreData
        {
            get { return _xenstoreData; }
            set { _xenstoreData = value; }
        }
        private Hashtable _xenstoreData;

        [Parameter(Position = 10,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable SmConfig
        {
            get { return _smConfig; }
            set { _smConfig = value; }
        }
        private Hashtable _smConfig;

        [Parameter(Position = 11,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool Managed
        {
            get { return _managed; }
            set { _managed = value; }
        }
        private bool _managed;

        [Parameter(Position = 12,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long VirtualSize
        {
            get { return _virtualSize; }
            set { _virtualSize = value; }
        }
        private long _virtualSize;

        [Parameter(Position = 13,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long PhysicalUtilisation
        {
            get { return _physicalUtilisation; }
            set { _physicalUtilisation = value; }
        }
        private long _physicalUtilisation;

        [Parameter(Position = 14,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object MetadataOfPool
        {
            get { return _metadataOfPool; }
            set { _metadataOfPool = value; }
        }
        private object _metadataOfPool;

        [Parameter(Position = 15,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool IsASnapshot
        {
            get { return _isASnapshot; }
            set { _isASnapshot = value; }
        }
        private bool _isASnapshot;

        [Parameter(Position = 16,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public DateTime SnapshotTime
        {
            get { return _snapshotTime; }
            set { _snapshotTime = value; }
        }
        private DateTime _snapshotTime;

        [Parameter(Position = 17,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object SnapshotOf
        {
            get { return _snapshotOf; }
            set { _snapshotOf = value; }
        }
        private object _snapshotOf;

        
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
            

            if (SR == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"SR\" must be set"), "", ErrorCategory.InvalidArgument, SR));
            }
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

            

            

            

            if (OtherConfig == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"OtherConfig\" must be set"), "", ErrorCategory.InvalidArgument, OtherConfig));
            }
            

            if (Location == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Location\" must be set"), "", ErrorCategory.InvalidArgument, Location));
            }
            

            if (XenstoreData == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"XenstoreData\" must be set"), "", ErrorCategory.InvalidArgument, XenstoreData));
            }
            

            if (SmConfig == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"SmConfig\" must be set"), "", ErrorCategory.InvalidArgument, SmConfig));
            }
            

            

            

            

            if (MetadataOfPool == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"MetadataOfPool\" must be set"), "", ErrorCategory.InvalidArgument, MetadataOfPool));
            }
            string metadata_of_pool = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (MetadataOfPool is PSObject)
            {
                MetadataOfPool = ((PSObject)MetadataOfPool).BaseObject;
            }
  
            if (MetadataOfPool is XenAPI.Pool) // case object is XenObject
            {
                metadata_of_pool = ((XenAPI.Pool)MetadataOfPool).opaque_ref;
            }
            else if (MetadataOfPool is XenRef<XenAPI.Pool>) // case object is XenRef
            {
                metadata_of_pool = ((XenRef<XenAPI.Pool>)MetadataOfPool).opaque_ref;
            }
            else if (MetadataOfPool is string && CommonCmdletFunctions.IsOpaqueRef((string)MetadataOfPool)) // case object is OpaqueRef string
            {
                metadata_of_pool = (string)MetadataOfPool;
            }
            else if ((MetadataOfPool is string && CommonCmdletFunctions.IsUuid((string)MetadataOfPool)) || (MetadataOfPool is Guid)) // case object is uuid
            {
                if (MetadataOfPool is Guid)
                    MetadataOfPool = ((Guid)MetadataOfPool).ToString();
                XenRef<XenAPI.Pool> obj_ref = XenAPI.Pool.get_by_uuid(session, (string)MetadataOfPool);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Pool with uuid {0} does not exist",(string)MetadataOfPool)), "", ErrorCategory.InvalidArgument, MetadataOfPool));
                }
                metadata_of_pool = obj_ref.opaque_ref;
            }
            else if (MetadataOfPool is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.Pool does not support get_by_name_label and MetadataOfPool is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, MetadataOfPool));
            }
            else if (MetadataOfPool == null)
            {
                metadata_of_pool = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("MetadataOfPool must be of type XenAPI.Pool, XenRef<XenAPI.Pool>, Guid or string"), "", ErrorCategory.InvalidArgument, MetadataOfPool));
            }

            

            

            if (SnapshotOf == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"SnapshotOf\" must be set"), "", ErrorCategory.InvalidArgument, SnapshotOf));
            }
            string snapshot_of = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (SnapshotOf is PSObject)
            {
                SnapshotOf = ((PSObject)SnapshotOf).BaseObject;
            }
  
            if (SnapshotOf is XenAPI.VDI) // case object is XenObject
            {
                snapshot_of = ((XenAPI.VDI)SnapshotOf).opaque_ref;
            }
            else if (SnapshotOf is XenRef<XenAPI.VDI>) // case object is XenRef
            {
                snapshot_of = ((XenRef<XenAPI.VDI>)SnapshotOf).opaque_ref;
            }
            else if (SnapshotOf is string && CommonCmdletFunctions.IsOpaqueRef((string)SnapshotOf)) // case object is OpaqueRef string
            {
                snapshot_of = (string)SnapshotOf;
            }
            else if ((SnapshotOf is string && CommonCmdletFunctions.IsUuid((string)SnapshotOf)) || (SnapshotOf is Guid)) // case object is uuid
            {
                if (SnapshotOf is Guid)
                    SnapshotOf = ((Guid)SnapshotOf).ToString();
                XenRef<XenAPI.VDI> obj_ref = XenAPI.VDI.get_by_uuid(session, (string)SnapshotOf);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VDI with uuid {0} does not exist",(string)SnapshotOf)), "", ErrorCategory.InvalidArgument, SnapshotOf));
                }
                snapshot_of = obj_ref.opaque_ref;
            }
            else if (SnapshotOf is string)
            {
                if ((string)SnapshotOf == string.Empty)
                {
                    snapshot_of = "";
                }
                else
                {
                    List<XenRef<XenAPI.VDI>> obj_refs = XenAPI.VDI.get_by_name_label(session, (string)SnapshotOf);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VDI with name label {0} does not exist",(string)SnapshotOf)), "", ErrorCategory.InvalidArgument, SnapshotOf));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.VDI with name label {0} exist",(string)SnapshotOf)), "", ErrorCategory.InvalidArgument, SnapshotOf));
                    }
                    snapshot_of = obj_refs[0].opaque_ref;
                }
            }
            else if (SnapshotOf == null)
            {
                snapshot_of = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("SnapshotOf must be of type XenAPI.VDI, XenRef<XenAPI.VDI>, Guid or string"), "", ErrorCategory.InvalidArgument, SnapshotOf));
            }

            
            if (!ShouldProcess(Uuid, "VDI.introduce"))
                return;

            try
            {
                if (RunAsync)
                {
                    XenRef<XenAPI.Task> task_ref = XenAPI.VDI.async_introduce(session, Uuid, NameLabel, NameDescription, sr, Type, Sharable, ReadOnly, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(OtherConfig), Location, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(XenstoreData), CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(SmConfig), Managed, VirtualSize, PhysicalUtilisation, metadata_of_pool, IsASnapshot, SnapshotTime, snapshot_of);
                    XenAPI.Task taskRec = XenAPI.Task.get_record(session, task_ref.opaque_ref);
                    taskRec.opaque_ref = task_ref.opaque_ref;
                    WriteObject(taskRec, true);
                }
                else
                {
                    string obj_ref = XenAPI.VDI.introduce(session, Uuid, NameLabel, NameDescription, sr, Type, Sharable, ReadOnly, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(OtherConfig), Location, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(XenstoreData), CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(SmConfig), Managed, VirtualSize, PhysicalUtilisation, metadata_of_pool, IsASnapshot, SnapshotTime, snapshot_of);
                if (obj_ref == "OpaqueRef:NULL")
                    WriteObject (null, true);
                else
                {
                    XenAPI.VDI rec = XenAPI.VDI.get_record(session, obj_ref);
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
