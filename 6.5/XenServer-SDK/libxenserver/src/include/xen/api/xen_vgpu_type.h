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


#ifndef XEN_VGPU_TYPE_H
#define XEN_VGPU_TYPE_H

#include <xen/api/xen_common.h>
#include <xen/api/xen_gpu_group_decl.h>
#include <xen/api/xen_pgpu_decl.h>
#include <xen/api/xen_vgpu_decl.h>
#include <xen/api/xen_vgpu_type_decl.h>
#include <xen/api/xen_vgpu_type_xen_vgpu_type_record_map.h>


/*
 * The VGPU_type class.
 * 
 * A type of virtual GPU.
 */


/**
 * Free the given xen_vgpu_type.  The given handle must have been
 * allocated by this library.
 */
extern void
xen_vgpu_type_free(xen_vgpu_type vgpu_type);


typedef struct xen_vgpu_type_set
{
    size_t size;
    xen_vgpu_type *contents[];
} xen_vgpu_type_set;

/**
 * Allocate a xen_vgpu_type_set of the given size.
 */
extern xen_vgpu_type_set *
xen_vgpu_type_set_alloc(size_t size);

/**
 * Free the given xen_vgpu_type_set.  The given set must have been
 * allocated by this library.
 */
extern void
xen_vgpu_type_set_free(xen_vgpu_type_set *set);


typedef struct xen_vgpu_type_record
{
    xen_vgpu_type handle;
    char *uuid;
    char *vendor_name;
    char *model_name;
    int64_t framebuffer_size;
    int64_t max_heads;
    int64_t max_resolution_x;
    int64_t max_resolution_y;
    struct xen_pgpu_record_opt_set *supported_on_pgpus;
    struct xen_pgpu_record_opt_set *enabled_on_pgpus;
    struct xen_vgpu_record_opt_set *vgpus;
    struct xen_gpu_group_record_opt_set *supported_on_gpu_groups;
    struct xen_gpu_group_record_opt_set *enabled_on_gpu_groups;
} xen_vgpu_type_record;

/**
 * Allocate a xen_vgpu_type_record.
 */
extern xen_vgpu_type_record *
xen_vgpu_type_record_alloc(void);

/**
 * Free the given xen_vgpu_type_record, and all referenced values.  The
 * given record must have been allocated by this library.
 */
extern void
xen_vgpu_type_record_free(xen_vgpu_type_record *record);


typedef struct xen_vgpu_type_record_opt
{
    bool is_record;
    union
    {
        xen_vgpu_type handle;
        xen_vgpu_type_record *record;
    } u;
} xen_vgpu_type_record_opt;

/**
 * Allocate a xen_vgpu_type_record_opt.
 */
extern xen_vgpu_type_record_opt *
xen_vgpu_type_record_opt_alloc(void);

/**
 * Free the given xen_vgpu_type_record_opt, and all referenced values. 
 * The given record_opt must have been allocated by this library.
 */
extern void
xen_vgpu_type_record_opt_free(xen_vgpu_type_record_opt *record_opt);


typedef struct xen_vgpu_type_record_set
{
    size_t size;
    xen_vgpu_type_record *contents[];
} xen_vgpu_type_record_set;

/**
 * Allocate a xen_vgpu_type_record_set of the given size.
 */
extern xen_vgpu_type_record_set *
xen_vgpu_type_record_set_alloc(size_t size);

/**
 * Free the given xen_vgpu_type_record_set, and all referenced values. 
 * The given set must have been allocated by this library.
 */
extern void
xen_vgpu_type_record_set_free(xen_vgpu_type_record_set *set);



typedef struct xen_vgpu_type_record_opt_set
{
    size_t size;
    xen_vgpu_type_record_opt *contents[];
} xen_vgpu_type_record_opt_set;

/**
 * Allocate a xen_vgpu_type_record_opt_set of the given size.
 */
extern xen_vgpu_type_record_opt_set *
xen_vgpu_type_record_opt_set_alloc(size_t size);

/**
 * Free the given xen_vgpu_type_record_opt_set, and all referenced
 * values.  The given set must have been allocated by this library.
 */
extern void
xen_vgpu_type_record_opt_set_free(xen_vgpu_type_record_opt_set *set);


/**
 * Get a record containing the current state of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_record(xen_session *session, xen_vgpu_type_record **result, xen_vgpu_type vgpu_type);


/**
 * Get a reference to the VGPU_type instance with the specified UUID.
 */
extern bool
xen_vgpu_type_get_by_uuid(xen_session *session, xen_vgpu_type *result, char *uuid);


/**
 * Get the uuid field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_uuid(xen_session *session, char **result, xen_vgpu_type vgpu_type);


/**
 * Get the vendor_name field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_vendor_name(xen_session *session, char **result, xen_vgpu_type vgpu_type);


/**
 * Get the model_name field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_model_name(xen_session *session, char **result, xen_vgpu_type vgpu_type);


/**
 * Get the framebuffer_size field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_framebuffer_size(xen_session *session, int64_t *result, xen_vgpu_type vgpu_type);


/**
 * Get the max_heads field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_max_heads(xen_session *session, int64_t *result, xen_vgpu_type vgpu_type);


/**
 * Get the max_resolution_x field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_max_resolution_x(xen_session *session, int64_t *result, xen_vgpu_type vgpu_type);


/**
 * Get the max_resolution_y field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_max_resolution_y(xen_session *session, int64_t *result, xen_vgpu_type vgpu_type);


/**
 * Get the supported_on_PGPUs field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_supported_on_pgpus(xen_session *session, struct xen_pgpu_set **result, xen_vgpu_type vgpu_type);


/**
 * Get the enabled_on_PGPUs field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_enabled_on_pgpus(xen_session *session, struct xen_pgpu_set **result, xen_vgpu_type vgpu_type);


/**
 * Get the VGPUs field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_vgpus(xen_session *session, struct xen_vgpu_set **result, xen_vgpu_type vgpu_type);


/**
 * Get the supported_on_GPU_groups field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_supported_on_gpu_groups(xen_session *session, struct xen_gpu_group_set **result, xen_vgpu_type vgpu_type);


/**
 * Get the enabled_on_GPU_groups field of the given VGPU_type.
 */
extern bool
xen_vgpu_type_get_enabled_on_gpu_groups(xen_session *session, struct xen_gpu_group_set **result, xen_vgpu_type vgpu_type);


/**
 * Return a list of all the VGPU_types known to the system.
 */
extern bool
xen_vgpu_type_get_all(xen_session *session, struct xen_vgpu_type_set **result);


/**
 * Return a map of VGPU_type references to VGPU_type records for all
 * VGPU_types known to the system.
 */
extern bool
xen_vgpu_type_get_all_records(xen_session *session, xen_vgpu_type_xen_vgpu_type_record_map **result);


#endif
