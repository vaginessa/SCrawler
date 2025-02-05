﻿' Copyright (C) 2023  Andy https://github.com/AAndyProgram
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY
Imports SCrawler.API
Imports SCrawler.API.Base
Imports SCrawler.DownloadObjects.STDownloader
Imports PersonalUtilities.Tools.Notifications
Imports NotifyObj = SCrawler.SettingsCLS.NotificationObjects
Friend Class MainFrameObjects : Implements INotificator
    Friend ReadOnly Property MF As MainFrame
    Private WithEvents Notificator As NotificationsManager
    Friend ReadOnly Property PauseButtons As DownloadObjects.AutoDownloaderPauseButtons
    Friend Sub New(ByRef f As MainFrame)
        MF = f
        Notificator = New NotificationsManager
        PauseButtons = New DownloadObjects.AutoDownloaderPauseButtons(DownloadObjects.AutoDownloaderPauseButtons.ButtonsPlace.MainFrame)
        ProgramLogInitialize()
        With ProgramLog
            AddHandler .TextAdded, AddressOf ProgramLog_TextAdded
            AddHandler .TextCleared, AddressOf ProgramLog_TextCleared
        End With
    End Sub
#Region "Users"
    Friend Sub FocusUser(ByVal Key As String, Optional ByVal ActivateForm As Boolean = False)
        MF.FocusUser(Key, ActivateForm)
    End Sub
#End Region
#Region "Image handlers"
    Friend Sub ImageHandler(ByVal User As IUserData)
        ImageHandler(User, False)
        ImageHandler(User, True)
    End Sub
    Friend Sub ImageHandler(ByVal User As IUserData, ByVal Add As Boolean)
        Try
            If Add Then
                AddHandler User.UserUpdated, AddressOf MF.User_OnUserUpdated
            Else
                RemoveHandler User.UserUpdated, AddressOf MF.User_OnUserUpdated
            End If
        Catch
        End Try
    End Sub
    Friend Sub CollectionHandler(ByVal [Collection] As UserDataBind)
        Try
            AddHandler Collection.OnCollectionSelfRemoved, AddressOf MF.CollectionRemoved
            AddHandler Collection.OnUserRemoved, AddressOf MF.UserRemovedFromCollection
        Catch
        End Try
    End Sub
#End Region
#Region "Form functions"
    Friend Sub Focus(Optional ByVal Show As Boolean = False)
        ControlInvokeFast(MF, Sub()
                                  If Not MF.Visible And Show Then MF.Show()
                                  If MF.Visible Then MF.BringToFront() : MF.Activate()
                              End Sub)
    End Sub
    Friend Sub ChangeCloseVisible()
        ControlInvokeFast(MF.TRAY_CONTEXT, Sub() MF.BTT_TRAY_CLOSE_NO_SCRIPT.Visible =
                                                 Settings.ClosingCommand.Attribute And Not Settings.ClosingCommand.IsEmptyString)
    End Sub
    Friend Sub UpdateLogButton()
        MyMainLOG_UpdateLogButton(MF.BTT_LOG, MF.Toolbar_TOP)
    End Sub
    Friend Function GetUserListProvider(ByVal WithCollections As Boolean) As IFormatProvider
        Return MF.GetUserListProvider(WithCollections)
    End Function
    Friend Sub ShowLog()
        MyMainLOG_ShowForm(Settings.Design,,,, Sub()
                                                   UpdateLogButton()
                                                   LogFormClosed()
                                               End Sub)
    End Sub
#End Region
#Region "Notifications"
    Private Sub INotificator_ShowNotification(ByVal Text As String, ByVal Image As SFile) Implements INotificator.ShowNotification
        If Settings.ProcessNotification(NotifyObj.STDownloader) Then Notification.ShowNotification(Text,, $"{NotificationInternalKey}_{NotifyObj.STDownloader}", Image)
    End Sub
    Private Const NotificationInternalKey As String = "NotificationInternalKey"
    Friend Sub ShowNotification(ByVal Sender As NotifyObj, ByVal Message As String)
        If Settings.ProcessNotification(Sender) Then
            Using n As New Notification(Message) With {.Key = $"{NotificationInternalKey}_{Sender}"} : n.Show() : End Using
        End If
    End Sub
    Friend Sub ClearNotifications() Implements INotificator.Clear
        Notificator.Clear()
    End Sub
    Private Sub Notificator_OnClicked(ByVal Key As String) Handles Notificator.OnClicked
        If Not Key.IsEmptyString Then
            Dim found As Boolean = False
            Dim activateForm As Boolean = False
            If Key.StartsWith(NotificationInternalKey) Then
                Select Case Key
                    Case $"{NotificationInternalKey}_{NotifyObj.Channels}" : MF.MyChannels.FormShowS()
                    Case $"{NotificationInternalKey}_{NotifyObj.SavedPosts}" : MF.MySavedPosts.FormShowS()
                    Case $"{NotificationInternalKey}_{NotifyObj.STDownloader}" : VideoDownloader.FormShowS()
                    Case $"{NotificationInternalKey}_{NotifyObj.LOG}" : ControlInvokeFast(MF, AddressOf ShowLog, EDP.LogMessageValue)
                    Case Else : Focus(True)
                End Select
            ElseIf Settings.Automation Is Nothing OrElse Not Settings.Automation.NotificationClicked(Key, found, activateForm) Then
                Focus(True)
            ElseIf found Then
                Focus(activateForm)
            Else
                Focus(True)
            End If
        End If
    End Sub
#End Region
#Region "LOG events support"
    Private _LogNotificationsEnabled As Boolean = True
    Private Sub ProgramLog_TextAdded(ByVal Sender As Object, ByVal e As EventArgs)
        If _LogNotificationsEnabled Then _LogNotificationsEnabled = False : ShowNotification(NotifyObj.LOG, "There is new data in the log")
    End Sub
    Private Sub ProgramLog_TextCleared(ByVal Sender As Object, ByVal e As EventArgs)
        _LogNotificationsEnabled = True
    End Sub
    Friend Sub LogFormClosed()
        _LogNotificationsEnabled = True
    End Sub
#End Region
End Class