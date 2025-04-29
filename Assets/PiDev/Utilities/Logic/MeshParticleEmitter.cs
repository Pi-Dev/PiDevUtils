using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * 
 * The MIT License (MIT)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * ============= Description =============
 * Emits particles from mesh and skinned mesh vertex positions using a given ParticleSystem.
 * Supports dynamic velocity calculation based on mesh vertex distance and optional mesh exclusion.
 * Useful for effects like mesh-based bursts, trails, or vertex-driven particle systems.
 *
 * ============= Usage =============
 * MeshParticleEmitter.EmitFromMeshes(particleSystem, transform);
 * MeshParticleEmitter.EmitFromMeshVertices(particleSystem, transform, velocityMultiplier, ignoreList);
 */

namespace PiDev.Utilities
{
    public class MeshParticleEmitter : MonoBehaviour
    {
        public float strength;
        public void Emit() => EmitFromMeshes(GetComponent<ParticleSystem>(), transform);

        public static void EmitFromMeshes(ParticleSystem particleSystem, Transform rootTransform)
        {
            // Cache the shape module for optimization
            var shapeModule = particleSystem.shape;

            // Iterate over all MeshRenderers
            foreach (var meshRenderer in rootTransform.GetComponentsInChildren<MeshRenderer>())
            {
                if (meshRenderer == null) continue;

                var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null) continue;

                int vertexCount = meshFilter.sharedMesh.vertexCount; // Get the vertex count

                // Set the shape module to use the MeshRenderer
                shapeModule.shapeType = ParticleSystemShapeType.MeshRenderer;
                shapeModule.meshRenderer = meshRenderer;

                // Emit particles based on the vertex count
                particleSystem.Emit(vertexCount);
            }

            // Iterate over all SkinnedMeshRenderers
            foreach (var skinnedMeshRenderer in rootTransform.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null) continue;

                int vertexCount = skinnedMeshRenderer.sharedMesh.vertexCount; // Get the vertex count

                // Set the shape module to use the SkinnedMeshRenderer
                shapeModule.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                shapeModule.skinnedMeshRenderer = skinnedMeshRenderer;

                // Emit particles based on the vertex count
                particleSystem.Emit(vertexCount);
            }
        }


        public static void EmitFromMeshVertices(ParticleSystem particleSystem, Transform rootTransform, float velocityMultiplier = 1f, IEnumerable<string> ignoreList = default)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
            int particleCount = particleSystem.GetParticles(particles);

            foreach (var meshFilter in rootTransform.GetComponentsInChildren<MeshFilter>())
            {
                if (ignoreList != null && ignoreList.Contains(meshFilter.name)) continue;
                if (meshFilter.sharedMesh == null) continue;
                Vector3[] vertices = meshFilter.sharedMesh.vertices;
                //Vector3[] normals = meshFilter.sharedMesh.normals;
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (particleCount >= particles.Length) break;

                    Vector3 localVertex = meshFilter.transform.TransformPoint(vertices[i]);
                    Vector3 directionFromRoot = (localVertex - rootTransform.position);
                    Vector3 worldNormal = directionFromRoot * velocityMultiplier; // Scale velocity based on distance from root
                    Vector3 worldPosition = meshFilter.transform.TransformPoint(vertices[i]);

                    particles[particleCount].position = worldPosition;
                    particles[particleCount].velocity = (worldPosition - rootTransform.position) * velocityMultiplier; // worldNormal;
                    particles[particleCount].startLifetime = particleSystem.main.startLifetime.constant;
                    particles[particleCount].startSize = particleSystem.main.startSize.constant;
                    particles[particleCount].startColor = particleSystem.main.startColor.color;
                    particles[particleCount].remainingLifetime = particles[particleCount].startLifetime;
                    particleCount++;
                }
            }

            foreach (var skinnedMeshRenderer in rootTransform.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (ignoreList != null && ignoreList.Contains(skinnedMeshRenderer.name)) continue;
                if (skinnedMeshRenderer.sharedMesh == null) continue;

                Mesh bakedMesh = new Mesh();
                skinnedMeshRenderer.BakeMesh(bakedMesh);

                Vector3[] vertices = bakedMesh.vertices;
                //Vector3[] normals = bakedMesh.normals;

                for (int i = 0; i < vertices.Length; i++)
                {
                    if (particleCount >= particles.Length) break;

                    Vector3 worldPosition = skinnedMeshRenderer.transform.TransformPoint(vertices[i]);

                    particles[particleCount].position = worldPosition;
                    particles[particleCount].velocity = (worldPosition - rootTransform.position) * velocityMultiplier; // worldNormal;
                    particles[particleCount].startLifetime = particleSystem.main.startLifetime.constant;
                    particles[particleCount].startSize = particleSystem.main.startSize.constant;
                    particles[particleCount].startColor = particleSystem.main.startColor.color;
                    particles[particleCount].remainingLifetime = particles[particleCount].startLifetime;

                    particleCount++;
                }
            }
            particleSystem.SetParticles(particles, particleCount);
        }
    }
}