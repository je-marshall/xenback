import os
import sys
import re
import subprocess
from optparse import Optionparser

class VM:
	"""
Handles VM operations
	"""

	def __init__(self, uuid):
		
		self.uuid = uuid
		self.vm_base_args = ['xe', 'vm-param-get', 'uuid=%s' % self.uuid]

	def get_param(self, arg):
	# Returns a parameter associated with the VM
		args = { 'name' : 'param-name=name-label',
				 'snaps' : 'param-name=snapshots' }

		if arg not in args:
			# Log incorrect arg
			return False

		full_command = self.vm_base_args.extend([args[arg]])

		com, comerr = run_cmd(full_command)

		if not name:
			# Log error
			return False
		else:
			return com

	def take_snapshot(self):
	# Runs a snapshot
		snap_name = self.get_param('name')
		if not snap_name:
			# Log error
			return False

		snap_args = ['xe',
					 'vm-checkpoint',
					 'vm=%s' % snap_name.strip(),
					 'new-name-label=%s_%s' % (snap_name.strip(), time_string())
					]
		snap, snaperr = run_cmd(snap_args)

		if not snap:
			# Log snapshot error
			return False
		else:
			return True
	
	def list_snapshots(self):
	# Returns a formatted list of snapshots for this VM
		snap_list = []
		smap_list_fmt = self.get_param('snaps').strip()
		
		if not snap_list_fmt:
			# Log error
			return False

		for snap in snap_list_fmt.split(";"):
			snap_list.append(snap.strip())
		return snap_list

	def export_snapshot(self, snapshot):
	# Exports any given snapshot
		full_path = "%s/%s.xva" % (EXPORT_PATH, self.get_param('name'))

		export_args = ['xe',
					   'vm-export',
					   'vm=%s' % snapshot,
					   'filename=%s', % fullpath,
					  ]


