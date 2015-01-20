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

# MASSIVE TODO - LOGGING

# TODO - Add in task creation/management, so as to appease the XenAPI overlords

def run_backup(session, vm_exclude, network, host, dest):
	'''
		Runs the main backup loop, excluding any VM's that have been pulled from
		the config file
	'''

	# First we need to get a list of all the available VM's on the server, as
	# well as all of the VBD's
	all_vms = session.xenapi.VM.get_all_records()
	all_vbds = session.xenapi.VBD.get_all_records()
	backup_vms = {}
	
	# Now we want to weed out VM's that we don't need to back up. These are ones
	# that are off, the dom0, and any that have been added to an exlusion list
	# in the config file
	for opaqueref, vm in all_vms.items():
		for match in vm_exclude:
			if match in vm['name_label'] or match in vm['uuid']:
				print "Excluding virtual machine %s" % vm['name_label']
			elif not(vm['is_a_template']) and not(vm['is_control_domain'] and vm['power_state'] == "Running":
				backup_vms[opaqueref] = vm
	
	# Now we can begin the main loop
	for opaqueref, vm in backup_vms.items():
		# Create a VM instance for this vm and populate its VBD list
		this_vm = SnapbackHelpers.VM(session, opaqueref, vm)
		this_vm.get_vbd_list(all_vbds)
		
		# Attempt to pause it, skipping to the next if unable to
		# TODO - add logging
		if not this_vm.pause():
			print "Skipping VM %s : could not pause" % this_vm.name
			continue
	
		# After having paused the VM, take a snapshot of its first disk. Note
		# that this function can be overridden to snapshot all disks, but I
		# can't really think we'd ever need this, so that is up for discussion
		this_vdi_ref = this_vm.snapshot()

		# Check that the snapshot was successful. If not, report this, then
		# attmept to unpause the VM
		# TODO - add logging
		if not this_vdi_ref:
			print "Snapshotting failed for VM %s" % this_vm.name
			if not this_vm.unpause():
				# Note that if this happens, I'm not sure what to do about it
				# really. Potentially worth adding in an email/Zabbix alerting
				# section to the logging?
				print "Unpausing also failed for VM %s, requires manual intervention" % this_vm.name
				continue
			continue
	
		# Pull the record for the newly create VDI snapshot, and then create and
		# instance of the helper class to allow us to faff with it
		this_vdi_dict = session.xenapi.VDI.get_record(this_vdi_ref)
		this_vdi = SnapbackHelpers.VDI(session, this_vdi_ref, this_vdi_dict)
	
		# Attempt to expose the snapshot using the http API, if this hasn't
		# worked then skip to the next one
		expose_ref = this_vdi.expose(host, network)

		# TODO - add logging
		if not expose_ref:
			print "Failed to expose VDI for %s" % this_vm.name
			continue
		
		# Grab the full record for the exposed VDI and then extract the full url
		# for this -  note that due to XenAPI's questionable overuse of UUIDs,
		# we're having to loop to find it. Breaks if the VDI is not exposed.
		this_record = SnapbackHelpers.get_record(session, expose_ref, host)
		this_url = next(v for k,v in this_record.items() if 'url_full' in k)
		
		# Construct the full path for the saved snapshot on the server. This
		# could potentially do with some discussion, as the snapshot_time field
		# is annoyingly formatted
		full_path = dest + this_vm.name + str(test_record['snapshot_time'])

		print "Downloading snapshot for VM %s to destination %s" % (this_vm.name, full_path)
		
		# Attempt a download of the file - note that this handler needs a lot of
		# work in terms of error handling as it is totally bare at the moment
		SnapbackHelpers.download_file(this_url, full_path)
		
		# If we can't unexpose then this could be an issue I suppose
		if not this_vdi.unexpose():
			print "Unexpose failed for %s, manual intervention required"

		# TODO - Delete the snapshot VDI once done with it
			


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
