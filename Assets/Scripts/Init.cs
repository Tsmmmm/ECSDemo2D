using ET;
using UnityEngine;

public class Init : MonoBehaviour
{
    private ETCancellationToken cancellationToken;
    void Start()
    {
        cancellationToken = new ETCancellationToken();
        Player playerA = EntityFactory.Create<Player, string>("playerA");
        Player playerB = EntityFactory.Create<Player, string>("playerB");
        ComponentFactory.CreateWithEntity<TimerComponent>(playerA);
        Game.Scene.AddChild(playerA);
        Game.Scene.AddChild(playerB);
        TestAsync();
    }

    void Update()
    {
        Game.EventSystem.Update();
        if (Input.GetKeyDown(KeyCode.S)) 
        {
            cancellationToken.Cancel();
        }
    }

    void LateUpdate()
    {
        Game.EventSystem.LateUpdate();   
    }

    void OnApplicationQuit()
    {
        Game.Close();     
    }

    async void TestAsync() 
    {
        bool result = await Test1(cancellationToken);
        Log.Error($"test async   {result}");
        await TimerComponent.Instance.WaitAsync(5000);
        Log.Error("other await");
    }

    async ETTask<bool> Test1(ETCancellationToken cancellationToken) 
    {
        ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
        cancellationToken.Register(() => 
        {
            Log.Error("cancle");
        });
        await TimerComponent.Instance.WaitAsync(5000, cancellationToken);
        tcs.SetResult(true);
        Log.Debug("after cancle");
        return await tcs.Task;
    }
}
