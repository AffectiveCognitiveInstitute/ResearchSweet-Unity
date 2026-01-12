using ResearchSweet.Network;
using ResearchSweet.Transport;
using UnityEngine;
using ResearchSweetHelpers = ResearchSweet.Helpers.ResearchSweetHelpers;

namespace ResearchSweet
{
    public class ResearchSweetInitComponent : MonoBehaviour
    {
        public ConnectionScriptableObject connection;
        
        //public MyQuestionnairesEventChannelSO MyQuestionnairesChanged;
        //public VariableChangedEventChannelSO VariableChangedEventChannel;
        //public QuestionnaireChangedEventChannelSO QuestionnaireChangedChannel;
        //public AvailableQuestionnaireDefinitionsChangedEventChannelSO AvailableQuestionnaireDefinitionsChangedChannel;
        //public RequestScreenshotEventChannelSO RequestScreenshotEventChannel;

        [HideInInspector]
        public IResearchSweetClient ResearchSweetClient { get; private set; }

        void OnEnable()
        {
            if (ResearchSweetHelpers.Client == null)
            {
                var initResult = ResearchSweetExtensions.Initialize(gameObject, connection.apiKey, connection.endpointUrl);
                ResearchSweetClient = initResult.Client;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
