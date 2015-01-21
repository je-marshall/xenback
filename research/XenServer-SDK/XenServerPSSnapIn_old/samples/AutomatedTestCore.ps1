#
# Copyright (c) Citrix Systems, Inc.
# All rights reserved.
# 
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions
# are met:
# 
#   1) Redistributions of source code must retain the above copyright
#      notice, this list of conditions and the following disclaimer.
# 
#   2) Redistributions in binary form must reproduce the above
#      copyright notice, this list of conditions and the following
#      disclaimer in the documentation and/or other materials
#      provided with the distribution.
# 
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
# "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
# LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
# FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
# COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
# INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
# (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
# SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
# HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
# STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
# ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
# OF THE POSSIBILITY OF SUCH DAMAGE.
#

# Powershell Automated Tests

param($out_xml, $svr, $usr, $pwd, $sr_svr, $sr_path)

# Initial Setup

$BestEffort = $false
$NoWarnCertificates = $true
$info = $true
$warn = $true
$err = $true
$prog = $false

$Eap = $ErrorActionPreference
$Vp = $VerbosePreference
$Wp = $WarningPreference
$Ep = $ErrorPreference

$ErrorActionPreference = "Stop"
$VerbosePreference="Continue"
$WarningPreference="Continue"
$ErrorPreference="Continue"
$ErrorVariable

# End Initial Setup

# Helper Functions

function log_info([String]$msg)
{
  process
  {
    if($info)
	{
	  write-verbose $msg
	}
  }
}

function log_warn([String]$msg)
{
  process
  {
    if($warn)
	{
      write-warning $msg
	}
  }
}

function log_error([String]$msg)
{
  process
  {
    if($err) 
	{
      write-error $msg
	}
  }
}

function escape_for_xml([String]$content)
{
  return $content.replace("&", "&amp;").replace("'", "&apos;").replace('"', "&quot;").replace("<", "&lt;").replace(">", "&gt;")
}

function prep_xml_output([String]$out_file)
{
  $date = Get-Date
  "<results>" > $out_file
  ("<testrun>Test Run Info: PowerShell bindings test {0}</testrun>" -f $date) >> $out_file
  "<group>" >> $out_file
}

function close_xml_output([String]$out_file)
{
  "</group>" >> $out_file
  "</results>" >> $out_file
}

function add_result([String]$out_file,[String]$cmd, [String]$test_name, [Exception]$err)
{
  $out_cmd = escape_for_xml $cmd
  $out_test_name = escape_for_xml $test_name
  $out_err = escape_for_xml $err
  "<test>" >> $out_file
  ("<name>{0}</name>" -f $out_test_name) >> $out_file
  if (($err -ne $null))
  {
    "<state>Fail</state>" >> $out_file
	"<log>" >> $out_file
    ("Cmd: '{0}'" -f $out_cmd) >> $out_file
	("Exception: {0}" -f $out_err) >> $out_file
	"</log>" >> $out_file
  }
  else
  {
    "<state>Pass</state>" >> $out_file
    "<log />" >> $out_file
  }
  "</test>" >> $out_file
}


function exec([String]$test_name, [String]$cmd, [String]$expected)
{
  trap [Exception]
  {
     add_result $out_xml $cmd $test $_.Exception
	 $fails.Add($test_name, $_.Exception)
	 break
  }
  
  log_info ("Test '{0}' Started: cmd = {1}, expected = {2}" -f $test_name,$cmd,$expected)
  $result = Invoke-Expression $cmd
  if ($result -eq $expected)
  {
    add_result $out_xml $cmd $test_name $null
	return $true
  }
  else
  {
    $exc = new-object Exception("Test '{0}' Failed: expected '{1}'; actual '{2}'" -f $test_name,$expected,$result)
    add_result $out_xml $cmd $test_name $exc
	$fails.Add($test_name, $exc)
	return $false
  }
}

# End Helper Functions

# Connect Functions

function connect_server([String]$svr, [String]$usr, [String]$pwd)
{
  log_info ("connecting to server '{0}'" -f $svr)
  $session = Connect-XenServer -svr $svr -user $usr -pwd $pwd
  if($session.uuid -ne $null)
  {
    return ""
  }
  else
  {
    return $null
  }
}

function disconnect_server([String]$svr)
{
  log_info ("disconnecting from server '{0}'" -f $svr)
  Disconnect-XenServer -svr $svr
  return ""
}

# End Connect Functions

# VM Functions

function clone_vm([String]$name, [String]$template_name)
{
  log_info ("cloning vm '{0}' to '{1}'" -f $template_name,$name)
  $vm = Invoke-XenServer:VM.Clone $template_name $name -RunAsync | Wait-XenServer:Task -ShowProgress
  return Get-XenServer:VM -name $name
}

function provision_vm([XenAPI.VM]$template)
{
  log_info ("provisioning vm '{0}'" -f $template.name_label)
  Invoke-XenServer:VM.Provision $template -RunAsync | Wait-XenServer:Task -ShowProgress
  return ""
}

function destroy_vm([XenAPI.VM]$vm)
{
  log_info ("destroying vm '{0}'" -f $vm.name_label)
  $vdis = @()
  foreach($vbd in $vm.VBDs)
  {
	if(((Get-XenServer:VBD.Mode $vbd) -eq ([XenAPI.vbd_mode] "RW")))
	{
      $vdis += Get-XenServer:VBD.VDI $vbd
	}
  }
  Destroy-XenServer:VM $vm -RunAsync | Wait-XenServer:Task -ShowProgress
  foreach($vdi in $vdis)
  {
    Destroy-XenServer:VDI $vdi -RunAsync | Wait-XenServer:Task -ShowProgress
  }
  return ""
}

function add_cd_drive([XenAPI.VM]$vm)
{
  log_info ("creating cd drive for vm '{0}'" -f $vm.name_label)
  Create-XenServer:VBD $vm.uuid $null 3 $false ([XenAPI.vbd_mode] "RO") ([XenAPI.vbd_type] "CD") $true $true @{} "" @{}
}

function install_vm([String]$name, [String]$template_name, [String]$sr_name)
{
  trap [Exception]
  {
  	trap [Exception]
	{
	  log_warn "Clean up after failed vm install unsuccessful"
	  log_info "...failed!"
	  break
	}
    log_info "Attempting to clean up after failed vm install..."
	$vms = Get-XenServer:VM -name $name
	foreach($vm in $vms)
  	{
  	  if($vm -ne $null) 
	  {
   	    destroy_vm($vm) 
	  }
  	}
	log_info "...success."
	break
  }
  log_info ("installing vm '{0}' from template '{1}'" -f $template_name,$name)
  $vm = clone_vm $name $template_name
  $sr = Get-XenServer:SR -name $sr_name
  $other_config = $vm.other_config
  $other_config["disks"] = $other_config["disks"].Replace('sr=""', 'sr="{0}"' -f $sr.uuid)
  add_cd_drive $vm
  Set-XenServer:VM.OtherConfig $vm $other_config
  provision_vm $vm
  return ""
}

function uninstall_vm([String]$name)
{
  log_info ("uninstalling vm '{0}'" -f $name)
   
  $vms = Get-XenServer:VM -name $name
 foreach($vm in $vms)
  {
    if($vm -ne $null) {
    destroy_vm($vm) }
  }
  return ""
}

function vm_can_boot($vm, [XenApi.Host[]] $servers)
{
  trap [Exception]
  {
    $script:exceptions += $_.Exception
	continue
  }
  $script:exceptions = @()
  foreach ($server in $servers)
  {
    Invoke-XenServer:VM.AssertCanBootHere -Host $server -Self $vm
  }
  if ($exceptions.Length -lt $servers.Length)
  {
  	return $true
  }
  log_info "No suitable place to boot VM:"
  foreach ($excep in $script:exceptions)
  {
  	log_info ("Reason: {0}" -f $excep.Message)
  }
  return $false
}

function start_vm($vm_name)
{
  if (vm_can_boot $vm_name (Get-XenServer:Host))
  {
  	log_info ("starting vm '{0}'" -f $vm_name)
  }
  # even if we cant start it, attempt so we get the exception, reasons have been logged in vm_can_boot
  Invoke-XenServer:VM.Start $vm_name -RunAsync | Wait-XenServer:Task -ShowProgress
  return Get-XenServer:VM.PowerState $vm_name
}

function shutdown_vm($vm_name)
{
  log_info ("shutting down vm '{0}'" -f $vm_name)
  Invoke-XenServer:VM.HardShutdown $vm_name -RunAsync | Wait-XenServer:Task -ShowProgress
  return Get-XenServer:VM.PowerState $vm_name
}

# End VM Functions

# Host Functions

function get_master()
{
  $pool = Get-XenServer:Pool
  return Get-XenServer:Host -properties @{opaque_ref=$pool.master.opaque_ref}
}

# End Host Functions

# SR Functions

function get_default_sr()
{
  log_info ("getting default sr")
  $pool = Get-XenServer:Pool
  return Get-XenServer:SR -properties @{opaque_ref=$pool.default_sr.opaque_ref}
}

function create_nfs_sr([String]$sr_svr, [String]$sr_path, [String]$sr_name)
{
  log_info ("creating sr {0} at {1}:{2}" -f $sr_name,$sr_svr,$sr_path)
  $master = get_master
  $sr = Create-XenServer:SR $master @{ "server"=$sr_svr; "serverpath"=$sr_path; "options"="" } 0 $sr_name "" "nfs" "" $true @{} -RunAsync | Wait-XenServer:Task -ShowProgress
  return ""
}

function destroy_nfs_sr([String]$sr_name)
{
  log_info ("destroying sr {0}" -f $sr_name)
  $master = get_master
  $pbds = Get-XenServer:SR.PBDs $sr_name
  foreach($pbd in $pbds)
  {
    if($pbd.currently_attached)
    {
      Invoke-XenServer:PBD.Unplug $pbd
    }
  #  Destroy-XenServer:PBD $pbd
  }
  Destroy-XenServer:SR $sr_name -RunAsync | Wait-XenServer:Task -ShowProgress
  return ""
}

# End SR Functions

# Test List

$tests = @(
            @("Connect Server", "connect_server $svr $usr $pwd", ""),
			@("Create SR", "create_nfs_sr $sr_svr $sr_path PowerShellAutoTestSR", ""),
            @("Install VM", "install_vm PowerShellAutoTestVM 'Windows XP SP3 (32-bit)' PowerShellAutoTestSR", ""),
			@("Start VM", "start_vm PowerShellAutoTestVM", "Running"),
			@("Shutdown VM", "shutdown_vm PowerShellAutoTestVM", "Halted"),
			@("Uninstall VM", "uninstall_vm 'PowerShellAutoTestVM'", ""),
			@("Destroy SR", "destroy_nfs_sr PowerShellAutoTestSR", ""),
            @("Disconnect Server", "disconnect_server $svr", "")
          )
# End Test List

# Main Test Execution
$complete = 0;
$max = $tests.Count;

$fails = @{}

prep_xml_output $out_xml
foreach($test in $tests)
{
  trap [Exception]
  {
    # we encountered an exception in running the test before it completed
	# its already been logged, so continue
	continue
  }
  $success = $false
  $success = exec $test[0] $test[1] $test[2]
  if ($success)
  {
    $complete++
  }
}
close_xml_output $out_xml
$result = "Result: {0} completed out of {1}" -f $complete,$max;
write-host $result -f 2
if($fails.Count -gt 0)
{
  write-host "Failures:"
  $fails
}

$ErrorActionPreference = $Eap
$VerbosePreference = $Vp
$WarningPreference = $Wp
$ErrorPreference = $Ep

# End Main Test Execution
