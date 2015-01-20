#!/bin/env python2.7

import XenAPI
import sys
import logging
import time
import urllib2
import ConfigParser
from xml.dom import minidom


# Some default values
CONFIG = "Snapback.cfg"

class VDI:
	'''
		Simple class for VDI ops
	'''

	def __init__(self, session, vdi_ref, vdi_dict): 
		self.session = session
		self.vdi_ref = vdi_ref
		self.vdi_dict = vdi_dict
		self.uuid = vdi_dict["uuid"]
		self.name = vdi_dict["name_label"]
		self.is_exposed = False
		self.exposed_ref = False
		self.log = logging.getLogger(__name__)

	def expose(self, host, network):
		# Exposes this vdi as a vhd
		args = { 'transfer_mode' : 'http',
				 'vdi_uuid' : self.uuid,
				 'network_uuid' : network,
				 'expose_vhd' : 'true',
				 'read_only' : 'true' }

		try:
			expose_ref = self.session.xenapi.host.call_plugin(host, 'transfer', 'expose', args)
		except Exception, e:
			self.log.error("Failed to expose VDI %s" % self.uuid)
			self.log.error(e)
			return False
		
		self.is_exposed = True
		self.expose_ref = expose_ref

		return True
	
	def unexpose(self, host):
		# Unexposes a vdi
		args = {'record_handle' : self.expose_ref}
		try:
			response = self.session.xenapi.host.call_plugin(host, 'transfer', 'unexpose', args)
		except Exception, e:
			self.log.error("Failed to unexpose VDI %s" % self.uuid)
			self.log.error(e)
			return False

		if response == 'OK':
			return True
		else:
			# Log 
			return False

	def destroy(self):
		# Destroys a VDI
		try:
			self.session.xenapi.VDI.destroy(self.vdi_ref)
		except Exception, e:
			self.log.error("Failed to destroy VDI %s" % self.uuid)
			self.log.error(e)
			return False

		return True

class VM:
	''' 
		Simple holding class for VM operations
	'''

	def __init__(self, session, vm_ref, vm_dict):
		self.session = session
		self.vm_ref = vm_ref
		self.vm_dict = vm_dict
		self.uuid = vm_dict["uuid"]
		self.name = vm_dict["name_label"]
		self.vbd_list = []
		self.log = logging.getLogger(__name__)
	
	def get_vbd_list(self, all_vbds):
		# Returns a list of all the vbds associated with this vm

		for opaqueref, vbd in all_vbds.items():
			if opaqueref in self.vm_dict["VBDs"]:
				self.vbd_list.append(vbd)
				

	def pause(self):
		# Pauses the VM
		try:
			self.session.xenapi.VM.pause(self.vm_ref)
		except Exception, e:
			self.log.error("Failed to pause VM %s" % self.uuid)
			self.log.error(e)
			return False	

		return True

	def unpause(self):
		# Resumes the VM
		try:
			self.session.xenapi.VM.unpause(self.vm_ref)
		except Exception, e:
			self.log.error("Failed to unpause VM %s" % self.uuid)
			self.log.error(e)
			return False

		return True
	
	def snapshot_vdi(self):
		# Snapshots the first disk of the VM then returns the ref 
		
		for vbd in self.vbd_list:
			if vbd["userdevice"] == "0":
				try:
					return_ref = self.session.xenapi.VDI.snapshot(vbd["VDI"])
					return return_ref
				except Exception, e:
					self.log.error("Failed to snapshot VDI for VM %s" % self.uuid)
					self.log.error(e)
					return False


def get_record(session, expose_ref, host):
	# Returns the record data associated with an exposed VDI
	log = logging.getLogger(__name__)
	args = {'record_handle' : expose_ref}

	try:
		xml = session.xenapi.host.call_plugin(host, 'transfer', 'get_record', args)
	except Exception, e:
		log.error("Failed to parse XML stream")
		log.error(e)
		return False
	
	print xml
	record = {}
	doc = minidom.parseString(xml)

	try:
		el = doc.getElementsByTagName('transfer_record')[0]
		for k, v in el.attributes.items():
			record[str(k)] = str(v)
	finally:
		doc.unlink()

	return record

def get_network_uuid(session):
	# For now this is dumb and picks the one on eth0

	for ref, network in session.xenapi.network.get_all_records().items():
		if 'eth0' in network["name_label"]:
			return network["uuid"]

def download_file(url, filepath):
    # Downloads a file in chunks and logs datestamps before and after
    req = urllib2.urlopen(url)
    chunk_size = 16 * 1024
    # insert logging timestamp
    with open(filepath, 'wb') as fp:
        while True:
            chunk = req.read(chunk_size)
            if not chunk: break
            fp.write(chunk)

def parse_config(config_file):
	# reads the config from a file and returns a formatted dictionary

	config = ConfigParser.RawConfigParser()
	try:
		config.read(config_file)
	except Exception, e:
		# Log error and handle
		print "Config file missing"
		sys.exit(1)
	
	try:
		host_ip = config.get('Host', 'ip')
		user = config.get('Host', 'user')
		passwd = config.get('Host', 'pass')

		dldir = config.get('Local', 'dldir')
		logdir = config.get('Local', 'logdir')
		loglevel = config.get('Local', 'loglevel')
	except Exception, e:
		# Log error and bail
		print "Config file incorrectly formatted: %s" % e
		sys.exit(1)

	return_dict = { 'ip' : host_ip,
				    'user' : user,
					'passwd' : passwd,
					'dldir' : dldir,
					'logdir' : logdir,
					'loglevel' : loglevel
				   }

	return return_dict

# TODO - Add in task creation/management, so as to appease the XenAPI overlords

def run_backup(session, vm_exclude, network, host, dest):
	'''
		Runs the main backup loop, excluding any VM's that have been pulled from
		the config file
	'''
	log = logging.getLogger(__name__)
	if not vm_exclude:
		log.debug('No VMs to exclude')
		vm_exclude = []
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
				log.debug("Excluding virtual machine %s" % vm['name_label'])
			elif not vm['is_a_template'] and not vm['is_control_domain'] and vm['power_state'] == "Running":
				backup_vms[opaqueref] = vm
	log.debug("List of VMs assembled")
	
	# Now we can begin the main loop
	for opaqueref, vm in backup_vms.items():
		# Create a VM instance for this vm and populate its VBD list
		this_vm = VM(session, opaqueref, vm)
		this_vm.get_vbd_list(all_vbds)
		
		# Attempt to pause it, skipping to the next if unable to
		if not this_vm.pause():
			log.warning("Skipping VM %s : could not pause" % this_vm.name)
			continue
	
		# After having paused the VM, take a snapshot of its first disk. Note
		# that this function can be overridden to snapshot all disks, but I
		# can't really think we'd ever need this, so that is up for discussion
		this_vdi_ref = this_vm.snapshot()

		# Check that the snapshot was successful. If not, report this, then
		# attmept to unpause the VM
		if not this_vdi_ref:
			log.error("Snapshotting failed for VM %s" % this_vm.name)
			if not this_vm.unpause():
				# Note that if this happens, I'm not sure what to do about it
				# really. Potentially worth adding in an email/Zabbix alerting
				# section to the logging?
				log.critical("Unpausing also failed for VM %s, requires manual intervention" % this_vm.name)
				continue
			continue
	
		# Pull the record for the newly create VDI snapshot, and then create and
		# instance of the helper class to allow us to faff with it
		this_vdi_dict = session.xenapi.VDI.get_record(this_vdi_ref)
		this_vdi = VDI(session, this_vdi_ref, this_vdi_dict)
	
		# Attempt to expose the snapshot using the http API, if this hasn't
		# worked then skip to the next one
		expose_ref = this_vdi.expose(host, network)

		if not expose_ref:
			log.warning("Failed to expose VDI for %s" % this_vm.name)
			continue
		
		# Grab the full record for the exposed VDI and then extract the full url
		# for this -  note that due to XenAPI's questionable overuse of UUIDs,
		# we're having to loop to find it. Breaks if the VDI is not exposed.
		this_record = get_record(session, expose_ref, host)
		this_url = next(v for k,v in this_record.items() if 'url_full' in k)
		
		# Construct the full path for the saved snapshot on the server. This
		# could potentially do with some discussion, as the snapshot_time field
		# is annoyingly formatted
		full_path = dest + this_vm.name + str(test_record['snapshot_time'])

		log.info("Downloading snapshot for VM %s to destination %s" % (this_vm.name, full_path))
		
		# Attempt a download of the file - note that this handler needs a lot of
		# work in terms of error handling as it is totally bare at the moment
		download_file(this_url, full_path)
		
		# If we can't unexpose then this could be an issue I suppose
		if not this_vdi.unexpose():
			log.critical("Unexpose failed for %s, manual intervention required")
		
		# Finally, destroy the vdi that has been created as it is no longer
		# useful. Note that this would probably not be the case if the snapshot
		# were to be used as the basis of a differential, this will need more
		# thought.
		if not this_vdi.destroy():
			log.critical("Could not destroy snapshot for %s" % this_vm.name)
			


def main():
	'''
		Handles parsing config and shoving it into the main loop
	'''
	
	log = logging.getLogger(__name__)

	# Need to have a bit here so as to be able to override config from the
	# command line and also to be able to set things like weekly vs daily from
	# the command line. Could potentially edit that but for now lets just got
	# with load from config, error out if not
	config = parse_config(CONFIG)
	
	url = 'http://' + config['ip']
	session = XenAPI.Session(url)
	try:
		session.login_with_password(config['user'], config['passwd'])
	except Exception, e:
		print "Login failed with supplied details: %s" % e
		sys.exit(1)
	# Assuming the first host in the set is the currently connected host.
	# TODO: Does this work on XenServer pools??
	host_ref = session.xenapi.host.get_all()[0]
	# Grab the net uuid, again this is assuming the one that is on eth0
	network = get_network_uuid(session)
	
	# TODO - test that dirs exist
	run_backup(session, False, network, host_ref, config['dldir'])

if __name__ == '__main__':
	main()
