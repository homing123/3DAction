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
        CharacterData.LoadStreamingData();
        SceneManager.LoadScene("PlayerMoveTest");
    }
}
