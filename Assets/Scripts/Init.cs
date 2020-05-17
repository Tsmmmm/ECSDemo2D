using ET;
using UnityEngine;

public class Init : MonoBehaviour
{
    void Start()
    {
        Player player = EntityFactory.Create<Player, string>("model");
        Game.Scene.AddChild(player);
    }

    void OnGUI()
    {
        Game.CurCode = Event.current.keyCode;
    }

    void Update()
    {
        Game.EventSystem.Update();
    }

    void LateUpdate()
    {
        Game.EventSystem.LateUpdate();   
    }

    void OnApplicationQuit()
    {
        Game.Close();     
    }
}
