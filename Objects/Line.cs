using System.Linq;
using UnityEngine;
using Settings = ConnieLocal.ConnieLocal.Settings;

namespace ConnieLocal.Objects
{
    internal class Line : MonoBehaviour
    {
        internal static Line i;
        LineRenderer line;
        GhostSave ghost;

        void Awake()
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.material = ConnieLocal.bundle.LoadAsset<Material>("Assets/Materials/LineMaterial.mat");
            line.material.renderQueue = 3000; // im too lazy to actually edit the material
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.shadowBias = 0;
            line.numCapVertices = 3;
            line.numCornerVertices = 6;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.DistributePerSegment;

            i = this;
            gameObject.SetActive(false);
            Configure(true);
        }

        internal void Configure(bool mat = false)
        {
            line.widthMultiplier = Settings.lineWidth.Value;
            var g = new Gradient();
            g.SetKeys(
            [
                new GradientColorKey(Settings.lineColor.Value, 0f),
                new GradientColorKey(Settings.lineColor.Value, 1f),
            ], line.colorGradient.alphaKeys);
            line.colorGradient = g;

            if (mat)
            {
                var mate = line.material;
                mate.SetColor("_BorderColor", Settings.borderColor.Value);
                mate.SetFloat("_BorderStrong", Settings.borderStrong.Value);
                mate.SetFloat("_MinDist", Settings.minDist.Value);
                mate.SetFloat("_MaxDist", Settings.maxDist.Value);
                mate.SetFloat("_FadeStrong", Settings.fadeStrong.Value);
            }
        }

        static readonly int _TimeComp = Shader.PropertyToID("_TimeComp");
        internal void SetTime(float time) => line.material.SetFloat(_TimeComp, time);


        internal void SetGhost(GhostSave ghost, bool force = false)
        {
            if (!force)
            {
                if (ghost == null || this.ghost == ghost)
                    return;
            }
            else ghost ??= this.ghost;

            this.ghost = ghost;
            line.positionCount = ghost.frames.Count();
            line.SetPositions(ghost.frames.Select(f => f.pos + new Vector3(0, Settings.lineBudge.Value, 0)).ToArray());
            line.material.SetFloat("_MaxTime", ghost.totalTime);

            gameObject.SetActive(true);
        }
    }
}
