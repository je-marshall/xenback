#!/bin/env python2.7

import XenAPI
import getpass
import sys

def get_vdi(session):
	'''
		Asks for a VM name or UUID and returns the VDI's associated with it
	'''

	vm_name = raw_input("Name: ")

	all_vms = session.xenapi.VM.get_all_records()

	for opaqueref, vm in all_vms.items():
		if vm_name in vm["name_label"]:
			vm_dict = vm
	
	if not vm_dict:
		print "No results found"
		session.xenapi.logout()
		sys.exit(1)
	
	print vm_dict["name_label"]

	all_vbds = session.xenapi.VBD.get_all_records()
	
	vm_vbds = []

	for opaqueref, vbd in all_vbds.items():
		if opaqueref in vm_dict["VBDs"] and vbd["userdevice"] == '0':
			vm_vbds.append(vbd)

	all_vdis = session.xenapi.VDI.get_all_records()

	for opaqueref, vdi in all_vdis.items():
		for vbd in vm_vbds:
			if vbd["VDI"] in opaqueref:
				print vdi["name_label"]
				print vdi["uuid"]

if __name__ == '__main__':
	
	session = XenAPI.Session("http://10.0.10.20")
	user = raw_input("User: ")
	passwd = getpass.getpass()

	session.login_with_password(user, passwd)
	
	get_vdi(session)
