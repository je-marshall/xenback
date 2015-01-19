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



#include <stddef.h>
#include <stdlib.h>

#include "xen_internal.h"
#include <xen/api/xen_common.h>
#include <xen/api/xen_gpu_group.h>
#include <xen/api/xen_pgpu.h>
#include <xen/api/xen_vgpu.h>
#include <xen/api/xen_vgpu_type.h>
#include <xen/api/xen_vgpu_type_xen_vgpu_type_record_map.h>


XEN_FREE(xen_vgpu_type)
XEN_SET_ALLOC_FREE(xen_vgpu_type)
XEN_ALLOC(xen_vgpu_type_record)
XEN_SET_ALLOC_FREE(xen_vgpu_type_record)
XEN_ALLOC(xen_vgpu_type_record_opt)
XEN_RECORD_OPT_FREE(xen_vgpu_type)
XEN_SET_ALLOC_FREE(xen_vgpu_type_record_opt)


static const struct_member xen_vgpu_type_record_struct_members[] =
    {
        { .key = "uuid",
          .type = &abstract_type_string,
          .offset = offsetof(xen_vgpu_type_record, uuid) },
        { .key = "vendor_name",
          .type = &abstract_type_string,
          .offset = offsetof(xen_vgpu_type_record, vendor_name) },
        { .key = "model_name",
          .type = &abstract_type_string,
          .offset = offsetof(xen_vgpu_type_record, model_name) },
        { .key = "framebuffer_size",
          .type = &abstract_type_int,
          .offset = offsetof(xen_vgpu_type_record, framebuffer_size) },
        { .key = "max_heads",
          .type = &abstract_type_int,
          .offset = offsetof(xen_vgpu_type_record, max_heads) },
        { .key = "max_resolution_x",
          .type = &abstract_type_int,
          .offset = offsetof(xen_vgpu_type_record, max_resolution_x) },
        { .key = "max_resolution_y",
          .type = &abstract_type_int,
          .offset = offsetof(xen_vgpu_type_record, max_resolution_y) },
        { .key = "supported_on_PGPUs",
          .type = &abstract_type_ref_set,
          .offset = offsetof(xen_vgpu_type_record, supported_on_pgpus) },
        { .key = "enabled_on_PGPUs",
          .type = &abstract_type_ref_set,
          .offset = offsetof(xen_vgpu_type_record, enabled_on_pgpus) },
        { .key = "VGPUs",
          .type = &abstract_type_ref_set,
          .offset = offsetof(xen_vgpu_type_record, vgpus) },
        { .key = "supported_on_GPU_groups",
          .type = &abstract_type_ref_set,
          .offset = offsetof(xen_vgpu_type_record, supported_on_gpu_groups) },
        { .key = "enabled_on_GPU_groups",
          .type = &abstract_type_ref_set,
          .offset = offsetof(xen_vgpu_type_record, enabled_on_gpu_groups) }
    };

const abstract_type xen_vgpu_type_record_abstract_type_ =
    {
       .typename = STRUCT,
       .struct_size = sizeof(xen_vgpu_type_record),
       .member_count =
           sizeof(xen_vgpu_type_record_struct_members) / sizeof(struct_member),
       .members = xen_vgpu_type_record_struct_members
    };


static const struct struct_member xen_vgpu_type_xen_vgpu_type_record_members[] =
{
    {
        .type = &abstract_type_string,
        .offset = offsetof(xen_vgpu_type_xen_vgpu_type_record_map_contents, key)
    },
    {
        .type = &xen_vgpu_type_record_abstract_type_,
        .offset = offsetof(xen_vgpu_type_xen_vgpu_type_record_map_contents, val)
    }
};

const abstract_type abstract_type_string_xen_vgpu_type_record_map =
{
    .typename = MAP,
    .struct_size = sizeof(xen_vgpu_type_xen_vgpu_type_record_map_contents),
    .members = xen_vgpu_type_xen_vgpu_type_record_members
};


void
xen_vgpu_type_record_free(xen_vgpu_type_record *record)
{
    if (record == NULL)
    {
        return;
    }
    free(record->handle);
    free(record->uuid);
    free(record->vendor_name);
    free(record->model_name);
    xen_pgpu_record_opt_set_free(record->supported_on_pgpus);
    xen_pgpu_record_opt_set_free(record->enabled_on_pgpus);
    xen_vgpu_record_opt_set_free(record->vgpus);
    xen_gpu_group_record_opt_set_free(record->supported_on_gpu_groups);
    xen_gpu_group_record_opt_set_free(record->enabled_on_gpu_groups);
    free(record);
}


bool
xen_vgpu_type_get_record(xen_session *session, xen_vgpu_type_record **result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = xen_vgpu_type_record_abstract_type_;

    *result = NULL;
    XEN_CALL_("VGPU_type.get_record");

    if (session->ok)
    {
       (*result)->handle = xen_strdup_((*result)->uuid);
    }

    return session->ok;
}


bool
xen_vgpu_type_get_by_uuid(xen_session *session, xen_vgpu_type *result, char *uuid)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = uuid }
        };

    abstract_type result_type = abstract_type_string;

    *result = NULL;
    XEN_CALL_("VGPU_type.get_by_uuid");
    return session->ok;
}


bool
xen_vgpu_type_get_vendor_name(xen_session *session, char **result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_string;

    *result = NULL;
    XEN_CALL_("VGPU_type.get_vendor_name");
    return session->ok;
}


bool
xen_vgpu_type_get_model_name(xen_session *session, char **result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_string;

    *result = NULL;
    XEN_CALL_("VGPU_type.get_model_name");
    return session->ok;
}


bool
xen_vgpu_type_get_framebuffer_size(xen_session *session, int64_t *result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_int;

    XEN_CALL_("VGPU_type.get_framebuffer_size");
    return session->ok;
}


bool
xen_vgpu_type_get_max_heads(xen_session *session, int64_t *result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_int;

    XEN_CALL_("VGPU_type.get_max_heads");
    return session->ok;
}


bool
xen_vgpu_type_get_max_resolution_x(xen_session *session, int64_t *result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_int;

    XEN_CALL_("VGPU_type.get_max_resolution_x");
    return session->ok;
}


bool
xen_vgpu_type_get_max_resolution_y(xen_session *session, int64_t *result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_int;

    XEN_CALL_("VGPU_type.get_max_resolution_y");
    return session->ok;
}


bool
xen_vgpu_type_get_supported_on_pgpus(xen_session *session, struct xen_pgpu_set **result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_string_set;

    *result = NULL;
    XEN_CALL_("VGPU_type.get_supported_on_PGPUs");
    return session->ok;
}


bool
xen_vgpu_type_get_enabled_on_pgpus(xen_session *session, struct xen_pgpu_set **result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_string_set;

    *result = NULL;
    XEN_CALL_("VGPU_type.get_enabled_on_PGPUs");
    return session->ok;
}


bool
xen_vgpu_type_get_vgpus(xen_session *session, struct xen_vgpu_set **result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_string_set;

    *result = NULL;
    XEN_CALL_("VGPU_type.get_VGPUs");
    return session->ok;
}


bool
xen_vgpu_type_get_supported_on_gpu_groups(xen_session *session, struct xen_gpu_group_set **result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_string_set;

    *result = NULL;
    XEN_CALL_("VGPU_type.get_supported_on_GPU_groups");
    return session->ok;
}


bool
xen_vgpu_type_get_enabled_on_gpu_groups(xen_session *session, struct xen_gpu_group_set **result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_string_set;

    *result = NULL;
    XEN_CALL_("VGPU_type.get_enabled_on_GPU_groups");
    return session->ok;
}


bool
xen_vgpu_type_get_all(xen_session *session, struct xen_vgpu_type_set **result)
{

    abstract_type result_type = abstract_type_string_set;

    *result = NULL;
    xen_call_(session, "VGPU_type.get_all", NULL, 0, &result_type, result);
    return session->ok;
}


bool
xen_vgpu_type_get_all_records(xen_session *session, xen_vgpu_type_xen_vgpu_type_record_map **result)
{

    abstract_type result_type = abstract_type_string_xen_vgpu_type_record_map;

    *result = NULL;
    xen_call_(session, "VGPU_type.get_all_records", NULL, 0, &result_type, result);
    return session->ok;
}


bool
xen_vgpu_type_get_uuid(xen_session *session, char **result, xen_vgpu_type vgpu_type)
{
    abstract_value param_values[] =
        {
            { .type = &abstract_type_string,
              .u.string_val = vgpu_type }
        };

    abstract_type result_type = abstract_type_string;

    *result = NULL;
    XEN_CALL_("VGPU_type.get_uuid");
    return session->ok;
}

