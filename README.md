# xenback
Xenserver backup script, utilising XenAPI

Relatively simple script for backing up only the OS partition of running VM's on a Xenserver host. This was 
necessary because we often run fileservers with large data partitions that are backed up in other ways and
did not want to waste space on the Xenserver storage.

The script loops through all running hosts and then selectively snapshots and downloads the first parition it 
finds - the download can be done via http or iscsi. If using isci, the qemu-utils and iscsiadm need to be installed.
The script checks for this and will bail with an error message if they are not present and iscsi is selected.

