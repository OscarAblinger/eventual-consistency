﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using SharedClasses;

namespace ViewService
{
    public class ViewService : ICheetahViewService
    {
        private string ServiceId { get; set; }
        private bool IsRunning { get; set; } = false;
        private string FilePath;
        private ViewDataObject viewDO;

        private bool viewsChanged = true;

        private RPCViewServiceServer rpcServer = null;
        private IConnection Connection { get; set; } = null;
        private IModel Channel { get; set; } = null;

        private Timer viewUpdateTimer = null;


        private readonly static object addViewsLock = new object();

        public event OnViewUpdateHandler OnViewUpdate;
        public event OnLogHandler OnLog;

        public void Abort()
        {
            if (IsRunning)
            {
                IsRunning = false;
                viewUpdateTimer.Dispose();
                viewUpdateTimer = null;
                Connection.Close();

                rpcServer.Dispose();
                rpcServer = null;
            }
        }

        public int GetViewCount()
        {
            if (IsRunning)
            {
                lock (addViewsLock)
                {
                    return viewDO.TotalViews;
                }
            } else
            {
                throw new NotSetupException(
                    "Tried to get view count on stopped service");
            }
        }

        public void ShutDown()
        {
            if (IsRunning)
            {
                IsRunning = false;
                viewUpdateTimer.Dispose();
                viewUpdateTimer = null;
                Connection.Close();

                rpcServer.Dispose();
                rpcServer = null;

                PersistData();
            }            
        }

        private void PersistData()
        {
            string json = viewDO.ToJson();
            File.WriteAllText(FilePath, json);
        }

        public void StartUp(string uid, string contextPath)
        {
            ServiceId = uid;
            IsRunning = true;
            viewsChanged = true;

            FilePath = contextPath;

            if (!File.Exists(FilePath))
                viewDO = new ViewDataObject();
            else
            {
                string json = File.ReadAllText(FilePath);
                viewDO = ViewDataObject.FromJson(json);
            }

            rpcServer = new RPCViewServiceServer(
                ServiceId,
                (msg) => {
                    OnLog?.Invoke(this, new OnLogHandlerArgs(msg, LogReason.ERROR, OutputLevel.ERROR));
                },
                GetViewCount,
                number => AddViews(number ?? 1)
            );

            DeclareChannel();
            InitializeViewCountUpdaterTimer();
        }

        private void DeclareChannel()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            Connection = factory.CreateConnection();

            Channel = Connection.CreateModel();
            Channel.QueueDeclare("service-queue", false, false, false, null);
        }

        private void InitializeViewCountUpdaterTimer()
        {
            if (viewUpdateTimer != null)
            {
                viewUpdateTimer.Dispose();
                viewUpdateTimer = null;
            }
            viewUpdateTimer = new Timer(SaveAndBroadcastViews, null, 0, 5000);
        }

        private void SaveAndBroadcastViews(object args)
        {
            PersistData();
            if (viewsChanged)
                // no locking here, because race condition is not problematic (only update delay)
            {
                lock (addViewsLock)
                {
                    viewsChanged = false;
                }

                BroadcastViewCount();
            }
        }

        private void BroadcastViewCount()
        {
            Channel.BasicPublish("", "routing-key", null, Encoding.UTF8.GetBytes("test"));
            OnViewUpdate?.Invoke(this, new OnViewUpdateHandlerArgs(viewDO.OwnViews));
            OnLog?.Invoke(this, new OnLogHandlerArgs($"Sent {viewDO.OwnViews}", LogReason.DEBUG));
        }

        public void AddViews(int number = 1)
        {
            lock (addViewsLock)
            {
                viewsChanged = true;
                if (viewDO.Views.ContainsKey(ServiceId))
                {
                    viewDO.Views[ServiceId]++;
                }
                viewDO.OwnViews += number;
            }
        }

        bool IViewService.IsRunning()
        {
            return IsRunning;
        }
    }
}
