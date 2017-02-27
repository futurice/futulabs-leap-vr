using UnityEngine;
using System.Collections;

public class CategorySelector : MonoBehaviour, KinectGestures.GestureListenerInterface
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("Whether to use swipe-left and swipe-right gestures to change the clothing model.")]
	public bool swipeToChangeModel = true;

	[Tooltip("Whether to use left and right hand-raise gestures to change the model category.")]
	public bool raiseHandToChangeCategory = true;

	[Tooltip("Whether to put the clothing model hip and shoulder joints where the user joints are.")]
	public bool fixModelHipsAndShoulders = false;

	[Tooltip("GUI-Text used to display information messages.")]
	public GUIText infoText;


	// available model selectors
	private ModelSelector[] allModelSelectors;
	private int iCurSelector = -1;

	// current model selector
	private ModelSelector modelSelector;

	// last detected userId;
	private long lastUserId = 0;


	/// <summary>
	/// Gets the active model selector.
	/// </summary>
	/// <returns>The active model selector.</returns>
	public ModelSelector GetActiveModelSelector()
	{
		return modelSelector;
	}


	/// <summary>
	/// Activate the next model selector.
	/// </summary>
	public void ActivateNextModelSelector()
	{
		if (allModelSelectors.Length > 0) 
		{
			if (modelSelector)
				modelSelector.SetActiveSelector(false);

			iCurSelector++;
			if (iCurSelector >= allModelSelectors.Length)
				iCurSelector = 0;

			modelSelector = allModelSelectors [iCurSelector];
			modelSelector.SetActiveSelector(true);

			Debug.Log("Category: " + modelSelector.modelCategory);
		}
	}


	/// <summary>
	/// Activates the previous model selector.
	/// </summary>
	public void ActivatePrevModelSelector()
	{
		if (allModelSelectors.Length > 0) 
		{
			if (modelSelector)
				modelSelector.SetActiveSelector(false);

			iCurSelector--;
			if (iCurSelector < 0)
				iCurSelector = allModelSelectors.Length - 1;

			modelSelector = allModelSelectors [iCurSelector];
			modelSelector.SetActiveSelector(true);

			Debug.Log("Category: " + modelSelector.modelCategory);
		}
	}


	/////////////////////////////////////////////////////////////////////////////////


	void Start () 
	{
		ModelSelector[] modelSelectors = GetComponents<ModelSelector>();

		int countEnabled = 0;
		if (modelSelectors != null) 
		{
			foreach (ModelSelector modelSel in modelSelectors) 
			{
				if (modelSel.enabled && modelSel.playerIndex == playerIndex)
					countEnabled++;
			}
		}

		allModelSelectors = new ModelSelector[countEnabled];
		if (countEnabled > 0) 
		{
			for (int i = 0, j = 0; i < modelSelectors.Length; i++) 
			{
				if (modelSelectors[i].enabled && modelSelectors[i].playerIndex == playerIndex) 
				{
					allModelSelectors[j] = modelSelectors[i];
					modelSelectors[i].SetActiveSelector (false);

					j++;
				}
			}
		}

		if (allModelSelectors.Length > 0) 
		{
			iCurSelector = 0;

			modelSelector = allModelSelectors[iCurSelector];
			modelSelector.SetActiveSelector(true);

			Debug.Log("Category: " + modelSelector.modelCategory);
		}

		KinectManager manager = KinectManager.Instance;

		if (manager && manager.IsInitialized ()) 
		{
			if(infoText != null)
			{
				infoText.text = "Please stand in T-pose for calibration.";
			}
		} 
		else 
		{
			string sMessage = "KinectManager is missing or not initialized";
			Debug.LogError(sMessage);

			if(infoText != null)
			{
				infoText.text = sMessage;
			}
		}
	}


	void Update()
	{
		KinectManager manager = KinectManager.Instance;

		if(manager && manager.IsInitialized ()) 
		{
			long userId = manager.GetUserIdByIndex(playerIndex);

			if (userId != 0) 
			{
				MonoBehaviour[] monoScripts = FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];
				foreach(MonoBehaviour monoScript in monoScripts)
				{
//					if(typeof(AvatarScaler).IsAssignableFrom(monoScript.GetType()) &&
//						monoScript.enabled)
					if((monoScript is AvatarScaler) && monoScript.enabled)
					{
						AvatarScaler scaler = (AvatarScaler)monoScript;

						if(scaler.scalerInited && scaler.playerIndex == playerIndex && 
							scaler.currentUserId != userId)
						{
							scaler.currentUserId = userId;

							if(userId != 0)
							{
								scaler.GetUserBodySize(true, true, true);

								if(fixModelHipsAndShoulders)
									scaler.FixJointsBeforeScale();
								scaler.ScaleAvatar(0f);
							}
						}
					}
				}

				if (lastUserId != userId) 
				{
					if(infoText != null)
					{
						string sMessage = swipeToChangeModel && modelSelector ? "Swipe left or right to change clothing." : string.Empty;
						if (raiseHandToChangeCategory && allModelSelectors.Length > 1)
							sMessage += " Raise hand to change category.";
						
						infoText.text = sMessage;
					}

					lastUserId = userId;
				}
			}

			if(userId == 0 && userId != lastUserId)
			{
				lastUserId = userId;

				// destroy currently loaded models
				foreach (ModelSelector modSelector in allModelSelectors) 
				{
					modSelector.DestroySelectedModel();
				}

				if(infoText != null)
				{
					infoText.text = "Please stand in T-pose for calibration.";
				}
			}
		}
	}
	

	public void UserDetected(long userId, int userIndex)
	{
		KinectManager manager = KinectManager.Instance;
		if(!manager || (userIndex != playerIndex))
			return;

		if (raiseHandToChangeCategory) 
		{
			manager.DetectGesture(userId, KinectGestures.Gestures.RaiseRightHand);
			manager.DetectGesture(userId, KinectGestures.Gestures.RaiseLeftHand);
		}

		if (swipeToChangeModel) 
		{
			manager.DetectGesture(userId, KinectGestures.Gestures.SwipeLeft);
			manager.DetectGesture(userId, KinectGestures.Gestures.SwipeRight);
		}
	}

	public void UserLost(long userId, int userIndex)
	{
		if(userIndex != playerIndex)
			return;
	}

	public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos)
	{
		// nothing to do here
	}

	public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint, Vector3 screenPos)
	{
		if(userIndex != playerIndex)
			return false;

		switch (gesture)
		{
		case KinectGestures.Gestures.RaiseRightHand:
			ActivateNextModelSelector();
			break;
		case KinectGestures.Gestures.RaiseLeftHand:
			ActivatePrevModelSelector();
			break;
		case KinectGestures.Gestures.SwipeLeft:
			if (modelSelector) 
			{
				modelSelector.SelectNextModel();
			}
			break;
		case KinectGestures.Gestures.SwipeRight:
			if (modelSelector) 
			{
				modelSelector.SelectPrevModel();
			}
			break;
		}

		return true;
	}

	public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint)
	{
		if(userIndex != playerIndex)
			return false;

		return true;
	}
}
