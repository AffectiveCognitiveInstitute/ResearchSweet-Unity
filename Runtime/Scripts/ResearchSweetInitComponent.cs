using ResearchSweet.Transport;
using ResearchSweet.Transport.Events;
using UnityEngine;

namespace ResearchSweet
{
    public class ResearchSweetInitComponent : MonoBehaviour
    {
        public string EndpointUrl;
        public string ApiKey;
        public MyQuestionnairesEventChannelSO MyQuestionnairesChanged;
        public VariableChangedEventChannelSO VariableChangedEventChannel;
        public QuestionnaireChangedEventChannelSO QuestionnaireChangedChannel;
        public AvailableQuestionnaireDefinitionsChangedEventChannelSO AvailableQuestionnaireDefinitionsChangedChannel;
        public RequestScreenshotEventChannelSO RequestScreenshotEventChannel;

        [HideInInspector]
        public IResearchSweetClient ResearchSweetClient { get; private set; }

        void OnEnable()
        {
            var initResult = ResearchSweetExtensions.Initialize(gameObject, ApiKey, EndpointUrl, new EventChannels(VariableChangedEventChannel, MyQuestionnairesChanged, AvailableQuestionnaireDefinitionsChangedChannel, QuestionnaireChangedChannel, RequestScreenshotEventChannel));
            ResearchSweetClient = initResult.Client;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
