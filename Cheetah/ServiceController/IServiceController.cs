﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cheetah.ServiceController
{
    internal interface IServiceController
    {
        IReadOnlyList<ServiceInformation> RunningServices { get; }
        IReadOnlyList<ServiceInformation> AllServices { get; }

        /// <summary>
        /// Creates not only the service, but also a client for it
        /// </summary>
        /// <returns></returns>
        ServiceInformation CreateNewService();
        /// <summary>
        /// Starts an existing Service.
        /// Does nothing if the service is already started
        /// </summary>
        /// <param name="si">The service information for the service to start</param>
        /// <returns>Whether the service was running</returns>
        bool StartService(ServiceInformation si);
        /// <summary>
        /// Stops an existing Service
        /// </summary>
        /// <param name="si">The service information for the service to stop</param>
        /// <returns>Whether it was successfully stopped</returns>
        bool StopService(ServiceInformation si);
        /// <summary>
        /// Aborts a service
        /// </summary>
        /// <param name="si">The service information for the service to abort</param>
        void AbortService(ServiceInformation si);
        /// <summary>
        /// Sends views to a service
        /// </summary>
        /// <param name="si">The service information for the service to send the views to</param>
        /// <param name="amount">The amount of views to send. Must be at least 1</param>
        /// <returns>A task that completes once all views were sent</returns>
        Task<ServiceInformation> SendViews(ServiceInformation si, int amount = 1);
        /// <summary>
        /// Asks the client to start sending views in regular intervals to the given service.
        /// Replaces an old one, if one is already existing
        /// </summary>
        /// <param name="si">The service information for the service that the views should be sent to</param>
        /// <param name="interval">The amount of milliseconds in between every sending of views</param>
        /// <param name="amount">The amount of views sent after that interval</param>
        void StartSendingRepeatedViewsToService(ServiceInformation si, int interval, int amount = 1);
        /// <summary>
        /// Asks the client to stop sending repeated views to its service
        /// </summary>
        /// <param name="si">The service information for the service that the task should be stopped</param>
        /// <returns>Wheter there was an ongoing task running</returns>
        bool StopSendingRepeatedViewsToService(ServiceInformation si);
    }
}