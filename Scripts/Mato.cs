using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HKTool.FSM;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace NailMasterMod
{
    class Mato : MonoBehaviour
    {
        public GameObject boss = null;
        void Start()
        {
            gameObject.SetActive(false);
            boss = Instantiate(NailMaster.NailMasterPrefab);
            boss.transform.position = transform.position;
            PlayMakerFSM.BroadcastEvent("BG CLOSE");
            PlayMakerFSM control = boss.LocateMyFSM("nailmaster");
            
            boss.SetActive(true);

            control.Fsm.CreatePatch()
                .EditState("Init")
                .FindAction<BoolTest>(x => x.isFalse = x.isTrue)
                .FindTransition("ORO", t => t.ToFsmState = control.Fsm.GetState("Roar Antic"))
                .EditState("Cyclone")
                .AppendAction(FSMHelper.CreateMethodAction(action =>
                {
                    FSMUtility.SetFloat(action.Fsm.FsmComponent, "Topslash Y", 8);
                }))
                .EditState("First Idle")
                .ForEachFsmStateActions<SetFsmGameObject>(x => null)
                .EditState("Roar")
                .ForEachFsmStateActions<SetFsmString>(x=>null)
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
            yield return new WaitForSeconds(0.5f);
            while (!HeroController.instance.cState.onGround || !HeroController.instance.CanTalk()) yield return null;
            gameObject.LocateMyFSM("Conversation Control").SetState("Box Up 3");

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
