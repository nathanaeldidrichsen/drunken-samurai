using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*This script can be used on pretty much any gameObject. It provides several functions that can be called with 
animation events in the animation window.*/

public class AnimatorFunctions : MonoBehaviour
{
    // [SerializeField] private AudioSource audioSource;
    // [SerializeField] private ParticleSystem particleSystem;
    // [SerializeField] private Animator setBoolInAnimator;
    private bool stepSoundIsPlaying;

    [Header("Audio")]
    public SoundData stepSound;

    // If we don't specify what audio source to play sounds through, just use the one on player.
    void Start()
    {
        //if (!audioSource) audioSource = SoundManager.Instance.audioSource;
    }

    //Hide and unhide the player
    public void HidePlayer(bool hide)
    {
        //NewPlayer.Instance.Hide(hide);
    }

    //Sometimes we want an animated object to force the player to jump, like a jump pad.
    public void JumpPlayer(float power = 1f)
    {
        //NewPlayer.Instance.Jump(power);
    }

    IEnumerator FinishStepSound()
    {
        if (!stepSoundIsPlaying)
        {
            SoundManager.Instance.PlaySFX(stepSound);
            stepSoundIsPlaying = true;
        }

        yield return new WaitForSeconds(0.2f);
        stepSoundIsPlaying = false;

    }

    public void EmitParticles(int amount)
    {
        //particleSystem.Emit(amount);
    }


    // Called by animation event near end of Attack_1
    public void EnableCombo()
    {
        // Player.Instance.canCombo = true;
        CombatController.Instance.EnableComboWindow();
        // UnFreezeMyPlayer();
    }

    // Called by animation event at end of Attack animations
    public void ResetCombo()
    {
        CombatController.Instance.EndAttack();
    }

    public void AttackLunge()
    {
        CombatController.Instance.DoAttackLunge();
    }

    public void FreezeMyPlayer()
    {
        // Player.Instance.FreezePlayer(true);
    }
    public void UnFreezeMyPlayer()
    {
        Player.Instance.FreezePlayer(false);
    }

    public void ScreenShake(float power)
    {
        //NewPlayer.Instance.cameraEffects.Shake(power, 1f);
    }

    public void SetTimeScale(float time)
    {
        Time.timeScale = time;
    }

    public void SetAnimBoolToFalse(string boolName)
    {
        // setBoolInAnimator.SetBool(boolName, false);
    }

    public void StopRolling()
    {
        //Player.Instance.StopRolling();
    }

    public void PlayRollSound()
    {
        if (Player.Instance != null)
            Player.Instance.PlayRollSound();
    }

    public void SetAnimBoolToTrue(string boolName)
    {
        // setBoolInAnimator.SetBool(boolName, true);
    }

    public void FadeOutMusic()
    {
        //GameManager.Instance.gameMusic.GetComponent<AudioTrigger>().maxVolume = 0f;
    }

    public void LoadScene(string whichLevel)
    {
        SceneManager.LoadScene(whichLevel);
    }

    //Slow down or speed up the game's time scale!
    public void SetTimeScaleTo(float timeScale)
    {
        Time.timeScale = timeScale;
    }
}
