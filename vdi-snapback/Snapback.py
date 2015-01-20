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

def run_backup(session, vm_exclude, network, host, dest):
	'''
		Runs the main backup loop, excluding any VM's that have been pulled from
		the config file
	'''

	### Perhaps it is worth changing this to be one massive for loop, as it
	### would be helpful to have VM names available when downloading files

	all_vms = session.xenapi.VM.get_all_records()
	backup_vms = {}

	for opaqueref, vm in all_vms.items():
		for match in vm_exclude:
			if match in vm['name_label'] or match in vm['uuid']:
				print "Excluding virtual machine %s" % vm['name_label']
			elif not(vm['is_a_template']) and not(vm['is_control_domain'] and vm['power_state'] == "Running":
				backup_vms[opaqueref] = vm
	
	all_vbds = session.xenapi.VBD.get_all_records()

	for opaqueref, vm in backup_vms.items():
		this_vm = SnapbackHelpers.VM(session, opaqueref, vm)
		this_vm.get_vbd_list(all_vbds)

		if not this_vm.pause():
			print "Skipping VM %s : could not pause" % this_vm.name
			continue

		this_vdi_ref = this_vm.snapshot()

		if not this_vdi:
			print "Snapshotting failed for VM %s" % this_vm.name
			if not this_vm.unpause():
				print "Unpausing also failed for VM %s, requires manual intervention" % this_vm.name
				continue
			continue
		
		this_vdi_dict = session.xenapi.VDI.get_record(this_vdi_ref)
		this_vdi = SnapbackHelpers.VDI(session, this_vdi_ref, this_vdi_dict)
		
		expose_ref = this_vdi.expose(host, network)

		if not expose_ref:
			print "Failed to expose VDI for %s" % this_vm.name
			continue
		
		this_record = SnapbackHelpers.get_record(session, expose_ref, host)
		this_url = next(v for k,v in this_record.items() if 'url_full' in k)
		
		full_path = dest + this_vm.name + str(test_record['snapshot_time'])

		print "Downloading snapshot for VM %s to destination %s" % (this_vm.name, full_path)

		SnapbackHelpers.download_file(this_url, full_path)

		if not this_vdi.unexpose():
			print "Unexpose failed for %s, manual intervention required"
			


def main():
	'''
		Handles parsing config and shoving it into the main loop
	'''
	
	session = XenAPI.Session(host)
	session.login_with_password(user, password)
	# Assuming the first host in the set is the currently connected host.
	# TODO: Does this work on XenServer pools??
	host_ref = session.xenapi.host.get_all()[0]
	

if __name__ == '__main__':
	main()
