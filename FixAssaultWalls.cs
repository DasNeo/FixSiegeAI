﻿using HarmonyLib;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace FixSiegeAI
{
	// Get units to attack inner gate when outer gate's down
	[HarmonyPatch(typeof(CastleGate), "OnDestroyed")]
	public static class Patch_OnDestroyed
	{
		public static void Postfix(CastleGate __instance)
		{
			TeamAISiegeComponent aic = Mission.Current.AttackerTeam.TeamAI as TeamAISiegeComponent;
			if (__instance.GameEntity.HasTag("inner_gate"))
			{
				Main.Log("Inner gate destroyed!", true);
				foreach (Formation f in Mission.Current.PlayerTeam.FormationsIncludingSpecial)
				{
					if (Main.IsPIC(f) && !f.IsAIControlled)
					{
						Traverse.Create(f).Method("DisbandAttackEntityDetachment").GetValue();
					}
				}
			}
			if (__instance.GameEntity.HasTag("outer_gate"))
			{
				Main.Log("Outer gate destroyed!", true);
				foreach (MissionObject mo in Mission.Current.ActiveMissionObjects)
				{
					foreach (Formation f in Mission.Current.PlayerTeam.Formations)
					{
						var bat = f.GetUsedMachines().FirstOrDefault(r => r is BatteringRam);
						if ((bat != null) && !aic.InnerGate.IsDestroyed && Main.IsPIC(f) && !f.IsAIControlled)
						{
							if (Mission.Current.PlayerTeam.IsAttacker) { Main.Log("Sending detachment to attack inner gate!", true); }
							Traverse.Create(f).Method("FormAttackEntityDetachment", aic.InnerGate.GameEntity).GetValue();
						}
					}
				}
			}
			if (Mission.Current.PlayerTeam.IsAttacker && aic.InnerGate.IsDestroyed && aic.OuterGate.IsDestroyed)
			{ Main.Log("Charge when ready!", true); }
		}
	}

	// Still trying to debug charging
	//[HarmonyPatch(typeof(OrderController), "GetChargeOrderSubstituteForSiege")]
	public static class Patch_GetChargeOrderSubstituteForSiege
	{
		public static bool Prefix(OrderController __instance, ref MovementOrder __result, Formation formation)
		{
			Main.Log("Charge command given.");
			__result = MovementOrder.MovementOrderCharge;
			return true;
		}
	}

}