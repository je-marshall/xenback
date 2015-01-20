#!/bin/env python2.7

import XenAPI
import sys
import time
import urllib2
from xml.dom import minidom

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
			# log and handle error
			return e

		return expose_ref
	
	def unexpose(self, vdi_ref, host):
		# Unexposes a vdi
		args = {'record_handle' : vdi_ref}
		try:
			response = self.session.xenapi.host.call_plugin(host, 'transfer', 'unexpose', args)
		except Exception, e:
			# log and handle error
			pass

		if response == 'OK':
			return True
		else:
			# Log 
			return False

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
			pass
			# log error

	def unpause(self):
		# Resumes the VM
		try:
			self.session.xenapi.VM.unpause(self.vm_ref)
		except Exception, e:
			pass
			# log error
	
	def snapshot_vdi(self, all_disks=False):
		# Snapshots the first disk of the VM unless told otherwise, then returns
		# a list of the OpaqueRef Id's generated by xen
		
		snapshot_list = []

		if not all_disks:
			for vbd in self.vbd_list:
				if vbd["userdevice"] == "0":
					try:
						return_ref = self.session.xenapi.VDI.snapshot(vbd["VDI"])
						snapshot_list.append(return_ref)
					except Exception, e:
						pass
						# insert logging and error handling here

		else:
			for vbd in self.vbd_list:
				try:
					return_ref = self.session.xenapi.VDI.snapshot(vbd["VDI"])
					snapshot_list.append(return_ref)
				except Exception, e:
					pass
					# insert logging and error handling here

		return snapshot_list
	
def vdi_from_ref(vdi_ref, all_vdis):
	# Returns a VDI dict from an opaqueref
	
	for opaqueref, vdi in all_vdis.items():
		if vdi_ref in opaqueref:
			return_vdi = vdi

	return return_vdi

def get_record(session, expose_ref, host):
	# Returns the record data associated with an exposed VDI
	args = {'record_handle' : expose_ref}

	try:
		xml = session.xenapi.host.call_plugin(host, 'transfer', 'get_record', args)
	except Exception, e:
		# log and handle error
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

    # insert logging timestammp
