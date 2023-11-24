/*
 * Author: Porter Squires
 */

using UnityEngine;
using System;

namespace BufferedInput
{
    #region Input Data Components
    public enum InputStage
    {
        HELD,
        RELEASED
    }

    public enum InputProcessStage
    {
        PENDING,
        PROCESSING,
        INTERRUPTED
    }

    public enum SnappedDirection
    {
        UP,
        UP_RIGHT,
        RIGHT,
        DOWN_RIGHT,
        DOWN,
        DOWN_LEFT,
        LEFT,
        UP_LEFT,
        NONE
    }

    [Serializable]
    public struct DirectedInput
    {
        private static DirectedInput zero = new DirectedInput(Vector2.zero);
        public static DirectedInput Zero { get { return zero; } }

        [SerializeField] public SnappedDirection snappedDirection;
        [SerializeField] public SnappedDirection cardinalDirection;
        [SerializeField] public Vector2 starting;
        [SerializeField] public Vector2 current;
        public DirectedInput(Vector2 starting) : this(starting, starting) { }
        public DirectedInput(Vector2 starting, Vector2 current)
        {
            this.snappedDirection = DirectionFromVector(starting);
            this.cardinalDirection = CardinalSnappedDirectionFromVector(starting);
            this.starting = starting;
            this.current = current;
        }

        public DirectedInput(SnappedDirection direction, Vector2 starting) : this(direction, starting, starting) { }
        public DirectedInput(SnappedDirection direction, Vector2 starting, Vector2 current)
        {
            this.snappedDirection = direction;
            this.cardinalDirection = CardinalDirectionFromDirection(direction);
            this.starting = starting;
            this.current = current;
        }

        public static SnappedDirection CardinalSnappedDirectionFromVector(Vector2 dir, float verticalThresholdDegrees=44f, float threshold=0.1f, bool debugMessage=false)
        {
            if (Utility.WithinThreshold(dir.x, threshold) && Utility.WithinThreshold(dir.y, threshold))
                return SnappedDirection.NONE;

            verticalThresholdDegrees = Mathf.Clamp(verticalThresholdDegrees, 0, 90);
            float verticalThreshold = Mathf.Deg2Rad * verticalThresholdDegrees;

            float rad = Mathf.Atan2(dir.y, dir.x);
            float upMax = Mathf.PI - verticalThreshold;
            float upMin = verticalThreshold;
            float downMin = -Mathf.PI + verticalThreshold;
            float downMax = -verticalThreshold;
            SnappedDirection cardinal;

            if (rad >= upMin && rad <= upMax)
                cardinal = SnappedDirection.UP;
            else if (rad >= downMin && rad <= downMax)
                cardinal = SnappedDirection.DOWN;
            else if (rad <= Mathf.PI / 2 && rad >= -Mathf.PI / 2)
                cardinal = SnappedDirection.RIGHT;
            else
                cardinal = SnappedDirection.LEFT;

            if (debugMessage)
                Debug.Log("Current vector: " + dir + ", radians: " + rad + ", Direction: " + cardinal + ", Threshold: " + verticalThresholdDegrees);

            return cardinal;
        }

        public static SnappedDirection DirectionFromVector(Vector2 dir, float threshold=0.1f)
        {
            if (Utility.WithinThreshold(dir.x, threshold))
            {
                // both x and y are about 0
                if (Utility.WithinThreshold(dir.y, threshold))
                    return SnappedDirection.NONE;

                // x is about 0, y is up or down
                else
                    return dir.y > 0 ? SnappedDirection.UP : SnappedDirection.DOWN;
            } else
            {
                // x is left or right, y is about 0
                if (Utility.WithinThreshold(dir.y, threshold))
                    return dir.x > 0 ? SnappedDirection.RIGHT : SnappedDirection.LEFT;

                // both x and y are not about 0
                else
                {
                    // x is left or right, y is up
                    if (dir.y > 0)
                        return dir.x > 0 ? SnappedDirection.UP_RIGHT : SnappedDirection.UP_LEFT;

                    // x is left or right, y is down
                    else
                        return dir.x > 0 ? SnappedDirection.DOWN_RIGHT : SnappedDirection.DOWN_LEFT;
                }
            }
        }

        public static Vector2 VectorFromDirection(SnappedDirection dir)
        {
            switch (dir)
            {
                case SnappedDirection.UP:
                    return Vector2.up;
                case SnappedDirection.UP_RIGHT:
                    return (Vector2.up + Vector2.right).normalized;
                case SnappedDirection.RIGHT:
                    return Vector2.right;
                case SnappedDirection.DOWN_RIGHT:
                    return (Vector2.down + Vector2.right).normalized;
                case SnappedDirection.DOWN:
                    return Vector2.down;
                case SnappedDirection.DOWN_LEFT:
                    return (Vector2.down + Vector2.left).normalized;
                case SnappedDirection.LEFT:
                    return Vector2.left;
                case SnappedDirection.UP_LEFT:
                    return (Vector2.down + Vector2.left).normalized;
                default:
                    return Vector2.zero;
            }
        }

        public static SnappedDirection CardinalDirectionFromDirection(SnappedDirection dir, bool preferUp=true)
        {
            if (preferUp)
            {
                if (dir == SnappedDirection.UP_LEFT || dir == SnappedDirection.UP_RIGHT)
                    return SnappedDirection.UP;
                else if (dir == SnappedDirection.DOWN_LEFT || dir == SnappedDirection.DOWN_RIGHT)
                    return SnappedDirection.DOWN;
            } else
            {
                if (dir == SnappedDirection.UP_LEFT || dir == SnappedDirection.DOWN_LEFT)
                    return SnappedDirection.LEFT;
                else if (dir == SnappedDirection.UP_RIGHT || dir == SnappedDirection.DOWN_RIGHT)
                    return SnappedDirection.RIGHT;
            }
            return dir;
        }
    }
    #endregion

    public class InputPair
    {
        public InputData input;
        public int duration;
        public InputPair(InputData input, int duration=0)
        {
            this.input = input;
            this.duration = duration;
        }
    }

    public class InputData
    {
        public static TimeType READ_TIME_TYPE = TimeType.FIXED;
        public enum TimeType
        {
            FIXED,
            UPDATE
        }
        public static float Time { get { return READ_TIME_TYPE == TimeType.FIXED ? UnityEngine.Time.fixedTime : UnityEngine.Time.time; } }
        public static float DeltaTime { get { return READ_TIME_TYPE == TimeType.FIXED ? UnityEngine.Time.fixedDeltaTime : UnityEngine.Time.deltaTime; } }

        #region Variables
        [SerializeField] private ActionMask mask;
        public ActionMask Mask { get { return mask; } private set { mask = value; } }
        [SerializeField] private InputStage phase;
        public InputStage Phase { get { return phase; } set { phase = value; } }
        [SerializeField] public InputProcessStage processStage;
        public InputProcessStage ProcessStage { get { return processStage; } set { processStage = value; } }

        [SerializeField] private float inputTime;
        public float InputTime { get { return inputTime; } set { inputTime = value; } }
        [SerializeField] private float previousIncrement;
        [SerializeField] private float duration;
        public float Duration { get { return duration; } set { duration = value; } }
        [SerializeField] private float heldTime;
        public float HeldTime { get { return Phase == InputStage.HELD ? Time - inputTime : heldTime; } set { heldTime = value; } }

        [SerializeField] private DirectedInput direction;
        public DirectedInput Direction { get { return direction; } set { direction = value; } }

        [SerializeField] private InputValueTypes valueType;
        public InputValueTypes ValueType { get { return valueType; } set { valueType = value; } }
        [SerializeField] private bool interruptible;
        public bool Interruptible { get { return interruptible; } private set { interruptible = value; } }
        #endregion

        #region Constructors
        public InputData(ActionMask mask, InputValueTypes valueType, bool interruptible, InputStage phase) : this(mask, valueType, interruptible, phase, Vector2.zero) { }
        public InputData(ActionMask mask, InputValueTypes valueType, bool interruptible, InputStage phase, Vector2 direction)
        {
            Mask = mask;
            ValueType = valueType;
            Phase = phase;
            Direction = new DirectedInput(direction);
            InputTime = Time;
            Interruptible = interruptible;
        }
        #endregion

        public static bool IsControl(ActionMask mask)
        {
            return mask.Mask > 0;
        }

        public static bool IsButton(InputValueTypes valueType)
        {
            return valueType == InputValueTypes.BUTTON;
        }

        public static bool IsVector2(InputValueTypes valueTypes)
        {
            return valueTypes == InputValueTypes.VECTOR2;
        }

        public bool IsButton()
        {
            return IsButton(ValueType);
        }

        public bool IsVector2()
        {
            return IsVector2(ValueType);
        }

        public bool IsHeld()
        {
            return Phase == InputStage.HELD;
        }

        public bool IsReleased()
        {
            return Phase == InputStage.RELEASED;
        }

        public bool IsPending()
        {
            return ProcessStage == InputProcessStage.PENDING;
        }

        public bool IsProcessing()
        {
            return ProcessStage == InputProcessStage.PROCESSING;
        }

        public bool IsInterrupted()
        {
            return ProcessStage == InputProcessStage.INTERRUPTED;
        }

        public void UpdateStage(InputStage phase)
        {
            Phase = phase;
        }

        public void UpdateStage(InputStage phase, float heldTime)
        {
            Phase = phase;
            HeldTime = heldTime;
        }

        public void UpdateDirection(Vector2 direction)
        {
            Debug.Log("Updating direction to: " + direction.ToString());
            this.direction.current = direction;
        }

        public bool IncrementInputFrame()
        {
            if (previousIncrement != Time)
            {
                duration++;
                previousIncrement = Time;
                return true;
            }
            return false;
        }

        public bool TryIncrementInputFrame()
        {
            if (IsHeld() && previousIncrement != Time)
            {
                duration++;
                previousIncrement = Time;
                return true;
            }
            return false;
        }

        public bool IsCombinable()
        {
            return !IsReleased();
        }

        public InputData CombineWith(InputData input)
        {
            direction.current = input.direction.current;
            Phase = input.Phase;
            return this;
        }

        public static InputPair CombineInputPairs(InputPair current, InputPair next)
        {
            int durationLeft = next.input.IsReleased() ? current.duration : next.duration;
            InputData nextInput = current.input.CombineWith(next.input);
            return new InputPair(nextInput, durationLeft);
        }

        public override string ToString()
        {
            return "InputData: [control=" + Mask.name + "], [direction=" + Direction.snappedDirection.ToString() + ", vector=" + Direction.current + "], [processStage=" + processStage.ToString() + "]";
        }
    }
}