using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HKTool.FSM;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace NailMasterMod
{
    class Sly : MonoBehaviour
    {
        GameObject boss = null;
        void Start()
        {
            gameObject.SetActive(false);
            boss = Instantiate(NailMaster.Sly);
            boss.transform.position = transform.position;
            boss.SetActive(true);
            var ctrl = boss.LocateMyFSM("Control");
            FSMUtility.SetFloat(ctrl, "Topslash Y", 12);
            FSMUtility.SetFloat(ctrl, "Jump X", 75);

            ctrl.Fsm.CreateQuene()
                .EditState("Nail Fail")
                .FindAction<SetPosition>(a =>
                {
                    a.x.Value = 75;
                    a.y.Value = 14;
                })
                .EditState("Air Catch")
                .FindAction<SetPosition>(a =>
                {
                    a.x.Value = 75;
                })
                ;
        }
    }
}
