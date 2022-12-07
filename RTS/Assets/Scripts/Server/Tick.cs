using System.Collections.Generic;
using PlayerIOClient;
using Unity.Burst.Intrinsics;
using UnityEngine;

public partial class NetworkManager
{
    private struct Tick
    {
        public TickInput[] Inputs;

        public Tick(Message message)
        {
            List<TickInput> inputs = new List<TickInput>();

            uint i = 1;

            while (i < message.Count)
            {
                int performer = message.GetInt(i++);

                InputType type = (InputType)message.GetInt(i++);

                switch (type)
                {
                    case InputType.Spawn:
                        {
                            int prefab = message.GetInt(i++);

                            Vector2 position = new Vector2(message.GetFloat(i++), message.GetFloat(i++));

                            inputs.Add(TickInput.Spawn(prefab, position, performer));

                            break;
                        }

                    case InputType.Move:
                        {
                            Vector2 position = new Vector2(message.GetFloat(i++), message.GetFloat(i++));
                            int[] targets = Extract<int>(message, i, out i);

                            inputs.Add(TickInput.Move(targets, position, performer));

                            break;
                        }

                    case InputType.Build:
                        {
                            int prefab = message.GetInt(i++);
                            Debug.Log("tick " + prefab);
                            Vector2 position = new Vector2(message.GetFloat(i++), message.GetFloat(i++));

                            int[] ids = Extract<int>(message, i, out i);

                            inputs.Add(TickInput.Build(prefab, position, ids, performer));

                            break;
                        }

                    case InputType.Harvest:
                        {
                            Vector2 position = new Vector2(message.GetFloat(i++), message.GetFloat(i++));
                            int target = message.GetInt(i++);

                            inputs.Add(TickInput.Harvest(position, target, performer));
                            break;
                        }
                    case InputType.Attack:
                        {
                            int targetId = message.GetInt(i++);

                            Vector2 position = new Vector2(message.GetFloat(i++), message.GetFloat(i++));

                            int[] attackersIds = Extract<int>(message, i, out i);

                            inputs.Add(TickInput.Attack(targetId,position, attackersIds));
                            break;
                        }
                }
            }

            Inputs = inputs.ToArray();
        }
    }
}
