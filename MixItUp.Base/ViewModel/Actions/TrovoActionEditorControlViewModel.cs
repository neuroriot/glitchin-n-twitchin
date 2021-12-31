﻿using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class TrovoActionEditorControlViewModel : SubActionContainerControlViewModel
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Trovo; } }

        public IEnumerable<TrovoActionType> ActionTypes { get { return EnumHelper.GetEnumList<TrovoActionType>(); } }

        public TrovoActionType SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("ShowUsernameGrid");
                this.NotifyPropertyChanged("ShowAmountGrid");
                this.NotifyPropertyChanged("ShowRoleGrid");
            }
        }
        private TrovoActionType selectedActionType;

        public bool ShowUsernameGrid
        {
            get
            {
                return this.SelectedActionType == TrovoActionType.Host || this.SelectedActionType == TrovoActionType.AddUserRole || this.SelectedActionType == TrovoActionType.RemoveUserRole;
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

        public bool ShowAmountGrid
        {
            get
            {
                return this.SelectedActionType == TrovoActionType.EnableSlowMode;
            }
        }

        public int Amount
        {
            get { return this.amount; }
            set
            {
                this.amount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int amount;

        public bool ShowRoleGrid
        {
            get
            {
                return this.SelectedActionType == TrovoActionType.AddUserRole || this.SelectedActionType == TrovoActionType.RemoveUserRole;
            }
        }

        public string RoleName
        {
            get { return this.roleName; }
            set
            {
                this.roleName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string roleName;

        public TrovoActionEditorControlViewModel(TrovoActionModel action)
            : base(action, action.Actions)
        {
            this.SelectedActionType = action.ActionType;
        }

        public TrovoActionEditorControlViewModel() : base() { }

        public override async Task<Result> Validate()
        {
            if (this.ShowUsernameGrid)
            {
                if (string.IsNullOrEmpty(this.Username))
                {
                    return new Result(MixItUp.Base.Resources.TrovoActionUsernameMissing);
                }
            }

            if (this.ShowAmountGrid)
            {
                if (this.Amount <= 0)
                {
                    return new Result(MixItUp.Base.Resources.TrovoActionAmountMissing);
                }
            }

            if (this.ShowRoleGrid)
            {
                if (string.IsNullOrEmpty(this.RoleName))
                {
                    return new Result(MixItUp.Base.Resources.TrovoActionRoleNameMissing);
                }
            }

            return await base.Validate();
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.SelectedActionType == TrovoActionType.Host)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateHostAction(this.Username));
            }
            else if (this.SelectedActionType == TrovoActionType.EnableSlowMode)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateEnableSlowModeAction(this.Amount));
            }
            else if (this.SelectedActionType == TrovoActionType.DisableSlowMode)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateDisableSlowModeAction());
            }
            else if (this.SelectedActionType == TrovoActionType.EnableFollowerMode)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateEnableFollowerModeAction());
            }
            else if (this.SelectedActionType == TrovoActionType.DisableFollowerMode)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateDisableFollowerModeAction());
            }
            else if (this.SelectedActionType == TrovoActionType.AddUserRole)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateAddUserRoleAction(this.Username, this.RoleName));
            }
            else if (this.SelectedActionType == TrovoActionType.RemoveUserRole)
            {
                return Task.FromResult<ActionModelBase>(TrovoActionModel.CreateRemoveUserRoleAction(this.Username, this.RoleName));
            }
            return Task.FromResult<ActionModelBase>(null);
        }
    }
}