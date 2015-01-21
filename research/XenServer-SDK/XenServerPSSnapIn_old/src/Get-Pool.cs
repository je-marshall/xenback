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
    [Cmdlet("Get", "XenServer:Pool")]
    public class GetXenServerPoolCommand : PSCmdlet
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
        [Alias("Tags")]
        public string[] TagFilter
        {
            get { return tagFilter; }
            set { tagFilter = value; }
        }
        private string[] tagFilter = null;

        [Parameter]
        [Alias("Properties")]
        public Hashtable PropFilter
        {
            get { return props; }
            set { props = value; }
        }
        private Hashtable props = null;

        #endregion

        #region Cmdlet Methods

        protected override void ProcessRecord()
        {
            XenServerSessions sessions;
            Session session = CommonCmdletFunctions.GetXenSession(this,
                                                    out sessions,
                                                    ref url,
                                                    Server, Port);
            Dictionary<XenRef<XenAPI.Pool>, XenAPI.Pool> records = XenAPI.Pool.get_all_records(session);
            //save session dictionary back in the session variable (in case it was modified)
            CommonCmdletFunctions.SetXenServerSessions(this, sessions);
            Collection<XenAPI.Pool> result = new Collection<XenAPI.Pool>();
            foreach (KeyValuePair<XenRef<XenAPI.Pool>, XenAPI.Pool> record in records)
            {
                record.Value.opaque_ref = record.Key.opaque_ref;
                if (MatchesFilters(record.Value))
                    result.Add(record.Value);
            }
            if (result.Count == 0)
            {
                ThrowTerminatingError(new ErrorRecord(new Exception("No Pool was found that matched the filters."), "", ErrorCategory.InvalidArgument, null));
            }
            WriteObject(result, true);
        }
        
        #endregion
        
        private bool MatchesFilters(XenAPI.Pool record)
        {
            if (!MatchesTagFilters(record))
                return false;

           if (!MatchesPropertyFilters(record))
              return false;

           return true;
        }

        private bool MatchesTagFilters(XenAPI.Pool record)
        {
            if (TagFilter == null)
                return true;
            string[] tags = record.tags;
            foreach (string tag in TagFilter)
            {
                if (Array.IndexOf(tags,tag) == -1)
                    return false;
            }
            return true;
        }

        private bool MatchesPropertyFilters(XenAPI.Pool record)
        {
            return CommonCmdletFunctions.MatchesPropertyFilter(PropFilter, typeof(XenAPI.Pool), record);
        }
    }
}
