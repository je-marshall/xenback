#!/bin/env python2.7

import SnapbackHelpers
import XenAPI
import ConfigParser

#def parse_config(config_file):
#	# reads the config from a file and returns a formatted dictionary
#
#	return_dict = {}
#	config = ConfigParser.RawConfigParser()
#	try:
#		config.read(config_file)
#	except Exception, e:
#		# Log error and handle
#		pass
#
#	host_url = config.get('Host', 'url')
#	user = config.get('Host', 'user')
#	passwd = config.get('Host', 'pass')

def run_backup(session, vm_exclude, network, host):
	'''
		Runs the main backup loop, excluding any VM's that have been pulled from
		the config file
	'''

	all_vms = session.xenapi.VM.get_all_records()
	backup_vms = {}

	for opaqueref, vm in all_vms.items():
		for match in vm_exclude:
			if match in vm['name_label'] or match in vm['uuid']:
				print "Excluding virtual machine %s" % vm['name_label']
			elif not(vm['is_a_template']) and not(vm['is_control_domain'] and vm['power_state'] == "Running":
				backup_vms[opaqueref] = vm
	
	all_vbds = session.xenapi.VBD.get_all_records()
	backup_vdi_list = []

	for opaqueref, vm in backup_vms.items():
		this_vm = SnapbackHelpers.VM(session, opaqueref, vm)
		this_vm.get_vbd_list(all_vbds)

		try:
			this_vm.pause()
		except:
			print "Pause failed for %s VM, skipping" % this_vm.name
			continue

		try:
			this_snapshot_vdi = this_vm.snapshot_vdi()
			backup_vdi_list.append(this_snapshot_vdi)
		except:
			print "Snapshotting failed for %s VM, skipping" % this_vm.name
		finally:
			this_vm.unpause()
	
	backup_vdis = {}
	all_vdis = session.xenapi.VDI.get_all_records()

	for vdi in backup_vdi_list:
		vdi_dict = vdi_from_ref(vdi, all_vdis)
		backup_vdis[vdi] = vdi_dict

	for opaqueref, vdi in backup_vdis.items():
		this_vdi = SnapbackHelpers.VDI(session, opaqueref, vdi)

		try:
			expose_ref = this_vdi.expose(host, network)
		except:
			print "Could not expose %s VDI" % this_vdi.name
			continue

def main():
	'''
		Handles parsing config and shoving it into the main loop
	'''
	
	session = XenAPI.Session(host)
	session.login_with_password(user, password)
	# Assuming the first host in the set is the currently connected host.
	# TODO: Does this work on XenServer pools??
	host_ref = session.xenapi.host.get_all()[0]
	
