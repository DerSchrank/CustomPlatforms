using BS_Utils.Utilities;
using UnityEngine;

public class MeshBloomPrePassLightWithId : TubeBloomPrePassLightWithId
{
	public override void ColorWasSet(Color color)
	{
		MeshBloomPrePassLight _tubeBloomPrePassLight = (MeshBloomPrePassLight) ReflectionUtil.GetPrivateField<TubeBloomPrePassLight>(this, "_tubeBloomPrePassLight");
		_tubeBloomPrePassLight.UpdateColor(color);
	}
}