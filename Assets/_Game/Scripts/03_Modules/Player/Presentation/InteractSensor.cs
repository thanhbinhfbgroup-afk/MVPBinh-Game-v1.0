using System;
using System.Collections.Generic;
using BillGameCore.Core.Interaction;
using UnityEngine;

namespace BillGameCore.Modules.Player.Presentation
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class InteractSensor : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetLayers = Physics2D.AllLayers;
        private readonly List<Collider2D> _overlaps = new();

        private void Awake()
        {
            var triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider == null)
            {
                throw new MissingComponentException(
                    $"{nameof(InteractSensor)} on '{gameObject.name}' requires a {nameof(Collider2D)} component.");
            }

            if (!triggerCollider.isTrigger)
            {
                throw new InvalidOperationException(
                    $"{nameof(InteractSensor)} on '{gameObject.name}' requires its {nameof(Collider2D)} to be a trigger.");
            }
        }

        public bool TryGetNearestTarget(out Collider2D targetCollider, out IInteractable interactable)
        {
            targetCollider = null;
            interactable = null;

            var nearestDistanceSquared = float.MaxValue;
            var sensorPosition = (Vector2)transform.position;

            for (var i = _overlaps.Count - 1; i >= 0; i--)
            {
                var overlap = _overlaps[i];
                if (overlap == null)
                {
                    _overlaps.RemoveAt(i);
                    continue;
                }

                var currentInteractable = overlap.GetComponent<IInteractable>();
                if (currentInteractable == null)
                {
                    continue;
                }

                var targetPosition = overlap.ClosestPoint(sensorPosition);
                var distanceSquared = (targetPosition - sensorPosition).sqrMagnitude;
                if (distanceSquared >= nearestDistanceSquared)
                {
                    continue;
                }

                nearestDistanceSquared = distanceSquared;
                targetCollider = overlap;
                interactable = currentInteractable;
            }

            return targetCollider != null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null)
            {
                return;
            }
            if (!IsInTargetLayers(other.gameObject.layer))
            {
                return;
            }
            if (_overlaps.Contains(other))
            {
                return;
            }

            _overlaps.Add(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other == null)
            {
                return;
            }

            _overlaps.Remove(other);
        }
        private bool IsInTargetLayers(int layer)
        {
            return (_targetLayers.value & (1 << layer)) != 0;
        }
        private void OnDrawGizmos()
        {
            if (!TryGetWorldCircle(out var center, out var radius))
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, radius);
        }

        private bool TryGetWorldCircle(out Vector3 center, out float radius)
        {
            center = default;
            radius = 0f;

            var circleCollider = GetComponent<CircleCollider2D>();
            if (circleCollider == null)
            {
                return false;
            }

            var maxScale = Mathf.Max(
                Mathf.Abs(transform.lossyScale.x),
                Mathf.Abs(transform.lossyScale.y));

            center = transform.TransformPoint(circleCollider.offset);
            radius = circleCollider.radius * maxScale;
            return true;
        }
    }
}