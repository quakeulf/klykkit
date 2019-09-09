using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KlykkitChanimation : MonoBehaviour
{
    public Texture2D[] Idle1;
    public Texture2D[] Idle2;
    public Texture2D[] Lose;
    public Texture2D[] Swurl;

    public Texture2D[] Celebrate;
    public Texture2D[] Smug;
    public Texture2D[] Warn;
    public Texture2D[] X3;
    public Texture2D[] Speech;
    public Texture2D[] Win;

    public float IdleSwitch = 0f;

    public int MagicNumber = 0;

    public MeshRenderer Renderer;
    public Material Material;

    private const float IDLE_SWITCH_TIMER = 10f;

    public void PlayAnimation(Texture2D[] texture, int FPS = 6)
    {
        int index = (int)(Time.time * FPS);
        index = index % texture.Length;
        Material.mainTexture = texture[index];
    }

    public void AnimationPlayer()
    {
        switch(MagicNumber)
        {
            case 0:
                PlayIdle();
                break;

            case 1:
                PlayAnimation(Celebrate, 4);
                break;

            case 2:
                PlayAnimation(Lose);
                break;

            case 3:
                PlayAnimation(Smug);
                break;

            case 4:
                PlayAnimation(X3);
                break;

            case 5:
                PlayAnimation(Warn);
                break;

            case 6:
                PlayAnimation(Swurl);
                break;
            
            case 7:
                PlayAnimation(Speech, 4);
                break;

            case 8:
                PlayAnimation(Win, 4);
                break;

            default:
                PlayIdle();
                break;
        }
    }

    public void PlayIdle()
    {
        IdleSwitch += Time.deltaTime;

        if(IdleSwitch < IDLE_SWITCH_TIMER)
        {
            PlayAnimation(Idle1, 5);
        }
        else
        {
            PlayAnimation(Idle2, 5);
        }
    }

    public void Setup()
    {
        Renderer = this.GetComponentInChildren<MeshRenderer>();
        Material = Renderer.material;
    }
}
