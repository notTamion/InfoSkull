extern alias unityengineold;
using InfoSkull.core.components;
using UnityEngine;

namespace InfoSkull.core;

public abstract class ElementHandler : MonoBehaviour {
	public abstract void init(ElementController controller);
	public virtual void openAdjustUI() { }
	public virtual void closeAdjustUI() { }
}