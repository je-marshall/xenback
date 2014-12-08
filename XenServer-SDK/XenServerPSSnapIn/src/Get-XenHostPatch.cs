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
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;

using XenAPI;

namespace Citrix.XenServer.Commands
{
    [Cmdlet(VerbsCommon.Get, "XenHostPatch", DefaultParameterSetName = "Ref")]
    [OutputType(typeof(XenAPI.Host_patch[]))]
    public class GetXenHostPatchCommand : XenServerCmdlet
    {
        #region Cmdlet Parameters

        
        [Parameter(ParameterSetName = "Ref", ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("opaque_ref")]
        public XenRef<XenAPI.Host_patch> Ref { get; set; }
        
        [Parameter(ParameterSetName = "Uuid", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        public Guid Uuid { get; set; }

        [Parameter(ParameterSetName = "Name", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("name_label")]
        public string Name { get; set; }


        #endregion

        #region Cmdlet Methods

        protected override void ProcessRecord()
        {
            GetSession();

            var records = XenAPI.Host_patch.get_all_records(session);

            foreach (var record in records)
                record.Value.opaque_ref = record.Key;

            var results = new List<XenAPI.Host_patch>();

            if (Ref != null)
            {
                foreach (var record in records)
                    if (Ref.opaque_ref == record.Key.opaque_ref)
                    {
                        results.Add(record.Value);
                        break;
                    }
            }
            else if (Name != null)
            {
                var options = WildcardOptions.IgnoreCase
                              | WildcardOptions.Compiled
                              | WildcardOptions.CultureInvariant;
                var wildcard = new WildcardPattern(Name, options);

                foreach (var record in records)
                {
                    if (wildcard.IsMatch(record.Value.name_label))
                        results.Add(record.Value);
                }
            }
            else if (Uuid != Guid.Empty)
            {
                foreach (var record in records)
                    if (Uuid.ToString() == record.Value.uuid)
                    {
                        results.Add(record.Value);
                        break;
                    }
            }
            else
            {
                results.AddRange(records.Values);
            }

            if (results.Count == 0)
            {
                ThrowTerminatingError(new ErrorRecord(
                                          new Exception("No Host_patch was found that matched the filters."),
                                          string.Empty,
                                          ErrorCategory.InvalidArgument,
                                          null));
            }
            WriteObject(results, true);

            UpdateSessions();
        }

        #endregion
    }
}
