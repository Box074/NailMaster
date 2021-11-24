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
        public static GameObject Sheo = null;
        public static GameObject Gate = null;
        public static GameObject Sly = null;
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Nailmasters","Brothers/Oro"),
                ("GG_Painter","Battle Scene/Sheo Boss"),
                ("GG_Sly", "Battle Scene/Sly Boss"),
                ("Fungus1_04","Battle Gate A")
            };
        }
        public override void Initialize()
        {
            
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
            NailMasterPrefab = preloadedObjects["GG_Nailmasters"]["Brothers/Oro"];
            UnityEngine.Object.Destroy(NailMasterPrefab.GetComponent<ConstrainPosition>());
            Sheo = preloadedObjects["GG_Painter"]["Battle Scene/Sheo Boss"];
            Sly = preloadedObjects["GG_Sly"]["Battle Scene/Sly Boss"];
            Gate = preloadedObjects["Fungus1_04"]["Battle Gate A"];
        }

        private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
            if (key == "NM_Sly_01")
            {
                return "我要给你一个礼物，战士。我作为店主真是很难把它免费割舍给别的虫子啊。<page>" +
                    "所以作为伟大的骨钉贤者，我必须收你一点小钱。";
            }
            else
            {
                return orig;
            }
        }

        //TODO
        [FsmPatch("Room_Sly_Storeroom", "Sly Basement NPC", "Conversation Control")]
        private static void SlyNPCPatch(FSMQuene patch)
        {
            var gate = UnityEngine.Object.Instantiate(Gate);
            gate.transform.position = new Vector3(54.884f, 4.4081f, 0.004f);
            gate.transform.SetScaleY(1.1f);
            gate.SetActive(true);
            PlayMakerFSM.BroadcastEvent("BG QUICK OPEN");

            patch
                .AddState("BOSS_STATUS")
                .AddStateAndEdit("BOSS_TAKE YES")
                .AppendAction(FSMHelper.CreateMethodAction(action =>
                {
                    PlayMakerFSM.BroadcastEvent("BOX DOWN YN");
                    PlayMakerFSM.BroadcastEvent("BOX UP");
                    action.Fsm.SetState("Give 3");
                }))
                .AddStateAndEdit("BOSS_TAKE NO")
                .AppendAction(FSMHelper.CreateMethodAction(action =>
                {
                    action.Fsm.SetState("BOSS_STATUS");
                    HeroController.instance.RegainControl();
                    HeroController.instance.StartAnimationControl();
                    HeroController.instance.PreventCastByDialogueEnd();
                    PlayerData.instance.disablePause = false;
                }))
                .AddStateAndEdit("BOSS_TALK_1")
                .AppendAction(FSMHelper.CreateMethodAction(action =>
                {
                    PlayMakerFSM.BroadcastEvent("BOX DOWN");
                    var yn = FSMHelper.FindFsms("Dialogue Page Control").FirstOrDefault(x => x.GameObject.name == "Text YN")
                    .FsmComponent;
                    yn.Fsm.Variables.FindFsmGameObject("Requester").Value = action.Fsm.GameObject;
                    FSMUtility.SetInt(yn, "Toll Cost", int.MaxValue);
                    PlayMakerFSM.BroadcastEvent("BOX UP YN");
                }))
                
                .EditState("Give 1")
                .ForEachFsmStateActions<FsmStateAction>(a => null)
                .AppendAction(FSMHelper.CreateMethodAction(action =>
                {
                    action.Fsm.SetState("BOSS_TALK");

                }))
                .EditState("Give 3")
                .FindAction<Tk2dPlayAnimation>(a => a.clipName = "Pray Rise")
                .AddStateAndEdit("BOSS_TALK")
                .AppendAction(FSMHelper.CreateMethodAction(action =>
                {
                    UnityEngine.Object.FindObjectOfType<DialogueBox>().StartConversation("NM_Sly_01", "Sly");
                }))
                .AppendTransition("CONVO_FINISH", "BOSS_TALK_1")
                ;
        }

        [FsmPatch("Room_nailmaster_03", "NM Oro NPC", "Conversation Control")]
        private static void OroNPCPatch(FSMQuene patch)
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
                    action.Fsm.GameObject.LocateMyFSM("npc_control").SetState("Convo End");
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
                .FindTransition("FINISHED", t => t.ToFsmState = patch.TargetFSM.GetState("Get Msg"))
                .EditState("Fade Back")
                .FindAction<Wait>(a => a.time = 0.35f)
                .FindTransition("FINISHED", t => t.ToFsmState = patch.TargetFSM.GetState("Bow"))
                ;
        }
        [FsmPatch("Room_nailmaster", "NM Mato NPC", "Conversation Control")]
        private static void MatoNPCPatch(FSMQuene patch)
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
                    action.Fsm.GameObject.LocateMyFSM("npc_control").SetState("Convo End");
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
                .FindTransition("FINISHED", t => t.ToFsmState = patch.TargetFSM.GetState("Get Msg"))
                .EditState("Fade Back")
                .FindAction<Wait>(a => a.time = 0.35f)
                .FindTransition("FINISHED", t => t.ToFsmState = patch.TargetFSM.GetState("Bow"))
                ;
        }
    }
}
