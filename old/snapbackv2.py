# Snapback V2
# What is hopefully a massive improvement on the previous attempt. Can be used
# to run scheduled snapshots if called with no options via cron, or can run
# special one off snapshots

# Written by Jon Marshall

import sys
import os
import re
import subprocess
from optparse import OptionParser

VM_UUIDS = 'xe vm-list power-state=running is-control-domain=false'
VDI_UUIDS = 'xe vm-disk-list uuid='			 

def run_cmd(args):
	"""
Generic run command
	"""
	proc = subprocess.Popen(args,
							stdout = subprocess.PIPE,
							stderr = subprocess.PIPE)
	# Wait for command to complete
	procexit = proc.wait()
	procout = proc.stdout.read()
	procerr = proc.stderr.read()

	if procexit == 0:
		return procout, procerr
	else:
		return False, procerr

def get_uuids(options):
	"""
Returns a list of uuids in a nicely formatted way
	"""
	try:
		start_list = os.popen(options)
	except Exception:
		# Log the error and bail
		return False

	return_list = []
	for i in start_list:
		if re.findall("uuid", i):
			return_list.append(i.replace("uuid ( RO)           : ", "").rstrip())

	return return_list

class VM:
	"""
Main class for VM related functions
	"""
	def __init__(self, uuid):
		
		self.uuid = uuid
		self.vm_args = ['xe', 'vm-param-get', 'uuid=%s' % self.uuid]

	def get_name(self):
	# Returns the VM's name
		name_args =  self.vm_args.extend(['param-name=name-label'])
		name, nerr = run_cmd(name_args)
	
		if not name:
			# Log error
			return False
		else:
			return name

	def get_snapshots(self):
	# Returns a list of snapshots associated with the vm
		get_snap_args = self.vm_args.extend(['param-name=snapshots'])
		get_snaps, get_snaps_err = run_cmd(get_snap_args)
	
		if not get_snaps:
			# Log error
			return False
		elif len(get_snaps.strip()) == 0:
			# log this
			return False
		else:
			snap_list = []
			snap_fmt = get_snaps.strip()
			
			for snap in snap_fmt.split(";"):
				snap_list.append(snap.strip())
			return snap_list

	def get_vdis(self):
	# Returns a list of VDI's that need to be backed up
		disks = get_uuids(VDI_UUIDS+'%s' % self.uuid)

		for disk in disks:
			vdi_args = ['xe', 'vdi-param-get', 'param-name=XenCenter.CustomFields.snapshot', 'uuid=%s' % self.uuid]
			vdi, vdi_err = run_cmd(vdi_args)
			return_disks = []
			if not vdi:
				# Log that this disk isn't being backed up
				pass
			else:
				return_disks.append(disk)

		return return_disks

	def retention(self):
	# Check how long local images should be retained 
		retention_args = self.vm_args.extend(['param-name=other-config', 'param-key=XenCenter.CustomFields.retain'])
		retain, reterr = run_cmd(retention_args)
	
		if not retain:
			# Log error
			return False
		else:
			return retain
	
	def quiesce(self):
	# Check whether or not to quiesce disks
		quiesce_args = self.vm_args.extend(['param-name=other-config', 'param-key=XenCenter.CustomFields.quiesce'])
		quiesce, qerr = run_cmd(quiesce_args)
	
		if not quiesce:
			# Log error
			return False
		else:
			return quiesce

	def suspend(self):
	# Suspends a machine
		suspend_args = ['xe', 'vm-suspend', 'uuid=%s' % self.uuid]
		suspend, suspend_err = run_cmd(suspend_args)

		if not suspend:
			# Log the error and bail
			return False
		else:
			return True

	def resume(self):
	# Resumes a machine
		resume_args = ['xe', 'vm-resume', 'uuid=%s' % self.uuid]
		resume, resume_err = run_cmd(resume_args)

		if not resume:
			# Log the error and bail
			return False
		else:
			return True

	def snapshot(self, suffix):
	# Runs a snapshot
		if self.get_name():
			snap_name = "%s_%s" % (self.name.strip(), suffix)
			description = "Scripted snapshot made on %s" % time_string() #Placeholder
		else:
			# log this
			return False
		# Check if snapshot needs quiesce option
		if self.quiesce():
			snap_args = ['xe',
						 'vm-snapshot',
						 'vm=%s' %self.name.strip(),
						 'new-name-label=%s' % snap_name
						 'new-name-description=%s' % description]
			# Log that it is quiesced
		else:
			snap_args = ['xe',
						 'vm-snapshot-with-quiesce',
						 'vm=%s' % snap_name,
						 'new-name-label=%s' % snap_name,
						 'new-name-description=%s' % description]
			# Log that it isn't quiesced

		snap, snap_err = run_cmd(snap_args)
		
		if not snap:
			# Log snapshot error
			return False
		else:
			return True

class Snapshot:
	"""
Class to define common snapshot operations
	"""
	def __init__(self, uuid):
		self.uuid = uuid
		
		# Specify commands for each function
		self.snap_args = ['xe', 'snapshot-param-get', 'uuid=%s' % self.uuid]
		age_args = snap_args.extend(['param-name=snapshot-time'])

	def test_exists(self):
		"""
	Checks that the snapshot definitely exists by trying to access a parameter
		"""
		test_args = self.snap_args.extend(['param-name=other'])

		test, test_err = run_cmd(test_args)

		if not test:
			# Log the error
			return False
		else:
			return True
	
	def get_name(self):
		"""
	Returns the snapshots name, to be used when exporting
		"""
		name_args = self.snap_args.extend(['param-name=name-label'])

		name_unformatted, name_err = run_cmd(name_args)

		if not name_unformatted:
			# Log the error
			return False
		else:
			name = name_unformatted.strip()
			return name

	def export(self, dest, destroy=False):
		"""
	Exports the snapshot, checking first to see if this has already been done
		"""
		if not test_exists():
			# Log that export is bailing as the snapshot does not exist
			return False
		else:
			name = self.get_name()
			if not name:
				# Log that the name is not present
				return False
		
		full_path = "%s/%s/%s.xva" % ("/var/run/sr-mount/", dest, name.strip())

		check, check_err = run_cmd(['ls', full_path])

		if check:
			# Log that export already exists - note that each snapshot should
			# have a unique name
			return False

		export, export_err = run_cmd(['xe',
					   				  'snapshot-export-to-template',
					   				  'snapshot-uuid=%s' % self.uuid,
					   				  'filename=%s' % full_path])

		if not export:
			# Log the error
			return False
		elif destroy:
			destroy, destroy_err = run_cmd(['xe',
											'snapshot-destroy',
											'snapshot-uuid=%s' % self.uuid])
			if not destroy:
				# Log the error
				return False

		return True

	def age(self, retain=False):
		"""
	Checks the age of the snapshot and whether it is old enough to be exported
		"""
		if not retain:
			return False
		
		age, age_err = run_cmd(self.age_args)

		if not age:
			# Log the error
			return False
		
		snap_date, snap_time = age.split('T')
		
		# Not pretty but it works
		snap_date_fmt = datetime.date(int(snap_date[0:4]), int(snap_date[4:6]) int(snap_date[6:8]))
		# Calculate the retention limit
		retain_limit = datetime.date.today() - datetime.timedelta(int(retain))

def main():
	""" 
Get options for what functions to run, then run!
	"""


    usage = '''Run by itself, this script finds all VM's to be backed up on the vhost and takes snapshots 
			   of them. If their retention period is up then they are exported to remote storage. This can
			   be overridden by passing the -e flag'''

    parser = OptionParser(usage)
    parser.add_option("-o", "--once", action = "store_true", dest = "once",
                      help = '''Runs a single snapshot of a VM.''')
    parser.add_option("-v", "--vm-uuid", dest = "vm_uuid",
                      help = '''Required option for running one off snap''')
    parser.add_option("-d", "--description", dest = "description",
                      help = '''Optional description of one off snap''')
    parser.add_option("-e", "--no-export", action = "store_false"  dest = "no_export",
                      help = '''Optional description of one off snap''')


    (options, args) = parser.parse_args()
	
	snapstamp = datetime.date.today.strftime("%Y_%m_%d-%H_%M")

	# Check if a one off snapshot is to be made
	if options.once:
		if not options.vm_uuid:
			parser.error("Need to specify VM UUID for one off snapshot")
	else:
		# Set the name of the snapshot
		if options.description:
			suffix = "%s-once-%s" % (snapstamp, options.description)
		else:
			suffix = "%s-once" % (snapstamp)
		
		# Attempt to snapshot
		single_snap = VM(options.vm_uuid)
		if single_snap.snapshot(suffix):
			sys.exit()
		else:
			sys.exit(1)

	vm_list = get_uuids(VM_UUIDS)

	for uuid in vm_list:
		vm_inst = VM(uuid)
		if not vm_inst.snapshot(snapstamp):
			# Log error taking snap, moving on to next one
			pass
	
	
		

