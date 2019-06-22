﻿using System;
using System.Collections.Generic;
using System.Text;
using SyncService.Modules;

namespace SyncService
{
    public class SyncService : ICheetahSyncService
    {
        public event OnLogHandler OnLog;

        public bool IsRunning { get; private set; }
        public string SavePath { get; private set; }
        public string UID { get; private set; }
        public string SyncPath { get; private set; }
        public DateTime LastDomesticChangeTime { get; private set; }
        public IDictionary<string, DateTime> LastKnownChangeTime { get; private set; }

        private IList<IModule> modules;

        public SyncService()
        {
            modules = new List<IModule>();
        }

        public void Abort()
        {
            IsRunning = false;
            // TODO: deactivate modules
            OnLog?.Invoke(this, new OnLogHandlerArgs("Service Aborted", LogReason.STATUSCHANGE));
        }


        public void ShutDown()
        {
            IsRunning = false;
            // TODO: save everything & deactivate modules
            OnLog?.Invoke(this, new OnLogHandlerArgs("Service Shut down", LogReason.STATUSCHANGE));
        }

        public void StartUp(string uid, string syncPath, string savePath)
        {
            IsRunning = true;
            UID = uid;
            SyncPath = syncPath;
            SavePath = savePath;
        }
    }
}