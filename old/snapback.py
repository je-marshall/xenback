# SNAPBACK.PY
# A relatively simple script for use with XCP
# Can be used to run scheduled snapshots (called with no options),
# export snapshots to a remote repository and run one off
# snapshots for special occasions.
# 
# Written by Jon Marshall

import datetime
import sys
import os
import re
import subprocess
from optparse import OptionParser

LOGFILE = "/var/log/snapback.log"
SR_LOCAL = "30c72c32-126d-6de2-3304-27abbc43faf8"
SR_REMOTE = "227e9175-fa21-7577-a260-9acb2dba0ed0"
DAY_OF_WEEK = 7
DAY_OF_MONTH = 1

def time_string(fmt):
	'''Simple function to return current time as preformatted string for log file,
	   as it was getting bloody annoying'''

	# Check what type of string we're supposed to be returning
	if fmt == "logstamp":
		# Return log-formatted timestamp
		timestamp = datetime.datetime.today()
		timestamp_fmt = timestamp.strftime("[%Y-%m-%d][%H:%M:%S]")
		return timestamp_fmt
	elif fmt == "today":
		# Return false unless day of week is correct
		today = datetime.date.today()
		if today.weekday() == DAY_OF_WEEK:
			return True
		else:
			return False
	elif fmt == "day_of_month":
		# Return false unless day of month is correct
		today = datetime.date.today()
		if today.day == DAY_OF_MONTH:
			return True
		else:
			return False
	elif fmt == "snapstamp":
		# Return snapshot formatted timestamp
		snapstamp = datetime.date.today()
		snapstamp_fmt = snapstamp.strftime("%Y_%m_%d")
		return snapstamp_fmt	


	
def get_uuids():
	'''Returns a list of the uuids of the vms running on the system'''
	
	start_list = os.popen('xe vm-list power-state=running is-control-domain=false')
	
	end_list = []
	
	for i in start_list:
		if re.findall("uuid", i):
			unformatted = i.replace("uuid ( RO)           : ", "")
			end_list.append(unformatted.rstrip())

	return end_list

def run_cmd(args):
	'''Generic run command, commands in this script are simple enough
	   to warrant this.'''
	
	proc = subprocess.Popen(args,
						stdout = subprocess.PIPE,
						stderr = subprocess.PIPE)
	# Wait for command to complete - very important if its a move,
	# or a snapshot or something
	procexit = proc.wait()	
	procout = proc.stdout.read()
	procerr = proc.stderr.read()

	# check if command ran successfully
	if procexit == 0:
		return procout, procerr
	else:
		return False, procerr

def logger(is_error, error, message, uuid):
	'''Need to implement a logger function to write stuff synchronously
	   to the log file'''

	messages = { 1 : "Beginning normal snapshot for VM",
				 2 : "Beginning quiesce snapshot for VM",
				 3 : "Beginning one-off snapshot with quiesce for VM",
				 4 : "Beginning one-off normal snapshot for VM",
				 5 : "Beginning export of snapshot",
				 6 : "Snapshotting completed successfully for VM",
				 7 : "Snapshotting failed for VM",
				 8 : "Export completed successfully for snapshot",
				 9 : "Export failed for snapshot",
				 10: "Could not find specified VM",
				 11: "Could not find specified snapshot",
				 12: "Local copy left as retention limit not exceeded for snapshot",
				 13: "Local copy deleted as retention limit exceeded for snapshot",
				 14: "Exported copy already exists for snapshot"}
	
	if is_error:
		tag = "[ERROR]"
	else:
		tag = "[INFO]"
	
	logfile = open(LOGFILE, 'a')

	logfile.write("%s :%s %s %s\n" % (time_string("logstamp"), tag, messages[message], uuid.strip()))

	if error != "":
		logfile.write("%s :%s %s\n" % (time_string("logstamp"), tag, error))
	
	logfile.close()

class vm:
	'''Main class for VM functions'''

	def __init__(self, uuid):
		self.uuid = uuid
	
	def get_name(self):
		'''Get the VM's name'''

		name_args = ['xe',
					'vm-param-get',
					'uuid=%s' % self.uuid,
					'param-name=name-label']

		self.name, self.nerr = run_cmd(name_args)
		
		if self.name is False:
			return False
		else:
			return True


	def backup_type(self):
		'''Check backup options assigned to the VM in question'''
		
		backup_args = ['xe',
					'vm-param-get',
					'uuid=%s' % self.uuid,
					'param-name=other-config',
					'param-key=XenCenter.CustomFields.backup']

		self.backup, self.backerr = run_cmd(backup_args)

		if self.backup is False:
			return False
		else:
			return True

	def retention(self):
		'''Check how long local backup images should be retained'''

		retention_args = ['xe',
						'vm-param-get',
						'uuid=%s' % self.uuid,
						'param-name=other-config',
						'param-key=XenCenter.CustomFields.retain']

		self.retain, self.reterr = run_cmd(retention_args)

		if self.retain is False:
			return False
		else:
			return True

	def quiesce(self):
		'''Check if the VM is to be quiesced during snapshot'''

		quiesce_args = ['xe',
						'vm-param-get',
						'uuid=%s' % self.uuid,
						'param-name=other-config',
						'param-key=XenCenter.CustomFields.quiesce']

		self.quiesce, self.qerr = run_cmd(quiesce_args)

		if self.quiesce is False:
			return False
		else:
			return True

	def get_snapshots(self):
		'''Saves and returns a list of snapshots associated with a VM'''

		get_snap_args = ['xe',
						'vm-param-get',
						'uuid=%s' % self.uuid,
						'param-name=snapshots']

		self.get_snaps, self.get_snaps_err = run_cmd(get_snap_args)

		if self.get_snaps is False:
			return False
		elif len(self.get_snaps.strip()) == 0:
			self.get_snaps_err = "No snapshots located for this VM"
			return False
		else:
			self.snapshot_list = []
			snap_fmt = self.get_snaps.strip()
			snapshot_list_tmp = snap_fmt.split(";")
			
			# Quick loop to sort out the formatting
			for snap in snapshot_list_tmp:
				self.snapshot_list.append(snap.strip())
			
			return True

	def normal_snapshot(self, suffix):
		'''Run a normal snapshot of the Vm, with no quiescing'''
		
		snap_name = "%s_%s" % (self.name.strip(), suffix)
		description = "Scripted snapshot made on %s" % time_string("logstamp")

		normal_snap_args = ['xe',
							'vm-snapshot',
							'vm=%s' % self.name.strip(),
							'new-name-label=%s' % snap_name,
							'new-name-description=%s' % description]


		self.normal_snap, self.normal_snap_err = run_cmd(normal_snap_args)

		if self.normal_snap is False:
			return False
		else:
			return True

	def quiesce_snapshot(self, suffix):
		'''Run a snapshot with quiesce'''

		snap_name = "%s_%s" % (self.name.strip(), suffix)
		description = "Scripted snapshot made on %s" % time_string("logstamp")

		q_snap_args = ['xe',
					'vm-snapshot-with-quiesce',
					'vm=%s' % self.name.strip(),
					'new-name-label=%s' % snap_name,
					'new-name-description=%s' % description]
		

		self.q_snap, self.q_snap_err = run_cmd(q_snap_args)

		if self.q_snap is False:
			return False
		else:
			return True


class snapshot:
	'''Class to hold snapshot based functions, for convenience and readability'''

	def __init__(self, uuid):
		self.uuid = uuid
	
	def test_exists(self):
		'''Quick test to double double check that the snapshot definitely exists'''

		test_args = ['xe',
					'snapshot-param-get',
					'uuid=%s' % self.uuid,
					'param-name=other']

		self.test, self.test_err = run_cmd(test_args)

		if self.test is False:
			return False
		else:
			return True

	def get_name(self):
		'''Gets the snapshot's name-label and returns it - this helps for exporting snapshots'''

		name_args = ['xe',
					 'snapshot-param-get',
					 'uuid=%s' % self.uuid,
					 'param-name=name-label']

		self.name_unformatted, self.name_err = run_cmd(name_args)

		if self.name_unformatted is False:
			return False
		else:
			self.name = self.name_unformatted.strip()
			return True
	
	def check_if_export_exists(self, dest, name):
		'''Quick check to see if a snapshot already exists, saves on errors in the logs'''

		full_path = "%s/%s/%s.xva" % ("/var/run/sr-mount", dest, name.strip())

		check_if_args = ['ls',
						 full_path]

		self.check_if_export, self.check_if_err = run_cmd(check_if_args)

		if self.check_if_export is False:
			return False
		else:
			return True

	def export_snapshot(self, dest, destroy, name):
		'''Exports a snapshot to backup location and removes if from local/main SR'''

		filename = "%s/%s/%s.xva" % ("/var/run/sr-mount", dest, name.strip()) 

		export_args = ['xe',
					'snapshot-export-to-template',
					'snapshot-uuid=%s' % self.uuid,
					'filename=%s' % filename]

		self.export_snap, self.copy_err = run_cmd(export_args)

		if self.export_snap is False:
			self.exp_err = str(self.copy_err.strip())
			return False
		elif destroy is True:
			destroy_args = ['xe',
						'snapshot-destroy',
						'snapshot-uuid=%s' % self.uuid]

			self.destroy_snap, self.destroy_err = run_cmd(destroy_args)

			if self.destroy_snap is False:
				self.exp_err =str(self.destroy_err.strip())
				return False
			else:
				return True
		elif destroy is False:
			return True


	def snapshot_age(self, retain):
		'''Checks the age of a snapshot and establishes if its old enough to be exported'''
	
		# This needs working on. One off snapshots should not be deleted 
		# automatically. This is a good measure to make sure that one-offs
		# for VM's with no retention limit do not get deleted, but needs 
		# refining. Turns out int(False) = 0 - not good
		if retain is False:
			return False
		else:

			age_args = ['xe',
						'snapshot-param-get',
						'uuid=%s' % self.uuid,
						'param-name=snapshot-time']

			self.snap_age, self.age_err = run_cmd(age_args)

			if self.snap_age is False:
				return False
			else:
				self.snap_date, self.snap_time = self.snap_age.split('T')
				today = datetime.date.today()

				# Not very pretty but it works - turn the string date that XCP returns into
				# a date object, so its useful
				snap_date_obj = datetime.date(int(self.snap_date[0:4]), int(self.snap_date[4:6]), int(self.snap_date[6:8]))

				difference = datetime.timedelta(int(retain))
				retain_limit = today - difference
				retain_difference = retain_limit - snap_date_obj

				if retain_difference.days > 0:
					return True
				else:
					return False
			
			
			

def main():
	'''Get options for what functions to run, then run!'''


	usage = '''Run by itself, this script finds all VM's to be backed up on the vhost and takes snapshots of them. Additional arguments are available to export all snapshots to remote storage.'''

	parser = OptionParser(usage)
	parser.add_option("-o", "--once", action = "store_true", dest = "once",
					  help = '''Runs a single snapshot of a VM.''')
	parser.add_option("-v", "--vm-uuid", dest = "vm_uuid",
					  help = '''Required option for running one off snap''')
	parser.add_option("-d", "--description", dest = "description",
					  help = '''Optional description of one off snap''')
	parser.add_option("-e", "--export", action = "store_true", dest = "export",
					  help = '''Exports snapshots to remote storage and clears
					  			out snapshots older than the retention time''')
	parser.add_option("-s", "--snap-uuid", dest = "snap_uuid",
					  help = '''Specifies which snapshot to export. If not supplied
					  			then a normal export will run against all snapshots''')
	parser.add_option("-r", "--remove", action = "store_true", dest = "snap_remove",
					  help = '''If specified, removes the snapshot after exporting''')
	

	(options, args) = parser.parse_args()
	

	# Check if one-off backup and if so run that
	if options.once:
		# Check that UUID has been provided
		if not options.vm_uuid:
			parser.error("Need to specify VM uuid")
		else:
		# If no description provided, run with default
			if options.description:
				backup_suffix = "%s-once-%s" % (time_string("snapstamp"), options.description)
				print options.description
				print backup_suffix
			else:
				backup_suffix = "%s-once" %	time_string("snapstamp") 
				print backup_suffix
			single_snap = vm(options.vm_uuid)
			# Check that the supplied uuid is valid
			if single_snap.get_name():
				print "Beginning one-off backup of %s" % single_snap.name	
				# Check whether quiesce or not
				if single_snap.quiesce():
					print "Attempting snapshot with quiesce..."
					logger(False, "", 3, single_snap.name)
					if single_snap.quiesce_snapshot(backup_suffix):
						logger(False, "", 6, single_snap.name)
						print "Snapshot completed successfully"
						sys.exit()
					else:
						logger(True, single_snap.q_snap_err, 7, single_snap.name)
						print "Snapshotting failed"
						sys.exit(1)
				else:
					logger(False, "", 4, single_snap.name)
					print "Attempting normal snapshot..."
					if single_snap.normal_snapshot(backup_suffix):
						logger(False, "", 6, single_snap.name)
						print "Snapshot completed successfully"
						sys.exit()
					else:
						logger(True, single_snap.normal_snap_err, 7, single_snap.name)
						print "Snapshotting failed"
						sys.exit(1)
					
			else:
				logger(True, single_snap.nerr, 10, single_snap.name)
				print "Cannot find specified VM"
				sys.exit(1)
			

	# Check if export option has been set
	elif options.export:
		# Get UUIDs of all VMs
		
		if options.snap_uuid:
			# Perform one off export of specified snapshot
			snap_inst = snapshot(options.snap_uuid)
			# Check that the snapshot exists
			if snap_inst.test_exists():
				if snap_inst.get_name():	
					logger(False, "", 5, snap_inst.name)
					print "Attempting export of snapshot %s...\n" % snap_inst.name
					# Check if exported snapshot exists
					if snap_inst.check_if_export_exists(SR_REMOTE, snap_inst.name):
						logger(False, "", 14, snap_inst.name)
						print "Snapshot already exported for %s\n" %snap_inst.name
					else:
						# Check if the snapshot is to be removed after export
						if options.snap_remove:
							print "Snapshot will be removed after export\n"
							if snap_inst.export_snapshot(SR_REMOTE, True, "%s-special-export" % snap_inst.name):
								logger(False, "", 8, snap_inst.name)
								logger(False, "", 13, snap_inst.name)
								print "Export and removal completed successfully\n"
							else:
								logger(True, snap_inst.exp_err, 9, snap_inst.name)
								print "Error exporting snapshot\n"
								print snap_inst.exp_err, "\n"
						else:
							print "Snapshot will not be removed after export\n"
							if snap_inst.export_snapshot(SR_REMOTE, False, "%s-special-export" % snap_inst.name):
								logger(False, "", 8, snap_inst.name)
								logger(False, "", 12, snap_inst.name)
								print "Export completed successfully\n"
							else:
								logger(True, snap_inst.exp_err, 9, snap_inst.name)
								print "Error exporting snapshot\n"
								print snap_inst.exp_err, "\n"
			else:
				print "Snapshot does not exist"


		else:
			vm_list = get_uuids()
			for uuid in vm_list:
				vm_inst = vm(uuid)
				
				# Get the name for easier snapshot identification
				vm_inst.get_name()

				# Get retention period
				vm_inst.retention()
				retain = vm_inst.retain.strip()
					

				# Get all snapshots
				if vm_inst.get_snapshots():
					snap_list = vm_inst.snapshot_list
					# Loop through each snapshot to determine its age
					for uuid in snap_list:
						snap = snapshot(uuid)
						# Check that the snapshot definitely exists
						if snap.test_exists():
							if snap.get_name():
							# Check if snapshot has already been exported
								if snap.check_if_export_exists(SR_REMOTE, snap.name):
									logger(False, "", 14, snap.name)
								else:
									if snap.snapshot_age(vm_inst.retain):
										logger(False, "", 5, snap.name)
										# Pass the remote storage repository uuid to the function otherwise it'll go nowhere
										# Flag that this snapshot is old enough to be deleted from the local sr
										if snap.export_snapshot(SR_REMOTE, True, snap.name):
											logger(False, "", 8, snap.name)
											logger(False, "", 13, snap.name)
										else:
											logger(True, snap.exp_err, 9, snap.name)
									else:
										logger(False, "", 5, snap.name)
										# Flag that this snapshot is not old enough to be deleted from the local sr
										if snap.export_snapshot(SR_REMOTE, False, snap.name):
											logger(False, "", 8, snap.name)
											logger(False, "", 12, snap.name)
										else:
											logger(True, snap.exp_err, 9, snap.name)
						else:
							logger(True, snap.test_err, 11, snap.name)
				
		# Exports happen seperately from snapshots, so exit after this loop
		sys.exit()

	# No options left to parse, so get on with a normal backup
	else:
		vm_list = get_uuids()

		for uuid in vm_list:
			vm_inst = vm(uuid)
			# Lots of checking, but we don't want anything to go wrong
			if vm_inst.get_name():
				if vm_inst.backup_type():
					# If we have a backup type, great, but we need to check what type
					if vm_inst.backup.strip() == 'daily':
					# Daily backup goes here
						if vm_inst.quiesce():
							# Run a snapshot with quiesce
							logger(False, "", 2, vm_inst.name)
							if vm_inst.quiesce_snapshot("%s_daily" % time_string("snapstamp")):
								logger(False, "", 6, vm_inst.name)
							else:
								logger(True, vm_inst.q_snap_err, 7, vm_inst.name)
						else:
							# Run a normal snapshot
							logger(False, "", 1, vm_inst.name)
							if vm_inst.normal_snapshot("%s_daily" % time_string("snapstamp")):
								logger(False, "", 6, vm_inst.name)
							else:
								logger(True, vm-inst.normal_snap_err, 7, vm_inst.name)
					elif vm_inst.backup.strip() == 'weekly':
					# Weekly backup goes here
						if time_string("today"):
							if vm_inst.quiesce():
								# Run a snapshot with quiesce
								logger(False, "", 2, vm_inst.name)
								if vm_inst.quiesce_snapshot("%s_weekly" % time_string("snapstamp")):
									logger(False, "", 6, vm_inst.name)
								else:
									logger(True, vm_inst.q_snap_err, 7, vm_inst.name)
							else:
								# Run a normal snapshot
								logger(False, "", 1, vm_inst.name)
								if vm_inst.normal_snapshot("%s_weekly" % time_string("snapstamp")):
									logger(False, "", 6, vm_inst.name)
								else:
									logger(True, vm_inst.normal_snap_err, 7, vm_inst.name)
					elif vm_inst.backup.strip() == 'monthly':
					# Monthly backup goes here
						if time_string("day_of_month"):
							if vm_inst.quiesce():
								# Run a snapshot with quiesce
								logger(False, "", 2, vm_inst.name)
								if vm_inst.quiesce_snapshot("%s_monthly" % time_string("snapstamp")):
									logger(False, "", 6, vm_inst.name)
								else:
									logger(True, vm_inst.q_snap_err, vm_inst.name)
							else:
								# Run a normal snapshot
								logger(False, "", 1, vm_inst.name)
								if vm_inst.normal_snapshot("%s_monthly" % time_string("snapstamp")):
									logger(False, "", 6, vm_inst.name)
								else:
									logger(True, vm_inst.normal_snap_err, 7, vm_inst.name)
			


if __name__ == '__main__':
	main()
