using Audio;
using FMOD.Studio;
using Unity.VisualScripting;
using UnityEngine;

namespace Animations
{
    public class IncognitoEvents : MonoBehaviour
    {
        // Audio management
        private static EventInstance incognitoFootsteps;
        private static EventInstance incognitoIdle;
        private Incognito incognito;
        private IncognitoAnimation incognitoAnim;

        private void Start()
        {
            incognito = GetComponent<Incognito>();

            incognitoAnim = incognito.anim;

            incognitoFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.IncognitoFootsteps);
            incognitoFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            incognitoIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.IncognitoIdle);
            incognitoIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
        
        // This function is called when Incognito should emit its spit
        public void Spitting()
        {
            incognito.EmitSpit();
        }
        
        public void ShortDistanceSpit()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDistanceAttack1, transform.position);
        }
        
        public void EndShortSpit()
        {
            incognitoAnim.EndShortSpit = true;
        }

        public void LongDistanceSpitLoad()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDistanceAttack2Load, transform.position);
        }

        public void LongDistanceSpitShoot()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDistanceAttack2Spit, transform.position);
        }
        
        public void EndLongSpit()
        {
            incognitoAnim.EndShortSpit = true;
        }
        
        public void FallScream()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFallScream, transform.position);
        }

        public void FallFootstep1()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFallFootstep1, transform.position);
        }

        public void FallFootstep2()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFallFootstep2, transform.position);
        }

        public void FallThud()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFallThud, transform.position);
        }

        public void StandUpFootstep1()
        {
            // Audio management;
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoStandUpFootstep1, transform.position);
        }

        public void StandUpFootstep2()
        {
            // Audio management;
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoStandUpFootstep2, transform.position);
        }

        public void ReactLargeFromRight()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFromLeftOrRight, transform.position);
        }

        public void ReactLargeFromLeft()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFromLeftOrRight, transform.position);
        }

        public void ReactLargeGut()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFromFront2, transform.position);
        }

        public void ReactLargeFromFront()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFromFront1, transform.position);
        }

        public void DeathGrunt()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDieGrunt, transform.position);
        }

        public void DeathThud1()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDieThud1, transform.position);
        }

        public void DeathThud2()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDieThud2, transform.position);
        }

        public void DeathIncognito()
        {
            incognito.DestroyEnemy();
        }
        
        // Audio management
        public static void StartRunningSound()
        {
            incognitoFootsteps.start();
        }
        
        // Audio management
        public static void StopRunningSound()
        {
            incognitoFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
        
        // Audio management
        public static void StartIdleSound()
        {
            incognitoIdle.start();
        }
        
        // Audio management
        public static void StopIdleSound()
        {
            incognitoIdle.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}
