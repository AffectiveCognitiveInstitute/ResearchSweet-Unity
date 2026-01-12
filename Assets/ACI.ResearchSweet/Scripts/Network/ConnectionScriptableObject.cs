using UnityEngine;

namespace ResearchSweet.Network
{
    [CreateAssetMenu(fileName = "Connection", menuName = "ResearchSweet/Network/Connection")]
    public class ConnectionScriptableObject : ScriptableObject
    {
        #region Properties
        
        public string endpointUrl;
        public string apiKey;
        
        #endregion
    }
}
