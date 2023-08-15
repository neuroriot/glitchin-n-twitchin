﻿using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayTwitchClipV3ViewModel : OverlayItemV3ViewModelBase
    {
        public IEnumerable<OverlayTwitchClipV3ClipType> ClipTypes { get; set; } = EnumHelper.GetEnumList<OverlayTwitchClipV3ClipType>();

        public OverlayTwitchClipV3ClipType SelectedClipType
        {
            get { return this.selectedClipType; }
            set
            {
                this.selectedClipType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ShowUsername));
                this.NotifyPropertyChanged(nameof(this.ShowClipID));
            }
        }
        private OverlayTwitchClipV3ClipType selectedClipType = OverlayTwitchClipV3ClipType.LatestClip;

        public bool ShowUsername
        {
            get
            {
                return this.SelectedClipType == OverlayTwitchClipV3ClipType.RandomClip ||
                    this.SelectedClipType == OverlayTwitchClipV3ClipType.LatestClip;
            }
        }

        public string Username
        {
            get { return this.username; }
            set
            {
                this.username = value;
                this.NotifyPropertyChanged();
            }
        }
        private string username;

        public bool ShowClipID { get { return this.SelectedClipType == OverlayTwitchClipV3ClipType.SpecificClip; } }

        public string ClipID
        {
            get { return this.clipID; }
            set
            {
                this.clipID = value;
                this.NotifyPropertyChanged();
            }
        }
        private string clipID;

        public string Width
        {
            get { return this.width > 0 ? this.width.ToString() : string.Empty; }
            set
            {
                this.width = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int width;

        public string Height
        {
            get { return this.height > 0 ? this.height.ToString() : string.Empty; }
            set
            {
                this.height = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int height;

        public OverlayTwitchClipV3ViewModel() : base(OverlayItemV3Type.TwitchClip) { }

        public OverlayTwitchClipV3ViewModel(OverlayTwitchClipV3Model item)
            : base(item)
        {
            this.SelectedClipType = item.ClipType;
            if (this.SelectedClipType == OverlayTwitchClipV3ClipType.SpecificClip)
            {
                this.ClipID = item.ClipReferenceID;
            }
            else
            {
                this.Username = item.ClipReferenceID;
            }
            this.width = item.Width;
            this.height = item.Height;
        }

        public override Result Validate()
        {
            if (string.IsNullOrEmpty(this.Width))
            {
                return new Result(Resources.OverlayWidthMustBeValidValue);
            }

            if (string.IsNullOrEmpty(this.Height))
            {
                return new Result(Resources.OverlayHeightMustBeValidValue);
            }

            if (this.SelectedClipType == OverlayTwitchClipV3ClipType.SpecificClip)
            {
                if (string.IsNullOrEmpty(this.ClipID))
                {
                    return new Result(Resources.OverlayTwitchClipValidIDMustBeSpecified);
                }
            }

            return new Result();
        }

        protected override OverlayItemV3ModelBase GetItemInternal()
        {
            string typeID = this.Username;
            if (this.SelectedClipType == OverlayTwitchClipV3ClipType.SpecificClip)
            {
                typeID = this.ClipID;
            }

            OverlayTwitchClipV3Model result = new OverlayTwitchClipV3Model()
            {
                HTML = this.HTML,
                CSS = this.CSS,
                Javascript = this.Javascript,

                ClipType = this.SelectedClipType,
                ClipReferenceID = typeID,
                Width = this.width,
                Height = this.height,
            };

            return result;
        }
    }
}