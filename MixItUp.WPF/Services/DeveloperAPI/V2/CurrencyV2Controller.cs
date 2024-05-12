﻿using MixItUp.API.V2.Models;
using MixItUp.Base;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MixItUp.WPF.Services.DeveloperAPI.V2
{
    //Prefix
    [Route("api/v2/currency")]
    public class CurrencyV2Controller : ControllerBase
    {
        [Route("")]
        [HttpGet]
        public IActionResult GetCurrencies()
        {
            var currencies = new List<GetCurrencyResponse>();

            foreach (var currency in ChannelSession.Settings.Currency)
            {
                currencies.Add(new GetCurrencyResponse
                {
                    ID = currency.Value.ID,
                    Name = currency.Value.Name
                });
            }

            return Ok(currencies);
        }

        [Route("{currencyId:guid}/{userId:guid}")]
        [HttpGet]
        public async Task<IActionResult> GetCurrencyAmountForUser(Guid currencyId, Guid userId)
        {
            if (!ChannelSession.Settings.Currency.TryGetValue(currencyId, out var currency) || currency == null)
            {
                return NotFound();
            }

            await ServiceManager.Get<UserService>().LoadAllUserData();

            if (!ChannelSession.Settings.Users.TryGetValue(userId, out var user) || user == null)
            {
                return NotFound();
            }

            return Ok(currency.GetAmount(user));
        }

        [Route("{currencyId:guid}/{userId:guid}")]
        [HttpPatch]
        public async Task<IActionResult> UpdateCurrencyAmountForUser(Guid currencyId, Guid userId, [FromBody] UpdateCurrencyAmount updateAmount)
        {
            if (!ChannelSession.Settings.Currency.TryGetValue(currencyId, out var currency) || currency == null)
            {
                return NotFound();
            }

            await ServiceManager.Get<UserService>().LoadAllUserData();

            if (!ChannelSession.Settings.Users.TryGetValue(userId, out var user) || user == null)
            {
                return NotFound();
            }

            currency.AddAmount(user, updateAmount.Amount);

            return Ok(currency.GetAmount(user));
        }

        [Route("{currencyId:guid}/{userId:guid}")]
        [HttpPut]
        public async Task<IActionResult> SetCurrencyAmountForUser(Guid currencyId, Guid userId, [FromBody] UpdateCurrencyAmount updateAmount)
        {
            if (!ChannelSession.Settings.Currency.TryGetValue(currencyId, out var currency) || currency == null)
            {
                return NotFound();
            }

            await ServiceManager.Get<UserService>().LoadAllUserData();

            if (!ChannelSession.Settings.Users.TryGetValue(userId, out var user) || user == null)
            {
                return NotFound();
            }

            currency.SetAmount(user, updateAmount.Amount);

            return Ok(currency.GetAmount(user));
        }
    }
}