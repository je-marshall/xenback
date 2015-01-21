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
    [Cmdlet("Create", "XenServer:VM", SupportsShouldProcess=true)]
    public class CreateXenServerVM_CreateCommand : PSCmdlet
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
        public long UserVersion
        {
            get { return _userVersion; }
            set { _userVersion = value; }
        }
        private long _userVersion;

        [Parameter(Position = 3,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool IsATemplate
        {
            get { return _isATemplate; }
            set { _isATemplate = value; }
        }
        private bool _isATemplate;

        [Parameter(Position = 4,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object Affinity
        {
            get { return _affinity; }
            set { _affinity = value; }
        }
        private object _affinity;

        [Parameter(Position = 5,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long MemoryTarget
        {
            get { return _memoryTarget; }
            set { _memoryTarget = value; }
        }
        private long _memoryTarget;

        [Parameter(Position = 6,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long MemoryStaticMax
        {
            get { return _memoryStaticMax; }
            set { _memoryStaticMax = value; }
        }
        private long _memoryStaticMax;

        [Parameter(Position = 7,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long MemoryDynamicMax
        {
            get { return _memoryDynamicMax; }
            set { _memoryDynamicMax = value; }
        }
        private long _memoryDynamicMax;

        [Parameter(Position = 8,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long MemoryDynamicMin
        {
            get { return _memoryDynamicMin; }
            set { _memoryDynamicMin = value; }
        }
        private long _memoryDynamicMin;

        [Parameter(Position = 9,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long MemoryStaticMin
        {
            get { return _memoryStaticMin; }
            set { _memoryStaticMin = value; }
        }
        private long _memoryStaticMin;

        [Parameter(Position = 10,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable VCPUsParams
        {
            get { return _vCPUsParams; }
            set { _vCPUsParams = value; }
        }
        private Hashtable _vCPUsParams = new Hashtable();

        [Parameter(Position = 11,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long VCPUsMax
        {
            get { return _vCPUsMax; }
            set { _vCPUsMax = value; }
        }
        private long _vCPUsMax;

        [Parameter(Position = 12,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long VCPUsAtStartup
        {
            get { return _vCPUsAtStartup; }
            set { _vCPUsAtStartup = value; }
        }
        private long _vCPUsAtStartup;

        [Parameter(Position = 13,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public on_normal_exit ActionsAfterShutdown
        {
            get { return _actionsAfterShutdown; }
            set { _actionsAfterShutdown = value; }
        }
        private on_normal_exit _actionsAfterShutdown;

        [Parameter(Position = 14,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public on_normal_exit ActionsAfterReboot
        {
            get { return _actionsAfterReboot; }
            set { _actionsAfterReboot = value; }
        }
        private on_normal_exit _actionsAfterReboot;

        [Parameter(Position = 15,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public on_crash_behaviour ActionsAfterCrash
        {
            get { return _actionsAfterCrash; }
            set { _actionsAfterCrash = value; }
        }
        private on_crash_behaviour _actionsAfterCrash;

        [Parameter(Position = 16,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string PVBootloader
        {
            get { return _pVBootloader; }
            set { _pVBootloader = value; }
        }
        private string _pVBootloader;

        [Parameter(Position = 17,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string PVKernel
        {
            get { return _pVKernel; }
            set { _pVKernel = value; }
        }
        private string _pVKernel;

        [Parameter(Position = 18,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string PVRamdisk
        {
            get { return _pVRamdisk; }
            set { _pVRamdisk = value; }
        }
        private string _pVRamdisk;

        [Parameter(Position = 19,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string PVArgs
        {
            get { return _pVArgs; }
            set { _pVArgs = value; }
        }
        private string _pVArgs;

        [Parameter(Position = 20,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string PVBootloaderArgs
        {
            get { return _pVBootloaderArgs; }
            set { _pVBootloaderArgs = value; }
        }
        private string _pVBootloaderArgs;

        [Parameter(Position = 21,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string PVLegacyArgs
        {
            get { return _pVLegacyArgs; }
            set { _pVLegacyArgs = value; }
        }
        private string _pVLegacyArgs;

        [Parameter(Position = 22,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string HVMBootPolicy
        {
            get { return _hVMBootPolicy; }
            set { _hVMBootPolicy = value; }
        }
        private string _hVMBootPolicy;

        [Parameter(Position = 23,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable HVMBootParams
        {
            get { return _hVMBootParams; }
            set { _hVMBootParams = value; }
        }
        private Hashtable _hVMBootParams = new Hashtable();

        [Parameter(Position = 24,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public double HVMShadowMultiplier
        {
            get { return _hVMShadowMultiplier; }
            set { _hVMShadowMultiplier = value; }
        }
        private double _hVMShadowMultiplier;

        [Parameter(Position = 25,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable Platform
        {
            get { return _platform; }
            set { _platform = value; }
        }
        private Hashtable _platform = new Hashtable();

        [Parameter(Position = 26,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string PCIBus
        {
            get { return _pCIBus; }
            set { _pCIBus = value; }
        }
        private string _pCIBus;

        [Parameter(Position = 27,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable OtherConfig
        {
            get { return _otherConfig; }
            set { _otherConfig = value; }
        }
        private Hashtable _otherConfig = new Hashtable();

        [Parameter(Position = 28,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string Recommendations
        {
            get { return _recommendations; }
            set { _recommendations = value; }
        }
        private string _recommendations;

        [Parameter(Position = 29,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable XenstoreData
        {
            get { return _xenstoreData; }
            set { _xenstoreData = value; }
        }
        private Hashtable _xenstoreData = new Hashtable();

        [Parameter(Position = 30,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool HaAlwaysRun
        {
            get { return _haAlwaysRun; }
            set { _haAlwaysRun = value; }
        }
        private bool _haAlwaysRun;

        [Parameter(Position = 31,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string HaRestartPriority
        {
            get { return _haRestartPriority; }
            set { _haRestartPriority = value; }
        }
        private string _haRestartPriority;

        [Parameter(Position = 32,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string[] Tags
        {
            get { return _tags; }
            set { _tags = value; }
        }
        private string[] _tags = new string[0];

        [Parameter(Position = 33,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable BlockedOperations
        {
            get { return _blockedOperations; }
            set { _blockedOperations = value; }
        }
        private Hashtable _blockedOperations = new Hashtable();

        [Parameter(Position = 34,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object ProtectionPolicy
        {
            get { return _protectionPolicy; }
            set { _protectionPolicy = value; }
        }
        private object _protectionPolicy;

        [Parameter(Position = 35,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool IsSnapshotFromVmpp
        {
            get { return _isSnapshotFromVmpp; }
            set { _isSnapshotFromVmpp = value; }
        }
        private bool _isSnapshotFromVmpp;

        [Parameter(Position = 36,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object Appliance
        {
            get { return _appliance; }
            set { _appliance = value; }
        }
        private object _appliance;

        [Parameter(Position = 37,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long StartDelay
        {
            get { return _startDelay; }
            set { _startDelay = value; }
        }
        private long _startDelay;

        [Parameter(Position = 38,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long ShutdownDelay
        {
            get { return _shutdownDelay; }
            set { _shutdownDelay = value; }
        }
        private long _shutdownDelay;

        [Parameter(Position = 39,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long Order
        {
            get { return _order; }
            set { _order = value; }
        }
        private long _order;

        [Parameter(Position = 40,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object SuspendSR
        {
            get { return _suspendSR; }
            set { _suspendSR = value; }
        }
        private object _suspendSR;

        [Parameter(Position = 41,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long Version
        {
            get { return _version; }
            set { _version = value; }
        }
        private long _version;

        [Parameter(Position = 42,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string GenerationId
        {
            get { return _generationId; }
            set { _generationId = value; }
        }
        private string _generationId;

        
        [Parameter]
        public XenAPI.VM Record
        {
            get { return _record; }
            set { _record = value; }
        }
        private XenAPI.VM _record;

        
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
                Record = new XenAPI.VM();
                Record.name_label = NameLabel;
                Record.name_description = NameDescription;
                Record.user_version = UserVersion;
                Record.is_a_template = IsATemplate;
                
            string affinity = null;

            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (Affinity is PSObject)
            {
                Affinity = ((PSObject)Affinity).BaseObject;
            }

            if (Affinity is XenAPI.Host) // case object is XenObject
            {
                affinity = ((XenAPI.Host)Affinity).opaque_ref;
            }
            else if (Affinity is XenRef<XenAPI.Host>) // case object is XenRef
            {
                affinity = ((XenRef<XenAPI.Host>)Affinity).opaque_ref;
            }
            else if (Affinity is string && CommonCmdletFunctions.IsOpaqueRef((string)Affinity)) // case object is OpaqueRef string
            {
                affinity = (string)Affinity;
            }
            else if ((Affinity is string && CommonCmdletFunctions.IsUuid((string)Affinity)) || (Affinity is Guid)) // case object is uuid
             {
                 if (Affinity is Guid)
                     Affinity = ((Guid)Affinity).ToString();
                 XenRef<XenAPI.Host> obj_ref = XenAPI.Host.get_by_uuid(session, (string)Affinity);
                 if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                 {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Host with uuid {0} does not exist",(string)Affinity)), "", ErrorCategory.InvalidArgument, Affinity));
                 }
                 affinity = obj_ref.opaque_ref;
             }
            else if (Affinity is string)
            {
                if ((string)Affinity == string.Empty)
                {
                    affinity = "";
                }
                else
                {
                    List<XenRef<XenAPI.Host>> obj_refs = XenAPI.Host.get_by_name_label(session, (string)Affinity);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Host with name label {0} does not exist",(string)Affinity)), "", ErrorCategory.InvalidArgument, Affinity));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.Host with name label {0} exist",(string)Affinity)), "", ErrorCategory.InvalidArgument, Affinity));
                    }
                    affinity = obj_refs[0].opaque_ref;
                }
            }
            else if (Affinity == null)
            {
                affinity = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Affinity must be of type XenAPI.Host, XenRef<XenAPI.Host>, Guid or string"), "", ErrorCategory.InvalidArgument, Affinity));
            }
            Record.affinity = string.IsNullOrEmpty(affinity) ? null : new XenRef<XenAPI.Host>(affinity);
                Record.memory_target = MemoryTarget;
                Record.memory_static_max = MemoryStaticMax;
                Record.memory_dynamic_max = MemoryDynamicMax;
                Record.memory_dynamic_min = MemoryDynamicMin;
                Record.memory_static_min = MemoryStaticMin;
                Record.VCPUs_params = CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(VCPUsParams);
                Record.VCPUs_max = VCPUsMax;
                Record.VCPUs_at_startup = VCPUsAtStartup;
                Record.actions_after_shutdown = ActionsAfterShutdown;
                Record.actions_after_reboot = ActionsAfterReboot;
                Record.actions_after_crash = ActionsAfterCrash;
                Record.PV_bootloader = PVBootloader;
                Record.PV_kernel = PVKernel;
                Record.PV_ramdisk = PVRamdisk;
                Record.PV_args = PVArgs;
                Record.PV_bootloader_args = PVBootloaderArgs;
                Record.PV_legacy_args = PVLegacyArgs;
                Record.HVM_boot_policy = HVMBootPolicy;
                Record.HVM_boot_params = CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(HVMBootParams);
                Record.HVM_shadow_multiplier = HVMShadowMultiplier;
                Record.platform = CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(Platform);
                Record.PCI_bus = PCIBus;
                Record.other_config = CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(OtherConfig);
                Record.recommendations = Recommendations;
                Record.xenstore_data = CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(XenstoreData);
                Record.ha_always_run = HaAlwaysRun;
                Record.ha_restart_priority = HaRestartPriority;
                Record.tags = Tags;
                Record.blocked_operations = CommonCmdletFunctions.ConvertHashTableToDictionary<vm_operations, string>(BlockedOperations);
                
            string protection_policy = null;

            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (ProtectionPolicy is PSObject)
            {
                ProtectionPolicy = ((PSObject)ProtectionPolicy).BaseObject;
            }

            if (ProtectionPolicy is XenAPI.VMPP) // case object is XenObject
            {
                protection_policy = ((XenAPI.VMPP)ProtectionPolicy).opaque_ref;
            }
            else if (ProtectionPolicy is XenRef<XenAPI.VMPP>) // case object is XenRef
            {
                protection_policy = ((XenRef<XenAPI.VMPP>)ProtectionPolicy).opaque_ref;
            }
            else if (ProtectionPolicy is string && CommonCmdletFunctions.IsOpaqueRef((string)ProtectionPolicy)) // case object is OpaqueRef string
            {
                protection_policy = (string)ProtectionPolicy;
            }
            else if ((ProtectionPolicy is string && CommonCmdletFunctions.IsUuid((string)ProtectionPolicy)) || (ProtectionPolicy is Guid)) // case object is uuid
             {
                 if (ProtectionPolicy is Guid)
                     ProtectionPolicy = ((Guid)ProtectionPolicy).ToString();
                 XenRef<XenAPI.VMPP> obj_ref = XenAPI.VMPP.get_by_uuid(session, (string)ProtectionPolicy);
                 if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                 {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VMPP with uuid {0} does not exist",(string)ProtectionPolicy)), "", ErrorCategory.InvalidArgument, ProtectionPolicy));
                 }
                 protection_policy = obj_ref.opaque_ref;
             }
            else if (ProtectionPolicy is string)
            {
                if ((string)ProtectionPolicy == string.Empty)
                {
                    protection_policy = "";
                }
                else
                {
                    List<XenRef<XenAPI.VMPP>> obj_refs = XenAPI.VMPP.get_by_name_label(session, (string)ProtectionPolicy);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VMPP with name label {0} does not exist",(string)ProtectionPolicy)), "", ErrorCategory.InvalidArgument, ProtectionPolicy));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.VMPP with name label {0} exist",(string)ProtectionPolicy)), "", ErrorCategory.InvalidArgument, ProtectionPolicy));
                    }
                    protection_policy = obj_refs[0].opaque_ref;
                }
            }
            else if (ProtectionPolicy == null)
            {
                protection_policy = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("ProtectionPolicy must be of type XenAPI.VMPP, XenRef<XenAPI.VMPP>, Guid or string"), "", ErrorCategory.InvalidArgument, ProtectionPolicy));
            }
            Record.protection_policy = string.IsNullOrEmpty(protection_policy) ? null : new XenRef<XenAPI.VMPP>(protection_policy);
                Record.is_snapshot_from_vmpp = IsSnapshotFromVmpp;
                
            string appliance = null;

            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (Appliance is PSObject)
            {
                Appliance = ((PSObject)Appliance).BaseObject;
            }

            if (Appliance is XenAPI.VM_appliance) // case object is XenObject
            {
                appliance = ((XenAPI.VM_appliance)Appliance).opaque_ref;
            }
            else if (Appliance is XenRef<XenAPI.VM_appliance>) // case object is XenRef
            {
                appliance = ((XenRef<XenAPI.VM_appliance>)Appliance).opaque_ref;
            }
            else if (Appliance is string && CommonCmdletFunctions.IsOpaqueRef((string)Appliance)) // case object is OpaqueRef string
            {
                appliance = (string)Appliance;
            }
            else if ((Appliance is string && CommonCmdletFunctions.IsUuid((string)Appliance)) || (Appliance is Guid)) // case object is uuid
             {
                 if (Appliance is Guid)
                     Appliance = ((Guid)Appliance).ToString();
                 XenRef<XenAPI.VM_appliance> obj_ref = XenAPI.VM_appliance.get_by_uuid(session, (string)Appliance);
                 if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                 {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VM_appliance with uuid {0} does not exist",(string)Appliance)), "", ErrorCategory.InvalidArgument, Appliance));
                 }
                 appliance = obj_ref.opaque_ref;
             }
            else if (Appliance is string)
            {
                if ((string)Appliance == string.Empty)
                {
                    appliance = "";
                }
                else
                {
                    List<XenRef<XenAPI.VM_appliance>> obj_refs = XenAPI.VM_appliance.get_by_name_label(session, (string)Appliance);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VM_appliance with name label {0} does not exist",(string)Appliance)), "", ErrorCategory.InvalidArgument, Appliance));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.VM_appliance with name label {0} exist",(string)Appliance)), "", ErrorCategory.InvalidArgument, Appliance));
                    }
                    appliance = obj_refs[0].opaque_ref;
                }
            }
            else if (Appliance == null)
            {
                appliance = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Appliance must be of type XenAPI.VM_appliance, XenRef<XenAPI.VM_appliance>, Guid or string"), "", ErrorCategory.InvalidArgument, Appliance));
            }
            Record.appliance = string.IsNullOrEmpty(appliance) ? null : new XenRef<XenAPI.VM_appliance>(appliance);
                Record.start_delay = StartDelay;
                Record.shutdown_delay = ShutdownDelay;
                Record.order = Order;
                
            string suspend_sr = null;

            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (SuspendSR is PSObject)
            {
                SuspendSR = ((PSObject)SuspendSR).BaseObject;
            }

            if (SuspendSR is XenAPI.SR) // case object is XenObject
            {
                suspend_sr = ((XenAPI.SR)SuspendSR).opaque_ref;
            }
            else if (SuspendSR is XenRef<XenAPI.SR>) // case object is XenRef
            {
                suspend_sr = ((XenRef<XenAPI.SR>)SuspendSR).opaque_ref;
            }
            else if (SuspendSR is string && CommonCmdletFunctions.IsOpaqueRef((string)SuspendSR)) // case object is OpaqueRef string
            {
                suspend_sr = (string)SuspendSR;
            }
            else if ((SuspendSR is string && CommonCmdletFunctions.IsUuid((string)SuspendSR)) || (SuspendSR is Guid)) // case object is uuid
             {
                 if (SuspendSR is Guid)
                     SuspendSR = ((Guid)SuspendSR).ToString();
                 XenRef<XenAPI.SR> obj_ref = XenAPI.SR.get_by_uuid(session, (string)SuspendSR);
                 if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                 {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.SR with uuid {0} does not exist",(string)SuspendSR)), "", ErrorCategory.InvalidArgument, SuspendSR));
                 }
                 suspend_sr = obj_ref.opaque_ref;
             }
            else if (SuspendSR is string)
            {
                if ((string)SuspendSR == string.Empty)
                {
                    suspend_sr = "";
                }
                else
                {
                    List<XenRef<XenAPI.SR>> obj_refs = XenAPI.SR.get_by_name_label(session, (string)SuspendSR);
                    if (obj_refs.Count == 0)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.SR with name label {0} does not exist",(string)SuspendSR)), "", ErrorCategory.InvalidArgument, SuspendSR));
                    }
                    else if (obj_refs.Count > 1)
                    {
                        ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("More than 1 XenAPI.SR with name label {0} exist",(string)SuspendSR)), "", ErrorCategory.InvalidArgument, SuspendSR));
                    }
                    suspend_sr = obj_refs[0].opaque_ref;
                }
            }
            else if (SuspendSR == null)
            {
                suspend_sr = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("SuspendSR must be of type XenAPI.SR, XenRef<XenAPI.SR>, Guid or string"), "", ErrorCategory.InvalidArgument, SuspendSR));
            }
            Record.suspend_SR = string.IsNullOrEmpty(suspend_sr) ? null : new XenRef<XenAPI.SR>(suspend_sr);
                Record.version = Version;
                Record.generation_id = GenerationId;
                
            }
            else if (Record == null)
            {
                Record = new XenAPI.VM(HashTable);
            }
            // check commands for null-ness
            if (Record == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Record\" must be set"), "", ErrorCategory.InvalidArgument, Record));
            }
            

            
            if (!ShouldProcess(session.Url, "VM.create"))
                return;

            try
            {
                if (RunAsync)
                {
                    XenRef<XenAPI.Task> task_ref = XenAPI.VM.async_create(session, Record);
                    XenAPI.Task taskRec = XenAPI.Task.get_record(session, task_ref.opaque_ref);
                    taskRec.opaque_ref = task_ref.opaque_ref;
                    WriteObject(taskRec, true);
                }
                else
                {
                    string obj_ref = XenAPI.VM.create(session, Record);
                if (obj_ref == "OpaqueRef:NULL")
                    WriteObject (null, true);
                else
                {
                    XenAPI.VM rec = XenAPI.VM.get_record(session, obj_ref);
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
