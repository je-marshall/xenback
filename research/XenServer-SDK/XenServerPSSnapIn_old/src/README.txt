Citrix XenServer PowerShell Snap-In
===================================

Version 6.2.0-1.

The XenServer PowerShell Snap-In (XS-PS) is a complete SDK for Citrix XenServer,
exposing the XenServer API as Windows PowerShell 1.0 cmdlets.

For XenServer documentation, see http://docs.xensource.com.
XS-PS includes a cmdlet for each XenServer API call, so API documentation
and examples written for other languages will apply equally well to
PowerShell. In particular, the SDK Guide and API Documentation are ideal
for developers wishing to use this snap-in.

For community content, blogs, and downloads, visit the XenServer Developer
Network at http://community.citrix.com/cdn/xs.

This snap-in is free sofware. You can redistribute and modify it under the
terms of the BSD license. See LICENSE.txt for details.

This library may be accompanied by pedagogical examples. These do not form
part of this library, and are licensed for redistribution and modification
under the BSD license. Such examples are licensed clearly at the top
of each file.


Dependencies
------------

XS-PS is dependent upon XML-RPC.NET (aka CookComputing.XmlRpcV2.dll),
by Charles Cook. We would like to thank Charles for his contribution.
XML-RPC.NET is licensed under the MIT X11 license; see
LICENSE.CookComputing.XmlRpcV2 for details.


Downloads
---------

The XenServerPSSnapIn-6.2.0-1.msi installer
is available in the XenServer-6.2.0-SDK.zip. The source code
and sample code can be found in two separate folders in the same zip file.

The XenServer-6.2.0-SDK.zip is available from
http://www.citrix.com/downloads/xenserver/.

XML-RPC.NET is available from http://www.xml-rpc.net.


Getting Started
---------------

1.  Install XenServerPSSnapIn-6.2.0-1.msi.

2.  Start > Windows PowerShell 1.0 > Right click Windows PowerShell >
    Run as administrator.
	Note that in Vista you may need to "Run as administrator" even if your
	current user is privileged.

3.  Determine the current execution policy, like this:
    PS> Get-ExecutionPolicy

	If the current policy is Restricted, then you need to set it to
	RemoteSigned, like this:
	PS> Set-ExecutionPolicy RemoteSigned
	
	You should understand the security implications of this change. If you
	are unsure, see Microsoft's documentation on the matter:
	PS> Get-Help about_signing
	
	If the current policy is AllSigned, then this will work, but will be
	very inconvenient. You probably want to change this to RemoteSigned,
	as above.
	
	If the current policy is Unrestricted or RemoteSigned, then this is
	compatible with XS-PS, so there is nothing to do.

4.  Exit the privileged instance of PowerShell.

5.  Start > XenServer PowerShell SnapIn.

    PS> Get-Help about_XenServer
    PS> Connect-XenServer -url https://<servername>
    PS> Get-XenServer:VM
    PS> Get-Help Invoke-XenServer:VM.Start
    PS> Disconnect-XenServer
