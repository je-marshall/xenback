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


#ifndef XEN_PGPU_H
#define XEN_PGPU_H

#include <xen/api/xen_common.h>
#include <xen/api/xen_gpu_group_decl.h>
#include <xen/api/xen_host_decl.h>
#include <xen/api/xen_pci_decl.h>
#include <xen/api/xen_pgpu_decl.h>
#include <xen/api/xen_pgpu_xen_pgpu_record_map.h>
#include <xen/api/xen_string_string_map.h>
#include <xen/api/xen_task_decl.h>
#include <xen/api/xen_vgpu_decl.h>
#include <xen/api/xen_vgpu_type_decl.h>
#include <xen/api/xen_vgpu_type_int_map.h>


/*
 * The PGPU class.
 * 
 * A physical GPU (pGPU).
 */


/**
 * Free the given xen_pgpu.  The given handle must have been allocated
 * by this library.
 */
extern void
xen_pgpu_free(xen_pgpu pgpu);


typedef struct xen_pgpu_set
{
    size_t size;
    xen_pgpu *contents[];
} xen_pgpu_set;

/**
 * Allocate a xen_pgpu_set of the given size.
 */
extern xen_pgpu_set *
xen_pgpu_set_alloc(size_t size);

/**
 * Free the given xen_pgpu_set.  The given set must have been allocated
 * by this library.
 */
extern void
xen_pgpu_set_free(xen_pgpu_set *set);


typedef struct xen_pgpu_record
{
    xen_pgpu handle;
    char *uuid;
    struct xen_pci_record_opt *pci;
    struct xen_gpu_group_record_opt *gpu_group;
    struct xen_host_record_opt *host;
    xen_string_string_map *other_config;
    struct xen_vgpu_type_record_opt_set *supported_vgpu_types;
    struct xen_vgpu_type_record_opt_set *enabled_vgpu_types;
    struct xen_vgpu_record_opt_set *resident_vgpus;
    xen_vgpu_type_int_map *supported_vgpu_max_capacities;
} xen_pgpu_record;

/**
 * Allocate a xen_pgpu_record.
 */
extern xen_pgpu_record *
xen_pgpu_record_alloc(void);

/**
 * Free the given xen_pgpu_record, and all referenced values.  The
 * given record must have been allocated by this library.
 */
extern void
xen_pgpu_record_free(xen_pgpu_record *record);


typedef struct xen_pgpu_record_opt
{
    bool is_record;
    union
    {
        xen_pgpu handle;
        xen_pgpu_record *record;
    } u;
} xen_pgpu_record_opt;

/**
 * Allocate a xen_pgpu_record_opt.
 */
extern xen_pgpu_record_opt *
xen_pgpu_record_opt_alloc(void);

/**
 * Free the given xen_pgpu_record_opt, and all referenced values.  The
 * given record_opt must have been allocated by this library.
 */
extern void
xen_pgpu_record_opt_free(xen_pgpu_record_opt *record_opt);


typedef struct xen_pgpu_record_set
{
    size_t size;
    xen_pgpu_record *contents[];
} xen_pgpu_record_set;

/**
 * Allocate a xen_pgpu_record_set of the given size.
 */
extern xen_pgpu_record_set *
xen_pgpu_record_set_alloc(size_t size);

/**
 * Free the given xen_pgpu_record_set, and all referenced values.  The
 * given set must have been allocated by this library.
 */
extern void
xen_pgpu_record_set_free(xen_pgpu_record_set *set);



typedef struct xen_pgpu_record_opt_set
{
    size_t size;
    xen_pgpu_record_opt *contents[];
} xen_pgpu_record_opt_set;

/**
 * Allocate a xen_pgpu_record_opt_set of the given size.
 */
extern xen_pgpu_record_opt_set *
xen_pgpu_record_opt_set_alloc(size_t size);

/**
 * Free the given xen_pgpu_record_opt_set, and all referenced values. 
 * The given set must have been allocated by this library.
 */
extern void
xen_pgpu_record_opt_set_free(xen_pgpu_record_opt_set *set);


/**
 * Get a record containing the current state of the given PGPU.
 */
extern bool
xen_pgpu_get_record(xen_session *session, xen_pgpu_record **result, xen_pgpu pgpu);


/**
 * Get a reference to the PGPU instance with the specified UUID.
 */
extern bool
xen_pgpu_get_by_uuid(xen_session *session, xen_pgpu *result, char *uuid);


/**
 * Get the uuid field of the given PGPU.
 */
extern bool
xen_pgpu_get_uuid(xen_session *session, char **result, xen_pgpu pgpu);


/**
 * Get the PCI field of the given PGPU.
 */
extern bool
xen_pgpu_get_pci(xen_session *session, xen_pci *result, xen_pgpu pgpu);


/**
 * Get the GPU_group field of the given PGPU.
 */
extern bool
xen_pgpu_get_gpu_group(xen_session *session, xen_gpu_group *result, xen_pgpu pgpu);


/**
 * Get the host field of the given PGPU.
 */
extern bool
xen_pgpu_get_host(xen_session *session, xen_host *result, xen_pgpu pgpu);


/**
 * Get the other_config field of the given PGPU.
 */
extern bool
xen_pgpu_get_other_config(xen_session *session, xen_string_string_map **result, xen_pgpu pgpu);


/**
 * Get the supported_VGPU_types field of the given PGPU.
 */
extern bool
xen_pgpu_get_supported_vgpu_types(xen_session *session, struct xen_vgpu_type_set **result, xen_pgpu pgpu);


/**
 * Get the enabled_VGPU_types field of the given PGPU.
 */
extern bool
xen_pgpu_get_enabled_vgpu_types(xen_session *session, struct xen_vgpu_type_set **result, xen_pgpu pgpu);


/**
 * Get the resident_VGPUs field of the given PGPU.
 */
extern bool
xen_pgpu_get_resident_vgpus(xen_session *session, struct xen_vgpu_set **result, xen_pgpu pgpu);


/**
 * Get the supported_VGPU_max_capacities field of the given PGPU.
 */
extern bool
xen_pgpu_get_supported_vgpu_max_capacities(xen_session *session, xen_vgpu_type_int_map **result, xen_pgpu pgpu);


/**
 * Set the other_config field of the given PGPU.
 */
extern bool
xen_pgpu_set_other_config(xen_session *session, xen_pgpu pgpu, xen_string_string_map *other_config);


/**
 * Add the given key-value pair to the other_config field of the given
 * PGPU.
 */
extern bool
xen_pgpu_add_to_other_config(xen_session *session, xen_pgpu pgpu, char *key, char *value);


/**
 * Remove the given key and its corresponding value from the
 * other_config field of the given PGPU.  If the key is not in that Map, then
 * do nothing.
 */
extern bool
xen_pgpu_remove_from_other_config(xen_session *session, xen_pgpu pgpu, char *key);


/**
 * .
 */
extern bool
xen_pgpu_add_enabled_vgpu_types(xen_session *session, xen_pgpu self, xen_vgpu_type value);

/**
 * .
 */
extern bool
xen_pgpu_add_enabled_vgpu_types_async(xen_session *session, xen_task *result, xen_pgpu self, xen_vgpu_type value);


/**
 * .
 */
extern bool
xen_pgpu_remove_enabled_vgpu_types(xen_session *session, xen_pgpu self, xen_vgpu_type value);

/**
 * .
 */
extern bool
xen_pgpu_remove_enabled_vgpu_types_async(xen_session *session, xen_task *result, xen_pgpu self, xen_vgpu_type value);


/**
 * .
 */
extern bool
xen_pgpu_set_enabled_vgpu_types(xen_session *session, xen_pgpu self, struct xen_vgpu_type_set *value);

/**
 * .
 */
extern bool
xen_pgpu_set_enabled_vgpu_types_async(xen_session *session, xen_task *result, xen_pgpu self, struct xen_vgpu_type_set *value);


/**
 * .
 */
extern bool
xen_pgpu_set_gpu_group(xen_session *session, xen_pgpu self, xen_gpu_group value);

/**
 * .
 */
extern bool
xen_pgpu_set_gpu_group_async(xen_session *session, xen_task *result, xen_pgpu self, xen_gpu_group value);


/**
 * .
 */
extern bool
xen_pgpu_get_remaining_capacity(xen_session *session, int64_t *result, xen_pgpu self, xen_vgpu_type vgpu_type);

/**
 * .
 */
extern bool
xen_pgpu_get_remaining_capacity_async(xen_session *session, xen_task *result, xen_pgpu self, xen_vgpu_type vgpu_type);


/**
 * Return a list of all the PGPUs known to the system.
 */
extern bool
xen_pgpu_get_all(xen_session *session, struct xen_pgpu_set **result);


/**
 * Return a map of PGPU references to PGPU records for all PGPUs known
 * to the system.
 */
extern bool
xen_pgpu_get_all_records(xen_session *session, xen_pgpu_xen_pgpu_record_map **result);


#endif
