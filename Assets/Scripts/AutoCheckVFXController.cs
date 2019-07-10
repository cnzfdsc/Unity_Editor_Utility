using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCheckVFXController : MonoBehaviour
{
	public bool IsLoop;
	/// <summary>
	/// 与Unity的粒子发射器的Duration保持一致. 表示特效的播放时长. 超过这个时间后, 所有的发射器不再继续发射, 其他特效脚本也会停止播放
	/// </summary>
	public float Duration;
	/// <summary>
	/// 开始播放后多久直接销毁特效
	/// </summary>
	public float DestroyDelayAfterPlay;

	public List<ParticleSystem> PSList = new List<ParticleSystem>();

    void Start()
    {
		IEnumerator autoStop = AutoStop();
		StartCoroutine(autoStop);

		StartCoroutine("AutoDestroy");
	}

	IEnumerator AutoStop()
	{
		yield return new WaitForSeconds(Duration);
		StopFX();
	}

	private void StopFX()
	{
		for (int iPS = 0; iPS < PSList.Count; iPS++)
		{
			PSList[iPS].Stop();
		}
	}

	IEnumerator AutoDestroy()
	{
		yield return new WaitForSeconds(DestroyDelayAfterPlay);
		Destroy(gameObject);
	}
}
