using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HKTool.FSM;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace NailMasterMod
{
    class Oro : MonoBehaviour
    {
        GameObject boss = null;
        void Start()
        {
            gameObject.SetActive(false);
            boss = Instantiate(NailMaster.NailMasterPrefab);
            boss.transform.position = transform.position;
            PlayMakerFSM.BroadcastEvent("BG CLOSE");
            PlayMakerFSM control = boss.LocateMyFSM("nailmaster");
            var hm = boss.GetComponent<HealthManager>();
            if(hm != null)
            {
                hm.hp = 1200;
            }

            boss.SetActive(true);

            control.Fsm.CreateQuene()
                .EditState("Init")
                .FindAction<BoolTest>(x => x.isFalse = x.isTrue)
                .FindTransition("ORO", t => t.ToFsmState = control.Fsm.GetState("Roar Antic"))
                .EditState("Dash Forward")
                .InsertAction(FSMHelper.CreateMethodAction(action =>
                {
                    FSMUtility.SetFloat(action.Fsm.FsmComponent, "End X", HeroController.instance.transform.position.x);
                }), 0)
                .EditState("First Idle")
                .ForEachFsmStateActions<SetFsmGameObject>(x => null)
                .EditState("Roar")
                .ForEachFsmStateActions<SetFsmString>(x => null)
                .EditState("Death Land")
                .ForEachFsmStateActions<FsmStateAction>(x => null)
                .AppendAction(FSMHelper.CreateMethodAction(Dead))
                .EndFSMEdit()
                ;

            control.Fsm.SetState("Init");
            FSMUtility.SetBool(control, "Oro", true);
            FSMUtility.SetBool(control, "Brothered", false);
            FSMUtility.SetFloat(control, "Topslash Y", 8);
        }
        IEnumerator Talk()
        {
            while (!HeroController.instance.cState.onGround || !HeroController.instance.CanTalk()) yield return null;
            gameObject.LocateMyFSM("Conversation Control").SetState("Box Up 3");
            HeroController.instance.GetComponent<tk2dSpriteAnimator>().Stop();
            PlayerData.instance.disablePause = true;
            HeroController.instance.RelinquishControl();
            HeroController.instance.StopAnimationControl();
            PlayMakerFSM.BroadcastEvent("NPC CONVO START");
            if (HeroController.instance.transform.position.x > transform.position.x)
            {
                var ls = transform.localScale;
                ls.x = -ls.x;
                transform.localScale = ls;
            }
        }
        void Dead(FsmStateAction _)
        {
            
            PlayMakerFSM.BroadcastEvent("BG OPEN");
            gameObject.transform.position = boss.transform.position;
            gameObject.SetActive(true);
            StartCoroutine(Talk());
            Destroy(boss);
        }
    }
}
