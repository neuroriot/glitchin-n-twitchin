﻿using LinqToTwitter;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    [DataContract]
    public class Tweet
    {
        [DataMember]
        public ulong ID { get; set; }

        [DataMember]
        public ulong UserID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }

        [DataMember]
        public List<string> Links { get; set; }

        [JsonIgnore]
        public string TweetLink { get { return string.Format("https://twitter.com/{0}/status/{1}", this.UserName, this.ID); } }

        public Tweet()
        {
            this.Links = new List<string>();
        }

        public bool IsStreamTweet { get { return this.Links.Any(l => l.ToLower().Contains(string.Format("twitch.tv/{0}", ChannelSession.TwitchUserNewAPI.login.ToLower()))); } }
    }

    public interface ITwitterService : IOAuthExternalService
    {
        void SetAuthPin(string pin);

        Task<IEnumerable<Tweet>> GetLatestTweets();

        Task<Result> SendTweet(string tweet, string imagePath = null);

        Task<Result> UpdateName(string name);
    }

    public class TwitterService : OAuthExternalServiceBase, ITwitterService
    {
        private const string ClientID = "gV0xMGKNgAaaqQ0XnR4JoX91U";

        private string authPin;
        private IAuthorizer authorizer;

        private DateTimeOffset lastTweet = DateTimeOffset.MinValue;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public TwitterService() : base("") { }

        public override string Name { get { return "Twitter"; } }

        public override async Task<Result> Connect()
        {
            try
            {
                PinAuthorizer pinAuthorizer = new PinAuthorizer()
                {
                    CredentialStore = new InMemoryCredentialStore
                    {
                        ConsumerKey = TwitterService.ClientID,
                        ConsumerSecret = ChannelSession.Services.Secrets.GetSecret("TwitterSecret"),
                    },
                    GoToTwitterAuthorization = pageLink => ProcessHelper.LaunchLink(pageLink),
                    GetPin = () =>
                    {
                        while (string.IsNullOrEmpty(this.authPin))
                        {
                            Task.Delay(1000).Wait();
                        }
                        return this.authPin;
                    }
                };

                await pinAuthorizer.AuthorizeAsync();
                this.authorizer = pinAuthorizer;

                this.authPin = null;

                return await this.InitializeInternal();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public override async Task<Result> Connect(OAuthTokenModel token)
        {
            try
            {
                SingleUserAuthorizer singleUserAuthorizer = new SingleUserAuthorizer
                {
                    CredentialStore = new SingleUserInMemoryCredentialStore
                    {
                        ConsumerKey = TwitterService.ClientID,
                        ConsumerSecret = ChannelSession.Services.Secrets.GetSecret("TwitterSecret"),

                        AccessToken = token.accessToken,
                        AccessTokenSecret = token.refreshToken,

                        UserID = ulong.Parse(token.clientID),
                        ScreenName = token.authorizationCode,
                    }
                };

                await singleUserAuthorizer.AuthorizeAsync();
                this.authorizer = singleUserAuthorizer;

                return await this.InitializeInternal();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public override Task Disconnect()
        {
            this.token = null;
            return Task.FromResult(0);
        }

        public void SetAuthPin(string pin)
        {
            this.authPin = pin;
        }

        public async Task<IEnumerable<Tweet>> GetLatestTweets()
        {
            List<Tweet> results = new List<Tweet>();
            try
            {
                using (var twitterCtx = new TwitterContext(this.authorizer))
                {
                    List<Status> tweets = await (from tweet in twitterCtx.Status
                                                 where tweet.Type == StatusType.User && tweet.ScreenName == this.authorizer.CredentialStore.ScreenName && tweet.TweetMode == TweetMode.Extended
                                                 select tweet).ToListAsync();

                    foreach (Status tweet in tweets)
                    {
                        Tweet t = new Tweet()
                        {
                            ID = tweet.StatusID,
                            UserID = tweet.UserID,
                            UserName = tweet.ScreenName,
                            Text = tweet.FullText,
                            DateTime = new DateTimeOffset(tweet.CreatedAt, DateTimeOffset.UtcNow.Offset),
                        };

                        foreach (var urlEntry in tweet.Entities.UrlEntities)
                        {
                            t.Links.Add((!string.IsNullOrEmpty(urlEntry.ExpandedUrl) ? urlEntry.ExpandedUrl : urlEntry.DisplayUrl));
                        }

                        results.Add(t);
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return results;
        }

        public async Task<Result> SendTweet(string tweet, string imagePath = null)
        {
            try
            {
                if (this.lastTweet.TotalMinutesFromNow() > 5)
                {
                    this.lastTweet = DateTimeOffset.Now;

                    using (var twitterCtx = new TwitterContext(this.authorizer))
                    {
                        List<ulong> mediaIds = new List<ulong>();

                        try
                        {
                            if (!string.IsNullOrEmpty(imagePath))
                            {
                                // Download the image and upload to Twitter
                                using (WebClient client = new WebClient())
                                {
                                    var bytes = await Task.Run<byte[]>(async () => { return await client.DownloadDataTaskAsync(imagePath); });
                                    string mediaType = $"image/" + ChannelSession.Services.Image.GetImageFormat(bytes);
                                    Media media = await twitterCtx.UploadMediaAsync(bytes, mediaType, "tweet_image");
                                    mediaIds.Add(media.MediaID);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                            return new Result(ex);
                        }

                        await twitterCtx.TweetAsync(tweet, mediaIds);

                        return new Result();
                    }
                }
                return new Result(Resources.TwitterRateLimited);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public async Task<Result> UpdateName(string name)
        {
            try
            {
                using (var twitterCtx = new TwitterContext(this.authorizer))
                {
                    await twitterCtx.UpdateAccountProfileAsync(name, null, null, null, true);
                    return new Result();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public override OAuthTokenModel GetOAuthTokenCopy()
        {
            if (this.authorizer != null)
            {
                this.token = new OAuthTokenModel();
                this.token.accessToken = this.authorizer.CredentialStore.OAuthToken;
                this.token.refreshToken = this.authorizer.CredentialStore.OAuthTokenSecret;
                this.token.clientID = this.authorizer.CredentialStore.UserID.ToString();
                this.token.authorizationCode = this.authorizer.CredentialStore.ScreenName;
            }
            return base.GetOAuthTokenCopy();
        }

        protected override async Task<Result> InitializeInternal()
        {
            if (!string.IsNullOrEmpty(this.authorizer.CredentialStore.OAuthToken))
            {
                await this.RefreshOAuthToken();
                this.TrackServiceTelemetry("Twitter");
                return new Result();
            }
            return new Result(Resources.TwitterAuthFailed);
        }

        protected override Task RefreshOAuthToken()
        {
            this.GetOAuthTokenCopy();
            return Task.FromResult(0);
        }

        protected override void DisposeInternal()
        {
            this.cancellationTokenSource.Dispose();
        }
    }
}
