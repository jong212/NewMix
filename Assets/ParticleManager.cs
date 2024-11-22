using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] ParticleSystem _selector;
    public ParticleSystem Selector { get => _selector; set => _selector = value; }
    private void Awake()
    {
        Selector.Stop();
    }
}
