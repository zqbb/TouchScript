﻿/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TouchScript.Hit;
using UnityEngine;

namespace TouchScript.Layers
{
    /// <summary>
    /// Touch layer which represents a 3d camera looking into the world. Determines which objects may be hit in the view of a camera attached to parent GameObject.
    /// </summary>
    [AddComponentMenu("TouchScript/Layers/Camera Layer")]
    public sealed class CameraLayer : CameraLayerBase
    {
        #region Private variables

        private List<RaycastHit> sortedHits;

        #endregion

        #region Unity methods

        private void OnEnable()
        {
            sortedHits = new List<RaycastHit>();
        }

        #endregion

        #region Protected functions

        /// <inheritdoc />
        protected override LayerHitResult castRay(Ray ray, out ITouchHit hit)
        {
            hit = null;
            var hits = Physics.RaycastAll(ray, float.PositiveInfinity, LayerMask);

            if (hits.Length == 0) return LayerHitResult.Miss;
            if (hits.Length > 1) hits = sortHits(hits);

            var success = false;
            foreach (var raycastHit in hits)
            {
                hit = TouchHitFactory.Instance.GetTouchHit(raycastHit);
                var hitTests = raycastHit.transform.GetComponents<HitTest>();
                if (hitTests.Length == 0)
                {
                    success = true;
                    break;
                }

                var hitResult = HitTest.ObjectHitResult.Hit;
                foreach (var test in hitTests)
                {
                    if (!test.enabled) continue;
                    hitResult = test.IsHit(hit);
                    if (hitResult == HitTest.ObjectHitResult.Miss || hitResult == HitTest.ObjectHitResult.Discard) break;
                }

                if (hitResult == HitTest.ObjectHitResult.Hit)
                {
                    success = true;
                    break;
                }
                if (hitResult == HitTest.ObjectHitResult.Discard) break;
            }

            if (success) return LayerHitResult.Hit;

            return LayerHitResult.Miss;
        }

        private RaycastHit[] sortHits(RaycastHit[] hits)
        {
            var cameraPos = camera.transform.position;
            sortedHits.Clear();
            sortedHits.AddRange(hits);
            sortedHits.Sort((a, b) =>
            {
                if (a.collider.transform == b.collider.transform) return 0;
                var distA = (a.point - cameraPos).sqrMagnitude;
                var distB = (b.point - cameraPos).sqrMagnitude;
                return distA < distB ? -1 : 1;
            });

            return sortedHits.ToArray();
        }

        #endregion
    }
}