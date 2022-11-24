using System.Collections.Generic;
using PlayerIOClient;
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
                            int id = message.GetInt(i++);

                            Vector2 position = new Vector2(message.GetFloat(i++), message.GetFloat(i++));

                            inputs.Add(TickInput.Spawn(id, position, performer));

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
                            int id = message.GetInt(i++);

                            Vector2 position = new Vector2(message.GetFloat(i++), message.GetFloat(i++));

                            int[] ids = Extract<int>(message, i, out i);

                            inputs.Add(TickInput.Build(id, position, ids, performer));

                            break;
                        }
                }
            }

            Inputs = inputs.ToArray();
        }
    }
}
