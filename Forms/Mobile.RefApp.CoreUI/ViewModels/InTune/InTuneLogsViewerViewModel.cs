﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Mobile.RefApp.CoreUI.Base;
using Mobile.RefApp.Lib.Intune.Logging;
using Mobile.RefApp.Lib.Logging;

namespace Mobile.RefApp.CoreUI.ViewModels
{
    public class InTuneLogsViewerViewModel
        : ViewModelBase
    {
        private readonly ILoggingService _loggingService;

        public ObservableCollection<LoggingMessage> Logs { get; private set; }

        public InTuneLogsViewerViewModel(
            ILoggingService loggingService)
        {
            _loggingService = loggingService;
            Logs = new ObservableCollection<LoggingMessage>();
            Title = "InTune Log Viewer";
        }

        public override async Task Initialize(
            Dictionary<string, object> navigationsParams = null)
        {
            try
            {
                await Task.Run(() =>
                {
                    InTuneLoggingService.Instance.Messages.ForEach(x => Logs.Add(x));
                        
                }).ConfigureAwait(false); 
            }
            catch(Exception ex)
            {
                _loggingService.LogError(typeof(InTuneLogsViewerViewModel), ex, ex.Message);
            }
        }
    }
}
