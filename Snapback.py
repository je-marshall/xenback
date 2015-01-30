#!/usr/bin/python2.7

import XenAPI
import os
import signal
import argparse
import datetime
import sys
import logging
import time
import urllib2
import ConfigParser
from xml.dom import minidom


# Some default values
CONFIG = "Snapback.cfg"
DESCRIPTION = '''
Need to work on this
			  '''

def signal_term_handler(signal, frame, session):
	# Handles any SIGTERMs
	
	log = logging.getLogger(__name__)
	log.error("Caught SIGTERM, bailing")
	log.error("VDI's may be exposed and VM's paused, needs manual intervention")
	session.logout()
	sys.exit(1)

def signal_int_handler(signal, frame, session):
	# Handles and SIGINTs
	
	log = logging.getLogger(__name__)
	log.error("Caught SIGINT, attempting to shut down gracefully")

	all_vms = session.xenapi.VM.get_all_records()
	all_vdis = session.xenapi.VDI.get_all_records()

	for opaqueref, vm in all_vms.items():
		if vm['power_state'] == 'Paused':
			this_vm = VM(session, opaqueref, vm)
			log.info("CLEANUP - Unpausing VM %s" % this_vm.name)
			if not this_vm.unpause():
				log.error("CLEANUP - could not unpause VM %s" % this_vm.name)
			log.info("CLEANUP - Unpaused VM %s successfully" % this_vm.name)
	
	for opaqueref, vdi in all_vdis.items():
		if vdi['is_snapshot']:
			this_vdi = VDI(session, opaqueref, vdi)
	        if not this_vm.vm_dict['affinity']:
	            host = session.xenapi.host.get_all()[0]
	            log.debug("No host found, defaulting to pool master %s" % host)
	        else:
	            host = this_vm.vm_dict['affinity']
	            log.debug("Using host %s" % host)
			if not this_vdi.get_record(host):
				continue
			else:
				log.debug("Attempting to unexpose VDI %s" % this_vdi.uuid)
				if this_vdi.unexpose(host):
					this_vdi.destroy()
					log.info("CLEANUP - destroyed VDI %s" % this_vdi.uuid)

	log.error("Cleanup successful, still needs checking manually")
	sys.exit(1)


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
			self.is_exposed = False
			return True
		else:
			log.error("Unexpose failed: %s" % response)
			return False

	def get_expose_record(self, host):
		# Returns the record data associated with an exposed VDI
		log = logging.getLogger(__name__)
	
		args = {'record_handle' : self.expose_ref}

		try:
			xml = self.session.xenapi.host.call_plugin(host, 'transfer', 'get_record', args)
		except Exception, e:
			log.error("Failed to retrieve record")
			log.error(e)
			return False
		
		record = {}
		try:
			doc = minidom.parseString(xml)
		except Exception, e:
			log.error("Failed to parse XML stream")
			log.error(e)
			return False
	
		try:
			el = doc.getElementsByTagName('transfer_record')[0]
			for k, v in el.attributes.items():
				record[str(k)] = str(v)
		finally:
			doc.unlink()
	
		return record

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


def get_network_uuid(session):
	# For now this is dumb and picks the one on eth0
	for ref, network in session.xenapi.network.get_all_records().items():
		if 'eth0' in network["name_label"]:
			return network["uuid"]

def download_file(record, filepath):
	# This has now got a bit more complicated as it needs to be able to parse
	# the username and password for basic http auth out of the incoming record

	# Check the file path exists, if not create it
	# NOTE - the behaviour is default to overwrite
	log = logging.getLogger(__name__)

	try:
		if not os.path.isfile(filepath):
			open(filepath, 'a').close()
	except Exception, e:
		log.error("Incorrect filepath %s" % filepath)
		log.debug(e)
		return False
	
	
	# Construct a full url without the auth bits in it
	full_url = record['transfer_mode'] + '://' + record['ip'] + ':' + record['port'] + record['url_path']
	
	# Instantiate the password manager
	password_manager = urllib2.HTTPPasswordMgrWithDefaultRealm()
	
	# Add auth details
	password_manager.add_password(None, full_url, record['username'], record['password'])
	
	# Specify handler
	handler = urllib2.HTTPBasicAuthHandler(password_manager)
	
	# Specify opener
	opener = urllib2.build_opener(handler)
	
	# Attempt to auth and download the file
	try:
		log.debug("Download begun from %s to %s" % (full_url, filepath))
		req = opener.open(full_url)
		chunk_size = 16 * 1024
		with open(filepath, 'wb') as fp:
			while True:
				chunk = req.read(chunk_size)
				if not chunk: break
				fp.write(chunk)
	except urllib2.HTTPError as e:
		log.error("The server could not fulfill the request")
		log.debug(e)
		return False
	except urllib2.URLError as e:
		log.error("Server unreachable")
		log.debug(e)
		return False
	else:
		return True

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

def pre_command(command):
	# Runs the command specified in the config file
	pass

def post_command(command):
	# Runs the command specified in the config file
	pass

# TODO - Add in task creation/management, so as to appease the XenAPI overlords

def run_backup(session, network, dest, vm_exclude=[]):
	'''
		Runs the main backup loop, excluding any VM's that have been pulled from
		the config file
	'''
	log = logging.getLogger(__name__)
	# First we need to get a list of all the available VM's on the server, as
	# well as all of the VBD's
	all_vms = session.xenapi.VM.get_all_records()
	all_vbds = session.xenapi.VBD.get_all_records()
	backup_vms = {}
	
	# Now we want to weed out VM's that we don't need to back up. These are ones
	# that are off, the dom0, and any that have been added to an exlusion list
	# in the config file
	for opaqueref, vm in all_vms.items():
		if not vm['is_a_template'] and not vm['is_control_domain'] and vm['power_state'] == "Running":
			log.info("Adding VM %s to backup queue" % vm['name_label'])
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
		
		log.debug("VM %s paused successfully" % this_vm.name)
		# After having paused the VM, take a snapshot of its first disk.
		this_vdi_ref = this_vm.snapshot_vdi()

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
		
		if not this_vm.unpause():
			log.critical("Unpause operation failed for VM %s" % this_vm.name)

		log.info("Snapshot successful for VM %s" % this_vm.name)
		log.debug("%s VM now unpaused" % this_vm.name)
		
		# Check which host the vm is running on - not sure if this will help
		log.debug("Checking which host to use to download")
		if not this_vm.vm_dict['affinity']:
			host = session.xenapi.host.get_all()[0]
			log.debug("No host found, defaulting to pool master %s" % host)
		else:
			host = this_vm.vm_dict['affinity']
			log.debug("Using host %s" % host)

		# Pull the record for the newly create VDI snapshot, and then create and
		# instance of the helper class to allow us to faff with it
		try:
			this_vdi_dict = session.xenapi.VDI.get_record(this_vdi_ref)
		except Exception, e:
			log.error("Error retrieving VDI details for VDI %s" % this_vdi_ref)
			log.error("Skipping this VDI")
			log.debug(e)
			continue

		this_vdi = VDI(session, this_vdi_ref, this_vdi_dict)
	
		# Attempt to expose the snapshot using the http API, if this hasn't
		# worked then skip to the next one

		if not this_vdi.expose(host, network):
			log.warning("Failed to expose VDI for %s" % this_vm.name)
			log.warning("Detroying snapshot %s and skipping" % this_vdi.uuid)
			if not this_vdi.destroy():
				log.error("Could not clean up by destroying VDI %s" % this_vdi.uuid)
			continue

		log.debug("Snapshot exposed successfully")
		
		# Grab the full record for the exposed VDI and then extract the full url
		# for this -  note that due to XenAPI's questionable overuse of UUIDs,
		# we're having to loop to find it. Breaks if the VDI is not exposed.
		this_record = this_vdi.get_expose_record(host)
		if this_record:
			this_url = next(v for k,v in this_record.items() if 'url_full' in k)
		else:
			log.error("Could not retrive url, skipping")
			if not this_vdi.unexpose(host):
				log.critical("Could not unexpose VDI %s" % this_vdi.uuid)
			if not this_vdi.destroy():
				log.critical("Could not destroy VDI %s" % this_vdi.uuid)
			continue

		# Construct the full path for the saved snapshot on the server.
		full_path = dest + this_vm.name + '.xva'

		log.info("Downloading snapshot for VM %s to destination %s" % (this_vm.name, full_path))
		log.debug("URL: %s" % this_url)
		
		# Attempt a download of the file - note that this handler needs a lot of
		# work in terms of error handling as it is totally bare at the moment
		
		## COMMENTED OUT FOR NOW JUST WANT TO TEST THE REST
		download_file(this_record, full_path)
		
		log.info("Download successful for VM %s" % this_vm.name)
		# If we can't unexpose then this could be an issue I suppose

		log.debug("Attempting to unexpose snapshot")
		if not this_vdi.unexpose(host):
			log.critical("Unexpose failed for %s, manual intervention required")

		log.debug("Snapshot unexposed")
		
		# Finally, destroy the vdi that has been created as it is no longer
		# useful. Note that this would probably not be the case if the snapshot
		# were to be used as the basis of a differential, this will need more
		# thought.
		log.debug("Destroying snapshot")
		if not this_vdi.destroy():
			log.critical("Could not destroy snapshot for %s" % this_vm.name)
		log.debug("Snapshot destroyed")
			


def main():
	'''
		Handles parsing config and shoving it into the main loop
	'''
	
	log = logging.getLogger(__name__)

	parser = argparse.ArgumentParser(description=DESCRIPTION)

	parser.add_argument('-c', '--conf', help='''Specify config file''')
	parser.add_argument('-v', '--verbose', action='store_true')
	args = parser.parse_args()
	
	if args.conf:
		config = parse_config(args.conf)
	else:
		config = parse_config(CONFIG)
	
	# Get log level
	numeric_level = getattr(logging, config['loglevel'].upper(), None)
	
	# Check log level is real
	if not isinstance(numeric_level, int):
		raise ValueError('Invalid log level: %s' % config['loglevel'])
	
	# Check log filepath
	if not os.path.isdir(config['logdir']):
		try:
			os.makedirs(config['logdir'])
		except:
			raise ValueError('Invalid log path: %s' % config['logdir'])
	
	# Generate new log file (hour and mins included to allow multiple runs
	timestamp = datetime.datetime.utcnow().strftime("%d-%m-%y_%H-%M")
	logfile = config['logdir'] + '/' + timestamp + '.log'

	formatter = logging.Formatter('%(asctime)s: %(levelname)s: %(message)s')
	fhandler = logging.FileHandler(logfile)
	shandler = logging.StreamHandler()

	fhandler.setFormatter(formatter)
	# Set logging 
	#logging.basicConfig(filename=logfile, level=numeric_level)
	log.setLevel(numeric_level)
	log.addHandler(fhandler)
	
	if args.verbose:
		log.addHandler(shandler)
		shandler.setFormatter(formatter)
	
	url = 'http://' + config['ip']
	session = XenAPI.Session(url)
	try:
		session.login_with_password(config['user'], config['passwd'])
	except Exception, e:
		log.critical("Login failed with supplied details: %s" % e)
		sys.exit(1)

	log.debug("Login successful to host %s as user %s" % (config['ip'], config['user']))
	# Grab the net uuid, again this is assuming the one that is on eth0
	network = get_network_uuid(session)
	log.debug("Retrieved Xen network reference %s" % network)

	signal.signal(signal.SIGTERM, signal_term_handler(session))
	signal.signal(signal.SIGINT, signal_term_handler(session))
	
	log.debug("Attempting to run pre-command")
	pre_command()
	log.debug("Beginning main backup loop")
	run_backup(session, network, config['dldir'])

if __name__ == '__main__':
	main()
