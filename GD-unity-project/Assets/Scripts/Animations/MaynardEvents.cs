using Audio;
using FMOD.Studio;
using UnityEngine;

namespace Animations
{
    public class MaynardEvents : MonoBehaviour
    {
        // Audio management
        private EventInstance maynardFootsteps;
        private EventInstance maynardIdle;
        
        private MaynardAnimation _maynardAnim;
        private Maynard maynard;

        private void Start()
        {
            _maynardAnim = maynard.anim;
        }
        
        private void Awake()
        {
            maynard = GetComponent<Maynard>();
            
            // Audio management
            maynardFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.MaynardFootsteps);
            maynardFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            maynardIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.MaynardIdle);
            maynardIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
        
        private void FixedUpdate()
        {
            // Audio management: update Maynard's position as he's a sound source
            maynardFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            maynardIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
        
        private void OnDestroy()
        {
            // Stop events immediately to prevent the sound from continuing after destruction
            // and releases the resources used by the instances
            if (GamePlayAudioManager.instance != null)
            {
                GamePlayAudioManager.instance.ReleaseInstance(maynardFootsteps);
                GamePlayAudioManager.instance.ReleaseInstance(maynardIdle);
            }
        }

        public void Scream()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardDistanceAttack1, transform.position);

            maynard.EmitScream();
        }
        
        public void EndScream()
        {
            _maynardAnim.EndScream = true;
        }

        public void MutantRoaring()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardDistanceAttack2, transform.position);
        }

        public void Attack()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardCloseAttack, transform.position);
        }

        public void CloseAttackHit()
        {
            maynard.CheckCloseAttackDamage();
        }
        
        public void EndCloseAttack()
        {
            _maynardAnim.EndCloseAttack=true;
        }

        public void FallScream()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFallScream, transform.position);
        }

        public void FallThud()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFallThud, transform.position);
        }

        public void StandUpRoar()
        {
            // Audio management;
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardStandUpRoar, transform.position);
        }

        public void StandUpFootstep1()
        {
            // Audio management;
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardStandUpFootstep1, transform.position);
        }

        public void StandUpFootstep2()
        {
            // Audio management;
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardStandUpFootstep2, transform.position);
        }

        public void StandUpBreath()
        {
            // Audio management;
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardStandUpBreath, transform.position);
        }

        public void ReactLargeFromRight()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFromLeftOrRight, transform.position);
        }

        public void ReactLargeFromLeft()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFromLeftOrRight, transform.position);
        }

        public void ReactLargeFromFront()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFromFront, transform.position);
        }

        public void ReactLargeFromBack()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFromBack, transform.position);
        }

        public void DeathScream()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardDieScream, transform.position);
        }

        public void DeathThud()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardDieThud, transform.position);
        }
        public void DeathMaynard()
        {
            maynard.DestroyEnemy();
        }
        
        // Audio management
        public void StartRunningSound()
        {
            maynardFootsteps.start();
        }
            
        // Audio management
        public void StopRunningSound()
        {
            maynardFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }

        // Audio management
        public void StartIdleSound()
        {
            maynardIdle.start();
        }
        
        // Audio management
        public void StopIdleSound()
        {
            maynardIdle.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}