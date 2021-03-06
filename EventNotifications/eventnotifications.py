﻿from shootblues.common import log, remoteCall, playSound, showBalloonTip, showMessageBox, pid
from shootblues.common.service import forceStart, forceStop
from shootblues.common.messaging import subscribe, unsubscribe
from shootblues.common.messaging import send as messageSend
import types
import json

notificationSettings = {}
serviceRunning = False

def notifySettingsChanged(newSettingsJson):
    global notificationSettings
    
    notificationSettings = json.loads(newSettingsJson)

def DefineEvent(eventName):
    remoteCall("EventNotifications.Script.dll", "DefineEvent", eventName)

def joinLines(text):
    if isinstance(text, str) or isinstance(text, unicode):
        return text
    else:
        try:
            iterator = iter(text)
            del iterator
            return "\r\n".join(list(text))
        except TypeError:
            return repr(text)

def getLines(text):
    if isinstance(text, str) or isinstance(text, unicode):
        return [text]
    else:
        try:
            iterator = iter(text)
            del iterator
            return list(text)
        except TypeError:
            return [repr(text)]

def handleEvent(source, name, data):
    global notificationSettings
    
    import shootblues.common
    if source != shootblues.common.pid:
        return
    
    settings = notificationSettings.get(name, None)
    if settings:
        sound = data.get("sound", settings.get("sound", None))
        
        if sound:
            playSound(str(sound))
            
        text = joinLines(data.get("text", source))
        
        if settings.get("balloonTip", False):
            showBalloonTip(str(data.get("title", name)), text)
        if settings.get("messageBox", False):
            showMessageBox(str(data.get("title", name)), text)
        
        endpoint = settings.get("endpoint", None)
        if endpoint:
            endpoint = str(endpoint)
            body = getLines(data.get("text", name))
            for line in body:
                sendToEndpoint(endpoint, str(line))

def sendToEndpoint(endpoint, body):
    remoteCall("EventNotifications.Script.dll", "SendToEndpoint", endpoint, body)

def fireEvent(name, **extraData):
    messageSend(name, **extraData)

def initialize():
    global serviceRunning
    if not serviceRunning:
        serviceRunning = True
        subscribe(handleEvent)

def __unload__():
    global serviceRunning
    if serviceRunning:
        serviceRunning = False
        unsubscribe(handleEvent)