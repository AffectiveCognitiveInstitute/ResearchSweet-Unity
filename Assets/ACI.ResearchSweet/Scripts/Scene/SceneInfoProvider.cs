using System.Collections.Generic;
using System.Linq;
using ResearchSweet.Transport;
using UnityEngine.SceneManagement;

namespace ResearchSweet.Scene
{
    internal class UnitySceneInfoProvider : ISceneInfoProvider
    {
        #region Constructor

        public UnitySceneInfoProvider()
        {
            GenerateSceneDictionary();
        }

        #endregion

        #region Helper

        private Dictionary<int, string> _scenes = new Dictionary<int, string>();

        private void GenerateSceneDictionary()
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = path.Split('/').LastOrDefault().Replace(".unity", "");
                _scenes.Add(i, name);
            }
        }

        #endregion

        #region ISceneInfoProvider

        public IResearchSweetClient ResearchSweetClient { get; set; }
        public Dictionary<int, string> Scenes => _scenes;

        public int ActiveScene => SceneManager.GetActiveScene().buildIndex;

        #endregion
    }
}
