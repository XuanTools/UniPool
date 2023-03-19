using UnityEngine;
using TMPro;

public class FPSText : MonoBehaviour
{
	public TMP_Text fpsText;
	public float updateTime = 0.5f;

	private float lastTime;
	private float deltaFrame;

	private void Start()
	{
		lastTime = Time.time;
		deltaFrame = 0;
	}

	private void Update()
	{
		deltaFrame ++;
		if (Time.time - lastTime >= updateTime) 
		{
			fpsText.text = $"fps: {deltaFrame / updateTime}";
			lastTime = Time.time;
			deltaFrame = 0;
		}
	}
}
