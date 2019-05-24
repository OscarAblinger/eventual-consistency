﻿using Client;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewService;

namespace Cheetah.ServiceController
{
    internal class ServiceController : IServiceController
    {
        private List<ServiceInformation> services;

        public event OnServiceLogHandler OnServiceLog;

        public IReadOnlyList<ServiceInformation> RunningServices => services.Where(s => s.IsRunning).ToList().AsReadOnly();
        public IReadOnlyList<ServiceInformation> AllServices => services.AsReadOnly();

        public ServiceController()
        {
            services = new List<ServiceInformation>();
        }

        public void AbortService(ServiceInformation si)
        {
            si.Service.Abort();
            si.IsRunning = false;
        }

        public ServiceInformation CreateNewService()
        {
            ICheetahViewService service = new ViewService.ViewService();
            IClient client = new Client.Client();

            var si = new ServiceInformation(IDService.GenerateNextID(), service, client);

            service.OnLog += (sender, args) =>
            {
                OnServiceLog?.Invoke(this, new OnServiceLogHandlerArgs(si, args));
            };

            services.Add(si);

            return si;
        }

        public void SendViews(ServiceInformation si, int amount = 1)
        {
            si.Client.AddViews(amount);
        }

        public void StartSendingRepeatedViewsToService(ServiceInformation si, int interval, int amount = 1)
        {
            si.Client.StartPeriodicRequests(amount, interval);
        }

        public bool StartService(ServiceInformation si)
        {
            if (si.IsRunning)
                return true;

            var serviceId = IDService.GetServiceUIDForId(si.ID);
            si.Service.StartUp(serviceId, Path.Combine(PersistenceConfiguration.DBDirectory, serviceId));
            si.Client.Setup(serviceId);
            si.IsRunning = true;
            return false;
        }

        public void StopSendingRepeatedViewsToService(ServiceInformation si)
        {
            si.Client.StopPeriodicRequests();
        }

        public void StopService(ServiceInformation si)
        {
            si.Service.ShutDown();
            si.IsRunning = false;
        }
    }
}
