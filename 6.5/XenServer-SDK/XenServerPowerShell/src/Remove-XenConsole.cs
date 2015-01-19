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
    [Cmdlet(VerbsCommon.Remove, "XenConsole", SupportsShouldProcess = true)]
    [OutputType(typeof(XenAPI.Console))]
    [OutputType(typeof(XenAPI.Task))]
    [OutputType(typeof(void))]
    public class RemoveXenConsole : XenServerCmdlet
    {
        #region Cmdlet Parameters
        
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter(ParameterSetName = "XenObject", Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public XenAPI.Console Console { get; set; }
        
        [Parameter(ParameterSetName = "Ref", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("opaque_ref")]
        public XenRef<XenAPI.Console> Ref { get; set; }
        
        [Parameter(ParameterSetName = "Uuid", Mandatory = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        public Guid Uuid { get; set; }


        protected override bool GenerateAsyncParam
        {
            get { return true; }
        }

        #endregion

        #region Cmdlet Methods
        
        protected override void ProcessRecord()
        {
            GetSession();

            string console = ParseConsole();
            
            ProcessRecordDestroy(console);

            UpdateSessions();
        }
        
        #endregion
    
        #region Private Methods

        private string ParseConsole()
        {
            string console = null;

            if (Console != null)
                console = (new XenRef<XenAPI.Console>(Console)).opaque_ref;
            else if (Uuid != Guid.Empty)
            {
                var xenRef = XenAPI.Console.get_by_uuid(session, Uuid.ToString());
                if (xenRef != null)
                    console = xenRef.opaque_ref;
            }
            else if (Ref != null)
                console = Ref.opaque_ref;
            else
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("At least one of the parameters 'Console', 'Ref', 'Uuid' must be set"),
                    string.Empty,
                    ErrorCategory.InvalidArgument,
                    Console));
            }

            return console;
        }

        private void ProcessRecordDestroy(string console)
        {
            if (!ShouldProcess(console, "Console.destroy"))
                return;

            RunApiCall(()=>
            {
                var contxt = _context as XenServerCmdletDynamicParameters;
                
                if (contxt != null && contxt.Async)
                {
                    taskRef = XenAPI.Console.async_destroy(session, console);

                    if (PassThru)
                    {
                        XenAPI.Task taskObj = null;
                        if (taskRef != "OpaqueRef:NULL")
                        {
                            taskObj = XenAPI.Task.get_record(session, taskRef.opaque_ref);
                            taskObj.opaque_ref = taskRef.opaque_ref;
                        }
                    
                        WriteObject(taskObj, true);
                    }
                }
                else
                {
                    XenAPI.Console.destroy(session, console);

                }

            });
        }

        #endregion
    }
}
