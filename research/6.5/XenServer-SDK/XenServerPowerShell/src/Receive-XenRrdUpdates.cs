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
using System.Net;
using System.Text;

using XenAPI;

namespace Citrix.XenServer.Commands
{
    [Cmdlet(VerbsCommunications.Receive, "XenRrdUpdates", SupportsShouldProcess = false)]
    [OutputType(typeof(void))]
    public class ReceiveXenRrdUpdatesCommand : XenServerHttpCmdlet
    {
        #region Cmdlet Parameters

        [Parameter]
        public HTTP.DataCopiedDelegate DataCopiedDelegate { get; set; }

        [Parameter]
        public long Start { get; set; }

        [Parameter]
        public string Cf { get; set; }

        [Parameter]
        public long Interval { get; set; }

        [Parameter]
        public bool IsHost { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string Uuid { get; set; }

        [Parameter]
        public bool Json { get; set; }

        #endregion
        
        #region Cmdlet Methods
        
        protected override void ProcessRecord()
        {
            GetSession();

            RunApiCall(() => XenAPI.HTTP_actions.rrd_updates(DataCopiedDelegate,
                CancellingDelegate, TimeoutMs, XenHost, Proxy, Path, TaskRef,
                session.opaque_ref, Start, Cf, Interval, IsHost, Uuid, Json));
        }
        
        #endregion
    }
}