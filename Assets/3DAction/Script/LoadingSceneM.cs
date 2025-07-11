using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
public class LoadingSceneM : MonoBehaviour
{
    
    void Start()
    {
        LoadingSceneSequence().Forget();
    }

    async UniTask LoadingSceneSequence()
    {
        TextData.LoadStreamingData();
        SkillData.LoadStreamingData();
        SceneManager.LoadScene("PlayerMoveTest");
    }
}
