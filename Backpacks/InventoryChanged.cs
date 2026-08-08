using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Backpacks;

public static class InventoryChanged
{
	private static Inventory activeBackpack;

	public static void BackpackInventoryChanged(Player player, Inventory backpack)
	{
		activeBackpack = backpack;
		player.backpackInventoryChanged();
	}
	
    private static void backpackInventoryChanged(this Player player)
    {
    	throw new NotImplementedException("Was not patched ...");
    }

    [HarmonyPatch(typeof(InventoryChanged), nameof(backpackInventoryChanged))]
    private static class LoadPlayerChanged
    {
    	// replace this.m_inventory by activeBackpack 
    	private static IEnumerable<CodeInstruction> Transpiler(ILGenerator ilGenerator)
    	{
		    FieldInfo playerInventory = AccessTools.DeclaredField(typeof(Humanoid), nameof(Humanoid.m_inventory));
    		List<CodeInstruction> instructions = PatchProcessor.GetCurrentInstructions(AccessTools.DeclaredMethod(typeof(Player), nameof(Player.OnInventoryChanged)), generator: ilGenerator);
		    for (int i = 0; i < instructions.Count - 1; ++i)
		    {
			    if (instructions[i].opcode == OpCodes.Ldarg_0 && instructions[i + 1].LoadsField(playerInventory))
			    {
				    instructions[i].opcode = OpCodes.Nop;
				    instructions[i + 1] = new CodeInstruction(OpCodes.Ldsfld, AccessTools.DeclaredField(typeof(InventoryChanged), nameof(activeBackpack)));
			    }
		    }

		    return instructions;
	    }
    }
}