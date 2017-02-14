using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futulabs
{
	
public class SettingsManager : Singleton<SettingsManager>
{
	[Header("Instructions")]
	public float InstructionChangeInterval;
	public List<string> InstructionTexts = null;

	[Header("Wall Cube Outline Colors")]
	public Color WallCubeOutlineOffEmissionColor;
	public Color WallCubeOutlineOnEmissionColor;

	[Header("Interactable Material Outline Colors")]
	public Color InteractableMaterialMinDiffuseColor;
	public Color InteractableMaterialMaxDiffuseColor;
	public Color InteractableMaterialMinEmissionColor;
	public Color InteractableMaterialMaxEmissionColor;
	public float InteractableMaterialMinEmissionGain = 0f;
	public float InteractableMaterialMaxEmissionGain = 0.4f;

	public float InteractableMaterialOutlineTransitionFactor = 3f;
	public float InteractableMaterialOutlineMinGlowTime = 0.5f;
	public float InteractableMaterialOutlineMaxGlowTime = 2f;
}

}