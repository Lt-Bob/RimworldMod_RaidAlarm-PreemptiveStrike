﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using PreemptiveStrike.Mod;

namespace PreemptiveStrike.Interceptor
{
    class InterceptedIncident_HumanCrowd_RaidEnemy : InterceptedIncident_HumanCrowd
    {
        public override bool IsHostileToPlayer => true;

        public bool raidStrategy_revealed = false;

        private string strategyString;
        public virtual string StrategyString
        {
            get
            {
                if(strategyString == null)
                {
                    Type workerClass = parms.raidStrategy.workerClass;
                    if (workerClass == typeof(RaidStrategyWorker_ImmediateAttack))
                        strategyString = "Frontal assault";
                    if (workerClass == typeof(RaidStrategyWorker_ImmediateAttackSappers))
                        strategyString = "Sappers";
                    if (workerClass == typeof(RaidStrategyWorker_ImmediateAttackSmart))
                        strategyString = "Smart Attack";
                    if (workerClass == typeof(RaidStrategyWorker_Siege))
                        strategyString = "Siege";
                    if (workerClass == typeof(RaidStrategyWorker_StageThenAttack))
                        strategyString = "Stage then attack";
                }
                return strategyString;
            }
        }

        protected virtual void RevealStrategy()
        {
            raidStrategy_revealed = true;

            if (PES_Settings.DebugModeOn)
            {

            }
        }

        protected override void RevealCrowdSize()
        {
            crowdSize_revealed = true;

            if (PES_Settings.DebugModeOn)
            {
                Log.Message("CrowedSize revealed!!!");
                StringBuilder sb = new StringBuilder("pawn number:");
                sb.Append(pawnList.Count + " ");
                foreach (var x in pawnList)
                {
                    sb.Append("\n");
                    sb.Append(x.Name);
                }
                Log.Message(sb.ToString());
            }
        }

        public override bool ManualDeterminParams()
        {
            pawnList = IncidentInterceptorUtility.GenerateRaidPawns(parms);
            ResolveLookTargets();
            return true;
        }

        protected virtual void ResolveLookTargets()
        {
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, parms.target as Map, 8, null);
            lookTargets = new TargetInfo(loc, parms.target as Map, false);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref raidStrategy_revealed, "raidStrategy_revealed", false, false);
        }

        public override void ExecuteNow()
        {
            IncidentInterceptorUtility.IsIntercepting_IncidentExcecution = false;
            IncidentInterceptorUtility.IsIntercepting_PawnGeneration = PawnPatchType.ReturnTempList;
            IncidentInterceptorUtility.tmpPawnList = this.pawnList;

            if (this.incidentDef != null && this.parms != null)
                this.incidentDef.Worker.TryExecute(this.parms);
            else
                Log.Error("No IncidentDef or parms in InterceptedIncident!");

            IncidentInterceptorUtility.tmpPawnList = null;
            IncidentInterceptorUtility.IsIntercepting_IncidentExcecution = true;
        }

        public override void RevealRandomInformation()
        {
            List<Action> availables = new List<Action>();
            if (!intention_revealed)
                availables.Add(RevealIntention);
            if (!faction_revealed)
                availables.Add(RevealFaction);
            if (!crowdSize_revealed)
                availables.Add(RevealCrowdSize);
            if (!spawnPosition_revealed)
                availables.Add(RevealSpawnPosition);
            if (!raidStrategy_revealed && intention_revealed)
                availables.Add(RevealStrategy);
            if (availables.Count != 0)
            {
                Action OneToReveal = availables.RandomElement<Action>();
                OneToReveal();
            }
        }

        public override void RevealAllInformation()
        {
            base.RevealAllInformation();
            if (!raidStrategy_revealed)
                RevealStrategy();
        }

        public override void RevealInformationWhenCommunicationEstablished()
        {
            RevealFaction();
            RevealIntention();
        }

    }
}