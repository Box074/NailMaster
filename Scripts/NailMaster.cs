using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using HKTool;
using HKTool.FSM;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;

namespace NailMasterMod
{
    public class NailMaster : ModBase
    {
        public static GameObject NailMasterPrefab = null;
        public static GameObject Gate = null;
        public static GameObject Sly = null;
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Nailmasters","Brothers/Oro"),
                ("Fungus1_04","Battle Gate A")
            };
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            NailMasterPrefab = preloadedObjects["GG_Nailmasters"]["Brothers/Oro"];
            UnityEngine.Object.Destroy(NailMasterPrefab.GetComponent<ConstrainPosition>());
            Gate = preloadedObjects["Fungus1_04"]["Battle Gate A"];
            
        }
        private static void TalkEnd()
        {
            HeroController.instance.RegainControl();
            HeroController.instance.StartAnimationControl();
            PlayerData.instance.disablePause = false;
            HeroController.instance.PreventCastByDialogueEnd();
        }

        [FsmPatcher("Room_nailmaster_03", "NM Oro NPC", "Conversation Control")]
        private static void OroNPCPatch(FSMPatch patch)
        {

            var gate = UnityEngine.Object.Instantiate(Gate);
            gate.transform.position = new Vector3(14.15f, 4.4081f, 0.004f);
            gate.transform.SetScaleY(1.1f);
            gate.SetActive(true);
            PlayMakerFSM.BroadcastEvent("BG QUICK OPEN");
            patch
                .AddState("BOSS_STATUS")
                .EditState("Get Msg")
                .ForEachFsmStateActions<Translate>(a => null)
                .EditState("Bow")
                .AppendAction(FSMHelper.CreateMethodAction(action =>
                {
                    TalkEnd();
                    action.Fsm.GameObject.LocateMyFSM("npc_control").enabled = false;
                }))
                .EditState("Decline Pause")
                .AppendAction(FSMHelper.CreateMethodAction(
                    (action) =>
                    {
                        action.Fsm.SetState("BOSS_STATUS");
                        HeroController.instance.RegainControl();
                        HeroController.instance.StartAnimationControl();
                        HeroController.instance.PreventCastByDialogueEnd();
                        PlayerData.instance.disablePause = false;
                        action.Fsm.GameObject.AddComponent<Oro>();
                    }
                    ))
                .EditState("Box Down 2")
                .ChangeTransition("FINISHED", "Get Msg")
                .EditState("Fade Back")
                .FindAction<Wait>(a => a.time = 0.35f)
                .ChangeTransition("FINISHED", "Bow")
                ;
        }
        [FsmPatcher("Room_nailmaster", "NM Mato NPC", "Conversation Control")]
        private static void MatoNPCPatch(FSMPatch patch)
        {
            var gate = UnityEngine.Object.Instantiate(Gate);
            gate.transform.position = new Vector3(50.4461f, 4.4081f, 0.004f);
            gate.transform.SetScaleY(1.1f);
            gate.SetActive(true);
            PlayMakerFSM.BroadcastEvent("BG QUICK OPEN");
            patch
                .AddState("BOSS_STATUS")
                .EditState("Get Msg")
                .ForEachFsmStateActions<Translate>(a => null)
                .EditState("Bow")
                .AppendAction(FSMHelper.CreateMethodAction(action =>
                {
                    TalkEnd();
                    action.Fsm.GameObject.LocateMyFSM("npc_control").enabled = false;
                }))
                .EditState("Yes")
                .AppendAction(FSMHelper.CreateMethodAction(action =>
                {
                    action.Fsm.SetState("BOSS_STATUS");
                    HeroController.instance.RegainControl();
                    HeroController.instance.StartAnimationControl();
                    HeroController.instance.PreventCastByDialogueEnd();
                    PlayerData.instance.disablePause = false;
                    action.Fsm.GameObject.AddComponent<Mato>();
                }))
                .EditState("Box Down 2")
                .ChangeTransition("FINISHED", "Get Msg")
                .EditState("Fade Back")
                .FindAction<Wait>(a => a.time = 0.35f)
                .ChangeTransition("FINISHED", "Bow")
                ;
        }
    }
}
