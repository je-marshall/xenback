# This file was automatically generated by pywxrc.
# -*- coding: UTF-8 -*-

import wx
import wx.xrc as xrc

__res = None

def get_resources():
    """ This function provides access to the XML resources in this module."""
    global __res
    if __res == None:
        __init_resources()
    return __res




class xrcmainFrame(wx.Frame):
#!XRCED:begin-block:xrcmainFrame.PreCreate
    def PreCreate(self, pre):
        """ This function is called during the class's initialization.
        
        Override it for custom setup before the window is created usually to
        set additional window styles using SetWindowStyle() and SetExtraStyle().
        """
        pass
        
#!XRCED:end-block:xrcmainFrame.PreCreate

    def __init__(self, parent):
        # Two stage creation (see http://wiki.wxpython.org/index.cgi/TwoStageCreation)
        pre = wx.PreFrame()
        self.PreCreate(pre)
        get_resources().LoadOnFrame(pre, parent, "mainFrame")
        self.PostCreate(pre)

        # Define variables for the controls, bind event handlers

        self.Bind(wx.EVT_BUTTON, self.OnButton_hostButton, id=xrc.XRCID('hostButton'))
        self.Bind(wx.EVT_COMBOBOX, self.OnCombobox_srCombo, id=xrc.XRCID('srCombo'))
        self.Bind(wx.EVT_RADIOBOX, self.OnRadiobox_vmRadioBox, id=xrc.XRCID('vmRadioBox'))
        self.Bind(wx.EVT_TEXT, self.OnText_vmTextCtrl, id=xrc.XRCID('vmTextCtrl'))
        self.Bind(wx.EVT_BUTTON, self.OnButton_vmButton, id=xrc.XRCID('vmButton'))
        self.Bind(wx.EVT_BUTTON, self.OnButton_backupButton, id=xrc.XRCID('backupButton'))
        self.Bind(wx.EVT_BUTTON, self.OnButton_quitButton, id=xrc.XRCID('quitButton'))

#!XRCED:begin-block:xrcmainFrame.OnButton_hostButton
    def OnButton_hostButton(self, evt):
        # Replace with event handler code
        print "OnButton_hostButton()"
#!XRCED:end-block:xrcmainFrame.OnButton_hostButton        

#!XRCED:begin-block:xrcmainFrame.OnCombobox_srCombo
    def OnCombobox_srCombo(self, evt):
        # Replace with event handler code
        print "OnCombobox_srCombo()"
#!XRCED:end-block:xrcmainFrame.OnCombobox_srCombo        

#!XRCED:begin-block:xrcmainFrame.OnRadiobox_vmRadioBox
    def OnRadiobox_vmRadioBox(self, evt):
        # Replace with event handler code
        print "OnRadiobox_vmRadioBox()"
#!XRCED:end-block:xrcmainFrame.OnRadiobox_vmRadioBox        

#!XRCED:begin-block:xrcmainFrame.OnText_vmTextCtrl
    def OnText_vmTextCtrl(self, evt):
        # Replace with event handler code
        print "OnText_vmTextCtrl()"
#!XRCED:end-block:xrcmainFrame.OnText_vmTextCtrl        

#!XRCED:begin-block:xrcmainFrame.OnButton_vmButton
    def OnButton_vmButton(self, evt):
        # Replace with event handler code
        print "OnButton_vmButton()"
#!XRCED:end-block:xrcmainFrame.OnButton_vmButton        

#!XRCED:begin-block:xrcmainFrame.OnButton_backupButton
    def OnButton_backupButton(self, evt):
        # Replace with event handler code
        print "OnButton_backupButton()"
#!XRCED:end-block:xrcmainFrame.OnButton_backupButton        

#!XRCED:begin-block:xrcmainFrame.OnButton_quitButton
    def OnButton_quitButton(self, evt):
        # Replace with event handler code
        print "OnButton_quitButton()"
#!XRCED:end-block:xrcmainFrame.OnButton_quitButton        


class xrcvmDialog(wx.Dialog):
#!XRCED:begin-block:xrcvmDialog.PreCreate
    def PreCreate(self, pre):
        """ This function is called during the class's initialization.
        
        Override it for custom setup before the window is created usually to
        set additional window styles using SetWindowStyle() and SetExtraStyle().
        """
        pass
        
#!XRCED:end-block:xrcvmDialog.PreCreate

    def __init__(self, parent):
        # Two stage creation (see http://wiki.wxpython.org/index.cgi/TwoStageCreation)
        pre = wx.PreDialog()
        self.PreCreate(pre)
        get_resources().LoadOnDialog(pre, parent, "vmDialog")
        self.PostCreate(pre)

        # Define variables for the controls, bind event handlers

        self.Bind(wx.EVT_TREE_SEL_CHANGED, self.OnTree_sel_changed_vmTreeList, id=xrc.XRCID('vmTreeList'))
        self.Bind(wx.EVT_BUTTON, self.OnButton_OkButton, id=xrc.XRCID('OkButton'))
        self.Bind(wx.EVT_BUTTON, self.OnButton_CancelButton, id=xrc.XRCID('CancelButton'))

#!XRCED:begin-block:xrcvmDialog.OnTree_sel_changed_vmTreeList
    def OnTree_sel_changed_vmTreeList(self, evt):
        # Replace with event handler code
        print "OnTree_sel_changed_vmTreeList()"
#!XRCED:end-block:xrcvmDialog.OnTree_sel_changed_vmTreeList        

#!XRCED:begin-block:xrcvmDialog.OnButton_OkButton
    def OnButton_OkButton(self, evt):
        # Replace with event handler code
        print "OnButton_OkButton()"
#!XRCED:end-block:xrcvmDialog.OnButton_OkButton        

#!XRCED:begin-block:xrcvmDialog.OnButton_CancelButton
    def OnButton_CancelButton(self, evt):
        # Replace with event handler code
        print "OnButton_CancelButton()"
#!XRCED:end-block:xrcvmDialog.OnButton_CancelButton        


class xrcpassDialog(wx.Dialog):
#!XRCED:begin-block:xrcpassDialog.PreCreate
    def PreCreate(self, pre):
        """ This function is called during the class's initialization.
        
        Override it for custom setup before the window is created usually to
        set additional window styles using SetWindowStyle() and SetExtraStyle().
        """
        pass
        
#!XRCED:end-block:xrcpassDialog.PreCreate

    def __init__(self, parent):
        # Two stage creation (see http://wiki.wxpython.org/index.cgi/TwoStageCreation)
        pre = wx.PreDialog()
        self.PreCreate(pre)
        get_resources().LoadOnDialog(pre, parent, "passDialog")
        self.PostCreate(pre)

        # Define variables for the controls, bind event handlers

        self.Bind(wx.EVT_TEXT, self.OnText_passTextCtrl, id=xrc.XRCID('passTextCtrl'))
        self.Bind(wx.EVT_BUTTON, self.OnButton_OkButton, id=xrc.XRCID('OkButton'))
        self.Bind(wx.EVT_BUTTON, self.OnButton_CancelButton, id=xrc.XRCID('CancelButton'))

#!XRCED:begin-block:xrcpassDialog.OnText_passTextCtrl
    def OnText_passTextCtrl(self, evt):
        # Replace with event handler code
        print "OnText_passTextCtrl()"
#!XRCED:end-block:xrcpassDialog.OnText_passTextCtrl        

#!XRCED:begin-block:xrcpassDialog.OnButton_OkButton
    def OnButton_OkButton(self, evt):
        # Replace with event handler code
        print "OnButton_OkButton()"
#!XRCED:end-block:xrcpassDialog.OnButton_OkButton        

#!XRCED:begin-block:xrcpassDialog.OnButton_CancelButton
    def OnButton_CancelButton(self, evt):
        # Replace with event handler code
        print "OnButton_CancelButton()"
#!XRCED:end-block:xrcpassDialog.OnButton_CancelButton        




# ------------------------ Resource data ----------------------

def __init_resources():
    global __res
    __res = xrc.EmptyXmlResource()

    __res.Load('XenBackupGui.xrc')