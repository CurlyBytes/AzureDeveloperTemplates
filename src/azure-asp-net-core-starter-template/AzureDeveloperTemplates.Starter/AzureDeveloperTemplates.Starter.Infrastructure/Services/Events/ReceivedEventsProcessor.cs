﻿using AzureDeveloperTemplates.Starter.Infrastructure.Services.Events.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDeveloperTemplates.Starter.Infrastructure.Services.Events
{
    public class ReceivedEventsProcessor : IReceivedEventsProcessor
    {
        private readonly IEventsReceiverService _eventsReceiverService;
        private readonly ILogger<ReceivedEventsProcessor> _logger;
        private readonly IList<Exception> _exceptions;

        public ReceivedEventsProcessor(IEventsReceiverService eventsReceiverService,
                                                                    ILogger<ReceivedEventsProcessor> logger)
        {
            _eventsReceiverService = eventsReceiverService;
            _logger = logger;
            _exceptions = new List<Exception>();
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken, Action<string> callback = null)
        {
            stoppingToken.Register(() =>
                _logger.LogInformation($"{nameof(ReceivedEventsProcessor)} background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _eventsReceiverService.ReceiveEventsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "A problem occurred while invoking a callback method");
                    _exceptions.Add(ex);
                }
            }
            _logger.LogInformation(stoppingToken.IsCancellationRequested.ToString());
            _logger.LogInformation($"{nameof(ReceivedEventsProcessor)} background task is stopping.");
        }

        private void _eventsReceiverService_NewEventMessageReceived(object sender, string e)
        {
            _logger.LogInformation(e);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_exceptions.Any())
            {
                _logger.LogCritical(new AggregateException(_exceptions), "The host threw exceptions unexpectedly");
            }
            return Task.CompletedTask;
        }
    }
}
