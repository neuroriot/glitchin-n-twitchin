﻿using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Window;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class CurrencyRankInventoryContainerViewModel
    {
        public UserCurrencyModel Currency { get; private set; }
        public UserInventoryModel Inventory { get; private set; }

        public CurrencyRankInventoryContainerViewModel(UserCurrencyModel currency) { this.Currency = currency; }

        public CurrencyRankInventoryContainerViewModel(UserInventoryModel inventory) { this.Inventory = inventory; }

        public string Name
        {
            get
            {
                if (this.Inventory != null) { return this.Inventory.Name; }
                else { return this.Currency.Name; }
            }
        }

        public string Type
        {
            get
            {
                if (this.Inventory != null) { return Resources.Inventory; }
                else if (this.Currency.IsRank) { return Resources.Rank; }
                else { return Resources.Currency; }
            }
        }

        public string AmountSpecialIdentifiers
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (this.Inventory != null)
                {
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Inventory.UserAmountSpecialIdentifierExample);
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + "target" + this.Inventory.UserAmountSpecialIdentifierExample);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Inventory.UserAllAmountSpecialIdentifier);
                }
                else
                {
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Currency.UserAmountSpecialIdentifier);
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + "target" + this.Currency.UserAmountSpecialIdentifier);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Currency.Top10SpecialIdentifier);
                }
                return stringBuilder.ToString().Trim(new char[] { '\r', '\n' });
            }
            set { }
        }

        public string RankSpecialIdentifiers
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (this.Currency != null && this.Currency.IsRank)
                {
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + this.Currency.UserRankNameSpecialIdentifier);
                    stringBuilder.AppendLine(SpecialIdentifierStringBuilder.SpecialIdentifierHeader + "target" + this.Currency.UserRankNameSpecialIdentifier);
                }
                return stringBuilder.ToString().Trim(new char[] { '\r', '\n' });
            }
            set { }
        }
    }

    public class CurrencyRankInventoryMainControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<CurrencyRankInventoryContainerViewModel> Items { get; set; } = new ObservableCollection<CurrencyRankInventoryContainerViewModel>();

        public CurrencyRankInventoryMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatCommandMessageReceived;
        }

        public void RefreshList()
        {
            this.Items.Clear();
            foreach (var kvp in ChannelSession.Settings.Currencies)
            {
                if (kvp.Value.IsRank)
                {
                    this.Items.Add(new CurrencyRankInventoryContainerViewModel(kvp.Value));
                }
                else
                {
                    this.Items.Add(new CurrencyRankInventoryContainerViewModel(kvp.Value));
                }
            }
            foreach (var kvp in ChannelSession.Settings.Inventories)
            {
                this.Items.Add(new CurrencyRankInventoryContainerViewModel(kvp.Value));
            }
        }

        public async void DeleteItem(CurrencyRankInventoryContainerViewModel item)
        {
            if (await DialogHelper.ShowConfirmation("Are you sure you wish to delete this?"))
            {
                if (item.Inventory != null)
                {
                    await item.Inventory.Reset();
                    ChannelSession.Settings.Inventories.Remove(item.Inventory.ID);
                }
                else
                {
                    await item.Currency.Reset();
                    ChannelSession.Settings.Currencies.Remove(item.Currency.ID);
                }
                this.RefreshList();
            }
        }

        protected override async Task OnLoadedInternal()
        {
            this.RefreshList();
            await base.OnLoadedInternal();
        }

        protected override async Task OnVisibleInternal()
        {
            this.RefreshList();
            await base.OnVisibleInternal();
        }

        private async void GlobalEvents_OnChatCommandMessageReceived(object sender, ChatMessageViewModel message)
        {
            foreach (UserInventoryModel inventory in ChannelSession.Settings.Inventories.Values)
            {
                if (inventory.ShopEnabled && message.PlainTextMessage.StartsWith(inventory.ShopCommand))
                {
                    string args = message.PlainTextMessage.Replace(inventory.ShopCommand, "");
                    await inventory.PerformShopCommand(message.User, args.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries), message.Platform);
                }
                else if (inventory.TradeEnabled && message.PlainTextMessage.StartsWith(inventory.TradeCommand))
                {
                    string args = message.PlainTextMessage.Replace(inventory.TradeCommand, "");
                    await inventory.PerformTradeCommand(message.User, args.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries), message.Platform);
                }
            }
        }
    }
}
