using ResearchSweet.Transport;
using ResearchSweet.Transport.Helpers;
using System;
using System.Threading.Tasks;
using UnityEngine;
using static ResearchSweet.Transport.Helpers.ResearchSweetHelpers;

namespace ResearchSweet
{
    public static class ResearchSweetExtensions
    {
        private static ResearchSweetClient _client;
        private static Task _initTask;

        public static InitializeResult Initialize(string apiKey, string endpointUrl = null)
        {
            var gameObject = new GameObject();
            return Initialize(gameObject, apiKey, endpointUrl);
        }

        public static InitializeResult Initialize(GameObject target, string apiKey, string endpointUrl = null)
        {
            var gameObjectTracker = GameObjectTracker.Instance;
            MainThreadRunnerExtensions.RegisterMainThreadRunner(MainThreadRunner.Instance);
            GameObjectTrackerExtensions.RegisterGameObjectTracker(gameObjectTracker);

            var initResult = ResearchSweetHelpers.InitializeResearchSweet(target, options =>
            {
                options.UseUnityHandlers(true)
                .UseApiKey(apiKey);

                if (!string.IsNullOrEmpty(endpointUrl))
                {
                    options.UseUrl(endpointUrl);
                }
            });

            _client = initResult.Client;

            _client.OnConnected += _client_OnConnected;
            _client.OnError += _client_OnError;

            gameObjectTracker.AssignResearchSweetClient(_client);

            Debug.Log("[ResearchSweet] connecting...");
            _initTask = _client.InitAsync(exception =>
            {
                Debug.LogException(exception);
            });

            return initResult;
        }

        private static void _client_OnError(Exception exception)
        {
            Debug.Log("[ResearchSweet] error!");
            Debug.LogException(exception);
        }

        private static Task _client_OnConnected()
        {
            Debug.Log("[ResearchSweet] connected!");
            return Task.CompletedTask;
        }
    }
}
