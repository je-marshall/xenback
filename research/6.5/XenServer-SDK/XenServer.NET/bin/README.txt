XenServer.NET
=============

Version 6.5.0

XenServer.NET is a complete SDK for Citrix XenServer,
exposing the XenServer API as .NET classes. It is written in C#.

For XenServer documentation, see http://docs.xensource.com.
XenServer.NET includes a class for every XenServer class, and a method for
each XenServer API call, so API documentation and examples written for
for other languages will apply equally well to .NET.
In particular, the SDK Guide and API Documentation are ideal for developers
wishing to use XenServer.NET.

For community content, blogs, and downloads, visit the XenServer Developer
Network at http://community.citrix.com/cdn/xs.

XenServer.NET is free sofware. You can redistribute and modify it under the
terms of the BSD license. See LICENSE.txt for details.

This library may be accompanied by pedagogical examples. These do not form
part of this library, and are licensed for redistribution and modification
under the BSD license. Such examples are licensed clearly at the top
of each file.


Dependencies
------------

XenServer.NET is dependent upon XML-RPC.NET (aka CookComputing.XmlRpcV2.dll),
by Charles Cook. We would like to thank Charles for his contribution.
XML-RPC.NET is licensed under the MIT X11 license; see
LICENSE.CookComputing.XmlRpcV2 for details.


Downloads
---------

XenServer.NET is available in the XenServer-SDK-6.5.0.zip in three separate
folders, one for the compiled binaries, one for the source code, and one
containing sample code.

The XenServer-SDK-6.5.0.zip is available from
http://www.citrix.com/downloads/xenserver/.

XML-RPC.NET is available from http://www.xml-rpc.net.
Getting Started
---------------

1.  Extract the XenServer.NET binary archive.

2.  Move both DLLs into your own workspace.

3.  In Visual Studio, add references to both DLLs from your own program.
    Project > Add Reference > Browse.

4.  You should now be ready to compile against XenServer.NET. Refer to
    our sample code (see Downloads above) for examples on how to use the
    library.
