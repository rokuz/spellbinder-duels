using UnityEngine;
using System.Collections;

public class SfxVolume : MonoBehaviour
{
  private AudioSource audioSource;
  private float initialVolume = 1.0f;
  private float lastVolume = -1.0f;

  void Start()
  {
    this.audioSource = this.gameObject.GetComponent<AudioSource>();
    if (this.audioSource != null)
      this.initialVolume = this.audioSource.volume;
  }

  void Update()
  {
    if (this.audioSource == null)
      return;
    
    float currentVolume = Persistence.gameConfig.sfxVolume;
    if (Mathf.Abs(currentVolume - lastVolume) > 1e-5)
    {
      this.audioSource.volume = initialVolume * currentVolume;
      lastVolume = currentVolume;
    }
  }
}
