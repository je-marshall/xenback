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

using CookComputing.XmlRpc;


namespace XenAPI
{
    public partial class Pool : XenObject<Pool>
    {
        public Pool()
        {
        }

        public Pool(string uuid,
            string name_label,
            string name_description,
            XenRef<Host> master,
            XenRef<SR> default_SR,
            XenRef<SR> suspend_image_SR,
            XenRef<SR> crash_dump_SR,
            Dictionary<string, string> other_config,
            bool ha_enabled,
            Dictionary<string, string> ha_configuration,
            string[] ha_statefiles,
            long ha_host_failures_to_tolerate,
            long ha_plan_exists_for,
            bool ha_allow_overcommit,
            bool ha_overcommitted,
            Dictionary<string, XenRef<Blob>> blobs,
            string[] tags,
            Dictionary<string, string> gui_config,
            string wlb_url,
            string wlb_username,
            bool wlb_enabled,
            bool wlb_verify_cert,
            bool redo_log_enabled,
            XenRef<VDI> redo_log_vdi,
            string vswitch_controller,
            Dictionary<string, string> restrictions,
            List<XenRef<VDI>> metadata_VDIs)
        {
            this.uuid = uuid;
            this.name_label = name_label;
            this.name_description = name_description;
            this.master = master;
            this.default_SR = default_SR;
            this.suspend_image_SR = suspend_image_SR;
            this.crash_dump_SR = crash_dump_SR;
            this.other_config = other_config;
            this.ha_enabled = ha_enabled;
            this.ha_configuration = ha_configuration;
            this.ha_statefiles = ha_statefiles;
            this.ha_host_failures_to_tolerate = ha_host_failures_to_tolerate;
            this.ha_plan_exists_for = ha_plan_exists_for;
            this.ha_allow_overcommit = ha_allow_overcommit;
            this.ha_overcommitted = ha_overcommitted;
            this.blobs = blobs;
            this.tags = tags;
            this.gui_config = gui_config;
            this.wlb_url = wlb_url;
            this.wlb_username = wlb_username;
            this.wlb_enabled = wlb_enabled;
            this.wlb_verify_cert = wlb_verify_cert;
            this.redo_log_enabled = redo_log_enabled;
            this.redo_log_vdi = redo_log_vdi;
            this.vswitch_controller = vswitch_controller;
            this.restrictions = restrictions;
            this.metadata_VDIs = metadata_VDIs;
        }

        /// <summary>
        /// Creates a new Pool from a Proxy_Pool.
        /// </summary>
        /// <param name="proxy"></param>
        public Pool(Proxy_Pool proxy)
        {
            this.UpdateFromProxy(proxy);
        }

        public override void UpdateFrom(Pool update)
        {
            uuid = update.uuid;
            name_label = update.name_label;
            name_description = update.name_description;
            master = update.master;
            default_SR = update.default_SR;
            suspend_image_SR = update.suspend_image_SR;
            crash_dump_SR = update.crash_dump_SR;
            other_config = update.other_config;
            ha_enabled = update.ha_enabled;
            ha_configuration = update.ha_configuration;
            ha_statefiles = update.ha_statefiles;
            ha_host_failures_to_tolerate = update.ha_host_failures_to_tolerate;
            ha_plan_exists_for = update.ha_plan_exists_for;
            ha_allow_overcommit = update.ha_allow_overcommit;
            ha_overcommitted = update.ha_overcommitted;
            blobs = update.blobs;
            tags = update.tags;
            gui_config = update.gui_config;
            wlb_url = update.wlb_url;
            wlb_username = update.wlb_username;
            wlb_enabled = update.wlb_enabled;
            wlb_verify_cert = update.wlb_verify_cert;
            redo_log_enabled = update.redo_log_enabled;
            redo_log_vdi = update.redo_log_vdi;
            vswitch_controller = update.vswitch_controller;
            restrictions = update.restrictions;
            metadata_VDIs = update.metadata_VDIs;
        }

        internal void UpdateFromProxy(Proxy_Pool proxy)
        {
            uuid = proxy.uuid == null ? null : (string)proxy.uuid;
            name_label = proxy.name_label == null ? null : (string)proxy.name_label;
            name_description = proxy.name_description == null ? null : (string)proxy.name_description;
            master = proxy.master == null ? null : XenRef<Host>.Create(proxy.master);
            default_SR = proxy.default_SR == null ? null : XenRef<SR>.Create(proxy.default_SR);
            suspend_image_SR = proxy.suspend_image_SR == null ? null : XenRef<SR>.Create(proxy.suspend_image_SR);
            crash_dump_SR = proxy.crash_dump_SR == null ? null : XenRef<SR>.Create(proxy.crash_dump_SR);
            other_config = proxy.other_config == null ? null : Maps.convert_from_proxy_string_string(proxy.other_config);
            ha_enabled = (bool)proxy.ha_enabled;
            ha_configuration = proxy.ha_configuration == null ? null : Maps.convert_from_proxy_string_string(proxy.ha_configuration);
            ha_statefiles = proxy.ha_statefiles == null ? new string[] {} : (string [])proxy.ha_statefiles;
            ha_host_failures_to_tolerate = proxy.ha_host_failures_to_tolerate == null ? 0 : long.Parse((string)proxy.ha_host_failures_to_tolerate);
            ha_plan_exists_for = proxy.ha_plan_exists_for == null ? 0 : long.Parse((string)proxy.ha_plan_exists_for);
            ha_allow_overcommit = (bool)proxy.ha_allow_overcommit;
            ha_overcommitted = (bool)proxy.ha_overcommitted;
            blobs = proxy.blobs == null ? null : Maps.convert_from_proxy_string_XenRefBlob(proxy.blobs);
            tags = proxy.tags == null ? new string[] {} : (string [])proxy.tags;
            gui_config = proxy.gui_config == null ? null : Maps.convert_from_proxy_string_string(proxy.gui_config);
            wlb_url = proxy.wlb_url == null ? null : (string)proxy.wlb_url;
            wlb_username = proxy.wlb_username == null ? null : (string)proxy.wlb_username;
            wlb_enabled = (bool)proxy.wlb_enabled;
            wlb_verify_cert = (bool)proxy.wlb_verify_cert;
            redo_log_enabled = (bool)proxy.redo_log_enabled;
            redo_log_vdi = proxy.redo_log_vdi == null ? null : XenRef<VDI>.Create(proxy.redo_log_vdi);
            vswitch_controller = proxy.vswitch_controller == null ? null : (string)proxy.vswitch_controller;
            restrictions = proxy.restrictions == null ? null : Maps.convert_from_proxy_string_string(proxy.restrictions);
            metadata_VDIs = proxy.metadata_VDIs == null ? null : XenRef<VDI>.Create(proxy.metadata_VDIs);
        }

        public Proxy_Pool ToProxy()
        {
            Proxy_Pool result_ = new Proxy_Pool();
            result_.uuid = (uuid != null) ? uuid : "";
            result_.name_label = (name_label != null) ? name_label : "";
            result_.name_description = (name_description != null) ? name_description : "";
            result_.master = (master != null) ? master : "";
            result_.default_SR = (default_SR != null) ? default_SR : "";
            result_.suspend_image_SR = (suspend_image_SR != null) ? suspend_image_SR : "";
            result_.crash_dump_SR = (crash_dump_SR != null) ? crash_dump_SR : "";
            result_.other_config = Maps.convert_to_proxy_string_string(other_config);
            result_.ha_enabled = ha_enabled;
            result_.ha_configuration = Maps.convert_to_proxy_string_string(ha_configuration);
            result_.ha_statefiles = ha_statefiles;
            result_.ha_host_failures_to_tolerate = ha_host_failures_to_tolerate.ToString();
            result_.ha_plan_exists_for = ha_plan_exists_for.ToString();
            result_.ha_allow_overcommit = ha_allow_overcommit;
            result_.ha_overcommitted = ha_overcommitted;
            result_.blobs = Maps.convert_to_proxy_string_XenRefBlob(blobs);
            result_.tags = tags;
            result_.gui_config = Maps.convert_to_proxy_string_string(gui_config);
            result_.wlb_url = (wlb_url != null) ? wlb_url : "";
            result_.wlb_username = (wlb_username != null) ? wlb_username : "";
            result_.wlb_enabled = wlb_enabled;
            result_.wlb_verify_cert = wlb_verify_cert;
            result_.redo_log_enabled = redo_log_enabled;
            result_.redo_log_vdi = (redo_log_vdi != null) ? redo_log_vdi : "";
            result_.vswitch_controller = (vswitch_controller != null) ? vswitch_controller : "";
            result_.restrictions = Maps.convert_to_proxy_string_string(restrictions);
            result_.metadata_VDIs = (metadata_VDIs != null) ? Helper.RefListToStringArray(metadata_VDIs) : new string[] {};
            return result_;
        }

        /// <summary>
        /// Creates a new Pool from a Hashtable.
        /// </summary>
        /// <param name="table"></param>
        public Pool(Hashtable table)
        {
            uuid = Marshalling.ParseString(table, "uuid");
            name_label = Marshalling.ParseString(table, "name_label");
            name_description = Marshalling.ParseString(table, "name_description");
            master = Marshalling.ParseRef<Host>(table, "master");
            default_SR = Marshalling.ParseRef<SR>(table, "default_SR");
            suspend_image_SR = Marshalling.ParseRef<SR>(table, "suspend_image_SR");
            crash_dump_SR = Marshalling.ParseRef<SR>(table, "crash_dump_SR");
            other_config = Maps.convert_from_proxy_string_string(Marshalling.ParseHashTable(table, "other_config"));
            ha_enabled = Marshalling.ParseBool(table, "ha_enabled");
            ha_configuration = Maps.convert_from_proxy_string_string(Marshalling.ParseHashTable(table, "ha_configuration"));
            ha_statefiles = Marshalling.ParseStringArray(table, "ha_statefiles");
            ha_host_failures_to_tolerate = Marshalling.ParseLong(table, "ha_host_failures_to_tolerate");
            ha_plan_exists_for = Marshalling.ParseLong(table, "ha_plan_exists_for");
            ha_allow_overcommit = Marshalling.ParseBool(table, "ha_allow_overcommit");
            ha_overcommitted = Marshalling.ParseBool(table, "ha_overcommitted");
            blobs = Maps.convert_from_proxy_string_XenRefBlob(Marshalling.ParseHashTable(table, "blobs"));
            tags = Marshalling.ParseStringArray(table, "tags");
            gui_config = Maps.convert_from_proxy_string_string(Marshalling.ParseHashTable(table, "gui_config"));
            wlb_url = Marshalling.ParseString(table, "wlb_url");
            wlb_username = Marshalling.ParseString(table, "wlb_username");
            wlb_enabled = Marshalling.ParseBool(table, "wlb_enabled");
            wlb_verify_cert = Marshalling.ParseBool(table, "wlb_verify_cert");
            redo_log_enabled = Marshalling.ParseBool(table, "redo_log_enabled");
            redo_log_vdi = Marshalling.ParseRef<VDI>(table, "redo_log_vdi");
            vswitch_controller = Marshalling.ParseString(table, "vswitch_controller");
            restrictions = Maps.convert_from_proxy_string_string(Marshalling.ParseHashTable(table, "restrictions"));
            metadata_VDIs = Marshalling.ParseSetRef<VDI>(table, "metadata_VDIs");
        }

        public bool DeepEquals(Pool other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Helper.AreEqual2(this._uuid, other._uuid) &&
                Helper.AreEqual2(this._name_label, other._name_label) &&
                Helper.AreEqual2(this._name_description, other._name_description) &&
                Helper.AreEqual2(this._master, other._master) &&
                Helper.AreEqual2(this._default_SR, other._default_SR) &&
                Helper.AreEqual2(this._suspend_image_SR, other._suspend_image_SR) &&
                Helper.AreEqual2(this._crash_dump_SR, other._crash_dump_SR) &&
                Helper.AreEqual2(this._other_config, other._other_config) &&
                Helper.AreEqual2(this._ha_enabled, other._ha_enabled) &&
                Helper.AreEqual2(this._ha_configuration, other._ha_configuration) &&
                Helper.AreEqual2(this._ha_statefiles, other._ha_statefiles) &&
                Helper.AreEqual2(this._ha_host_failures_to_tolerate, other._ha_host_failures_to_tolerate) &&
                Helper.AreEqual2(this._ha_plan_exists_for, other._ha_plan_exists_for) &&
                Helper.AreEqual2(this._ha_allow_overcommit, other._ha_allow_overcommit) &&
                Helper.AreEqual2(this._ha_overcommitted, other._ha_overcommitted) &&
                Helper.AreEqual2(this._blobs, other._blobs) &&
                Helper.AreEqual2(this._tags, other._tags) &&
                Helper.AreEqual2(this._gui_config, other._gui_config) &&
                Helper.AreEqual2(this._wlb_url, other._wlb_url) &&
                Helper.AreEqual2(this._wlb_username, other._wlb_username) &&
                Helper.AreEqual2(this._wlb_enabled, other._wlb_enabled) &&
                Helper.AreEqual2(this._wlb_verify_cert, other._wlb_verify_cert) &&
                Helper.AreEqual2(this._redo_log_enabled, other._redo_log_enabled) &&
                Helper.AreEqual2(this._redo_log_vdi, other._redo_log_vdi) &&
                Helper.AreEqual2(this._vswitch_controller, other._vswitch_controller) &&
                Helper.AreEqual2(this._restrictions, other._restrictions) &&
                Helper.AreEqual2(this._metadata_VDIs, other._metadata_VDIs);
        }

        public override string SaveChanges(Session session, string opaqueRef, Pool server)
        {
            if (opaqueRef == null)
            {
                System.Diagnostics.Debug.Assert(false, "Cannot create instances of this type on the server");
                return "";
            }
            else
            {
                if (!Helper.AreEqual2(_name_label, server._name_label))
                {
                    Pool.set_name_label(session, opaqueRef, _name_label);
                }
                if (!Helper.AreEqual2(_name_description, server._name_description))
                {
                    Pool.set_name_description(session, opaqueRef, _name_description);
                }
                if (!Helper.AreEqual2(_default_SR, server._default_SR))
                {
                    Pool.set_default_SR(session, opaqueRef, _default_SR);
                }
                if (!Helper.AreEqual2(_suspend_image_SR, server._suspend_image_SR))
                {
                    Pool.set_suspend_image_SR(session, opaqueRef, _suspend_image_SR);
                }
                if (!Helper.AreEqual2(_crash_dump_SR, server._crash_dump_SR))
                {
                    Pool.set_crash_dump_SR(session, opaqueRef, _crash_dump_SR);
                }
                if (!Helper.AreEqual2(_other_config, server._other_config))
                {
                    Pool.set_other_config(session, opaqueRef, _other_config);
                }
                if (!Helper.AreEqual2(_ha_allow_overcommit, server._ha_allow_overcommit))
                {
                    Pool.set_ha_allow_overcommit(session, opaqueRef, _ha_allow_overcommit);
                }
                if (!Helper.AreEqual2(_tags, server._tags))
                {
                    Pool.set_tags(session, opaqueRef, _tags);
                }
                if (!Helper.AreEqual2(_gui_config, server._gui_config))
                {
                    Pool.set_gui_config(session, opaqueRef, _gui_config);
                }
                if (!Helper.AreEqual2(_wlb_enabled, server._wlb_enabled))
                {
                    Pool.set_wlb_enabled(session, opaqueRef, _wlb_enabled);
                }
                if (!Helper.AreEqual2(_wlb_verify_cert, server._wlb_verify_cert))
                {
                    Pool.set_wlb_verify_cert(session, opaqueRef, _wlb_verify_cert);
                }

                return null;
            }
        }

        public static Pool get_record(Session session, string _pool)
        {
            return new Pool((Proxy_Pool)session.proxy.pool_get_record(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static XenRef<Pool> get_by_uuid(Session session, string _uuid)
        {
            return XenRef<Pool>.Create(session.proxy.pool_get_by_uuid(session.uuid, (_uuid != null) ? _uuid : "").parse());
        }

        public static string get_uuid(Session session, string _pool)
        {
            return (string)session.proxy.pool_get_uuid(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static string get_name_label(Session session, string _pool)
        {
            return (string)session.proxy.pool_get_name_label(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static string get_name_description(Session session, string _pool)
        {
            return (string)session.proxy.pool_get_name_description(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static XenRef<Host> get_master(Session session, string _pool)
        {
            return XenRef<Host>.Create(session.proxy.pool_get_master(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static XenRef<SR> get_default_SR(Session session, string _pool)
        {
            return XenRef<SR>.Create(session.proxy.pool_get_default_sr(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static XenRef<SR> get_suspend_image_SR(Session session, string _pool)
        {
            return XenRef<SR>.Create(session.proxy.pool_get_suspend_image_sr(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static XenRef<SR> get_crash_dump_SR(Session session, string _pool)
        {
            return XenRef<SR>.Create(session.proxy.pool_get_crash_dump_sr(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static Dictionary<string, string> get_other_config(Session session, string _pool)
        {
            return Maps.convert_from_proxy_string_string(session.proxy.pool_get_other_config(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static bool get_ha_enabled(Session session, string _pool)
        {
            return (bool)session.proxy.pool_get_ha_enabled(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static Dictionary<string, string> get_ha_configuration(Session session, string _pool)
        {
            return Maps.convert_from_proxy_string_string(session.proxy.pool_get_ha_configuration(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static string[] get_ha_statefiles(Session session, string _pool)
        {
            return (string [])session.proxy.pool_get_ha_statefiles(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static long get_ha_host_failures_to_tolerate(Session session, string _pool)
        {
            return long.Parse((string)session.proxy.pool_get_ha_host_failures_to_tolerate(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static long get_ha_plan_exists_for(Session session, string _pool)
        {
            return long.Parse((string)session.proxy.pool_get_ha_plan_exists_for(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static bool get_ha_allow_overcommit(Session session, string _pool)
        {
            return (bool)session.proxy.pool_get_ha_allow_overcommit(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static bool get_ha_overcommitted(Session session, string _pool)
        {
            return (bool)session.proxy.pool_get_ha_overcommitted(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static Dictionary<string, XenRef<Blob>> get_blobs(Session session, string _pool)
        {
            return Maps.convert_from_proxy_string_XenRefBlob(session.proxy.pool_get_blobs(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static string[] get_tags(Session session, string _pool)
        {
            return (string [])session.proxy.pool_get_tags(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static Dictionary<string, string> get_gui_config(Session session, string _pool)
        {
            return Maps.convert_from_proxy_string_string(session.proxy.pool_get_gui_config(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static string get_wlb_url(Session session, string _pool)
        {
            return (string)session.proxy.pool_get_wlb_url(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static string get_wlb_username(Session session, string _pool)
        {
            return (string)session.proxy.pool_get_wlb_username(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static bool get_wlb_enabled(Session session, string _pool)
        {
            return (bool)session.proxy.pool_get_wlb_enabled(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static bool get_wlb_verify_cert(Session session, string _pool)
        {
            return (bool)session.proxy.pool_get_wlb_verify_cert(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static bool get_redo_log_enabled(Session session, string _pool)
        {
            return (bool)session.proxy.pool_get_redo_log_enabled(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static XenRef<VDI> get_redo_log_vdi(Session session, string _pool)
        {
            return XenRef<VDI>.Create(session.proxy.pool_get_redo_log_vdi(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static string get_vswitch_controller(Session session, string _pool)
        {
            return (string)session.proxy.pool_get_vswitch_controller(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static Dictionary<string, string> get_restrictions(Session session, string _pool)
        {
            return Maps.convert_from_proxy_string_string(session.proxy.pool_get_restrictions(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static List<XenRef<VDI>> get_metadata_VDIs(Session session, string _pool)
        {
            return XenRef<VDI>.Create(session.proxy.pool_get_metadata_vdis(session.uuid, (_pool != null) ? _pool : "").parse());
        }

        public static void set_name_label(Session session, string _pool, string _name_label)
        {
            session.proxy.pool_set_name_label(session.uuid, (_pool != null) ? _pool : "", (_name_label != null) ? _name_label : "").parse();
        }

        public static void set_name_description(Session session, string _pool, string _name_description)
        {
            session.proxy.pool_set_name_description(session.uuid, (_pool != null) ? _pool : "", (_name_description != null) ? _name_description : "").parse();
        }

        public static void set_default_SR(Session session, string _pool, string _default_sr)
        {
            session.proxy.pool_set_default_sr(session.uuid, (_pool != null) ? _pool : "", (_default_sr != null) ? _default_sr : "").parse();
        }

        public static void set_suspend_image_SR(Session session, string _pool, string _suspend_image_sr)
        {
            session.proxy.pool_set_suspend_image_sr(session.uuid, (_pool != null) ? _pool : "", (_suspend_image_sr != null) ? _suspend_image_sr : "").parse();
        }

        public static void set_crash_dump_SR(Session session, string _pool, string _crash_dump_sr)
        {
            session.proxy.pool_set_crash_dump_sr(session.uuid, (_pool != null) ? _pool : "", (_crash_dump_sr != null) ? _crash_dump_sr : "").parse();
        }

        public static void set_other_config(Session session, string _pool, Dictionary<string, string> _other_config)
        {
            session.proxy.pool_set_other_config(session.uuid, (_pool != null) ? _pool : "", Maps.convert_to_proxy_string_string(_other_config)).parse();
        }

        public static void add_to_other_config(Session session, string _pool, string _key, string _value)
        {
            session.proxy.pool_add_to_other_config(session.uuid, (_pool != null) ? _pool : "", (_key != null) ? _key : "", (_value != null) ? _value : "").parse();
        }

        public static void remove_from_other_config(Session session, string _pool, string _key)
        {
            session.proxy.pool_remove_from_other_config(session.uuid, (_pool != null) ? _pool : "", (_key != null) ? _key : "").parse();
        }

        public static void set_ha_allow_overcommit(Session session, string _pool, bool _ha_allow_overcommit)
        {
            session.proxy.pool_set_ha_allow_overcommit(session.uuid, (_pool != null) ? _pool : "", _ha_allow_overcommit).parse();
        }

        public static void set_tags(Session session, string _pool, string[] _tags)
        {
            session.proxy.pool_set_tags(session.uuid, (_pool != null) ? _pool : "", _tags).parse();
        }

        public static void add_tags(Session session, string _pool, string _value)
        {
            session.proxy.pool_add_tags(session.uuid, (_pool != null) ? _pool : "", (_value != null) ? _value : "").parse();
        }

        public static void remove_tags(Session session, string _pool, string _value)
        {
            session.proxy.pool_remove_tags(session.uuid, (_pool != null) ? _pool : "", (_value != null) ? _value : "").parse();
        }

        public static void set_gui_config(Session session, string _pool, Dictionary<string, string> _gui_config)
        {
            session.proxy.pool_set_gui_config(session.uuid, (_pool != null) ? _pool : "", Maps.convert_to_proxy_string_string(_gui_config)).parse();
        }

        public static void add_to_gui_config(Session session, string _pool, string _key, string _value)
        {
            session.proxy.pool_add_to_gui_config(session.uuid, (_pool != null) ? _pool : "", (_key != null) ? _key : "", (_value != null) ? _value : "").parse();
        }

        public static void remove_from_gui_config(Session session, string _pool, string _key)
        {
            session.proxy.pool_remove_from_gui_config(session.uuid, (_pool != null) ? _pool : "", (_key != null) ? _key : "").parse();
        }

        public static void set_wlb_enabled(Session session, string _pool, bool _wlb_enabled)
        {
            session.proxy.pool_set_wlb_enabled(session.uuid, (_pool != null) ? _pool : "", _wlb_enabled).parse();
        }

        public static void set_wlb_verify_cert(Session session, string _pool, bool _wlb_verify_cert)
        {
            session.proxy.pool_set_wlb_verify_cert(session.uuid, (_pool != null) ? _pool : "", _wlb_verify_cert).parse();
        }

        public static void join(Session session, string _master_address, string _master_username, string _master_password)
        {
            session.proxy.pool_join(session.uuid, (_master_address != null) ? _master_address : "", (_master_username != null) ? _master_username : "", (_master_password != null) ? _master_password : "").parse();
        }

        public static XenRef<Task> async_join(Session session, string _master_address, string _master_username, string _master_password)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_join(session.uuid, (_master_address != null) ? _master_address : "", (_master_username != null) ? _master_username : "", (_master_password != null) ? _master_password : "").parse());
        }

        public static void join_force(Session session, string _master_address, string _master_username, string _master_password)
        {
            session.proxy.pool_join_force(session.uuid, (_master_address != null) ? _master_address : "", (_master_username != null) ? _master_username : "", (_master_password != null) ? _master_password : "").parse();
        }

        public static XenRef<Task> async_join_force(Session session, string _master_address, string _master_username, string _master_password)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_join_force(session.uuid, (_master_address != null) ? _master_address : "", (_master_username != null) ? _master_username : "", (_master_password != null) ? _master_password : "").parse());
        }

        public static void eject(Session session, string _host)
        {
            session.proxy.pool_eject(session.uuid, (_host != null) ? _host : "").parse();
        }

        public static XenRef<Task> async_eject(Session session, string _host)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_eject(session.uuid, (_host != null) ? _host : "").parse());
        }

        public static void emergency_transition_to_master(Session session)
        {
            session.proxy.pool_emergency_transition_to_master(session.uuid).parse();
        }

        public static void emergency_reset_master(Session session, string _master_address)
        {
            session.proxy.pool_emergency_reset_master(session.uuid, (_master_address != null) ? _master_address : "").parse();
        }

        public static List<XenRef<Host>> recover_slaves(Session session)
        {
            return XenRef<Host>.Create(session.proxy.pool_recover_slaves(session.uuid).parse());
        }

        public static XenRef<Task> async_recover_slaves(Session session)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_recover_slaves(session.uuid).parse());
        }

        public static List<XenRef<PIF>> create_VLAN(Session session, string _device, string _network, long _vlan)
        {
            return XenRef<PIF>.Create(session.proxy.pool_create_vlan(session.uuid, (_device != null) ? _device : "", (_network != null) ? _network : "", _vlan.ToString()).parse());
        }

        public static XenRef<Task> async_create_VLAN(Session session, string _device, string _network, long _vlan)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_create_vlan(session.uuid, (_device != null) ? _device : "", (_network != null) ? _network : "", _vlan.ToString()).parse());
        }

        public static List<XenRef<PIF>> create_VLAN_from_PIF(Session session, string _pif, string _network, long _vlan)
        {
            return XenRef<PIF>.Create(session.proxy.pool_create_vlan_from_pif(session.uuid, (_pif != null) ? _pif : "", (_network != null) ? _network : "", _vlan.ToString()).parse());
        }

        public static XenRef<Task> async_create_VLAN_from_PIF(Session session, string _pif, string _network, long _vlan)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_create_vlan_from_pif(session.uuid, (_pif != null) ? _pif : "", (_network != null) ? _network : "", _vlan.ToString()).parse());
        }

        public static void enable_ha(Session session, List<XenRef<SR>> _heartbeat_srs, Dictionary<string, string> _configuration)
        {
            session.proxy.pool_enable_ha(session.uuid, (_heartbeat_srs != null) ? Helper.RefListToStringArray(_heartbeat_srs) : new string[] {}, Maps.convert_to_proxy_string_string(_configuration)).parse();
        }

        public static XenRef<Task> async_enable_ha(Session session, List<XenRef<SR>> _heartbeat_srs, Dictionary<string, string> _configuration)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_enable_ha(session.uuid, (_heartbeat_srs != null) ? Helper.RefListToStringArray(_heartbeat_srs) : new string[] {}, Maps.convert_to_proxy_string_string(_configuration)).parse());
        }

        public static void disable_ha(Session session)
        {
            session.proxy.pool_disable_ha(session.uuid).parse();
        }

        public static XenRef<Task> async_disable_ha(Session session)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_disable_ha(session.uuid).parse());
        }

        public static void sync_database(Session session)
        {
            session.proxy.pool_sync_database(session.uuid).parse();
        }

        public static XenRef<Task> async_sync_database(Session session)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_sync_database(session.uuid).parse());
        }

        public static void designate_new_master(Session session, string _host)
        {
            session.proxy.pool_designate_new_master(session.uuid, (_host != null) ? _host : "").parse();
        }

        public static XenRef<Task> async_designate_new_master(Session session, string _host)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_designate_new_master(session.uuid, (_host != null) ? _host : "").parse());
        }

        public static void ha_prevent_restarts_for(Session session, long _seconds)
        {
            session.proxy.pool_ha_prevent_restarts_for(session.uuid, _seconds.ToString()).parse();
        }

        public static bool ha_failover_plan_exists(Session session, long _n)
        {
            return (bool)session.proxy.pool_ha_failover_plan_exists(session.uuid, _n.ToString()).parse();
        }

        public static long ha_compute_max_host_failures_to_tolerate(Session session)
        {
            return long.Parse((string)session.proxy.pool_ha_compute_max_host_failures_to_tolerate(session.uuid).parse());
        }

        public static long ha_compute_hypothetical_max_host_failures_to_tolerate(Session session, Dictionary<XenRef<VM>, string> _configuration)
        {
            return long.Parse((string)session.proxy.pool_ha_compute_hypothetical_max_host_failures_to_tolerate(session.uuid, Maps.convert_to_proxy_XenRefVM_string(_configuration)).parse());
        }

        public static Dictionary<XenRef<VM>, Dictionary<string, string>> ha_compute_vm_failover_plan(Session session, List<XenRef<Host>> _failed_hosts, List<XenRef<VM>> _failed_vms)
        {
            return Maps.convert_from_proxy_XenRefVM_Dictionary_string_string(session.proxy.pool_ha_compute_vm_failover_plan(session.uuid, (_failed_hosts != null) ? Helper.RefListToStringArray(_failed_hosts) : new string[] {}, (_failed_vms != null) ? Helper.RefListToStringArray(_failed_vms) : new string[] {}).parse());
        }

        public static void set_ha_host_failures_to_tolerate(Session session, string _self, long _value)
        {
            session.proxy.pool_set_ha_host_failures_to_tolerate(session.uuid, (_self != null) ? _self : "", _value.ToString()).parse();
        }

        public static XenRef<Task> async_set_ha_host_failures_to_tolerate(Session session, string _self, long _value)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_set_ha_host_failures_to_tolerate(session.uuid, (_self != null) ? _self : "", _value.ToString()).parse());
        }

        public static XenRef<Blob> create_new_blob(Session session, string _pool, string _name, string _mime_type, bool _public)
        {
            return XenRef<Blob>.Create(session.proxy.pool_create_new_blob(session.uuid, (_pool != null) ? _pool : "", (_name != null) ? _name : "", (_mime_type != null) ? _mime_type : "", _public).parse());
        }

        public static XenRef<Task> async_create_new_blob(Session session, string _pool, string _name, string _mime_type, bool _public)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_create_new_blob(session.uuid, (_pool != null) ? _pool : "", (_name != null) ? _name : "", (_mime_type != null) ? _mime_type : "", _public).parse());
        }

        public static void enable_external_auth(Session session, string _pool, Dictionary<string, string> _config, string _service_name, string _auth_type)
        {
            session.proxy.pool_enable_external_auth(session.uuid, (_pool != null) ? _pool : "", Maps.convert_to_proxy_string_string(_config), (_service_name != null) ? _service_name : "", (_auth_type != null) ? _auth_type : "").parse();
        }

        public static void disable_external_auth(Session session, string _pool, Dictionary<string, string> _config)
        {
            session.proxy.pool_disable_external_auth(session.uuid, (_pool != null) ? _pool : "", Maps.convert_to_proxy_string_string(_config)).parse();
        }

        public static void detect_nonhomogeneous_external_auth(Session session, string _pool)
        {
            session.proxy.pool_detect_nonhomogeneous_external_auth(session.uuid, (_pool != null) ? _pool : "").parse();
        }

        public static void initialize_wlb(Session session, string _wlb_url, string _wlb_username, string _wlb_password, string _xenserver_username, string _xenserver_password)
        {
            session.proxy.pool_initialize_wlb(session.uuid, (_wlb_url != null) ? _wlb_url : "", (_wlb_username != null) ? _wlb_username : "", (_wlb_password != null) ? _wlb_password : "", (_xenserver_username != null) ? _xenserver_username : "", (_xenserver_password != null) ? _xenserver_password : "").parse();
        }

        public static XenRef<Task> async_initialize_wlb(Session session, string _wlb_url, string _wlb_username, string _wlb_password, string _xenserver_username, string _xenserver_password)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_initialize_wlb(session.uuid, (_wlb_url != null) ? _wlb_url : "", (_wlb_username != null) ? _wlb_username : "", (_wlb_password != null) ? _wlb_password : "", (_xenserver_username != null) ? _xenserver_username : "", (_xenserver_password != null) ? _xenserver_password : "").parse());
        }

        public static void deconfigure_wlb(Session session)
        {
            session.proxy.pool_deconfigure_wlb(session.uuid).parse();
        }

        public static XenRef<Task> async_deconfigure_wlb(Session session)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_deconfigure_wlb(session.uuid).parse());
        }

        public static void send_wlb_configuration(Session session, Dictionary<string, string> _config)
        {
            session.proxy.pool_send_wlb_configuration(session.uuid, Maps.convert_to_proxy_string_string(_config)).parse();
        }

        public static XenRef<Task> async_send_wlb_configuration(Session session, Dictionary<string, string> _config)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_send_wlb_configuration(session.uuid, Maps.convert_to_proxy_string_string(_config)).parse());
        }

        public static Dictionary<string, string> retrieve_wlb_configuration(Session session)
        {
            return Maps.convert_from_proxy_string_string(session.proxy.pool_retrieve_wlb_configuration(session.uuid).parse());
        }

        public static XenRef<Task> async_retrieve_wlb_configuration(Session session)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_retrieve_wlb_configuration(session.uuid).parse());
        }

        public static Dictionary<XenRef<VM>, string[]> retrieve_wlb_recommendations(Session session)
        {
            return Maps.convert_from_proxy_XenRefVM_string_array(session.proxy.pool_retrieve_wlb_recommendations(session.uuid).parse());
        }

        public static XenRef<Task> async_retrieve_wlb_recommendations(Session session)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_retrieve_wlb_recommendations(session.uuid).parse());
        }

        public static string send_test_post(Session session, string _host, long _port, string _body)
        {
            return (string)session.proxy.pool_send_test_post(session.uuid, (_host != null) ? _host : "", _port.ToString(), (_body != null) ? _body : "").parse();
        }

        public static XenRef<Task> async_send_test_post(Session session, string _host, long _port, string _body)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_send_test_post(session.uuid, (_host != null) ? _host : "", _port.ToString(), (_body != null) ? _body : "").parse());
        }

        public static void certificate_install(Session session, string _name, string _cert)
        {
            session.proxy.pool_certificate_install(session.uuid, (_name != null) ? _name : "", (_cert != null) ? _cert : "").parse();
        }

        public static XenRef<Task> async_certificate_install(Session session, string _name, string _cert)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_certificate_install(session.uuid, (_name != null) ? _name : "", (_cert != null) ? _cert : "").parse());
        }

        public static void certificate_uninstall(Session session, string _name)
        {
            session.proxy.pool_certificate_uninstall(session.uuid, (_name != null) ? _name : "").parse();
        }

        public static XenRef<Task> async_certificate_uninstall(Session session, string _name)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_certificate_uninstall(session.uuid, (_name != null) ? _name : "").parse());
        }

        public static string[] certificate_list(Session session)
        {
            return (string [])session.proxy.pool_certificate_list(session.uuid).parse();
        }

        public static XenRef<Task> async_certificate_list(Session session)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_certificate_list(session.uuid).parse());
        }

        public static void crl_install(Session session, string _name, string _cert)
        {
            session.proxy.pool_crl_install(session.uuid, (_name != null) ? _name : "", (_cert != null) ? _cert : "").parse();
        }

        public static XenRef<Task> async_crl_install(Session session, string _name, string _cert)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_crl_install(session.uuid, (_name != null) ? _name : "", (_cert != null) ? _cert : "").parse());
        }

        public static void crl_uninstall(Session session, string _name)
        {
            session.proxy.pool_crl_uninstall(session.uuid, (_name != null) ? _name : "").parse();
        }

        public static XenRef<Task> async_crl_uninstall(Session session, string _name)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_crl_uninstall(session.uuid, (_name != null) ? _name : "").parse());
        }

        public static string[] crl_list(Session session)
        {
            return (string [])session.proxy.pool_crl_list(session.uuid).parse();
        }

        public static XenRef<Task> async_crl_list(Session session)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_crl_list(session.uuid).parse());
        }

        public static void certificate_sync(Session session)
        {
            session.proxy.pool_certificate_sync(session.uuid).parse();
        }

        public static XenRef<Task> async_certificate_sync(Session session)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_certificate_sync(session.uuid).parse());
        }

        public static void enable_redo_log(Session session, string _sr)
        {
            session.proxy.pool_enable_redo_log(session.uuid, (_sr != null) ? _sr : "").parse();
        }

        public static XenRef<Task> async_enable_redo_log(Session session, string _sr)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_enable_redo_log(session.uuid, (_sr != null) ? _sr : "").parse());
        }

        public static void disable_redo_log(Session session)
        {
            session.proxy.pool_disable_redo_log(session.uuid).parse();
        }

        public static XenRef<Task> async_disable_redo_log(Session session)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_disable_redo_log(session.uuid).parse());
        }

        public static void set_vswitch_controller(Session session, string _address)
        {
            session.proxy.pool_set_vswitch_controller(session.uuid, (_address != null) ? _address : "").parse();
        }

        public static XenRef<Task> async_set_vswitch_controller(Session session, string _address)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_set_vswitch_controller(session.uuid, (_address != null) ? _address : "").parse());
        }

        public static string test_archive_target(Session session, string _self, Dictionary<string, string> _config)
        {
            return (string)session.proxy.pool_test_archive_target(session.uuid, (_self != null) ? _self : "", Maps.convert_to_proxy_string_string(_config)).parse();
        }

        public static void enable_local_storage_caching(Session session, string _self)
        {
            session.proxy.pool_enable_local_storage_caching(session.uuid, (_self != null) ? _self : "").parse();
        }

        public static XenRef<Task> async_enable_local_storage_caching(Session session, string _self)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_enable_local_storage_caching(session.uuid, (_self != null) ? _self : "").parse());
        }

        public static void disable_local_storage_caching(Session session, string _self)
        {
            session.proxy.pool_disable_local_storage_caching(session.uuid, (_self != null) ? _self : "").parse();
        }

        public static XenRef<Task> async_disable_local_storage_caching(Session session, string _self)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_disable_local_storage_caching(session.uuid, (_self != null) ? _self : "").parse());
        }

        public static Dictionary<string, string> get_license_state(Session session, string _self)
        {
            return Maps.convert_from_proxy_string_string(session.proxy.pool_get_license_state(session.uuid, (_self != null) ? _self : "").parse());
        }

        public static XenRef<Task> async_get_license_state(Session session, string _self)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_get_license_state(session.uuid, (_self != null) ? _self : "").parse());
        }

        public static void apply_edition(Session session, string _self, string _edition)
        {
            session.proxy.pool_apply_edition(session.uuid, (_self != null) ? _self : "", (_edition != null) ? _edition : "").parse();
        }

        public static XenRef<Task> async_apply_edition(Session session, string _self, string _edition)
        {
            return XenRef<Task>.Create(session.proxy.async_pool_apply_edition(session.uuid, (_self != null) ? _self : "", (_edition != null) ? _edition : "").parse());
        }

        public static List<XenRef<Pool>> get_all(Session session)
        {
            return XenRef<Pool>.Create(session.proxy.pool_get_all(session.uuid).parse());
        }

        public static Dictionary<XenRef<Pool>, Pool> get_all_records(Session session)
        {
            return XenRef<Pool>.Create<Proxy_Pool>(session.proxy.pool_get_all_records(session.uuid).parse());
        }

        private string _uuid;
        public virtual string uuid {
             get { return _uuid; }
             set { if (!Helper.AreEqual(value, _uuid)) { _uuid = value; Changed = true; NotifyPropertyChanged("uuid"); } }
         }

        private string _name_label;
        public virtual string name_label {
             get { return _name_label; }
             set { if (!Helper.AreEqual(value, _name_label)) { _name_label = value; Changed = true; NotifyPropertyChanged("name_label"); } }
         }

        private string _name_description;
        public virtual string name_description {
             get { return _name_description; }
             set { if (!Helper.AreEqual(value, _name_description)) { _name_description = value; Changed = true; NotifyPropertyChanged("name_description"); } }
         }

        private XenRef<Host> _master;
        public virtual XenRef<Host> master {
             get { return _master; }
             set { if (!Helper.AreEqual(value, _master)) { _master = value; Changed = true; NotifyPropertyChanged("master"); } }
         }

        private XenRef<SR> _default_SR;
        public virtual XenRef<SR> default_SR {
             get { return _default_SR; }
             set { if (!Helper.AreEqual(value, _default_SR)) { _default_SR = value; Changed = true; NotifyPropertyChanged("default_SR"); } }
         }

        private XenRef<SR> _suspend_image_SR;
        public virtual XenRef<SR> suspend_image_SR {
             get { return _suspend_image_SR; }
             set { if (!Helper.AreEqual(value, _suspend_image_SR)) { _suspend_image_SR = value; Changed = true; NotifyPropertyChanged("suspend_image_SR"); } }
         }

        private XenRef<SR> _crash_dump_SR;
        public virtual XenRef<SR> crash_dump_SR {
             get { return _crash_dump_SR; }
             set { if (!Helper.AreEqual(value, _crash_dump_SR)) { _crash_dump_SR = value; Changed = true; NotifyPropertyChanged("crash_dump_SR"); } }
         }

        private Dictionary<string, string> _other_config;
        public virtual Dictionary<string, string> other_config {
             get { return _other_config; }
             set { if (!Helper.AreEqual(value, _other_config)) { _other_config = value; Changed = true; NotifyPropertyChanged("other_config"); } }
         }

        private bool _ha_enabled;
        public virtual bool ha_enabled {
             get { return _ha_enabled; }
             set { if (!Helper.AreEqual(value, _ha_enabled)) { _ha_enabled = value; Changed = true; NotifyPropertyChanged("ha_enabled"); } }
         }

        private Dictionary<string, string> _ha_configuration;
        public virtual Dictionary<string, string> ha_configuration {
             get { return _ha_configuration; }
             set { if (!Helper.AreEqual(value, _ha_configuration)) { _ha_configuration = value; Changed = true; NotifyPropertyChanged("ha_configuration"); } }
         }

        private string[] _ha_statefiles;
        public virtual string[] ha_statefiles {
             get { return _ha_statefiles; }
             set { if (!Helper.AreEqual(value, _ha_statefiles)) { _ha_statefiles = value; Changed = true; NotifyPropertyChanged("ha_statefiles"); } }
         }

        private long _ha_host_failures_to_tolerate;
        public virtual long ha_host_failures_to_tolerate {
             get { return _ha_host_failures_to_tolerate; }
             set { if (!Helper.AreEqual(value, _ha_host_failures_to_tolerate)) { _ha_host_failures_to_tolerate = value; Changed = true; NotifyPropertyChanged("ha_host_failures_to_tolerate"); } }
         }

        private long _ha_plan_exists_for;
        public virtual long ha_plan_exists_for {
             get { return _ha_plan_exists_for; }
             set { if (!Helper.AreEqual(value, _ha_plan_exists_for)) { _ha_plan_exists_for = value; Changed = true; NotifyPropertyChanged("ha_plan_exists_for"); } }
         }

        private bool _ha_allow_overcommit;
        public virtual bool ha_allow_overcommit {
             get { return _ha_allow_overcommit; }
             set { if (!Helper.AreEqual(value, _ha_allow_overcommit)) { _ha_allow_overcommit = value; Changed = true; NotifyPropertyChanged("ha_allow_overcommit"); } }
         }

        private bool _ha_overcommitted;
        public virtual bool ha_overcommitted {
             get { return _ha_overcommitted; }
             set { if (!Helper.AreEqual(value, _ha_overcommitted)) { _ha_overcommitted = value; Changed = true; NotifyPropertyChanged("ha_overcommitted"); } }
         }

        private Dictionary<string, XenRef<Blob>> _blobs;
        public virtual Dictionary<string, XenRef<Blob>> blobs {
             get { return _blobs; }
             set { if (!Helper.AreEqual(value, _blobs)) { _blobs = value; Changed = true; NotifyPropertyChanged("blobs"); } }
         }

        private string[] _tags;
        public virtual string[] tags {
             get { return _tags; }
             set { if (!Helper.AreEqual(value, _tags)) { _tags = value; Changed = true; NotifyPropertyChanged("tags"); } }
         }

        private Dictionary<string, string> _gui_config;
        public virtual Dictionary<string, string> gui_config {
             get { return _gui_config; }
             set { if (!Helper.AreEqual(value, _gui_config)) { _gui_config = value; Changed = true; NotifyPropertyChanged("gui_config"); } }
         }

        private string _wlb_url;
        public virtual string wlb_url {
             get { return _wlb_url; }
             set { if (!Helper.AreEqual(value, _wlb_url)) { _wlb_url = value; Changed = true; NotifyPropertyChanged("wlb_url"); } }
         }

        private string _wlb_username;
        public virtual string wlb_username {
             get { return _wlb_username; }
             set { if (!Helper.AreEqual(value, _wlb_username)) { _wlb_username = value; Changed = true; NotifyPropertyChanged("wlb_username"); } }
         }

        private bool _wlb_enabled;
        public virtual bool wlb_enabled {
             get { return _wlb_enabled; }
             set { if (!Helper.AreEqual(value, _wlb_enabled)) { _wlb_enabled = value; Changed = true; NotifyPropertyChanged("wlb_enabled"); } }
         }

        private bool _wlb_verify_cert;
        public virtual bool wlb_verify_cert {
             get { return _wlb_verify_cert; }
             set { if (!Helper.AreEqual(value, _wlb_verify_cert)) { _wlb_verify_cert = value; Changed = true; NotifyPropertyChanged("wlb_verify_cert"); } }
         }

        private bool _redo_log_enabled;
        public virtual bool redo_log_enabled {
             get { return _redo_log_enabled; }
             set { if (!Helper.AreEqual(value, _redo_log_enabled)) { _redo_log_enabled = value; Changed = true; NotifyPropertyChanged("redo_log_enabled"); } }
         }

        private XenRef<VDI> _redo_log_vdi;
        public virtual XenRef<VDI> redo_log_vdi {
             get { return _redo_log_vdi; }
             set { if (!Helper.AreEqual(value, _redo_log_vdi)) { _redo_log_vdi = value; Changed = true; NotifyPropertyChanged("redo_log_vdi"); } }
         }

        private string _vswitch_controller;
        public virtual string vswitch_controller {
             get { return _vswitch_controller; }
             set { if (!Helper.AreEqual(value, _vswitch_controller)) { _vswitch_controller = value; Changed = true; NotifyPropertyChanged("vswitch_controller"); } }
         }

        private Dictionary<string, string> _restrictions;
        public virtual Dictionary<string, string> restrictions {
             get { return _restrictions; }
             set { if (!Helper.AreEqual(value, _restrictions)) { _restrictions = value; Changed = true; NotifyPropertyChanged("restrictions"); } }
         }

        private List<XenRef<VDI>> _metadata_VDIs;
        public virtual List<XenRef<VDI>> metadata_VDIs {
             get { return _metadata_VDIs; }
             set { if (!Helper.AreEqual(value, _metadata_VDIs)) { _metadata_VDIs = value; Changed = true; NotifyPropertyChanged("metadata_VDIs"); } }
         }


    }
}
