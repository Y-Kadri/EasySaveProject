﻿using EasySave_Project.Model;
using System;
using System.Text.Json.Serialization;
using EasySave_Project.Util;
using System.Collections.Generic;
using EasySave_Project.Service;
using System.ComponentModel;
using EasySave_Project.Dto;

namespace EasySave_Project.Model
{
    /// <summary>
    /// Represents a backup job that can be observed for changes.
    /// </summary>
    public class JobModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        // Propriétés publiques
        public int Id { get; set; }

        private JobSaveStateEnum _saveState;

        [JsonConverter(typeof(EnumConverterUtil.JsonEnumConverter<JobSaveStateEnum>))]
        public JobSaveStateEnum SaveState
        {
            get => _saveState;
            set
            {
                if (_saveState != value)
                {
                    _saveState = value;
                    OnPropertyChanged(nameof(SaveState));
                    OnPropertyChanged(nameof(CanExecute));
                    OnPropertyChanged(nameof(IsCheckBoxVisibleAndEnable));
                    OnPropertyChanged(nameof(IsJobInPending));
                }
            }
        }

        [JsonConverter(typeof(EnumConverterUtil.JsonEnumConverter<JobSaveTypeEnum>))]
        public JobSaveTypeEnum SaveType { get; set; }

        public string Name { get; set; }

        public string FileSource { get; set; }

        public string FileTarget { get; set; }

        public string FileSize { get; set; } = "0 KB";

        public string FileTransferTime { get; set; } = "0 sec";

        public string LastFullBackupPath { get; set; } = null;

        public string LastSaveDifferentialPath { get; set; } = null;

        public DateTime Time { get; set; } = DateTime.Now;

        public FileInPendingJobDTO FileInPending { get; set; }

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
        public JobModel(string name, string fileSource, string fileTarget, JobSaveTypeEnum jobSaveTypeEnum, string lastFullBackupPath, string lastSaveDifferentialPath)
        {
            this.Name = name;
            this.FileSource = fileSource;
            this.FileTarget = fileTarget;
            this.SaveType = jobSaveTypeEnum;
            this.LastFullBackupPath = lastFullBackupPath;
            this.LastSaveDifferentialPath = lastSaveDifferentialPath;
            this.Id = FileUtil.GetCurrentJobIndex();
            this.SaveState = JobSaveStateEnum.INACTIVE; // Initialise par défaut à INACTIVE
        }

        /// <summary>
        /// Returns a string representation of the job.
        /// </summary>
        /// <returns>A formatted string describing the job.</returns>
        public override string ToString()
        {
            var translationService = TranslationService.GetInstance();
            return string.Format(
                "{0} {1} : {2} {3} {4} {5}, {6} {7}, {8}: {9}",
                translationService.GetText("job"),
                Name,
                translationService.GetText("from"),
                FileSource,
                translationService.GetText("to"),
                FileTarget,
                translationService.GetText("createdOn"),
                Time,
                translationService.GetText("type"),
                SaveType
            );
        }



        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool CanExecute => SaveState == JobSaveStateEnum.ACTIVE;


        public bool IsCheckBoxVisibleAndEnable => SaveState == JobSaveStateEnum.INACTIVE
            || SaveState == JobSaveStateEnum.END
            || SaveState == JobSaveStateEnum.CANCEL;

        public bool IsJobInPending => SaveState == JobSaveStateEnum.PENDING;

    }
}
