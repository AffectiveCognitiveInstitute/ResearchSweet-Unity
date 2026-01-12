using System;
using System.Threading.Tasks;
using ResearchSweet.Tracker;
using ResearchSweet.Transport;
using UnityEngine;
using ResearchSweet.Transport.Helpers;
using ResearchSweetHelpers = ResearchSweet.Helpers.ResearchSweetHelpers;

namespace ResearchSweet
{
    public static class ResearchSweetExtensions
    {
        public static IResearchSweetClient Client => _client;
        private static ResearchSweetClient _client;
        private static Task _initTask;

        public static ResearchSweetHelpers.InitializeResult Initialize(string apiKey, string endpointUrl = null)
        {
            var gameObject = new GameObject();
            return Initialize(gameObject, apiKey, endpointUrl);
        }

        public static ResearchSweetHelpers.InitializeResult Initialize(GameObject target, string apiKey, string endpointUrl = null)
        {
            var gameObjectTracker = GameObjectTracker.Instance;
            MainThreadRunnerExtensions.RegisterMainThreadRunner(MainThreadRunner.Instance);
            Tracker.GameObjectTrackerExtensions.RegisterGameObjectTracker(gameObjectTracker);

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
            Debug.Log($"[ResearchSweet] {exception.ToString()}");
            Debug.LogException(exception);
        }

        private static Task _client_OnConnected()
        {
            Debug.Log("[ResearchSweet] connected!");
            return Task.CompletedTask;
        }
    }

    //public class ResearchSweetChannels
    //{
    //    public MyQuestionnairesEventChannelSO MyQuestionnairesChanged { get; set; }
    //    public VariableChangedEventChannelSO VariableChangedEventChannel { get; set; }
    //    public QuestionnaireChangedEventChannelSO QuestionnaireChangedChannel { get; set; }
    //    public AvailableQuestionnaireDefinitionsChangedEventChannelSO AvailableQuestionnaireDefinitionsChangedChannel { get; set; }
    //}
}
