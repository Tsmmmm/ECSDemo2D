using ET;
using UnityEngine;
using UnityEngine.InputSystem;

public class Init : MonoBehaviour
{
    void Start()
    {
        Player player = EntityFactory.Create<Player, string>("model");
        Game.Scene.AddChild(player);
        InputComponent input = ComponentFactory.CreateWithEntity<InputComponent, string, string>(Game.Scene, "GameInputControl", "KeyBordControl");
        input.onStarted += (ctx) =>
        {
            player.GetComponent<Live2dComponent>().PlayAnimation("Walk");
        };

        input.onPerformed += (ctx) =>
        {
            player.GetComponent<Live2dComponent>().PlayAnimation("Run");
        };

        input.onCanceled += (ctx) =>
        {
            player.GetComponent<Live2dComponent>().PlayAnimation("Idle");
        };
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
