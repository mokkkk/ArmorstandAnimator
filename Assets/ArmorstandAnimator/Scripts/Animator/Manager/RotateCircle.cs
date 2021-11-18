using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArmorstandAnimator
{
    public enum Axis
    {
        X, Y, Z
    }
    public class RotateCircle : MonoBehaviour
    {
        public Axis axis;
        public int segments;
        public float xradius;
        public float yradius;
        LineRenderer line;

        private float _colliderThickness = 0.1f;

        void Start()
        {
            line = gameObject.GetComponent<LineRenderer>();

            line.positionCount = (segments + 1);
            line.useWorldSpace = false;
            CreatePoints();
        }


        void CreatePoints()
        {
            float p1, p2, p3;
            p1 = p2 = p3 = 0.0f;

            float angle = 20f;

            var pointList = new List<Vector2>();
            var setPointList = new List<Vector2>();

            for (int i = 0; i < (segments + 1); i++)
            {
                p1 = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
                p2 = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;
                var pos = Vector3.zero;

                if (axis == Axis.X)
                    pos = new Vector3(p3, p1, p2);
                else if (axis == Axis.Y)
                    pos = new Vector3(p1, p3, p2);
                else if (axis == Axis.Z)
                    pos = new Vector3(p1, p2, p3);

                line.SetPosition(i, pos);

                var col = this.gameObject.AddComponent<SphereCollider>();
                col.center = pos;
                col.radius = 0.1f;

                angle += (360f / segments);
            }
        }
    }
}