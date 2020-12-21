﻿using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.User;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    [DataContract]
    public abstract class RequirementModelBase
    {
        protected DateTimeOffset errorCooldown = DateTimeOffset.MinValue;

        public virtual Task<bool> Validate(CommandParametersModel parameters) { return Task.FromResult(true); }

        public virtual Task Perform(CommandParametersModel parameters)
        {
            this.errorCooldown = DateTimeOffset.Now;
            return Task.FromResult(0);
        }

        public virtual Task Refund(CommandParametersModel parameters) { return Task.FromResult(0); }

        public virtual void Reset() { }

        protected async Task SendErrorChatMessage(UserViewModel user, string message)
        {
            if (this.errorCooldown <= DateTimeOffset.Now)
            {
                if (ChannelSession.Services.Chat != null)
                {
                    if (ChannelSession.Settings.IncludeUsernameWithRequirementErrors)
                    {
                        message = $"@{user.Username}: {message}";
                    }
                    await ChannelSession.Services.Chat.SendMessage(message);
                    this.errorCooldown = DateTimeOffset.Now.AddSeconds(10);
                }
            }
        }
    }
}
