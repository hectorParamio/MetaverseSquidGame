using System;
using UnityEngine;

namespace UnityStandardAssets.Effects
{
    public class ParticleSystemMultiplier : MonoBehaviour
    {
        // a simple script to scale the size, speed and lifetime of a particle system

        public float multiplier = 1;


        private void Start()
        {
            var systems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem system in systems)
            {
                var main = system.main;
                main.startLifetimeMultiplier = main.startLifetimeMultiplier * multiplier;
                main.startSpeedMultiplier = main.startSpeedMultiplier * multiplier;
                main.startSizeMultiplier = main.startSizeMultiplier * multiplier;
                system.Clear();
                system.Play();
            }
        }
    }
}
