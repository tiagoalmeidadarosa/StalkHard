﻿using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace StalkHard.Models
{
    public class Login
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "accessTokenFacebook")]
        public string AccessTokenFacebook { get; set; }

        [JsonProperty(PropertyName = "accessTokenTwitter")]
        public TwitAuthenticateResponse AccessTokenTwitter { get; set; }

        [JsonProperty(PropertyName = "userFacebook")]
        public string UserFacebook { get; set; }

        [JsonProperty(PropertyName = "userTwitter")]
        public string UserTwitter { get; set; }

        [JsonProperty(PropertyName = "keyPhrases")]
        public List<KeyPhrase> KeyPhrases { get; set; } = new List<KeyPhrase>();
    }

    public class TwitAuthenticateResponse
    {
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
    }

    public class KeyPhrase
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "references")]
        public List<References> References { get; set; } = new List<References>();
    }

    public class References
    {
        [JsonProperty(PropertyName = "idTweet")]
        public string IdTweet { get; set; }

        //[JsonProperty(PropertyName = "textTweet")]
        //public string TextTweet { get; set; }

        //[JsonProperty(PropertyName = "idPost")]
        //public string IdPost { get; set; }
    }
}