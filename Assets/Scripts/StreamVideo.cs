using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StreamVideo : MonoBehaviour
{
  public RawImage rawImage;
  public VideoPlayer videoPlayer;

  void Start()
  {
    StartCoroutine(PlayVideo());
  }

  public IEnumerator PlayVideo()
  {
    videoPlayer.Prepare();
    WaitForSeconds waitForSeconds = new WaitForSeconds(1);

    while (!videoPlayer.isPrepared)
    {
      yield return waitForSeconds;
      break;
    }

    rawImage.texture = videoPlayer.texture;
    videoPlayer.Play();
  }

  public void Stream()
  {
    videoPlayer.Prepare();

    rawImage.texture = videoPlayer.texture;
    videoPlayer.Play();
  }
}