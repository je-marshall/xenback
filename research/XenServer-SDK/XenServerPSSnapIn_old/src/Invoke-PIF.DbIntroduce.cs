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
    [Cmdlet("Invoke", "XenServer:PIF.DbIntroduce", SupportsShouldProcess=true)]
    public class InvokeXenServerPIF_DbIntroduceCommand : PSCmdlet
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
        public string Device
        {
            get { return _device; }
            set { _device = value; }
        }
        private string _device;

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
        public new object Host
        {
            get { return _host; }
            set { _host = value; }
        }
        private object _host;

        [Parameter(Position = 3,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string MAC
        {
            get { return _mAC; }
            set { _mAC = value; }
        }
        private string _mAC;

        [Parameter(Position = 4,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long MTU
        {
            get { return _mTU; }
            set { _mTU = value; }
        }
        private long _mTU;

        [Parameter(Position = 5,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public long VLAN
        {
            get { return _vLAN; }
            set { _vLAN = value; }
        }
        private long _vLAN;

        [Parameter(Position = 6,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool Physical
        {
            get { return _physical; }
            set { _physical = value; }
        }
        private bool _physical;

        [Parameter(Position = 7,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public ip_configuration_mode IpConfigurationMode
        {
            get { return _ipConfigurationMode; }
            set { _ipConfigurationMode = value; }
        }
        private ip_configuration_mode _ipConfigurationMode;

        [Parameter(Position = 8,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string IP
        {
            get { return _iP; }
            set { _iP = value; }
        }
        private string _iP;

        [Parameter(Position = 9,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string Netmask
        {
            get { return _netmask; }
            set { _netmask = value; }
        }
        private string _netmask;

        [Parameter(Position = 10,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string Gateway
        {
            get { return _gateway; }
            set { _gateway = value; }
        }
        private string _gateway;

        [Parameter(Position = 11,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string DNS
        {
            get { return _dNS; }
            set { _dNS = value; }
        }
        private string _dNS;

        [Parameter(Position = 12,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object BondSlaveOf
        {
            get { return _bondSlaveOf; }
            set { _bondSlaveOf = value; }
        }
        private object _bondSlaveOf;

        [Parameter(Position = 13,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public object VLANMasterOf
        {
            get { return _vLANMasterOf; }
            set { _vLANMasterOf = value; }
        }
        private object _vLANMasterOf;

        [Parameter(Position = 14,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool Management
        {
            get { return _management; }
            set { _management = value; }
        }
        private bool _management;

        [Parameter(Position = 15,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public Hashtable OtherConfig
        {
            get { return _otherConfig; }
            set { _otherConfig = value; }
        }
        private Hashtable _otherConfig;

        [Parameter(Position = 16,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public bool DisallowUnplug
        {
            get { return _disallowUnplug; }
            set { _disallowUnplug = value; }
        }
        private bool _disallowUnplug;

        [Parameter(Position = 17,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public ipv6_configuration_mode Ipv6ConfigurationMode
        {
            get { return _ipv6ConfigurationMode; }
            set { _ipv6ConfigurationMode = value; }
        }
        private ipv6_configuration_mode _ipv6ConfigurationMode;

        [Parameter(Position = 18,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string[] IPv6
        {
            get { return _iPv6; }
            set { _iPv6 = value; }
        }
        private string[] _iPv6;

        [Parameter(Position = 19,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public string Ipv6Gateway
        {
            get { return _ipv6Gateway; }
            set { _ipv6Gateway = value; }
        }
        private string _ipv6Gateway;

        [Parameter(Position = 20,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        public primary_address_type PrimaryAddressType
        {
            get { return _primaryAddressType; }
            set { _primaryAddressType = value; }
        }
        private primary_address_type _primaryAddressType;

        
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
            if (Device == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Device\" must be set"), "", ErrorCategory.InvalidArgument, Device));
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

            if (MAC == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"MAC\" must be set"), "", ErrorCategory.InvalidArgument, MAC));
            }
            

            

            

            

            

            if (IP == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"IP\" must be set"), "", ErrorCategory.InvalidArgument, IP));
            }
            

            if (Netmask == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Netmask\" must be set"), "", ErrorCategory.InvalidArgument, Netmask));
            }
            

            if (Gateway == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Gateway\" must be set"), "", ErrorCategory.InvalidArgument, Gateway));
            }
            

            if (DNS == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"DNS\" must be set"), "", ErrorCategory.InvalidArgument, DNS));
            }
            

            if (BondSlaveOf == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"BondSlaveOf\" must be set"), "", ErrorCategory.InvalidArgument, BondSlaveOf));
            }
            string bond_slave_of = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (BondSlaveOf is PSObject)
            {
                BondSlaveOf = ((PSObject)BondSlaveOf).BaseObject;
            }
  
            if (BondSlaveOf is XenAPI.Bond) // case object is XenObject
            {
                bond_slave_of = ((XenAPI.Bond)BondSlaveOf).opaque_ref;
            }
            else if (BondSlaveOf is XenRef<XenAPI.Bond>) // case object is XenRef
            {
                bond_slave_of = ((XenRef<XenAPI.Bond>)BondSlaveOf).opaque_ref;
            }
            else if (BondSlaveOf is string && CommonCmdletFunctions.IsOpaqueRef((string)BondSlaveOf)) // case object is OpaqueRef string
            {
                bond_slave_of = (string)BondSlaveOf;
            }
            else if ((BondSlaveOf is string && CommonCmdletFunctions.IsUuid((string)BondSlaveOf)) || (BondSlaveOf is Guid)) // case object is uuid
            {
                if (BondSlaveOf is Guid)
                    BondSlaveOf = ((Guid)BondSlaveOf).ToString();
                XenRef<XenAPI.Bond> obj_ref = XenAPI.Bond.get_by_uuid(session, (string)BondSlaveOf);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.Bond with uuid {0} does not exist",(string)BondSlaveOf)), "", ErrorCategory.InvalidArgument, BondSlaveOf));
                }
                bond_slave_of = obj_ref.opaque_ref;
            }
            else if (BondSlaveOf is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.Bond does not support get_by_name_label and BondSlaveOf is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, BondSlaveOf));
            }
            else if (BondSlaveOf == null)
            {
                bond_slave_of = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("BondSlaveOf must be of type XenAPI.Bond, XenRef<XenAPI.Bond>, Guid or string"), "", ErrorCategory.InvalidArgument, BondSlaveOf));
            }

            if (VLANMasterOf == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"VLANMasterOf\" must be set"), "", ErrorCategory.InvalidArgument, VLANMasterOf));
            }
            string vlan_master_of = null;
            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (VLANMasterOf is PSObject)
            {
                VLANMasterOf = ((PSObject)VLANMasterOf).BaseObject;
            }
  
            if (VLANMasterOf is XenAPI.VLAN) // case object is XenObject
            {
                vlan_master_of = ((XenAPI.VLAN)VLANMasterOf).opaque_ref;
            }
            else if (VLANMasterOf is XenRef<XenAPI.VLAN>) // case object is XenRef
            {
                vlan_master_of = ((XenRef<XenAPI.VLAN>)VLANMasterOf).opaque_ref;
            }
            else if (VLANMasterOf is string && CommonCmdletFunctions.IsOpaqueRef((string)VLANMasterOf)) // case object is OpaqueRef string
            {
                vlan_master_of = (string)VLANMasterOf;
            }
            else if ((VLANMasterOf is string && CommonCmdletFunctions.IsUuid((string)VLANMasterOf)) || (VLANMasterOf is Guid)) // case object is uuid
            {
                if (VLANMasterOf is Guid)
                    VLANMasterOf = ((Guid)VLANMasterOf).ToString();
                XenRef<XenAPI.VLAN> obj_ref = XenAPI.VLAN.get_by_uuid(session, (string)VLANMasterOf);
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(string.Format("XenAPI.VLAN with uuid {0} does not exist",(string)VLANMasterOf)), "", ErrorCategory.InvalidArgument, VLANMasterOf));
                }
                vlan_master_of = obj_ref.opaque_ref;
            }
            else if (VLANMasterOf is string)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("XenAPI.VLAN does not support get_by_name_label and VLANMasterOf is neither a uuid or an opaque ref"), "", ErrorCategory.InvalidArgument, VLANMasterOf));
            }
            else if (VLANMasterOf == null)
            {
                vlan_master_of = "";
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("VLANMasterOf must be of type XenAPI.VLAN, XenRef<XenAPI.VLAN>, Guid or string"), "", ErrorCategory.InvalidArgument, VLANMasterOf));
            }

            

            if (OtherConfig == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"OtherConfig\" must be set"), "", ErrorCategory.InvalidArgument, OtherConfig));
            }
            

            

            

            if (IPv6 == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"IPv6\" must be set"), "", ErrorCategory.InvalidArgument, IPv6));
            }
            

            if (Ipv6Gateway == null)
            {
                ThrowTerminatingError(new ErrorRecord(new ArgumentException("Parameter \"Ipv6Gateway\" must be set"), "", ErrorCategory.InvalidArgument, Ipv6Gateway));
            }
            

            

            
            if (!ShouldProcess(Device, "PIF.db_introduce"))
                return;

            try
            {
                if (RunAsync)
                {
                    XenRef<XenAPI.Task> task_ref = XenAPI.PIF.async_db_introduce(session, Device, network, host, MAC, MTU, VLAN, Physical, IpConfigurationMode, IP, Netmask, Gateway, DNS, bond_slave_of, vlan_master_of, Management, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(OtherConfig), DisallowUnplug, Ipv6ConfigurationMode, IPv6, Ipv6Gateway, PrimaryAddressType);
                    XenAPI.Task taskRec = XenAPI.Task.get_record(session, task_ref.opaque_ref);
                    taskRec.opaque_ref = task_ref.opaque_ref;
                    WriteObject(taskRec, true);
                }
                else
                {
                    string obj_ref = XenAPI.PIF.db_introduce(session, Device, network, host, MAC, MTU, VLAN, Physical, IpConfigurationMode, IP, Netmask, Gateway, DNS, bond_slave_of, vlan_master_of, Management, CommonCmdletFunctions.ConvertHashTableToDictionary<string, string>(OtherConfig), DisallowUnplug, Ipv6ConfigurationMode, IPv6, Ipv6Gateway, PrimaryAddressType);
                if (obj_ref == "OpaqueRef:NULL")
                    WriteObject (null, true);
                else
                {
                    XenAPI.PIF rec = XenAPI.PIF.get_record(session, obj_ref);
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
