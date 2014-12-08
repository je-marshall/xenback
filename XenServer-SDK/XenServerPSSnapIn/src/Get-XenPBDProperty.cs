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
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

using XenAPI;

namespace Citrix.XenServer.Commands
{
    [Cmdlet(VerbsCommon.Get, "XenPBDProperty", SupportsShouldProcess=false)]
    public class GetXenPBDProperty : XenServerCmdlet
    {
        #region Cmdlet Parameters

        [Parameter(ParameterSetName = "XenObject", Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public XenAPI.PBD PBD { get; set; }
        
        [Parameter(ParameterSetName = "Ref", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("opaque_ref")]
        public XenRef<XenAPI.PBD> Ref { get; set; }


        [Parameter(Mandatory = true)]
        public XenPBDProperty XenProperty { get; set; }
        
        #endregion

        #region Cmdlet Methods
        
        protected override void ProcessRecord()
        {
            GetSession();
            
            string pbd = ParsePBD();
            
            switch (XenProperty)
            {
                case XenPBDProperty.Uuid:
                    ProcessRecordUuid(pbd);
                    break;
                case XenPBDProperty.Host:
                    ProcessRecordHost(pbd);
                    break;
                case XenPBDProperty.SR:
                    ProcessRecordSR(pbd);
                    break;
                case XenPBDProperty.DeviceConfig:
                    ProcessRecordDeviceConfig(pbd);
                    break;
                case XenPBDProperty.CurrentlyAttached:
                    ProcessRecordCurrentlyAttached(pbd);
                    break;
                case XenPBDProperty.OtherConfig:
                    ProcessRecordOtherConfig(pbd);
                    break;
            }
            
            UpdateSessions();
        }
        
        #endregion
    
        #region Private Methods

        private string ParsePBD()
        {
            string pbd = null;

            if (PBD != null)
                pbd = (new XenRef<XenAPI.PBD>(PBD)).opaque_ref;
            else if (Ref != null)
                pbd = Ref.opaque_ref;
            else
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("At least one of the parameters 'PBD', 'Ref', 'Uuid' must be set"),
                    string.Empty,
                    ErrorCategory.InvalidArgument,
                    PBD));
            }

            return pbd;
        }

        private void ProcessRecordUuid(string pbd)
        {
            RunApiCall(()=>
            {
                    string obj = XenAPI.PBD.get_uuid(session, pbd);

                        WriteObject(obj, true);
            });
        }

        private void ProcessRecordHost(string pbd)
        {
            RunApiCall(()=>
            {
                    string objRef = XenAPI.PBD.get_host(session, pbd);

                        XenAPI.Host obj = null;

                        if (objRef != "OpaqueRef:NULL")
                        {
                            obj = XenAPI.Host.get_record(session, objRef);
                            obj.opaque_ref = objRef;
                        }

                        WriteObject(obj, true);
            });
        }

        private void ProcessRecordSR(string pbd)
        {
            RunApiCall(()=>
            {
                    string objRef = XenAPI.PBD.get_SR(session, pbd);

                        XenAPI.SR obj = null;

                        if (objRef != "OpaqueRef:NULL")
                        {
                            obj = XenAPI.SR.get_record(session, objRef);
                            obj.opaque_ref = objRef;
                        }

                        WriteObject(obj, true);
            });
        }

        private void ProcessRecordDeviceConfig(string pbd)
        {
            RunApiCall(()=>
            {
                    var dict = XenAPI.PBD.get_device_config(session, pbd);

                        Hashtable ht = CommonCmdletFunctions.ConvertDictionaryToHashtable(dict);
                        WriteObject(ht, true);
            });
        }

        private void ProcessRecordCurrentlyAttached(string pbd)
        {
            RunApiCall(()=>
            {
                    bool obj = XenAPI.PBD.get_currently_attached(session, pbd);

                        WriteObject(obj, true);
            });
        }

        private void ProcessRecordOtherConfig(string pbd)
        {
            RunApiCall(()=>
            {
                    var dict = XenAPI.PBD.get_other_config(session, pbd);

                        Hashtable ht = CommonCmdletFunctions.ConvertDictionaryToHashtable(dict);
                        WriteObject(ht, true);
            });
        }

        #endregion
    }
    
    public enum XenPBDProperty
    {
        Uuid,
        Host,
        SR,
        DeviceConfig,
        CurrentlyAttached,
        OtherConfig
    }

}
