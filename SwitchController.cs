using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class demonstrates how to send messages using the ChannelBus system.
/// In this example, the switch acts as a transmitter — when the player clicks the button,
/// a message is broadcast on a specific numeric channel.
/// Any GameObject listening on the same channel (for example, a LightBulb) will react instantly,
/// without needing a direct reference or connection.
/// </summary>
public class SwitchController : MonoBehaviour
{
    // Each communication type should have its own channel number.
    // Think of channels like frequencies on a walkie-talkie — only objects
    // tuned to the same channel will receive the message.
    private const int SWITCH_CHANNEL = 345;

    // This is the message that will be sent through the ChannelBus.
    // The listener can interpret it however it likes (e.g., "Turn on", "Turn off", "TOGGLE").
    private const string COMMAND = "TOGGLE";

    // Reference to the UI button that acts as our physical switch.
    [SerializeField] private Button switchButton;

    private void Awake()
    {
        // Subscribe the OnSwitchButtonClicked method to the button's click event.
        // When clicked, it will trigger our message broadcast.
        switchButton.onClick.AddListener(OnSwitchButtonClicked);
    }

    /// <summary>
    /// Called when the player clicks the switch button.
    /// Instead of directly controlling another object, we send a message
    /// using the ChannelBus on the defined channel.
    /// This keeps systems loosely coupled — the switch doesn’t need to know who is listening,
    /// only that the message will be delivered to any subscriber on the same channel.
    /// </summary>
    private void OnSwitchButtonClicked()
    {
        ChannelBus.Send(SWITCH_CHANNEL, COMMAND);
    }
}
