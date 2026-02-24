using UnityEngine;

public class SmokeController : MonoBehaviour
{
    #region Parameters
   [SerializeField] private ParticleSystem _smokeParticleSystem;
    #endregion

    #region Unity Methods
    private void OnValidate()
    {
        if (_smokeParticleSystem == null)
        {
            Debug.LogError("Smoke Particle System reference is missing in SmokeController.");
        }
    }
    #endregion

    #region Class Methods
    public void PlaySmokeEffect()
    {
        if (_smokeParticleSystem != null)
        {
            _smokeParticleSystem.Play();
        }
    }
    #endregion
}
