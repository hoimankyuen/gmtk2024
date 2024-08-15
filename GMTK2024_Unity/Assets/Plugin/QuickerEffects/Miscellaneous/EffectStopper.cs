using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickerEffects
{
	public class EffectStopper : MonoBehaviour
	{
		public enum ApplyTarget
		{
			SelfOnly,
			SelfAndChildren,
		}

		public bool stopOutlineEffect = true;
		public bool stopOverlayEffect = true;
		public bool stopBlinkerEffect = true;

		public ApplyTarget applyTarget = ApplyTarget.SelfAndChildren;
	}
}
