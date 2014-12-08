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
    [Cmdlet(VerbsCommon.Get, "XenPGPUProperty", SupportsShouldProcess=false)]
    public class GetXenPGPUProperty : XenServerCmdlet
    {
        #region Cmdlet Parameters

        [Parameter(ParameterSetName = "XenObject", Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public XenAPI.PGPU PGPU { get; set; }
        
        [Parameter(ParameterSetName = "Ref", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("opaque_ref")]
        public XenRef<XenAPI.PGPU> Ref { get; set; }


        [Parameter(Mandatory = true)]
        public XenPGPUProperty XenProperty { get; set; }
        
        #endregion

        #region Cmdlet Methods
        
        protected override void ProcessRecord()
        {
            GetSession();
            
            string pgpu = ParsePGPU();
            
            switch (XenProperty)
            {
                case XenPGPUProperty.Uuid:
                    ProcessRecordUuid(pgpu);
                    break;
                case XenPGPUProperty.PCI:
                    ProcessRecordPCI(pgpu);
                    break;
                case XenPGPUProperty.GPUGroup:
                    ProcessRecordGPUGroup(pgpu);
                    break;
                case XenPGPUProperty.Host:
                    ProcessRecordHost(pgpu);
                    break;
                case XenPGPUProperty.OtherConfig:
                    ProcessRecordOtherConfig(pgpu);
                    break;
            }
            
            UpdateSessions();
        }
        
        #endregion
    
        #region Private Methods

        private string ParsePGPU()
        {
            string pgpu = null;

            if (PGPU != null)
                pgpu = (new XenRef<XenAPI.PGPU>(PGPU)).opaque_ref;
            else if (Ref != null)
                pgpu = Ref.opaque_ref;
            else
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("At least one of the parameters 'PGPU', 'Ref', 'Uuid' must be set"),
                    string.Empty,
                    ErrorCategory.InvalidArgument,
                    PGPU));
            }

            return pgpu;
        }

        private void ProcessRecordUuid(string pgpu)
        {
            RunApiCall(()=>
            {
                    string obj = XenAPI.PGPU.get_uuid(session, pgpu);

                        WriteObject(obj, true);
            });
        }

        private void ProcessRecordPCI(string pgpu)
        {
            RunApiCall(()=>
            {
                    string objRef = XenAPI.PGPU.get_PCI(session, pgpu);

                        XenAPI.PCI obj = null;

                        if (objRef != "OpaqueRef:NULL")
                        {
                            obj = XenAPI.PCI.get_record(session, objRef);
                            obj.opaque_ref = objRef;
                        }

                        WriteObject(obj, true);
            });
        }

        private void ProcessRecordGPUGroup(string pgpu)
        {
            RunApiCall(()=>
            {
                    string objRef = XenAPI.PGPU.get_GPU_group(session, pgpu);

                        XenAPI.GPU_group obj = null;

                        if (objRef != "OpaqueRef:NULL")
                        {
                            obj = XenAPI.GPU_group.get_record(session, objRef);
                            obj.opaque_ref = objRef;
                        }

                        WriteObject(obj, true);
            });
        }

        private void ProcessRecordHost(string pgpu)
        {
            RunApiCall(()=>
            {
                    string objRef = XenAPI.PGPU.get_host(session, pgpu);

                        XenAPI.Host obj = null;

                        if (objRef != "OpaqueRef:NULL")
                        {
                            obj = XenAPI.Host.get_record(session, objRef);
                            obj.opaque_ref = objRef;
                        }

                        WriteObject(obj, true);
            });
        }

        private void ProcessRecordOtherConfig(string pgpu)
        {
            RunApiCall(()=>
            {
                    var dict = XenAPI.PGPU.get_other_config(session, pgpu);

                        Hashtable ht = CommonCmdletFunctions.ConvertDictionaryToHashtable(dict);
                        WriteObject(ht, true);
            });
        }

        #endregion
    }
    
    public enum XenPGPUProperty
    {
        Uuid,
        PCI,
        GPUGroup,
        Host,
        OtherConfig
    }

}
