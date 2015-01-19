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

Param([Parameter(Mandatory=$true)][String]$out_xml,
       [Parameter(Mandatory=$true)][String]$svr,
       [Parameter(Mandatory=$true)][String]$usr,
       [Parameter(Mandatory=$true)][String]$pwd,
       [Parameter(Mandatory=$true)][String]$sr_svr,
       [Parameter(Mandatory=$true)][String]$sr_path)

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
    $exc = new-object Exception("Test '{0}' Failed: expected '{1}'; actual '{2}'" `
                                -f $test_name,$expected,$result)
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
  $session = Connect-XenServer -Server $svr -UserName $usr -Password $pwd -PassThru

  if($session -eq $null)
  {
    return $false
  }
  return $true

}

function disconnect_server([String]$svr)
{
  log_info ("disconnecting from server '{0}'" -f $svr)
  Get-XenSession -Server $svr | Disconnect-XenServer

  if ((Get-XenSession -Server $svr) -eq $null)
  {
    return $true
  }
  return $false
}

# End Connect Functions

# VM Functions

function destroy_vm([XenAPI.VM]$vm)
{
  if ($vm -eq $null)
  {
    return
  }

  log_info ("destroying vm '{0}'" -f $vm.name_label)
  
  $vdis = @()
  
  foreach($vbd in $vm.VBDs)
  {
	if((Get-XenVBDProperty -Ref $vbd -XenProperty Mode) -eq [XenAPI.vbd_mode]::RW)
	{
      $vdis += Get-XenVBDProperty -Ref $vbd -XenProperty VDI
	}
  }
  
  Remove-XenVM -VM $vm -Async -PassThru | Wait-XenTask -ShowProgress
  
  foreach($vdi in $vdis)
  {
    Remove-XenVDI -VDI $vdi -Async -PassThru | Wait-XenTask -ShowProgress
  }
}

function install_vm([String]$name, [String]$sr_name)
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

	$vms = Get-XenVM -Name $name

	foreach($vm in $vms)
  	{
  	  destroy_vm($vm)
  	}
	log_info "...success."
	break
  }

  #find a windows template
  log_info "looking for a Windows template..."
  $template = @(Get-XenVM -Name 'Windows XP*' | where {$_.is_a_template})[0]

  log_info ("installing vm '{0}' from template '{1}'" -f $template.name_label,$name)
  
  #clone template
  log_info ("cloning vm '{0}' to '{1}'" -f $template.name_label,$name)
  Invoke-XenVM -VM $template -XenAction Clone -NewName $name -Async `
                     -PassThru | Wait-XenTask -ShowProgress
  
  $vm = Get-XenVM -Name $name  
  $sr = Get-XenSR -Name $sr_name
  $other_config = $vm.other_config
  $other_config["disks"] = $other_config["disks"].Replace('sr=""', 'sr="{0}"' -f $sr.uuid)
  
  #add cd drive
  log_info ("creating cd drive for vm '{0}'" -f $vm.name_label)
  New-XenVBD -VM $vm -VDI $null -Userdevice 3 -Bootable $false -Mode RO `
             -Type CD -Unpluggable $true -Empty $true -OtherConfig @{} `
             -QosAlgorithmType "" -QosAlgorithmParams @{}

  Set-XenVM -VM $vm -OtherConfig $other_config
  
  #provision vm 
  log_info ("provisioning vm '{0}'" -f $vm.name_label)
  Invoke-XenVM -VM $vm -XenAction Provision -Async -PassThru | Wait-XenTask -ShowProgress
  
  return $true
}

function uninstall_vm([String]$name)
{
  log_info ("uninstalling vm '{0}'" -f $name)
   
  $vms = Get-XenVM -Name $name
  
  foreach($vm in $vms)
  {
    destroy_vm($vm)
  }
  
  return $true
}

function vm_can_boot($vm_name, [XenApi.Host[]] $servers)
{
  trap [Exception]
  {
    $script:exceptions += $_.Exception
	continue
  }

  $script:exceptions = @()
  foreach ($server in $servers)
  {
    Invoke-XenVM -Name $vm_name -XenAction AssertCanBootHere -XenHost $server
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

function start_vm([String]$vm_name)
{
  if (vm_can_boot $vm_name @(Get-XenHost))
  {
  	log_info ("starting vm '{0}'" -f $vm_name)
  }

  # even if we cant start it, attempt so we get the exception, reasons have been logged in vm_can_boot
  Invoke-XenVM -Name $vm_name -XenAction Start -Async -PassThru | Wait-XenTask -ShowProgress
  return Get-XenVM -Name $vm_name | Get-XenVMProperty -XenProperty PowerState
}

function shutdown_vm([String]$vm_name)
{
  log_info ("shutting down vm '{0}'" -f $vm_name)
  Invoke-XenVM -Name $vm_name -XenAction HardShutdown -Async -PassThru | Wait-XenTask -ShowProgress
  return (Get-XenVM -Name $vm_name).power_state
}

# End VM Functions

# Host Functions

function get_master()
{
  $pool = Get-XenPool
  return Get-XenHost -Ref $pool.master
}

# End Host Functions

# SR Functions

function get_default_sr()
{
  log_info ("getting default sr")
  $pool = Get-XenPool
  return (Get-XenPool).default_SR | Get-XenSR 
}

function create_nfs_sr([String]$sr_svr, [String]$sr_path, [String]$sr_name)
{
  log_info ("creating sr {0} at {1}:{2}" -f $sr_name,$sr_svr,$sr_path)
  $master = get_master
  $sr_opq = New-XenSR -XenHost $master -DeviceConfig @{ "server"=$sr_svr; "serverpath"=$sr_path; "options"="" } `
                  -PhysicalSize 0 -NameLabel $sr_name -NameDescription "" -Type "nfs" -ContentType "" `
                  -Shared $true -SmConfig @{} -Async -PassThru `
        | Wait-XenTask -ShowProgress -PassThru

  if ($sr_opq -eq $null)
  {
    return $false
  }
  return $true
}

function detach_nfs_sr([String]$sr_name)
{
  log_info ("destroying sr {0}" -f $sr_name)

  $pbds = Get-XenPBD
  $sr_opq = (Get-XenSR -Name $sr_name).opaque_ref

  foreach($pbd in $pbds)
  {
    if(($pbd.SR.opaque_ref -eq $sr_opq) -and $pbd.currently_attached)
    {
      Invoke-XenPBD -PBD $pbd -XenAction Unplug
    }
  }
  
  $sr_opq = Remove-XenSR -Name $sr_name -Async -PassThru | Wait-XenTask -ShowProgress
 
  if ($sr_opq -eq $null)
  {
    return $true
  }
  return $false
}

# End SR Functions

# Helper Functions

function append_random_string_to([String]$toAppend, $length = 10)
{
	$randomisedString = $toAppend
	$charSet = "0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray()
	for($i; $i -le $length; $i++)
	{
		$randomisedString += $charSet | Get-Random
	}
	return $randomisedString
}

# End Helper Functions

# Test List

$tests = @(
            @("Connect Server", "connect_server $svr $usr $pwd", $true),
			@("Create SR", "create_nfs_sr $sr_svr $sr_path PowerShellAutoTestSR", $true),
            @("Install VM", "install_vm PowerShellAutoTestVM PowerShellAutoTestSR", $true),
			@("Start VM", "start_vm PowerShellAutoTestVM", "Running"),
			@("Shutdown VM", "shutdown_vm PowerShellAutoTestVM", "Halted"),
			@("Uninstall VM", "uninstall_vm 'PowerShellAutoTestVM'", $true),
			@("Destroy SR", "detach_nfs_sr PowerShellAutoTestSR", $true),
            @("Disconnect Server", "disconnect_server $svr", $true)
          )
# End Test List

# Main Test Execution
$complete = 0;
$max = $tests.Count;

$fails = @{}

prep_xml_output $out_xml

$vmName = append_random_string_to "PowerShellAutoTestVM"
$srName = append_random_string_to "PowerShellAutoTestSR"

foreach($test in $tests)
{
  trap [Exception]
  {
    # we encountered an exception in running the test before it completed
	# its already been logged, so continue
	continue
  }
  $success = $false
  
  # Add randomness to the names of the test VM and SR to 
  # allow a parallel execution context
  $cmd = $test[1]
  $cmd = $cmd -replace "PowerShellAutoTestVM", $vmName
  $cmd = $cmd -replace "PowerShellAutoTestSR", $srName
  
  $success = exec $test[0] $cmd $test[2]
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

# SIG # Begin signature block
# MIIZKgYJKoZIhvcNAQcCoIIZGzCCGRcCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUoC0RdEK5oVpskxtI7EFxCZ3Q
# QOWgghQeMIID7jCCA1egAwIBAgIQfpPr+3zGTlnqS5p31Ab8OzANBgkqhkiG9w0B
# AQUFADCBizELMAkGA1UEBhMCWkExFTATBgNVBAgTDFdlc3Rlcm4gQ2FwZTEUMBIG
# A1UEBxMLRHVyYmFudmlsbGUxDzANBgNVBAoTBlRoYXd0ZTEdMBsGA1UECxMUVGhh
# d3RlIENlcnRpZmljYXRpb24xHzAdBgNVBAMTFlRoYXd0ZSBUaW1lc3RhbXBpbmcg
# Q0EwHhcNMTIxMjIxMDAwMDAwWhcNMjAxMjMwMjM1OTU5WjBeMQswCQYDVQQGEwJV
# UzEdMBsGA1UEChMUU3ltYW50ZWMgQ29ycG9yYXRpb24xMDAuBgNVBAMTJ1N5bWFu
# dGVjIFRpbWUgU3RhbXBpbmcgU2VydmljZXMgQ0EgLSBHMjCCASIwDQYJKoZIhvcN
# AQEBBQADggEPADCCAQoCggEBALGss0lUS5ccEgrYJXmRIlcqb9y4JsRDc2vCvy5Q
# WvsUwnaOQwElQ7Sh4kX06Ld7w3TMIte0lAAC903tv7S3RCRrzV9FO9FEzkMScxeC
# i2m0K8uZHqxyGyZNcR+xMd37UWECU6aq9UksBXhFpS+JzueZ5/6M4lc/PcaS3Er4
# ezPkeQr78HWIQZz/xQNRmarXbJ+TaYdlKYOFwmAUxMjJOxTawIHwHw103pIiq8r3
# +3R8J+b3Sht/p8OeLa6K6qbmqicWfWH3mHERvOJQoUvlXfrlDqcsn6plINPYlujI
# fKVOSET/GeJEB5IL12iEgF1qeGRFzWBGflTBE3zFefHJwXECAwEAAaOB+jCB9zAd
# BgNVHQ4EFgQUX5r1blzMzHSa1N197z/b7EyALt0wMgYIKwYBBQUHAQEEJjAkMCIG
# CCsGAQUFBzABhhZodHRwOi8vb2NzcC50aGF3dGUuY29tMBIGA1UdEwEB/wQIMAYB
# Af8CAQAwPwYDVR0fBDgwNjA0oDKgMIYuaHR0cDovL2NybC50aGF3dGUuY29tL1Ro
# YXd0ZVRpbWVzdGFtcGluZ0NBLmNybDATBgNVHSUEDDAKBggrBgEFBQcDCDAOBgNV
# HQ8BAf8EBAMCAQYwKAYDVR0RBCEwH6QdMBsxGTAXBgNVBAMTEFRpbWVTdGFtcC0y
# MDQ4LTEwDQYJKoZIhvcNAQEFBQADgYEAAwmbj3nvf1kwqu9otfrjCR27T4IGXTdf
# plKfFo3qHJIJRG71betYfDDo+WmNI3MLEm9Hqa45EfgqsZuwGsOO61mWAK3ODE2y
# 0DGmCFwqevzieh1XTKhlGOl5QGIllm7HxzdqgyEIjkHq3dlXPx13SYcqFgZepjhq
# IhKjURmDfrYwggSjMIIDi6ADAgECAhAOz/Q4yP6/NW4E2GqYGxpQMA0GCSqGSIb3
# DQEBBQUAMF4xCzAJBgNVBAYTAlVTMR0wGwYDVQQKExRTeW1hbnRlYyBDb3Jwb3Jh
# dGlvbjEwMC4GA1UEAxMnU3ltYW50ZWMgVGltZSBTdGFtcGluZyBTZXJ2aWNlcyBD
# QSAtIEcyMB4XDTEyMTAxODAwMDAwMFoXDTIwMTIyOTIzNTk1OVowYjELMAkGA1UE
# BhMCVVMxHTAbBgNVBAoTFFN5bWFudGVjIENvcnBvcmF0aW9uMTQwMgYDVQQDEytT
# eW1hbnRlYyBUaW1lIFN0YW1waW5nIFNlcnZpY2VzIFNpZ25lciAtIEc0MIIBIjAN
# BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAomMLOUS4uyOnREm7Dv+h8GEKU5Ow
# mNutLA9KxW7/hjxTVQ8VzgQ/K/2plpbZvmF5C1vJTIZ25eBDSyKV7sIrQ8Gf2Gi0
# jkBP7oU4uRHFI/JkWPAVMm9OV6GuiKQC1yoezUvh3WPVF4kyW7BemVqonShQDhfu
# ltthO0VRHc8SVguSR/yrrvZmPUescHLnkudfzRC5xINklBm9JYDh6NIipdC6Anqh
# d5NbZcPuF3S8QYYq3AhMjJKMkS2ed0QfaNaodHfbDlsyi1aLM73ZY8hJnTrFxeoz
# C9Lxoxv0i77Zs1eLO94Ep3oisiSuLsdwxb5OgyYI+wu9qU+ZCOEQKHKqzQIDAQAB
# o4IBVzCCAVMwDAYDVR0TAQH/BAIwADAWBgNVHSUBAf8EDDAKBggrBgEFBQcDCDAO
# BgNVHQ8BAf8EBAMCB4AwcwYIKwYBBQUHAQEEZzBlMCoGCCsGAQUFBzABhh5odHRw
# Oi8vdHMtb2NzcC53cy5zeW1hbnRlYy5jb20wNwYIKwYBBQUHMAKGK2h0dHA6Ly90
# cy1haWEud3Muc3ltYW50ZWMuY29tL3Rzcy1jYS1nMi5jZXIwPAYDVR0fBDUwMzAx
# oC+gLYYraHR0cDovL3RzLWNybC53cy5zeW1hbnRlYy5jb20vdHNzLWNhLWcyLmNy
# bDAoBgNVHREEITAfpB0wGzEZMBcGA1UEAxMQVGltZVN0YW1wLTIwNDgtMjAdBgNV
# HQ4EFgQURsZpow5KFB7VTNpSYxc/Xja8DeYwHwYDVR0jBBgwFoAUX5r1blzMzHSa
# 1N197z/b7EyALt0wDQYJKoZIhvcNAQEFBQADggEBAHg7tJEqAEzwj2IwN3ijhCcH
# bxiy3iXcoNSUA6qGTiWfmkADHN3O43nLIWgG2rYytG2/9CwmYzPkSWRtDebDZw73
# BaQ1bHyJFsbpst+y6d0gxnEPzZV03LZc3r03H0N45ni1zSgEIKOq8UvEiCmRDoDR
# EfzdXHZuT14ORUZBbg2w6jiasTraCXEQ/Bx5tIB7rGn0/Zy2DBYr8X9bCT2bW+IW
# yhOBbQAuOA2oKY8s4bL0WqkBrxWcLC9JG9siu8P+eJRRw4axgohd8D20UaF5Mysu
# e7ncIAkTcetqGVvP6KUwVyyJST+5z3/Jvz4iaGNTmr1pdKzFHTx/kuDDvBzYBHUw
# ggVzMIIEW6ADAgECAhBcLODY4Qf42EkmUHSSJ0KbMA0GCSqGSIb3DQEBBQUAMIG0
# MQswCQYDVQQGEwJVUzEXMBUGA1UEChMOVmVyaVNpZ24sIEluYy4xHzAdBgNVBAsT
# FlZlcmlTaWduIFRydXN0IE5ldHdvcmsxOzA5BgNVBAsTMlRlcm1zIG9mIHVzZSBh
# dCBodHRwczovL3d3dy52ZXJpc2lnbi5jb20vcnBhIChjKTEwMS4wLAYDVQQDEyVW
# ZXJpU2lnbiBDbGFzcyAzIENvZGUgU2lnbmluZyAyMDEwIENBMB4XDTEyMTExNTAw
# MDAwMFoXDTE0MTIxNTIzNTk1OVowgbYxCzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpD
# YWxpZm9ybmlhMRQwEgYDVQQHEwtTYW50YSBDbGFyYTEdMBsGA1UEChQUQ2l0cml4
# IFN5c3RlbXMsIEluYy4xPjA8BgNVBAsTNURpZ2l0YWwgSUQgQ2xhc3MgMyAtIE1p
# Y3Jvc29mdCBTb2Z0d2FyZSBWYWxpZGF0aW9uIHYyMR0wGwYDVQQDFBRDaXRyaXgg
# U3lzdGVtcywgSW5jLjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAL8s
# VW6kqOqV073ghymTNR36S0RmB1sCBwQA07I1i1tQXi945PA853HNcKmZoEm65Mkd
# TaO1myn4NLGgLSQSIygm0a74hHIwpDa62aTg9/GpGVG4DnEo5ZAEy7OQgCffeQqh
# LtvWuL8I/lgI8wkPf9YvWxxc7A0zwNPeLlVoRwAP4TspNpbeHjnMaSRJcbP7iz5J
# XZjzZ07RK1+7FmXcFUYyVfYeBVV9vkYxzwvLxApdu7UcMaEh5pAKacUPBu4f2dWT
# N53121LyE1+EFKHkr4P17ozd2RRYQTA8YfY8duUfU5WHLkByWRLzk36js3rOsoT5
# ZMFz+7MF+ZepEFJ1l6ECAwEAAaOCAXswggF3MAkGA1UdEwQCMAAwDgYDVR0PAQH/
# BAQDAgeAMEAGA1UdHwQ5MDcwNaAzoDGGL2h0dHA6Ly9jc2MzLTIwMTAtY3JsLnZl
# cmlzaWduLmNvbS9DU0MzLTIwMTAuY3JsMEQGA1UdIAQ9MDswOQYLYIZIAYb4RQEH
# FwMwKjAoBggrBgEFBQcCARYcaHR0cHM6Ly93d3cudmVyaXNpZ24uY29tL3JwYTAT
# BgNVHSUEDDAKBggrBgEFBQcDAzBxBggrBgEFBQcBAQRlMGMwJAYIKwYBBQUHMAGG
# GGh0dHA6Ly9vY3NwLnZlcmlzaWduLmNvbTA7BggrBgEFBQcwAoYvaHR0cDovL2Nz
# YzMtMjAxMC1haWEudmVyaXNpZ24uY29tL0NTQzMtMjAxMC5jZXIwHwYDVR0jBBgw
# FoAUz5mp6nsm9EvJjo/X8AUm7+PSp50wEQYJYIZIAYb4QgEBBAQDAgQQMBYGCisG
# AQQBgjcCARsECDAGAQEAAQH/MA0GCSqGSIb3DQEBBQUAA4IBAQCbX5oqpzPoND0Y
# Lh5LssZX7rsc7YNyCmraGiRtClme3Q1YWiDGyS2vzWzYC+68jWevCxPeXVAtw1Xv
# rkBs2+MVQw4+JghRu50FA2oa3XBArE5gWwECAZekoODb+ote/LLzdc+TdqxQVJAc
# Hfpe+71wej2zHe+2G1kGz9jgKZAiV/Cn8M5eXcjlpM18UOeyt2nt8S4IMvVMhKG8
# 1fxhCYujLLzElQnnW1o0w5jNw1CTYb0qNcW58NT8m+x0+5q2iFEtI+U876zzYTwc
# 67aJxMVVd2vRKH/f5fUipdnjJJbSWo7XnQxXi2/qnaGPdE/7dNaOIp24BlmIrv4U
# ITfhB4OAMIIGCjCCBPKgAwIBAgIQUgDlqiVW/BqG7ZbJ1EszxzANBgkqhkiG9w0B
# AQUFADCByjELMAkGA1UEBhMCVVMxFzAVBgNVBAoTDlZlcmlTaWduLCBJbmMuMR8w
# HQYDVQQLExZWZXJpU2lnbiBUcnVzdCBOZXR3b3JrMTowOAYDVQQLEzEoYykgMjAw
# NiBWZXJpU2lnbiwgSW5jLiAtIEZvciBhdXRob3JpemVkIHVzZSBvbmx5MUUwQwYD
# VQQDEzxWZXJpU2lnbiBDbGFzcyAzIFB1YmxpYyBQcmltYXJ5IENlcnRpZmljYXRp
# b24gQXV0aG9yaXR5IC0gRzUwHhcNMTAwMjA4MDAwMDAwWhcNMjAwMjA3MjM1OTU5
# WjCBtDELMAkGA1UEBhMCVVMxFzAVBgNVBAoTDlZlcmlTaWduLCBJbmMuMR8wHQYD
# VQQLExZWZXJpU2lnbiBUcnVzdCBOZXR3b3JrMTswOQYDVQQLEzJUZXJtcyBvZiB1
# c2UgYXQgaHR0cHM6Ly93d3cudmVyaXNpZ24uY29tL3JwYSAoYykxMDEuMCwGA1UE
# AxMlVmVyaVNpZ24gQ2xhc3MgMyBDb2RlIFNpZ25pbmcgMjAxMCBDQTCCASIwDQYJ
# KoZIhvcNAQEBBQADggEPADCCAQoCggEBAPUjS16l14q7MunUV/fv5Mcmfq0ZmP6o
# nX2U9jZrENd1gTB/BGh/yyt1Hs0dCIzfaZSnN6Oce4DgmeHuN01fzjsU7obU0PUn
# NbwlCzinjGOdF6MIpauw+81qYoJM1SHaG9nx44Q7iipPhVuQAU/Jp3YQfycDfL6u
# fn3B3fkFvBtInGnnwKQ8PEEAPt+W5cXklHHWVQHHACZKQDy1oSapDKdtgI6QJXvP
# vz8c6y+W+uWHd8a1VrJ6O1QwUxvfYjT/HtH0WpMoheVMF05+W/2kk5l/383vpHXv
# 7xX2R+f4GXLYLjQaprSnTH69u08MPVfxMNamNo7WgHbXGS6lzX40LYkCAwEAAaOC
# Af4wggH6MBIGA1UdEwEB/wQIMAYBAf8CAQAwcAYDVR0gBGkwZzBlBgtghkgBhvhF
# AQcXAzBWMCgGCCsGAQUFBwIBFhxodHRwczovL3d3dy52ZXJpc2lnbi5jb20vY3Bz
# MCoGCCsGAQUFBwICMB4aHGh0dHBzOi8vd3d3LnZlcmlzaWduLmNvbS9ycGEwDgYD
# VR0PAQH/BAQDAgEGMG0GCCsGAQUFBwEMBGEwX6FdoFswWTBXMFUWCWltYWdlL2dp
# ZjAhMB8wBwYFKw4DAhoEFI/l0xqGrI2Oa8PPgGrUSBgsexkuMCUWI2h0dHA6Ly9s
# b2dvLnZlcmlzaWduLmNvbS92c2xvZ28uZ2lmMDQGA1UdHwQtMCswKaAnoCWGI2h0
# dHA6Ly9jcmwudmVyaXNpZ24uY29tL3BjYTMtZzUuY3JsMDQGCCsGAQUFBwEBBCgw
# JjAkBggrBgEFBQcwAYYYaHR0cDovL29jc3AudmVyaXNpZ24uY29tMB0GA1UdJQQW
# MBQGCCsGAQUFBwMCBggrBgEFBQcDAzAoBgNVHREEITAfpB0wGzEZMBcGA1UEAxMQ
# VmVyaVNpZ25NUEtJLTItODAdBgNVHQ4EFgQUz5mp6nsm9EvJjo/X8AUm7+PSp50w
# HwYDVR0jBBgwFoAUf9Nlp8Ld7LvwMAnzQzn6Aq8zMTMwDQYJKoZIhvcNAQEFBQAD
# ggEBAFYi5jSkxGHLSLkBrVaoZA/ZjJHEu8wM5a16oCJ/30c4Si1s0X9xGnzscKmx
# 8E/kDwxT+hVe/nSYSSSFgSYckRRHsExjjLuhNNTGRegNhSZzA9CpjGRt3HGS5kUF
# YBVZUTn8WBRr/tSk7XlrCAxBcuc3IgYJviPpP0SaHulhncyxkFz8PdKNrEI9ZTbU
# tD1AKI+bEM8jJsxLIMuQH12MTDTKPNjlN9ZvpSC9NOsm2a4N58Wa96G0IZEzb4bo
# WLslfHQOWP51G2M/zjF8m48blp7FU3aEW5ytkfqs7ZO6XcghU8KCU2OvEg1QhxEb
# PVRSloosnD2SGgiaBS7Hk6VIkdMxggR2MIIEcgIBATCByTCBtDELMAkGA1UEBhMC
# VVMxFzAVBgNVBAoTDlZlcmlTaWduLCBJbmMuMR8wHQYDVQQLExZWZXJpU2lnbiBU
# cnVzdCBOZXR3b3JrMTswOQYDVQQLEzJUZXJtcyBvZiB1c2UgYXQgaHR0cHM6Ly93
# d3cudmVyaXNpZ24uY29tL3JwYSAoYykxMDEuMCwGA1UEAxMlVmVyaVNpZ24gQ2xh
# c3MgMyBDb2RlIFNpZ25pbmcgMjAxMCBDQQIQXCzg2OEH+NhJJlB0kidCmzAJBgUr
# DgMCGgUAoHQwFAYKKwYBBAGCNwIBDDEGMASgAoAAMBkGCSqGSIb3DQEJAzEMBgor
# BgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEVMCMGCSqGSIb3
# DQEJBDEWBBTxbGWQJZIp/lHNZWkzpjCSXcEx2zANBgkqhkiG9w0BAQEFAASCAQBV
# P4cu3iAMVrF/2NhRLbtCchIK1OD0JLC8k8J0a1oWw+c9VLXlA4xHG+eROQJ34HS+
# /etcBIMfutgSWbpNNU5zeXVBQsHH7QMz+dfm8mMbyQzZpnQiK/OEpwDZ2tiBC+Mi
# 2a3C4csAkriJqk5g72X+za44GI37QtFbQQ0OqVkjFv1c/CZH1OG65nkTBUA0y4tw
# 4iaD9N5AarBXU8xQsKchKWk3q9yawlMpHselke8Zlc2xK2e5CTKaJ1w6K1hVbYbN
# V74aOMHMb7sjE56iV6LMZ4ONbVoEvyWJeyuFfLh4v3Mg4BzPd9ePaIxnb/vjzpAg
# JEjCAoG/OIkX4LoT6ScpoYICCzCCAgcGCSqGSIb3DQEJBjGCAfgwggH0AgEBMHIw
# XjELMAkGA1UEBhMCVVMxHTAbBgNVBAoTFFN5bWFudGVjIENvcnBvcmF0aW9uMTAw
# LgYDVQQDEydTeW1hbnRlYyBUaW1lIFN0YW1waW5nIFNlcnZpY2VzIENBIC0gRzIC
# EA7P9DjI/r81bgTYapgbGlAwCQYFKw4DAhoFAKBdMBgGCSqGSIb3DQEJAzELBgkq
# hkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTE0MTIxMDA4NDI0NlowIwYJKoZIhvcN
# AQkEMRYEFCj3ln5GYM1sgduA9cQTgYlb6ytWMA0GCSqGSIb3DQEBAQUABIIBAJoQ
# NaIwAw63vSzLGFEO832ZJU2VMlBwhYxhjjNsig4QH88hoWR7YHuGxd2vtMMuLF8u
# oK44HfCmcFcaq8NCmIcnJUbgYUCDdG4PX+ELzCoWd6FzAzJiegj+4Hn4r8LC6E/G
# 1/AX7BmweH3DDDsi32yMW7UpzQPpvHmcgN3ajNPgkbXQOlnZKIiZ4L59kzZx52am
# +Pcl+fTdVomlstx8eTUDe5rqTtThxNGZbndQxMkDk+ZCdhG98Q3LyFFYGqBm3JMZ
# vMq4s2fQD8dBIaQaD0NKKHCQuk2X8z5RB1uFzHrgmjO0pkzNbMUSVjcbGEwiCB6R
# 0CFTJWIB2fBuD+dnhYk=
# SIG # End signature block
