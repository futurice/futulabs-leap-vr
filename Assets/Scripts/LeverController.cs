using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Futulabs
{

public delegate void LeverStateChangedEventHandler();

public class LeverController : MonoBehaviour
{
	private enum LeverEventType
	{
		NONE,
		ON_EVENT,
		OFF_EVENT
	}

	[SerializeField]
	private HingeJoint _hingeJoint;

	public event LeverStateChangedEventHandler OnLeverTurnedOn;
	public event LeverStateChangedEventHandler OnLeverTurnedOff;

	private LeverEventType _lastEventSent = LeverEventType.NONE;

	private void Update()
	{
		// On
		if (Mathf.Approximately(_hingeJoint.angle, _hingeJoint.limits.max))
		{
			if (_lastEventSent != LeverEventType.ON_EVENT)
			{
				if (OnLeverTurnedOn != null)
				{
					OnLeverTurnedOn.Invoke();
				}

				_lastEventSent = LeverEventType.ON_EVENT;
			}
		}
		// Off
		else if (Mathf.Approximately(_hingeJoint.angle, _hingeJoint.limits.min))
		{
			if (_lastEventSent != LeverEventType.OFF_EVENT)
			{
				if (OnLeverTurnedOff != null)
				{
					OnLeverTurnedOff.Invoke();
				}

				_lastEventSent = LeverEventType.OFF_EVENT;
			}
		}
	}
}

}
