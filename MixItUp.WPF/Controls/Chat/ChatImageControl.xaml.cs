﻿using MixItUp.Base;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.Chat.Glimesh;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.WPF.Services;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TwitchV5API = Twitch.Base.Models.V5.Emotes;

namespace MixItUp.WPF.Controls.Chat
{
    /// <summary>
    /// Interaction logic for ChatImageControl.xaml
    /// </summary>
    public partial class ChatImageControl : UserControl
    {
        public bool ShowText
        {
            get { return this.AltText.Visibility == Visibility.Visible; }
            set
            {
                if (value)
                {
                    this.AltText.Visibility = Visibility.Visible;
                }
                else
                {
                    this.AltText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static Dictionary<string, BitmapImage> bitmapImages = new Dictionary<string, BitmapImage>();

        public ChatImageControl()
        {
            InitializeComponent();

            this.Loaded += ChatEmoteControl_Loaded;
            this.DataContextChanged += EmoticonControl_DataContextChanged;
        }

        public ChatImageControl(TwitchV5API.EmoteModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(BetterTTVEmoteModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(FrankerFaceZEmoteModel emote) : this() { this.DataContext = emote; }

        public ChatImageControl(TwitchBitsCheerViewModel bitsCheer) : this() { this.DataContext = bitsCheer; }

        public ChatImageControl(GlimeshChatEmoteViewModel emote) : this() { this.DataContext = emote; }

        private void ChatEmoteControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.EmoticonControl_DataContextChanged(sender, new DependencyPropertyChangedEventArgs());
        }

        private async void EmoticonControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (this.DataContext != null)
                {
                    if (this.DataContext is TwitchV5API.EmoteModel)
                    {
                        TwitchV5API.EmoteModel emote = (TwitchV5API.EmoteModel)this.DataContext;
                        await this.ProcessImage(emote.code, emote.URL);
                    }
                    else if (this.DataContext is BetterTTVEmoteModel)
                    {
                        BetterTTVEmoteModel emote = (BetterTTVEmoteModel)this.DataContext;
                        if (emote.imageType.Equals("gif"))
                        {
                            this.ProcessGifImage(emote.code, emote.url);
                        }
                        else
                        {
                            await this.ProcessImage(emote.code, emote.url);
                        }
                    }
                    else if (this.DataContext is FrankerFaceZEmoteModel)
                    {
                        FrankerFaceZEmoteModel emote = (FrankerFaceZEmoteModel)this.DataContext;
                        await this.ProcessImage(emote.name, emote.url);
                    }
                    else if (this.DataContext is TwitchBitsCheerViewModel)
                    {
                        TwitchBitsCheerViewModel bitsCheer = (TwitchBitsCheerViewModel)this.DataContext;
                        await this.ProcessImage(bitsCheer.Text, (ChannelSession.AppSettings.IsDarkBackground) ? bitsCheer.Tier.DarkImage : bitsCheer.Tier.LightImage);
                        this.Text.Visibility = Visibility.Visible;
                        this.Text.Text = bitsCheer.Amount.ToString();
                    }
                    else if (this.DataContext is GlimeshChatEmoteViewModel)
                    {
                        GlimeshChatEmoteViewModel emote = (GlimeshChatEmoteViewModel)this.DataContext;
                        if (this.IsGifImage(emote.Url))
                        {
                            this.ProcessGifImage(emote.Name, emote.Url);
                        }
                        else
                        {
                            this.SVGImage.Visibility = Visibility.Visible;
                            this.SVGImage.ToolTip = this.AltText.Text = emote.Name;
                            this.ResizeImage(this.SVGImage);
                        }
                    }
                    else if (this.DataContext is string)
                    {
                        string imageUrl = (string)this.DataContext;
                        await this.ProcessImage(imageUrl, imageUrl);
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private async Task<BitmapImage> DownloadImageUrl(string url)
        {
            if (!ChatImageControl.bitmapImages.ContainsKey(url))
            {
                BitmapImage bitmap = new BitmapImage();
                using (WebClient client = new WebClient())
                {
                    var bytes = await Task.Run<byte[]>(async () => { return await client.DownloadDataTaskAsync(url); });
                    bitmap = WindowsImageService.Load(bytes);
                }
                ChatImageControl.bitmapImages[url] = bitmap;
            }
            return ChatImageControl.bitmapImages[url];
        }

        private bool IsGifImage(string url) { return url.Contains(".gif"); }

        private async Task ProcessImage(string name, string url)
        {
            this.Image.Source = await this.DownloadImageUrl(url);
            this.Image.ToolTip = this.AltText.Text = name;
            this.ResizeImage(this.Image);
        }

        private void ProcessGifImage(string name, string url)
        {
            this.GifImage.SetSize(ChannelSession.Settings.ChatFontSize * 2);
            this.GifImage.ToolTip = name;
            this.GifImage.DataContext = url;
            this.GifImage.Visibility = Visibility.Visible;
        }

        private void ResizeImage(Image image) { image.MaxWidth = image.MaxHeight = image.Width = image.Height = ChannelSession.Settings.ChatFontSize * 2; }
    }
}
