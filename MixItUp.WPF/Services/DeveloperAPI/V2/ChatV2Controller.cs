﻿using MixItUp.API.V2.Models;
using MixItUp.Base.Model;
using MixItUp.Base.Services;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MixItUp.WPF.Services.DeveloperAPI.V2
{
    /// <summary>
    /// Prefix
    /// </summary>
    [Route("api/v2/chat")]
    public class ChatV2Controller : ControllerBase
    {
        [Route("message")]
        [HttpPost]
        public async Task<IActionResult> SendChatMessage([FromBody] SendChatMessage chatMessage)
        {
            if (chatMessage == null)
            {
                return BadRequest($"Missing chat message");
            }

            StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.All;
            if (!string.IsNullOrEmpty(chatMessage.Platform) && !Enum.TryParse<StreamingPlatformTypeEnum>(chatMessage.Platform, ignoreCase: true, out platform))
            {
                return BadRequest($"Unknown platform: {chatMessage.Platform}");
            }

            await ServiceManager.Get<ChatService>().SendMessage(chatMessage.Message, platform, chatMessage.SendAsStreamer);

            return Ok();
        }

        [Route("clear")]
        [HttpPost]
        public async Task<IActionResult> ClearChat()
        {
            await ServiceManager.Get<ChatService>().ClearMessages(StreamingPlatformTypeEnum.All);
            return Ok();
        }
    }
}