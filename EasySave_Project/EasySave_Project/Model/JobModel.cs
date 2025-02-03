using EasySave_Project.Model;
using System;
using System.Text.Json.Serialization;
using EasySave_Project.Util;
using System;
using System.Collections.Generic;
using EasySave_Project.Service;

namespace EasySave_Project.Model
{
    /// <summary>
    /// Represents a backup job that can be observed for changes.
    /// Implements the observer pattern to notify registered observers when its state changes.
    /// </summary>
    public class JobModel
    {
        // Propriétés publiques
        public int Id { get; set; }
        
        [JsonConverter(typeof(EnumConverter.JsonEnumConverter<JobSaveStateEnum>))]
        public JobSaveStateEnum SaveState { get; set; } = JobSaveStateEnum.INACTIVE;
        
        [JsonConverter(typeof(EnumConverter.JsonEnumConverter<JobSaveTypeEnum>))]
        public JobSaveTypeEnum SaveType { get; set; }

        /// <summary>
        /// List of observers subscribed to this job.
        /// </summary>
        private List<JobObserverService> _observers = new List<JobObserverService>();


        public string Name { get; set; }


        public string FileSource { get; set; }

        public string FileTarget { get; set; }


        public string FileSize { get; set; } = "0 KB";


        public string FileTransferTime { get; set; } = "0 sec";

        public string LastFullBackupPath { get; set; } = null;


        public string LastSaveDifferentialPath { get; set; } = null;


        public DateTime Time { get; set; } = DateTime.Now;

        /// <summary>
        /// Mandatory constructor for .NET JSON Deserialization use
        /// </summary>
        public JobModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JobModel"/> class.
        /// </summary>
        /// <param name="name">The name of the job.</param>
        /// <param name="fileSource">The source file path.</param>
        /// <param name="fileTarget">The target file path.</param>
        /// <param name="jobSaveTypeEnum">The type of save operation.</param>
        public JobModel(string name, string fileSource, string fileTarget, JobSaveTypeEnum jobSaveTypeEnum, string LastFullBackupPath, string LastSaveDifferentialPath)
        {
            this.Name = name;
            this.FileSource = fileSource;
            this.FileTarget = fileTarget;
            this.SaveType = jobSaveTypeEnum;
            this.LastFullBackupPath = LastFullBackupPath;
            this.LastSaveDifferentialPath = LastSaveDifferentialPath;

        }

        /// <summary>
        /// Subscribes an observer to this job's notifications.
        /// </summary>
        /// <param name="observer">The observer to subscribe.</param>
        /// <returns>A disposable object to unsubscribe the observer.</returns>
        public IDisposable Subscribe(JobObserverService observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
            return new Unsubscriber(_observers, observer);
        }

        /// <summary>
        /// Notifies all subscribed observers about a change in the job's state.
        /// </summary>
        public void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(this);
            }
        }

        /// <summary>
        /// Returns a string representation of the job.
        /// </summary>
        /// <returns>A formatted string describing the job.</returns>
        public override string ToString()
        {
            return $"Job {Name} : From {FileSource} to {FileTarget}, created on {Time}, type: {SaveType}";
        }

        /// <summary>
        /// Helper class to handle unsubscription from job notifications.
        /// </summary>
        private class Unsubscriber : IDisposable
        {
            private List<JobObserverService> _observers;
            private JobObserverService _observer;

            /// <summary>
            /// Initializes a new instance of the <see cref="Unsubscriber"/> class.
            /// </summary>
            /// <param name="observers">The list of subscribed observers.</param>
            /// <param name="observer">The observer to remove when disposed.</param>
            public Unsubscriber(List<JobObserverService> observers, JobObserverService observer)
            {
                _observers = observers;
                _observer = observer;
            }

            /// <summary>
            /// Unsubscribes the observer when disposed.
            /// </summary>
            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                {
                    _observers.Remove(_observer);
                }
            }
        }
    }
}
