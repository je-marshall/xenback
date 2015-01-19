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


Param([Parameter(Mandatory=$true)][String]$svr,
        [Parameter(Mandatory=$true)][String]$usr,
        [Parameter(Mandatory=$true)][String]$pwd,
        [Parameter(Mandatory=$true)][String]$patchPath)

### Connect to a server

Connect-XenServer -Server $svr -UserName $usr -Password $pwd


### Create a VM

$template = @(Get-XenVM -Name 'Windows XP*' | where {$_.is_a_template})[0]

Invoke-XenVM -VM $template -XenAction Clone -NewName "testVM" -Async `
             -PassThru | Wait-XenTask -ShowProgress
  
$vm = Get-XenVM -Name "testVM"  
$sr = Get-XenSR -Ref (Get-XenPool).default_SR
$other_config = $vm.other_config
$other_config["disks"] = $other_config["disks"].Replace('sr=""', 'sr="{0}"' -f $sr.uuid)

New-XenVBD -VM $vm -VDI $null -Userdevice 3 -Bootable $false -Mode RO `
           -Type CD -Unpluggable $true -Empty $true -OtherConfig @{} `
           -QosAlgorithmType "" -QosAlgorithmParams @{}

Set-XenVM -VM $vm -OtherConfig $other_config
Invoke-XenVM -VM $vm -XenAction Provision -Async -PassThru | Wait-XenTask -ShowProgress


# Export the VM using the DataCopiedDelegate parameter to track bytes received

$path = $env:TEMP + "\vm.xva"

$trackDataReceived = [XenAPI.HTTP+DataCopiedDelegate]{
    param($bytes);
    Write-Host "Bytes received: $bytes" }

Export-XenVm -XenHost $svr -Uuid $vm.uuid -Path $path -DataCopiedDelegate $trackDataReceived

$vm | Remove-XenVM


### Import the previously exported VM using the ProgressDelegate parameter to track send progress

$trackProgress = [XenAPI.HTTP+UpdateProgressDelegate]{
    param($percent);
    Write-Progress -Activity "Importing Vm..." -PercentComplete $percent }

Import-XenVm -XenHost $svr -Path $path -ProgressDelegate $trackProgress


### Upload a patch

$trackProgress = [XenAPI.HTTP+UpdateProgressDelegate]{
    param($percent);
    Write-Progress -Activity "Uploading patch..." -PercentComplete $percent }

Send-XenPoolPatch -XenHost $svr -Path $patchPath


### Get host RRDs

$path = $env:TEMP + "\rrd.xml"
Receive-XenHostRrd -XenHost $svr -Path $path -DataCopiedDelegate $trackDataReceived


### Disconnect before finishing

Get-XenSession | Disconnect-XenServer
# SIG # Begin signature block
# MIIZKgYJKoZIhvcNAQcCoIIZGzCCGRcCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUH301+H38rcLFUx3KNdEUP38+
# l4CgghQeMIID7jCCA1egAwIBAgIQfpPr+3zGTlnqS5p31Ab8OzANBgkqhkiG9w0B
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
# DQEJBDEWBBT8RhSZsgUGcIbSNON7ngDGA7qZCDANBgkqhkiG9w0BAQEFAASCAQA5
# +up1FNid7ARq5JvnvVQJo6jy6WjPr17PFIQV67rKTmF1LR4qqwz/g5DLaa5pmCh1
# +GxLoVdmOGOAP7fErsNAmpRXmrLEI+x5i6/dkqEKKuCvkUUJfhCuKrLmXvaGMzpy
# KcweuOPHRT3QVueZ5VSvMGyiZ3ykM/Wufs+Pyr9qtZ2M2urZg7TQQhLMDVH9rwTe
# wFpChmisbxBBkOqdBhCnzUEffqT66eMQ7KfklvxfROwKIrAFO3rMvG4B2LwkHZCC
# vW8kuIJToiHC49D1qbLn6BqlZH1rMWv+37dHrbBbzj1yuDmdfxrubigzWrYFr+nX
# wOIH+1N1vh+30ZhDIMNqoYICCzCCAgcGCSqGSIb3DQEJBjGCAfgwggH0AgEBMHIw
# XjELMAkGA1UEBhMCVVMxHTAbBgNVBAoTFFN5bWFudGVjIENvcnBvcmF0aW9uMTAw
# LgYDVQQDEydTeW1hbnRlYyBUaW1lIFN0YW1waW5nIFNlcnZpY2VzIENBIC0gRzIC
# EA7P9DjI/r81bgTYapgbGlAwCQYFKw4DAhoFAKBdMBgGCSqGSIb3DQEJAzELBgkq
# hkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTE0MTIxMDA4NDI1NlowIwYJKoZIhvcN
# AQkEMRYEFM2caBcqQoO2iGrtrccu5Q8xdNxLMA0GCSqGSIb3DQEBAQUABIIBAIlf
# mBYt5euJFstQJA3MkkbzFMJavWZ6GC+LRdKjsMnrJlN99jZE4oOr41dRQmwEVGT4
# AbyCC3iJjEkqCiUhajgQsdv5TYtaZ4HDiinosEYIc2ZaHstWeS0UnWuN5beSaZKt
# EtPsTz5w18DiIaG0k2oWxypHwPZnHLvqWw3yWTMo90Q7sEfU9G3Hge5cRRUKXn6I
# /ZH2gqtxYpkjnuUV3HvRj5LPTskKh7clfk+XZebnMkex6fzW6OUhcy9HEMYY2Qz7
# QEAShrbYFbD87jl/VrVU9du+wF4If+x32Rc43HvlHSSd88/I6bGSQaMSB7QrnOV5
# aiZi5tN2f8hjCtuGDLQ=
# SIG # End signature block
