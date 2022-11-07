using System.Collections.Generic;
using PlayerIOClient;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    [SerializeField]
    private bool connect = false;

    private void Start()
    {
        if (!connect)
            return;

        PlayerIO.Authenticate(
            "rts-q2tnacekgeylj7irzdg",
            "public",
            new Dictionary<string, string> {
                { "userId", "Zalpheges" },
            },
            null,
            delegate (Client client) {
                Debug.Log("Successfully connected to Player.IO");

                client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);

                client.Multiplayer.CreateJoinRoom(
                    "test_room",
                    "Game",
                    true,
                    null,
                    null,
                    delegate (Connection connection) {
                        Debug.Log("Joined Room.");
                    },
                    delegate (PlayerIOError error) {
                        Debug.Log("Error Joining Room: " + error.ToString());
                    }
                );
            },
            delegate (PlayerIOError error) {
                Debug.Log("Error connecting: " + error.ToString());
            }
        );
    }
}
