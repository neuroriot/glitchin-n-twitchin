﻿using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class SoundActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Sound; } }

        public string FilePath
        {
            get { return this.filePath; }
            set
            {
                this.filePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string filePath;

        public ObservableCollection<string> AudioDevices { get; set; } = new ObservableCollection<string>().EnableSync();

        public string SelectedAudioDevice
        {
            get { return this.selectedAudioDevice; }
            set
            {
                this.selectedAudioDevice = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectedAudioDevice;

        public int Volume
        {
            get { return this.volume; }
            set
            {
                this.volume = value;
                this.NotifyPropertyChanged();
            }
        }
        private int volume = 100;

        public SoundActionEditorControlViewModel(SoundActionModel action)
            : base(action)
        {
            this.LoadSoundDevices();
            this.FilePath = action.FilePath;
            this.SelectedAudioDevice = (action.OutputDevice != null) ? action.OutputDevice : SoundActionModel.DefaultAudioDevice;
            this.Volume = action.VolumeScale;
        }

        public SoundActionEditorControlViewModel()
            : base()
        {
            this.LoadSoundDevices();
            this.SelectedAudioDevice = SoundActionModel.DefaultAudioDevice;
        }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.FilePath))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.SoundActionMissingFilePath));
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (!string.Equals(this.SelectedAudioDevice, SoundActionModel.DefaultAudioDevice))
            {
                return Task.FromResult<ActionModelBase>(new SoundActionModel(this.FilePath, this.Volume, this.SelectedAudioDevice));
            }
            else
            {
                return Task.FromResult<ActionModelBase>(new SoundActionModel(this.FilePath, this.Volume));
            }
        }

        private void LoadSoundDevices()
        {
            this.AudioDevices.Add(SoundActionModel.DefaultAudioDevice);
            this.AudioDevices.Add(SoundActionModel.MixItUpOverlay);
            foreach (string soundDevices in ChannelSession.Services.AudioService.GetOutputDevices())
            {
                this.AudioDevices.Add(soundDevices);
            }
        }
    }
}
