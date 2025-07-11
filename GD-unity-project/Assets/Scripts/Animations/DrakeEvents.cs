using Audio;
using FMOD.Studio;
using UnityEngine;

namespace Animations
{
    public class DrakeEvents : MonoBehaviour
    {
        // Audio management
        private EventInstance drakeFootsteps;
        private EventInstance drakeIdle;
        private DrakeAnimation drakeAnim;
        private Drake drake;
        
        private void Awake()
        {
            drake = GetComponent<Drake>();

            if (drake == null)
            {
                Debug.LogError($"{ToString()}: Drake not found");
            }
            
            drakeAnim = drake.anim;

            // Audio management
            drakeFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.DrakeFootsteps);
            drakeFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            drakeIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.DrakeIdle);
            drakeIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
        
        void FixedUpdate()
        {
            // Audio management: update Drake's position as he's a sound source
            drakeFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            drakeIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
        
        private void OnDestroy()
        {
            // Stop events immediately to prevent the sound from continuing after destruction
            // and releases the resources used by the instances
            if (GamePlayAudioManager.instance != null)
            {
                GamePlayAudioManager.instance.ReleaseInstance(drakeFootsteps);
                GamePlayAudioManager.instance.ReleaseInstance(drakeIdle);
            }
        }

        public void Bite()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeCloseAttack1, transform.position);

            drake.CheckBiteAttackDamage();
        }

        public void EndBite()
        {
            drakeAnim.EndBit = true;
        }

        public void Swiping()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeCloseAttack2, transform.position);
        }
        
        public void EndSwiping()
        {
            drakeAnim.EndSwiping = true;
        }
        
        public void SwipingAttackHit()
        {
            drake.CheckSwipingAttackDamage();
        }

        public void Defense()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeDefense, transform.position);
        }

        public void ReactLargeFromRight()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeHitFromLeftOrRight, transform.position);
        }

        public void ReactLargeFromLeft()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeHitFromLeftOrRight, transform.position);
        }

        public void ReactLargeFromFront()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeHitFromFrontOrBack, transform.position);
        }

        public void ReactLargeFromBack()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeHitFromFrontOrBack, transform.position);
        }

        public void DeathHit()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeDieHit, transform.position);
        }

        public void DeathFootstep1()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeDieFoostep1, transform.position);
        }

        public void DeathFootstep2()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeDieFoostep2, transform.position);
        }

        public void DeathDrake()
        {
            drake.DestroyEnemy();
        }

        public void DeathThud()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeDieThud, transform.position);
        }
        
        // Audio management
        public void StartRunningSound()
        {
            drakeFootsteps.start();
        }
            
        // Audio management
        public void StopRunningSound()
        {
            drakeFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }

        // Audio management
        public void StartIdleSound()
        {
            drakeIdle.start();
        }
        
        // Audio management
        public void StopIdleSound()
        {
            drakeIdle.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}