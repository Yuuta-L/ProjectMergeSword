using BurnicleFramework.Runtime;

namespace MergeSword
{
    public static class SceneExtension
    {
        public static void LoadSingleScene(this SceneComponent sceneComponent,string sceneAssetName,object userData = null)
        {
            var loadedSceneAssetNames = Burnicle.Scene.GetLoadedSceneAssetNames();
            for (var i = 0; i < loadedSceneAssetNames.Length; i++)
            {
                Burnicle.Scene.UnloadScene(loadedSceneAssetNames[i]);
            }
            Burnicle.Scene.LoadScene(sceneAssetName,userData);
        }
    }
}