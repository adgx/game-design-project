// using Audio;
// using FMOD.Studio;
// using UnityEngine;
//
// namespace Animations
// {
//     public class IncognitoEvents : MonoBehaviour
//     {
//         // Audio management
//         private EventInstance incognitoFootsteps;
//         private EventInstance incognitoIdle;
//
//         private bool isRunning = false; // TODO: to be removed once we have Incognito's FSM
//         private bool isIdle = false; // TODO: to be removed once we have Incognito's FSM
//
//         public void Idle()
//         {
//             Debug.Log("Idle");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             isIdle = true; // TODO: to be removed once we have Incognito's FSM
//         }
//
//         public void Run()
//         {
//             Debug.Log("Run");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             isRunning = true; // TODO: to be removed once we have Incognito's FSM
//         }
//
//         public void Bite()
//         {
//             Debug.Log("Bite");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.incognitoCloseAttack1, transform.position);
//         }
//
//         public void Swiping()
//         {
//             Debug.Log("Swiping");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.incognitoCloseAttack2, transform.position);
//         }
//
//         public void Defense()
//         {
//             Debug.Log("Defense");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.incognitoDefense, transform.position);
//         }
//
//         public void ReactLargeFromRight()
//         {
//             Debug.Log("ReactLargeFromRight");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.incognitoHitFromLeftOrRight, transform.position);
//         }
//
//         public void ReactLargeFromLeft()
//         {
//             Debug.Log("ReactLargeFromLeft");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.incognitoHitFromLeftOrRight, transform.position);
//         }
//
//         public void ReactLargeFromFront()
//         {
//             Debug.Log("ReactLargeFromFront");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.incognitoHitFromFrontOrBack, transform.position);
//         }
//
//         public void ReactLargeFromBack()
//         {
//             Debug.Log("ReactLargeFromBack");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.incognitoHitFromFrontOrBack, transform.position);
//         }
//
//         public void DeathHit()
//         {
//             Debug.Log("DeathHit");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.incognitoDieHit, transform.position);
//         }
//
//         public void DeathFootstep1()
//         {
//             Debug.Log("DeathFootstep1");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.incognitoDieFoostep1, transform.position);
//         }
//
//         public void DeathFootstep2()
//         {
//             Debug.Log("DeathFootstep2");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.incognitoDieFoostep2, transform.position);
//         }
//
//         public void DeathThud()
//         {
//             Debug.Log("DeathThud");
//
//             // Audio management
//             ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
//             GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.incognitoDieThud, transform.position);
//         }
//
//         // Audio management
//         private void Start()
//         {
//             incognitoFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.incognitoFootsteps);
//             incognitoFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
//
//             incognitoIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.incognitoIdle);
//             incognitoIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
//         }
//
//         // FixedUpdate is called once per frame
//         void FixedUpdate()
//         {
//             // Audio management
//             UpdateSound();
//         }
//
//         private void UpdateSound()
//         {
//             incognitoFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
//             incognitoIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
//
//             // Start footsteps event if incognito is moving
//             if (isRunning) // TODO: check Incognito's state (something like <<DrakState != Run>>)
//             {
//                 // Get the playback state for the footsteps event
//                 PLAYBACK_STATE footstepsPlaybackState;
//                 incognitoFootsteps.getPlaybackState(out footstepsPlaybackState);
//                 if (footstepsPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
//                 {
//                     incognitoFootsteps.start();
//                 }
//             }
//             // Otherwise, stop the footsteps event
//             else
//             {
//                 incognitoFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
//             }
//
//             // Start idle event if incognito is using the idle animation
//             if (isIdle) // TODO: check Incognito's state (something like <<DrakState != Idle>>)
//             {
//                 // Get the playback state for the idle event
//                 PLAYBACK_STATE idlePlaybackState;
//                 incognitoIdle.getPlaybackState(out idlePlaybackState);
//                 if (idlePlaybackState.Equals(PLAYBACK_STATE.STOPPED))
//                 {
//                     incognitoIdle.start();
//                 }
//             }
//             // Otherwise, stop the idle event
//             else
//             {
//                 incognitoIdle.stop(STOP_MODE.ALLOWFADEOUT);
//             }
//
//         }
//
//         // TODO: to be removed once we have Incognito's FSM
//         private void ResetAudioState()
//         {
//             isRunning = false;
//             isIdle = false;
//         }
//     }
// }
