﻿using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class CommandParametersModel
    {
        public static CommandParametersModel GetTestParameters(Dictionary<string, string> specialIdentifiers)
        {
            return new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.Default, new List<string>() { "@" + ChannelSession.User.Username }, specialIdentifiers) { TargetUser = ChannelSession.User };
        }

        [DataMember]
        public UserV2ViewModel User { get; set; } = ChannelSession.User;
        [DataMember]
        public StreamingPlatformTypeEnum Platform { get; set; } = StreamingPlatformTypeEnum.Default;
        [DataMember]
        public List<string> Arguments { get; set; } = new List<string>();
        [DataMember]
        public Dictionary<string, string> SpecialIdentifiers { get; set; } = new Dictionary<string, string>();

        [DataMember]
        public UserV2ViewModel TargetUser { get; set; }

        [DataMember]
        public string TriggeringChatMessageID { get; set; }

        public CommandParametersModel() : this(ChannelSession.User) { }

        public CommandParametersModel(UserV2ViewModel user) : this(user, StreamingPlatformTypeEnum.Default) { }

        public CommandParametersModel(ChatMessageViewModel message)
            : this(message.User, message.Platform, message.ToArguments())
        {
            this.SpecialIdentifiers["message"] = message.PlainTextMessage;

            this.TriggeringChatMessageID = message.ID;
        }

        public CommandParametersModel(Dictionary<string, string> specialIdentifiers) : this(ChannelSession.User, specialIdentifiers) { }

        public CommandParametersModel(UserV2ViewModel user, StreamingPlatformTypeEnum platform) : this(user, platform, null) { }

        public CommandParametersModel(UserV2ViewModel user, IEnumerable<string> arguments) : this(user, StreamingPlatformTypeEnum.Default, arguments, null) { }

        public CommandParametersModel(UserV2ViewModel user, Dictionary<string, string> specialIdentifiers) : this(user, null, specialIdentifiers) { }

        public CommandParametersModel(UserV2ViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments) : this(user, platform, arguments, null) { }

        public CommandParametersModel(UserV2ViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers) : this(user, StreamingPlatformTypeEnum.Default, arguments, specialIdentifiers) { }

        public CommandParametersModel(UserV2ViewModel user = null, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.Default, IEnumerable<string> arguments = null, Dictionary<string, string> specialIdentifiers = null)
        {
            if (user != null)
            {
                this.User = user;
            }

            if (arguments != null)
            {
                this.Arguments = new List<string>(arguments);
            }

            if (specialIdentifiers != null)
            {
                this.SpecialIdentifiers = new Dictionary<string, string>(specialIdentifiers);
            }

            if (platform != StreamingPlatformTypeEnum.Default)
            {
                this.Platform = platform;
            }
            else
            {
                this.Platform = this.User.Platform;
            }
        }

        public bool IsTargetUserSelf { get { return this.TargetUser == this.User; } }

        public CommandParametersModel Duplicate()
        {
            CommandParametersModel result = new CommandParametersModel(this.User, this.Platform, this.Arguments, this.SpecialIdentifiers);
            result.TargetUser = this.TargetUser;
            return result;
        }

        public async Task SetTargetUser()
        {
            if (this.TargetUser == null)
            {
                if (this.Arguments.Count > 0)
                {
                    this.TargetUser = await ServiceManager.Get<UserService>().GetUserByPlatformUsername(this.Platform, this.Arguments.First());
                }

                if (this.TargetUser == null || !UserService.SanitizeUsername(this.Arguments.ElementAt(0)).Equals(this.TargetUser.Username, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.TargetUser = this.User;
                }
            }
        }
    }
}
