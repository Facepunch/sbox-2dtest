using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    [GameResource("Sprite Animation Definition", "frames", "A 2D animation from a sprite atlas", Icon = "movie")]
    public partial class SpriteAnimation : GameResource
    {
        public float FrameRate { get; set; } = 30f;
        
        public List<SpriteSequence> Sequences { get; set; }

        public (Vector2 Min, Vector2 Max) GetFrameUvs(float time, int rows, int cols)
        {
            var totalDuration = 0f;

            foreach (var sequence in Sequences)
            {
                var frameTime = 1f / (FrameRate * sequence.PlaybackRate);
                totalDuration += sequence.Repetitions * frameTime * sequence.TotalFrameCount;
            }

            time -= MathF.Floor(time / totalDuration) * totalDuration;

            foreach (var sequence in Sequences)
            {
                var frameTime = 1f / (FrameRate * sequence.PlaybackRate);
                var sequenceDuration = sequence.Repetitions * frameTime * sequence.TotalFrameCount;

                if (time >= sequenceDuration)
                {
                    time -= sequenceDuration;
                    continue;
                }

                sequenceDuration = frameTime * sequence.TotalFrameCount;

                time -= MathF.Floor(time / sequenceDuration) * sequenceDuration;

                var frame = (time * sequence.TotalFrameCount / sequenceDuration).FloorToInt();

                return sequence.GetFrameUvs(frame, rows, cols);
            }

            return (default, default);
        }
    }

    public class SpriteSequence
    {
        public float PlaybackRate { get; set; } = 1f;
        public int Repetitions { get; set; } = 1;

        public int FirstFrameIndex { get; set; }
        public int LastFrameIndex { get; set; }

        [HideInEditor]
        public int TotalFrameCount => Math.Abs(LastFrameIndex - FirstFrameIndex) + 1;

        public bool FlipX { get; set; }
        public bool FlipY { get; set; }

        public (Vector2 Min, Vector2 Max) GetFrameUvs(int frame, int rows, int cols)
        {
            var frameIndex = FirstFrameIndex
                + Math.Clamp(frame, 0, TotalFrameCount - 1)
                * Math.Sign(LastFrameIndex - FirstFrameIndex);

            var row = (frameIndex / cols) % rows;
            var col = frameIndex % cols;
            
            var min = new Vector2((col + 0f) / cols, (row + 0f) / rows);
            var max = new Vector2((col + 1f) / cols, (row + 1f) / rows);

            if (FlipX) (min.x, max.x) = (max.x, min.x);
            if (FlipY) (min.y, max.y) = (max.y, min.y);

            return (min, max);
        }
    }
}
