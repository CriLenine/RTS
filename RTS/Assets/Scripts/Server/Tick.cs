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
                    case InputType.QueueSpawn:
                        {
                            int spawnerID = message.GetInt(i++);
                            int prefab = message.GetInt(i++);

                            inputs.Add(TickInput.QueueSpawn(prefab,spawnerID, performer));

                            break;
                        }

                    case InputType.UnqueueSpawn:
                        {
                            int spawnerID = message.GetInt(i++);
                            int prefab = message.GetInt(i++);

                            inputs.Add(TickInput.UnqueueSpawn(prefab, spawnerID, performer));

                            break;
                        }

                    case InputType.Kill:
                        {
                            int[] targets = Extract<int>(message, i, out i);

                            inputs.Add(TickInput.Kill(targets, performer));

                            break;
                        }

                    case InputType.Move:
                        {
                            Vector2 position = new Vector2(message.GetFloat(i++), message.GetFloat(i++));
                            int[] targets = Extract<int>(message, i, out i);

                            inputs.Add(TickInput.Move(targets, position, performer));

                            break;
                        }

                    case InputType.NewBuild:
                        {
                            int prefab = message.GetInt(i++);
                            Vector2 position = new Vector2(message.GetFloat(i++), message.GetFloat(i++));

                            int[] ids = Extract<int>(message, i, out i);

                            inputs.Add(TickInput.NewBuild(prefab, position, ids, performer));

                            break;
                        }

                    case InputType.Build:
                        {
                            int ID = message.GetInt(i++);

                            int[] ids = Extract<int>(message, i, out i);

                            inputs.Add(TickInput.Build(ID, ids, performer));

                            break;
                        }

                    case InputType.Destroy:
                        {
                            int ID = message.GetInt(i++);

                            inputs.Add(TickInput.Destroy(ID, performer));

                            break;
                        }

                    case InputType.Harvest:
                        {
                            Vector2 position = new Vector2(message.GetFloat(i++), message.GetFloat(i++));

                            int[] ids = Extract<int>(message, i, out i);

                            inputs.Add(TickInput.Harvest(position, ids, performer));
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
                    case InputType.GameOver:
                        {
                            inputs.Add(TickInput.GameOver(performer));
                            break;
                        }

                    case InputType.GuardPosition:
                        {
                            Vector2 position = new Vector2(message.GetFloat(i++), message.GetFloat(i++));

                            int[] IDs = Extract<int>(message, i, out i);

                            inputs.Add(TickInput.GuardPosition(position, IDs));
                            break;
                        }
                }
            }

            Inputs = inputs.ToArray();
        }
    }
}
