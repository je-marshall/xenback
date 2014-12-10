#!/bin/env python2.7

from VDISnapHelpers import VDI, VM
import XenAPI
import urllib2

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

def get_all_setup(session):
	
	all_vms = session.xenapi.VM.get_all_records()
	all_vbds = session.xenapi.VBD.get_all_records()
	all_vdis = session.xenapi.VDI.get_all_records()

	host = session.xenapi.host.get_all()

	return all_vms, all_vbds, all_vdis, host
