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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Xml;
using System.Net;
using System.Threading;

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

using XenAPI;

namespace Citrix.XenServer.Commands
{
    [Cmdlet("Wait", "XenServer:Task")]
    public class WaitXenServerTaskCommand : PSCmdlet
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

        [Parameter(Position = 0,
         ValueFromPipeline = true,
         ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public object Task
        {
            get { return _task; }
            set { _task = value; }
        }
        private object _task;

        [Parameter]
        public SwitchParameter ShowProgress
        {
            get { return showProgress; }
            set { showProgress = value; }
        }
        private bool showProgress;

        [Parameter]
        public ProgressBar Progressbar
        {
            get { return _progressBar; }
            set { _progressBar = value; }
        }
        public ProgressBar _progressBar;

        [Parameter]
        public int Min
        {
            get { return min; }
            set { min = value; }
        }
        private int min = 0;

        [Parameter]
        public int Max
        {
            get { return max; }
            set { max = value; }
        }
        private int max = 100;

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
            if (Task == null)
            {
                ThrowTerminatingError(new ErrorRecord(
                                          new ArgumentException(
                                          "Parameter \"Task\" must be set"),
                                          "",
                                          ErrorCategory.InvalidArgument, Task));
            }

            string task = null;

            // case object is PSObject (occasionally powershell will do this), we need our object back
            if (Task is PSObject)
            {
                Task = ((PSObject)Task).BaseObject;
            }

            if (Task is XenAPI.Task) // case object is XenObject
            {
                task = ((XenAPI.Task)Task).opaque_ref;
            }
            else if (Task is XenRef<XenAPI.Task>) // case object is XenRef
            {
                task = ((XenRef<XenAPI.Task>)Task).opaque_ref;
            }
            else if (Task is string && CommonCmdletFunctions.IsOpaqueRef((string)Task)) // case object is OpaqueRef string
            {
                task = (string)Task;
            }
            else if ((Task is string && CommonCmdletFunctions.IsUuid((string)Task)) || (Task is Guid)) // case object is uuid
            {
                if (Task is Guid)
                    Task = ((Guid)Task).ToString();

                XenRef<XenAPI.Task> obj_ref = XenAPI.Task.get_by_uuid(session, (string)Task);
                
                if (!CommonCmdletFunctions.IsOpaqueRef(obj_ref.opaque_ref))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException(string.Format("XenAPI.Task with uuid {0} does not exist", Task)),
                        "",
                        ErrorCategory.InvalidArgument,
                        Task));
                }
                task = obj_ref.opaque_ref;
            }
            else if (Task is string)
            {
                List<XenRef<XenAPI.Task>> obj_refs = XenAPI.Task.get_by_name_label(session, (string)Task);
                
                if (obj_refs.Count == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException(
                            string.Format("XenAPI.Task with name label {0} does not exist", Task)),
                            "",
                            ErrorCategory.InvalidArgument,
                            Task));
                }
                else if (obj_refs.Count > 1)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException(
                            string.Format("More than 1 XenAPI.Task with name label {0} exist", Task)),
                            "",
                            ErrorCategory.InvalidArgument,
                            Task));
                }
                task = obj_refs[0].opaque_ref;
            }
            else
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Task must be of type XenAPI.Task, XenRef<XenAPI.Task>, Guid or string"),
                    "",
                    ErrorCategory.InvalidArgument,
                    Task));
            }

            try
            {
                string name = XenAPI.Task.get_name_label(session, task);
                string uuid = XenAPI.Task.get_uuid(session, task);
                DateTime created = XenAPI.Task.get_created(session, task);
                ProgressRecord prog = new ProgressRecord(Math.Abs(uuid.GetHashCode()), name, "0% complete");
                prog.RecordType = ProgressRecordType.Processing;

                while (true)
                {
                    double progress = XenAPI.Task.get_progress(session, task);
                    prog.PercentComplete = min + (int)(progress * (max - min));
                    prog.StatusDescription = string.Format("{0}% complete", prog.PercentComplete);
                    TimeSpan elapsed = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - created.Ticks);
                    prog.CurrentOperation = string.Format("{0}:{1}:{2}",
                        elapsed.Hours.ToString("00"),
                        elapsed.Minutes.ToString("00"),
                        elapsed.Seconds.ToString("00"));

                    if (XenAPI.Task.get_status(session, task) != XenAPI.task_status_type.pending)
                    {
                        if (prog.PercentComplete == 100)
                            prog.RecordType = ProgressRecordType.Completed;

                        if (Progressbar != null)
                            Progressbar.Value = (int)(Progressbar.Minimum + (prog.PercentComplete * ((Progressbar.Maximum - Progressbar.Minimum) / 100d)));
                        else if (ShowProgress)
                            WriteProgress(prog);

                        break;
                    }
                    else
                    {
                        if (Progressbar != null)
                            Progressbar.Value = (int)(Progressbar.Minimum + (prog.PercentComplete * ((Progressbar.Maximum - Progressbar.Minimum) / 100d)));
                        else if (ShowProgress)
                            WriteProgress(prog);
                    }
                    Thread.Sleep(500);
                }

                if (XenAPI.Task.get_status(session, task) == XenAPI.task_status_type.failure)
                {
                    throw new Failure(XenAPI.Task.get_error_info(session, task));
                }
                
                if (XenAPI.Task.get_status(session, task) == XenAPI.task_status_type.cancelled)
                {
                    throw new Exception("User Cancelled");
                }

                WriteObject(XenAPI.Task.get_result(session, task).Replace("<value>", "").Replace("</value>", ""), true);
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
